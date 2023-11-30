using Application.Common.Accounts.PasswordService;
using Application.Common.Interfaces.DB.Repositories.Accounts;
using Application.Common.Interfaces.DB.Repositories.Accounts.Users;
using Application.Common.Interfaces.DB.Transactions;
using DB.Models;
using Domain.Language.ValueObjects;
using Domain.User;
using Domain.User.Entities;
using FluentResults;
using Infrastructure.Errors;

namespace Application.Commands.Accounts.Register;

public class RegisterAccountCommand : ICommand<RegisterAccountRequest>
{
    private readonly ITransactionProvider transactionProvider;
    private readonly IAccountRepository accountRepository;
    private readonly IPasswordService passwordService;

    public RegisterAccountCommand(
        ITransactionProvider transactionProvider,
        IAccountRepository accountRepository,
        IPasswordService passwordService
        )
    {
        this.transactionProvider = transactionProvider;
        this.accountRepository = accountRepository;
        this.passwordService = passwordService;
    }

    public async Task<Result> Handle(RegisterAccountRequest request)
    {
        var email = request.Email;
        var sameUser = await accountRepository.Query.Users.FindByEmail(email);

        if (sameUser != null)
            return new ConflictError("Email");

        var userIdResult = accountRepository.Users.GetUniqueId(new UserIdParams());
        
        if (userIdResult.IsFailed)
            return new InternalError();
        
        var passwordHash = passwordService.GeneratePasswordHash(request.Password);
        var user = new User(userIdResult.Value)
        {
            Email = email,
            UserName = request.UserName,
            PasswordHash = UserPassword.Create(userIdResult.Value, passwordHash).Value,
            Metadata = new UserMetadata(userIdResult.Value, LanguageId.Create(request.SuggestLanguageId).Value)
        };
        
        using var transaction = transactionProvider.CreateScope();

        var creatingResult = Result.Ok()
            .Bind(() => accountRepository.Users.Add(user));

        if (creatingResult.IsFailed)
        {
            return new InternalError();
        }

        transaction.Complete();
        return Result.Ok();
    }
}