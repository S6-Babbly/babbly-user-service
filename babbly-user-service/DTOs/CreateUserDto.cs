using System.ComponentModel.DataAnnotations;

namespace babbly_user_service.DTOs
{
    public class CreateUserDto
    {
        [Required(ErrorMessage = "Auth0Id is required")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Auth0Id must be between 5 and 100 characters")]
        public string Auth0Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "Username can only contain letters, numbers, dots, underscores, and hyphens")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        [RegularExpression(@"^(User|Admin|Moderator)$", ErrorMessage = "Role must be 'User', 'Admin', or 'Moderator'")]
        public string Role { get; set; } = "User"; // Default role

        public CreateUserExtraDataDto? ExtraData { get; set; }
    }
} 