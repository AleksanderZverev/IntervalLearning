using DomainServices.DB.Repositories.Accounts;
using FluentResults;
using GlobalTools;
using GlobalTools.Errors;

namespace Application.Commands.Accounts.RevokeToken;

public class RevokeTokenCommand : ICommand<RevokeTokenCommandRequest>
{
    private readonly IDateTimeProvider dateTimeProvider;
    private readonly IAccountRepository accountRepository;

    public RevokeTokenCommand(
        IDateTimeProvider dateTimeProvider,
        IAccountRepository accountRepository)
    {
        this.dateTimeProvider = dateTimeProvider;
        this.accountRepository = accountRepository;
    }

    public async Task<Result> Handle(RevokeTokenCommandRequest request)
    {
        var user = await accountRepository.Query.Users.FindUserByRefreshToken(request.RefreshToken);

        if (user == null)
        {
            return new BadRequestError("Refresh token is expired");
        }

        var refreshToken = user.RefreshTokens.Single(x => x.Token == request.RefreshToken);

        if (!refreshToken.IsActive)
            return new BadRequestError("Refresh token is invalid");
        
        refreshToken.Revoke(dateTimeProvider.UtcNow, request.IpAddress, "Revoked without replacement");

        accountRepository.Users.Update(user);
        return await accountRepository.SaveChangesAsync();
    }
}