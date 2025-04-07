namespace babbly_user_service.DTOs
{
    public class UserExtraDataDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? DisplayName { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Bio { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
} 