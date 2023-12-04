using Application.Commands.Accounts.Authenticate;
using Application.Common.Accounts.JwtService;
using Application.Common.Interfaces.DB.Repositories.Accounts;
using Application.Common.Interfaces.DB.Transactions;
using Domain.User;
using Domain.User.Entities;
using FluentResults;
using Infrastructure;
using Infrastructure.Errors;

namespace Application.Commands.Accounts.RefreshToken;

public class RefreshTokenCommand : ICommand<RefreshTokenCommandRequest, AuthenticateCommandResponse>
{
    private readonly ITransactionProvider transactionProvider;
    private readonly IDateTimeProvider dateTimeProvider;
    private readonly IJwtService jwtService;
    private readonly IAccountRepository accountRepository;

    public RefreshTokenCommand(
        ITransactionProvider transactionProvider,
        IDateTimeProvider dateTimeProvider,
        IJwtService jwtService,
        IAccountRepository accountRepository)
    {
        this.transactionProvider = transactionProvider;
        this.dateTimeProvider = dateTimeProvider;
        this.jwtService = jwtService;
        this.accountRepository = accountRepository;
    }

    public async Task<Result<AuthenticateCommandResponse>> Handle(RefreshTokenCommandRequest request)
    {
        var user = await accountRepository.Query.Users.FindUserByRefreshToken(request.RefreshToken);

        if (user == null)
        {
            return new BadRequestError("Refresh token is expired");
        }
        
        var refreshTokenItem = user.RefreshTokens.Single(t => t.Token == request.RefreshToken);

        if (refreshTokenItem.IsRevoked)
        {
            RevokeDescendantRefreshTokens(refreshTokenItem, user, request.IpAddress, $"Attempted reuse of revoked ancestor token: {refreshTokenItem.Token}");
            accountRepository.Users.Update(user);
        }

        if (!refreshTokenItem.IsActive)
            return new BadRequestError("Refresh token is invalid");

        var newRefreshToken = ReplaceOldRefreshToken(user, refreshTokenItem, request.IpAddress);
        user.RefreshTokens.Add(newRefreshToken);
        accountRepository.RefreshTokens.Add(newRefreshToken);

        var newRefreshTokenResult = await accountRepository.SaveChangesAsync();
        if (newRefreshTokenResult.IsFailed)
            return newRefreshTokenResult;
        
        RemoveOldRefreshTokens(user);
        var jwtToken = jwtService.GenerateJwtToken(user);
        
        return new AuthenticateCommandResponse(user, jwtToken, newRefreshToken.Token);
    }
    
    private RefreshTokenEntity ReplaceOldRefreshToken(User user, RefreshTokenEntity tokenToRevoke, string ipAddress)
    {
        var newRefreshToken = jwtService.GenerateRefreshToken(user, ipAddress);
        tokenToRevoke.Revoke(dateTimeProvider.UtcNow, ipAddress, "Replaced by new token", newRefreshToken.Token);
        return newRefreshToken;
    }
    
    private void RevokeDescendantRefreshTokens(RefreshTokenEntity refreshTokenEntity, User userEntity, string ipAddress, string reason)
    {
        if (string.IsNullOrEmpty(refreshTokenEntity.ReplacedByToken))
            return;
        
        var childToken = userEntity.RefreshTokens.SingleOrDefault(x => x.Token == refreshTokenEntity.ReplacedByToken);

        if (childToken == null)
            return;

        if (childToken.IsActive)
            childToken.Revoke(dateTimeProvider.UtcNow, ipAddress, reason);
        else
        {
            RevokeDescendantRefreshTokens(childToken, userEntity, ipAddress, reason);
        }
    }
    
    private void RemoveOldRefreshTokens(User userEntity)
    {
        //TODO: Move to event?
        var tokensToDelete = userEntity.RefreshTokens
            .Where(refreshToken => !refreshToken.IsActive && jwtService.IsTokenExpired(refreshToken.Created))
            .ToList();
        
        accountRepository.RefreshTokens.DeleteRange(tokensToDelete);
        accountRepository.SaveChanges();
    }
}