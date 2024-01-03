using Domain.Language.ValueObjects;
using Domain.User;
using Domain.User.Entities;
using DomainServices.BoundedContext.Accounts.PasswordService;
using DomainServices.DB.Repositories.Accounts;
using DomainServices.DB.Repositories.Accounts.Users;
using DomainServices.DB.Transactions;
using FluentResults;
using GlobalTools.Errors;

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

        var creatingResult = accountRepository.Users.AddAndSave(user);

        if (creatingResult.IsFailed)
        {
            return new InternalError();
        }

        transaction.Complete();
        return Result.Ok();
    }
}