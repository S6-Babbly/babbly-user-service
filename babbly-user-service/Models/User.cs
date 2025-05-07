using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace babbly_user_service.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [Required]
        [JsonPropertyName("auth0_id")]
        public string Auth0Id { get; set; } = string.Empty;
        
        [Required]
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [JsonPropertyName("role")]
        public string Role { get; set; } = "User";
        
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; } = string.Empty;
        
        [JsonPropertyName("last_name")]
        public string LastName { get; set; } = string.Empty;
        
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public UserExtraData? ExtraData { get; set; }
    }
} 