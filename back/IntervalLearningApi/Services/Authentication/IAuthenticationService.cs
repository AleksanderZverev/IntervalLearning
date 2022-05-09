using IntervalLearningApi.Models;

namespace IntervalLearningApi.Services.Authentication;

public interface IAuthenticationService
{
    (AuthenticateResponse? response, string? errorMessage) Authenticate(AuthenticateRequest req, string ipAddress);
    (AuthenticateResponse? response, string? error) RefreshToken(string refreshToken, string ipAddress);
    (bool ok, string? error) RevokeToken(string token, string ipAddress);
    AuthenticateResponse? TryAuthenticateByOldToken(string jwtToken, string refreshToken);
    (bool ok, string? error) Register(RegisterRequest request, string sourceIpAddress);
}