using FluentResults;
using IntervalLearningApi.Models;

namespace IntervalLearningApi.Services.Authentication;

public interface IAuthenticationService
{
    Result<AuthenticateResponse> Authenticate(AuthenticateRequest req, string ipAddress);
    Result<AuthenticateResponse> RefreshToken(string refreshToken, string ipAddress);
    Result RevokeToken(string token, string ipAddress);
    AuthenticateResponse? TryAuthenticateByOldToken(string jwtToken, string refreshToken);
    Result Register(RegisterRequest request, string sourceIpAddress);
}