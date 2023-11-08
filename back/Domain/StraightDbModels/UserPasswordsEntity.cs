using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Domain.User;
using Domain.User.ValueObjects;

namespace DB.Models
{
    public class UserPasswordsEntity : IParentUserReference
    {
        public UserId ParentUserId { get; set; }
        public User? ParentUser { get; set; }

        [MaxLength(60)]
        [Column(TypeName = "varchar(60)")]
        [JsonIgnore]
        public string? PasswordHash { get; set; }
    }
}
