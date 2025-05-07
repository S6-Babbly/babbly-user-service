using System.Text.Json;
using babbly_user_service.Models;
using Confluent.Kafka;

namespace babbly_user_service.Services
{
    public class KafkaProducerService
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaProducerService> _logger;
        private const string AUTHORIZATION_REQUEST_TOPIC = "auth-requests";

        public KafkaProducerService(IConfiguration configuration, ILogger<KafkaProducerService> logger)
        {
            _logger = logger;
            
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"] ?? Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS") ?? "localhost:9092",
                ClientId = "babbly-user-producer"
            };

            _producer = new ProducerBuilder<string, string>(producerConfig).Build();
            
            _logger.LogInformation("Kafka producer initialized with bootstrap servers: {servers}", 
                producerConfig.BootstrapServers);
        }

        public async Task<string> RequestAuthorizationAsync(string userId, List<string> roles, string resourcePath, string operation)
        {
            try
            {
                var authMessage = new AuthMessage
                {
                    UserId = userId,
                    Roles = roles,
                    IsAuthenticated = !string.IsNullOrEmpty(userId),
                    ResourcePath = resourcePath,
                    Operation = operation,
                    CorrelationId = Guid.NewGuid().ToString()
                };

                var messageJson = JsonSerializer.Serialize(authMessage);
                var message = new Message<string, string>
                {
                    Key = authMessage.CorrelationId,
                    Value = messageJson
                };

                var deliveryResult = await _producer.ProduceAsync(AUTHORIZATION_REQUEST_TOPIC, message);
                
                _logger.LogInformation("Authorization request delivered to {topic} [partition: {partition}, offset: {offset}, correlation: {correlationId}]", 
                    deliveryResult.Topic, deliveryResult.Partition, deliveryResult.Offset, authMessage.CorrelationId);
                
                return authMessage.CorrelationId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error producing authorization request message");
                throw;
            }
        }

        public void Dispose()
        {
            _producer?.Dispose();
        }
    }
} 