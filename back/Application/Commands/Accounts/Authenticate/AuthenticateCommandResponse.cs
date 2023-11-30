using Domain.User;

namespace Application.Commands.Accounts.Authenticate;

public record AuthenticateCommandResponse(
    User User,
    string JwtToken,
    string RefreshToken 
);