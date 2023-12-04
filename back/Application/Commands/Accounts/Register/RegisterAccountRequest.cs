using Domain.Common.ValueObjects.Text.SingleLine;
using Domain.Language.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Accounts.Register;

public record RegisterAccountRequest(
    EmailAddress Email,
    MediumSingleLineString Password,
    UserName UserName,
    LanguageId SuggestLanguageId,
    string SourceIpAddress
);