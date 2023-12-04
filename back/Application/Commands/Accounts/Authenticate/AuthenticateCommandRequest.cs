using Domain.Common.ValueObjects.Text.SingleLine;
using Domain.User.ValueObjects;

namespace Application.Commands.Accounts.Authenticate;

public record AuthenticateCommandRequest(
    EmailAddress Email,
    MediumSingleLineString Password,
    string IpAddress
);