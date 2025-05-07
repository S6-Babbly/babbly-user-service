using System.Collections.Concurrent;
using System.Text.Json;
using babbly_user_service.Models;
using Confluent.Kafka;

namespace babbly_user_service.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly ILogger<KafkaConsumerService> _logger;
        private const string AUTHORIZATION_RESPONSE_TOPIC = "auth-responses";
        
        // Store pending authorization responses by correlation ID
        private readonly ConcurrentDictionary<string, TaskCompletionSource<AuthMessage>> _pendingResponses = new();

        public KafkaConsumerService(IConfiguration configuration, ILogger<KafkaConsumerService> logger)
        {
            _logger = logger;
            
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"] ?? Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS") ?? "localhost:9092",
                GroupId = "babbly-user-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true
            };

            _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
            _consumer.Subscribe(AUTHORIZATION_RESPONSE_TOPIC);
            
            _logger.LogInformation("Kafka consumer initialized with bootstrap servers: {servers}", 
                consumerConfig.BootstrapServers);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Kafka consumer service starting");
            
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(stoppingToken);
                        
                        _logger.LogInformation("Message received from {topic} [partition: {partition}, offset: {offset}]", 
                            consumeResult.Topic, consumeResult.Partition, consumeResult.Offset);

                        await ProcessMessageAsync(consumeResult.Message.Value, stoppingToken);
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Error consuming message");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown
                _logger.LogInformation("Kafka consumer service stopping");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Kafka consumer service");
            }
            finally
            {
                _consumer?.Close();
                _consumer?.Dispose();
            }
        }

        private async Task ProcessMessageAsync(string messageJson, CancellationToken cancellationToken)
        {
            try
            {
                var authResponse = JsonSerializer.Deserialize<AuthMessage>(messageJson);
                
                if (authResponse == null)
                {
                    _logger.LogWarning("Failed to deserialize auth response");
                    return;
                }

                _logger.LogInformation("Processing auth response for correlation ID {correlationId}, authorized: {isAuthorized}", 
                    authResponse.CorrelationId, authResponse.IsAuthorized);

                // Check if we have a pending request for this correlation ID
                if (_pendingResponses.TryRemove(authResponse.CorrelationId, out var tcs))
                {
                    // Complete the task with the response
                    tcs.SetResult(authResponse);
                }
                else
                {
                    _logger.LogWarning("Received auth response for unknown correlation ID: {correlationId}", 
                        authResponse.CorrelationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
            }
        }

        /// <summary>
        /// Create a pending task to wait for an authorization response with the given correlation ID
        /// </summary>
        public Task<AuthMessage> WaitForAuthorizationResponseAsync(string correlationId, TimeSpan timeout)
        {
            var tcs = new TaskCompletionSource<AuthMessage>();
            
            // Add the TaskCompletionSource to our dictionary
            _pendingResponses[correlationId] = tcs;
            
            // Create a timeout task
            _ = Task.Delay(timeout).ContinueWith(t => 
            {
                if (_pendingResponses.TryRemove(correlationId, out var pendingTcs))
                {
                    _logger.LogWarning("Authorization request timed out for correlation ID: {correlationId}", correlationId);
                    pendingTcs.SetException(new TimeoutException($"Authorization request timed out for correlation ID: {correlationId}"));
                }
            });
            
            return tcs.Task;
        }

        public override void Dispose()
        {
            _consumer?.Close();
            _consumer?.Dispose();
            base.Dispose();
        }
    }
} 