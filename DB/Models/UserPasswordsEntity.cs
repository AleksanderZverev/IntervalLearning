using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DB.Models
{
    public class UserPasswordsEntity
    {
        [Key]
        public long Id { get; set; }

        [MaxLength(255)]
        [Column(TypeName = "varchar(255)")]
        [JsonIgnore]
        public string? PasswordHash { get; set; }
    }
}
