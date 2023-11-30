using Domain.Common.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Accounts.Authenticate;

public record AuthenticateCommandRequest(
    EmailAddress Email,
    MediumSingleLineString Password,
    string IpAddress
);