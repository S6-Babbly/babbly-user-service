using System.Text.Json.Serialization;

namespace babbly_user_service.Models
{
    public class Auth0UserProfile
    {
        [JsonPropertyName("auth0Id")]
        public string Auth0Id { get; set; } = string.Empty;
        
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
        
        [JsonPropertyName("username")]
        public string? Username { get; set; }
        
        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }
        
        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }
        
        [JsonPropertyName("fullName")]
        public string? FullName { get; set; }
        
        [JsonPropertyName("picture")]
        public string? Picture { get; set; }
        
        [JsonPropertyName("emailVerified")]
        public bool EmailVerified { get; set; }
        
        [JsonPropertyName("updatedAt")]
        public DateTime? UpdatedAt { get; set; }
    }
} 