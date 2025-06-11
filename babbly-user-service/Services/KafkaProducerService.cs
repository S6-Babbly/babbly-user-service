using Confluent.Kafka;
using System.Text.Json;
using babbly_user_service.Models;

namespace babbly_user_service.Services
{
    public class KafkaProducerService : IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaProducerService> _logger;
        private readonly string _userTopic;

        public KafkaProducerService(IConfiguration configuration, ILogger<KafkaProducerService> logger)
        {
            _logger = logger;
            
            // Get Kafka configuration
            var bootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS") ?? 
                                   configuration["Kafka:BootstrapServers"] ?? 
                                   "localhost:9092";
                                   
            _userTopic = Environment.GetEnvironmentVariable("KAFKA_USER_TOPIC") ?? 
                         configuration["Kafka:UserTopic"] ?? 
                         "user-events";

            // Configure Kafka producer
            var config = new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                ClientId = "babbly-user-service",
                Acks = Acks.Leader, // Wait for the leader to acknowledge the message
                MessageSendMaxRetries = 3,
                RetryBackoffMs = 1000, // 1 second backoff between retries
                EnableIdempotence = true // Ensure messages are not duplicated
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
            
            _logger.LogInformation("User Service Kafka producer initialized with bootstrap servers: {BootstrapServers}", bootstrapServers);
        }

        /// <summary>
        /// Publishes a user created event to Kafka
        /// </summary>
        public async Task PublishUserCreatedEventAsync(User user)
        {
            try
            {
                var message = new UserCreatedEvent
                {
                    EventId = Guid.NewGuid().ToString(),
                    EventType = "UserCreated",
                    Timestamp = DateTime.UtcNow,
                    Version = "1.0",
                    UserId = user.Id.ToString(),
                    Auth0Id = user.Auth0Id,
                    Email = user.Email,
                    Name = !string.IsNullOrEmpty(user.FirstName) && !string.IsNullOrEmpty(user.LastName) 
                        ? $"{user.FirstName} {user.LastName}".Trim() 
                        : user.ExtraData?.DisplayName ?? string.Empty,
                    Picture = user.ExtraData?.ProfilePicture ?? string.Empty,
                    CreatedAt = user.CreatedAt
                };

                string json = JsonSerializer.Serialize(message, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower 
                });
                
                var kafkaMessage = new Message<string, string>
                {
                    Key = user.Auth0Id, // Using Auth0Id as the message key for partitioning
                    Value = json
                };

                // Publish message asynchronously
                var deliveryResult = await _producer.ProduceAsync(_userTopic, kafkaMessage);
                
                _logger.LogInformation(
                    "User created event published to Kafka. UserId: {UserId}, Auth0Id: {Auth0Id}, Topic: {Topic}, Partition: {Partition}, Offset: {Offset}",
                    user.Id, user.Auth0Id, deliveryResult.Topic, deliveryResult.Partition, deliveryResult.Offset);
                    
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing user created event to Kafka for user {UserId}", user.Id);
                // Don't rethrow - user creation should succeed even if Kafka fails
            }
        }

        /// <summary>
        /// Publishes a user updated event to Kafka
        /// </summary>
        public async Task PublishUserUpdatedEventAsync(User user)
        {
            try
            {
                var message = new UserUpdatedEvent
                {
                    EventId = Guid.NewGuid().ToString(),
                    EventType = "UserUpdated",
                    Timestamp = DateTime.UtcNow,
                    Version = "1.0",
                    UserId = user.Id.ToString(),
                    Auth0Id = user.Auth0Id,
                    Email = user.Email,
                    Name = !string.IsNullOrEmpty(user.FirstName) && !string.IsNullOrEmpty(user.LastName) 
                        ? $"{user.FirstName} {user.LastName}".Trim() 
                        : user.ExtraData?.DisplayName ?? string.Empty,
                    Picture = user.ExtraData?.ProfilePicture ?? string.Empty,
                    UpdatedAt = user.UpdatedAt
                };

                string json = JsonSerializer.Serialize(message, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower 
                });
                
                var kafkaMessage = new Message<string, string>
                {
                    Key = user.Auth0Id,
                    Value = json
                };

                var deliveryResult = await _producer.ProduceAsync(_userTopic, kafkaMessage);
                
                _logger.LogInformation(
                    "User updated event published to Kafka. UserId: {UserId}, Auth0Id: {Auth0Id}, Topic: {Topic}, Partition: {Partition}, Offset: {Offset}",
                    user.Id, user.Auth0Id, deliveryResult.Topic, deliveryResult.Partition, deliveryResult.Offset);
                    
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing user updated event to Kafka for user {UserId}", user.Id);
                // Don't rethrow - user update should succeed even if Kafka fails
            }
        }

        public void Dispose()
        {
            try
            {
                _producer?.Flush(TimeSpan.FromSeconds(10));
                _producer?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing Kafka producer");
            }
        }
    }
} 