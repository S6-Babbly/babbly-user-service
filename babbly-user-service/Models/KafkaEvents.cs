using System.Text.Json.Serialization;

namespace babbly_user_service.Models
{
    /// <summary>
    /// Base class for Kafka events
    /// </summary>
    public class KafkaEventBase
    {
        [JsonPropertyName("event_id")]
        public string EventId { get; set; } = string.Empty;
        
        [JsonPropertyName("event_type")]
        public string EventType { get; set; } = string.Empty;
        
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
        
        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Event published when a user is created
    /// </summary>
    public class UserCreatedEvent : KafkaEventBase
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;
        
        [JsonPropertyName("auth0_id")]
        public string Auth0Id { get; set; } = string.Empty;
        
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
        
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        
        [JsonPropertyName("picture")]
        public string? Picture { get; set; }
        
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }
    
    /// <summary>
    /// Event published when a user is updated
    /// </summary>
    public class UserUpdatedEvent : KafkaEventBase
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;
        
        [JsonPropertyName("auth0_id")]
        public string Auth0Id { get; set; } = string.Empty;
        
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
        
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        
        [JsonPropertyName("picture")]
        public string? Picture { get; set; }
        
        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
} 