using System.Text.Json.Serialization;
using DB.Models;

namespace IntervalLearningApi.Models;

public class AuthenticateResponse
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string JwtToken { get; set; }

    [JsonIgnore] // refresh token is returned in http only cookie
    public string RefreshToken { get; set; }

    public AuthenticateResponse(UserEntity userEntity, string jwtToken, string refreshToken)
    {
        Id = userEntity.Id.ToString();
        FirstName = userEntity.FirstName;
        LastName = userEntity.LastName;
        Email = userEntity.Email;
        JwtToken = jwtToken;
        RefreshToken = refreshToken;
    }
}