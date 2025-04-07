namespace babbly_user_service.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Auth0Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public UserExtraDataDto? ExtraData { get; set; }
    }
} 