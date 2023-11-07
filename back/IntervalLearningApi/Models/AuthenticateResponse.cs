using System.Text.Json.Serialization;
using DB.Models;
using Domain.Language.ValueObjects;

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

    public string SuggestTranslationLanguageId { get; set; }
}