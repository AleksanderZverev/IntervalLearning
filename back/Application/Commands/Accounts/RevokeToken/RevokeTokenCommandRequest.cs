namespace Application.Commands.Accounts.RevokeToken;

public record RevokeTokenCommandRequest(
    string RefreshToken, 
    string IpAddress
);