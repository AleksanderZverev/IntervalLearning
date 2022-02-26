using IntervalLearningApi.Models;

namespace IntervalLearningApi.Services.Authentication;

public interface IAuthenticationService
{
    AuthenticateResponse Authenticate(AuthenticateRequest req, string ipAddress);
    AuthenticateResponse RefreshToken(string refreshToken, string ipAddress);
    void RevokeToken(string token, string ipAddress);
    (bool ok, string error) Register(RegisterRequest request, string sourceIpAddress);
}