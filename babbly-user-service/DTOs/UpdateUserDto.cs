using System.ComponentModel.DataAnnotations;

namespace babbly_user_service.DTOs
{
    public class UpdateUserDto
    {
        public string? Username { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? DisplayName { get; set; }

        public string? ProfilePicture { get; set; }

        public string? Bio { get; set; }
    }
} 