namespace Application.Commands.Accounts.AuthenticateByOldToken;

public record AuthenticateByOldTokenRequest(
    string JwtToken,
    string RefreshToken
);