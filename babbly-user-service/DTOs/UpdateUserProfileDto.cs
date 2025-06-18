using System.ComponentModel.DataAnnotations;

namespace babbly_user_service.DTOs
{
    public class UpdateUserProfileDto
    {
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
        public string? Username { get; set; }
        
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string? Email { get; set; }
        
        [StringLength(100, ErrorMessage = "Display name cannot exceed 100 characters")]
        public string? DisplayName { get; set; }
        
        [Url(ErrorMessage = "Profile picture must be a valid URL")]
        public string? ProfilePicture { get; set; }
        
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string? Address { get; set; }
        
        [Phone(ErrorMessage = "Invalid phone number")]
        public string? PhoneNumber { get; set; }
    }
} 