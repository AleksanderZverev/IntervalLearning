using System.Net.Mail;
using Domain.Common.ValueObjects;
using FluentResults;

namespace Domain.User.ValueObjects;

public class EmailAddress : SingleValueObject<string>
{
    private EmailAddress(string value) : base(value)
    {
    }

    public static Result<EmailAddress> Create(string dirtyEmail)
    {
        if (string.IsNullOrWhiteSpace(dirtyEmail))
            return Result.Fail("Email is empty");

        var email = dirtyEmail.ToLowerInvariant().Trim();

        if (email.Length > 255)
            return Result.Fail("Email is too long");

        if (!MailAddress.TryCreate(email, out var correctEmail))
            return Result.Fail("Incorrect email address");

        return new EmailAddress(correctEmail.Address);
    }
}