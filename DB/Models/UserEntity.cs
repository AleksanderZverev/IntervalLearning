using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DB.Models;
#nullable enable

public class UserEntity
{
    [Key]
    public long Id { get; set; }
        
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; }

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; }

    [Required]
    [MaxLength(255)]
    [Column(TypeName = "varchar(255)")]
    public string Email { get; set; }

    [JsonIgnore]
    public UserPasswordsEntity? PasswordHash { get; set; }

    public bool EmailConfirmed { get; set; }

    [JsonIgnore] 
    public List<RefreshTokenEntity> RefreshTokens { get; set; } = new();
}