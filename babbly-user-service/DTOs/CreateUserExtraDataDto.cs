using System.ComponentModel.DataAnnotations;

namespace babbly_user_service.DTOs
{
    public class CreateUserExtraDataDto
    {
        [StringLength(100, ErrorMessage = "Display name cannot exceed 100 characters")]
        public string? DisplayName { get; set; }

        [DataType(DataType.Url, ErrorMessage = "Profile picture must be a valid URL")]
        [StringLength(2048, ErrorMessage = "Profile picture URL cannot exceed 2048 characters")]
        public string? ProfilePicture { get; set; }

        [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters")]
        public string? Bio { get; set; }

        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string? Address { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? PhoneNumber { get; set; }
    }
} 