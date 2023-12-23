namespace Application.Commands.Accounts.RefreshToken;

public record RefreshTokenCommandRequest(
    string RefreshToken,
    string IpAddress
);