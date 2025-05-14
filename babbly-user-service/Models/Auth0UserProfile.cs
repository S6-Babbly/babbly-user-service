using System.Text.Json.Serialization;

namespace babbly_user_service.Models
{
    public class Auth0UserProfile
    {
        [JsonPropertyName("sub")]
        public string Auth0Id { get; set; } = string.Empty;
        
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
        
        [JsonPropertyName("username")]
        public string? Username { get; set; }
        
        [JsonPropertyName("given_name")]
        public string? FirstName { get; set; }
        
        [JsonPropertyName("family_name")]
        public string? LastName { get; set; }
        
        [JsonPropertyName("name")]
        public string? FullName { get; set; }
        
        [JsonPropertyName("picture")]
        public string? Picture { get; set; }
        
        [JsonPropertyName("email_verified")]
        public bool EmailVerified { get; set; }
        
        [JsonPropertyName("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
} 