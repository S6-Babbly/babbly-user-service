using System.ComponentModel.DataAnnotations;

namespace babbly_user_service.DTOs
{
    public class CreateUserDto
    {
        [Required]
        public string Auth0Id { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? DisplayName { get; set; }

        public string? ProfilePicture { get; set; }

        public string? Bio { get; set; }
    }
} 