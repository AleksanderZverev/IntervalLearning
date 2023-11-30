using System.Text.Json.Serialization;
using Application.Commands.Accounts.Authenticate;
using Mapster;

namespace IntervalLearningApi.Models;

public class AuthenticateResponseRegister : IRegister 
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AuthenticateCommandResponse, AuthenticateResponse>()
            .Map(d => d.RefreshToken, s => s.RefreshToken)
            .Map(d => d.JwtToken, s => s.JwtToken)
            .Map(d => d.SuggestTranslationLanguageId, s => s.User.Metadata.SuggestTranslationLanguageId)
            .Map(d => d, s => s.User.UserName)
            .Map(d => d, s => s.User);
    }
}

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