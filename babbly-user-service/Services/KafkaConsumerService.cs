using Confluent.Kafka;
using babbly_user_service.Models;
using System.Text.Json;
using babbly_user_service.Data;
using Microsoft.EntityFrameworkCore;

namespace babbly_user_service.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly string _userTopic;

        public KafkaConsumerService(IConfiguration configuration, IServiceScopeFactory scopeFactory, ILogger<KafkaConsumerService> logger)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;

            // Get Kafka configuration
            var bootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS") ?? 
                                   configuration["Kafka:BootstrapServers"] ?? 
                                   "localhost:9092";
                                   
            _userTopic = Environment.GetEnvironmentVariable("KAFKA_USER_TOPIC") ?? 
                         configuration["Kafka:UserTopic"] ?? 
                         "user-events";

            // Configure Kafka consumer
            var config = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = "babbly-user-service",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                EnableAutoOffsetStore = false
            };

            _consumer = new ConsumerBuilder<string, string>(config).Build();
            
            _logger.LogInformation("Kafka consumer initialized with bootstrap servers: {BootstrapServers}", bootstrapServers);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Kafka consumer service starting...");
            
            try
            {
                _consumer.Subscribe(_userTopic);
                _logger.LogInformation("Subscribed to topic: {Topic}", _userTopic);

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(stoppingToken);

                        if (consumeResult != null)
                        {
                            _logger.LogInformation("Received message: {Topic}, {Partition}, {Offset}",
                                consumeResult.Topic, consumeResult.Partition, consumeResult.Offset);

                            await ProcessMessageAsync(consumeResult.Message.Value);
                            
                            _consumer.Commit(consumeResult);
                            _consumer.StoreOffset(consumeResult);
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Error consuming message from Kafka");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // This is expected when stoppingToken is canceled
                _logger.LogInformation("Kafka consumer service stopping...");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in Kafka consumer service");
            }
            finally
            {
                _consumer.Close();
                _logger.LogInformation("Kafka consumer service stopped");
            }
        }

        private async Task ProcessMessageAsync(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                _logger.LogWarning("Received empty message");
                return;
            }

            try
            {
                // First, try to parse the base event to get the event type
                var baseEvent = JsonSerializer.Deserialize<KafkaEventBase>(message);
                
                if (baseEvent == null)
                {
                    _logger.LogWarning("Could not deserialize message: {Message}", message);
                    return;
                }

                switch (baseEvent.EventType)
                {
                    case "UserCreated":
                        await HandleUserCreatedEventAsync(message);
                        break;
                    case "UserUpdated":
                        await HandleUserUpdatedEventAsync(message);
                        break;
                    default:
                        _logger.LogWarning("Unknown event type: {EventType}", baseEvent.EventType);
                        break;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing message: {Message}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message: {Message}", message);
            }
        }

        private async Task HandleUserCreatedEventAsync(string message)
        {
            var userCreatedEvent = JsonSerializer.Deserialize<UserCreatedEvent>(message);
            
            if (userCreatedEvent == null)
            {
                _logger.LogWarning("Could not deserialize UserCreatedEvent");
                return;
            }

            _logger.LogInformation("Processing UserCreatedEvent for user: {UserId}", userCreatedEvent.UserId);

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Check if user already exists by Auth0Id
            var existingUser = await dbContext.Users
                .FirstOrDefaultAsync(u => u.Auth0Id == userCreatedEvent.Auth0Id);

            if (existingUser != null)
            {
                _logger.LogInformation("User already exists: {Auth0Id}", userCreatedEvent.Auth0Id);
                return;
            }

            // Create new user
            var user = new User
            {
                Auth0Id = userCreatedEvent.Auth0Id,
                Email = userCreatedEvent.Email,
                Username = GenerateUsername(userCreatedEvent.Email, userCreatedEvent.Name),
                FirstName = ExtractFirstName(userCreatedEvent.Name),
                LastName = ExtractLastName(userCreatedEvent.Name),
                Role = "User", // Default role
                CreatedAt = userCreatedEvent.CreatedAt,
                UpdatedAt = DateTime.UtcNow
            };

            // Create user extra data if name or picture is provided
            if (!string.IsNullOrEmpty(userCreatedEvent.Name) || !string.IsNullOrEmpty(userCreatedEvent.Picture))
            {
                user.ExtraData = new UserExtraData
                {
                    DisplayName = userCreatedEvent.Name,
                    ProfilePicture = userCreatedEvent.Picture,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
            }

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Created new user: {UserId}, {Auth0Id}", user.Id, user.Auth0Id);
        }

        private async Task HandleUserUpdatedEventAsync(string message)
        {
            var userUpdatedEvent = JsonSerializer.Deserialize<UserUpdatedEvent>(message);
            
            if (userUpdatedEvent == null)
            {
                _logger.LogWarning("Could not deserialize UserUpdatedEvent");
                return;
            }

            _logger.LogInformation("Processing UserUpdatedEvent for user: {UserId}", userUpdatedEvent.UserId);

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Find user by Auth0Id
            var user = await dbContext.Users
                .Include(u => u.ExtraData)
                .FirstOrDefaultAsync(u => u.Auth0Id == userUpdatedEvent.Auth0Id);

            if (user == null)
            {
                _logger.LogWarning("User not found: {Auth0Id}. Creating new user instead.", userUpdatedEvent.Auth0Id);
                await HandleUserCreatedEventAsync(message);
                return;
            }

            // Update user properties
            user.Email = userUpdatedEvent.Email;
            user.FirstName = ExtractFirstName(userUpdatedEvent.Name);
            user.LastName = ExtractLastName(userUpdatedEvent.Name);
            user.UpdatedAt = DateTime.UtcNow;

            // Update or create extra data
            if (user.ExtraData == null)
            {
                user.ExtraData = new UserExtraData
                {
                    DisplayName = userUpdatedEvent.Name,
                    ProfilePicture = userUpdatedEvent.Picture,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
            }
            else
            {
                user.ExtraData.DisplayName = userUpdatedEvent.Name;
                user.ExtraData.ProfilePicture = userUpdatedEvent.Picture;
                user.ExtraData.UpdatedAt = DateTime.UtcNow;
            }

            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated user: {UserId}, {Auth0Id}", user.Id, user.Auth0Id);
        }

        private string GenerateUsername(string email, string? name)
        {
            // Generate username from email or name
            if (!string.IsNullOrEmpty(name))
            {
                // Remove spaces and special characters
                var username = new string(name.ToLower()
                    .Where(c => char.IsLetterOrDigit(c) || c == '.' || c == '_' || c == '-')
                    .ToArray());
                
                return username.Length > 0 ? username : "user";
            }
            
            // Fall back to email prefix
            var emailPrefix = email.Split('@')[0];
            return emailPrefix;
        }

        private string ExtractFirstName(string? fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                return string.Empty;
                
            var parts = fullName.Split(' ');
            return parts[0];
        }

        private string ExtractLastName(string? fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                return string.Empty;
                
            var parts = fullName.Split(' ');
            return parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : string.Empty;
        }

        public override void Dispose()
        {
            _consumer?.Dispose();
            base.Dispose();
        }
    }
} 