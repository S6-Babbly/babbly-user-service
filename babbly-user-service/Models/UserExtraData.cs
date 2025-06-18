using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace babbly_user_service.Models
{
    public class UserExtraData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public string? DisplayName { get; set; }

        public string? ProfilePicture { get; set; }

        public string? Bio { get; set; }
        
        public string? Address { get; set; }
        
        public string? PhoneNumber { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property - JsonIgnore to prevent circular reference
        [ForeignKey("UserId")]
        [JsonIgnore]
        public User User { get; set; } = null!;
    }
} 