using System.Text.Json.Serialization;

namespace otr_backend.Models
{
    public class User
    {
        public uint Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        [JsonIgnore]
        public string HashedPassword { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime LastLogin { get; set; }
        public bool IsActive { get; set; }
        public bool IsEmailVerified { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public uint? ManagerId { get; set; }
    }
}
