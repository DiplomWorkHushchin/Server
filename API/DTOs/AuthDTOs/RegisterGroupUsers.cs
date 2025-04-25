using API.Entities;

namespace API.DTOs.AuthDTOs
{
    public class RegisterGroupUsers
    {
        public RegisterDto[] Users { get; set; } = [];
        public required string Group { get; set; }
        public required string Kurator { get; set; }
    }
}
