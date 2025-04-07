namespace babbly_user_service.DTOs
{
    public class CreateUserExtraDataDto
    {
        public string? DisplayName { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Bio { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
    }
} 