using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DB.Models
{
    public class UserPasswordsEntity : IParentUserReference
    {
        public long ParentUserId { get; set; }
        public UserEntity? ParentUser { get; set; }

        [MaxLength(255)]
        [Column(TypeName = "varchar(255)")]
        [JsonIgnore]
        public string? PasswordHash { get; set; }
    }
}
