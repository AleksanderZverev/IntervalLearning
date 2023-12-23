using Application.Common.Accounts.JwtService;
using Application.Common.Accounts.PasswordService;
using Application.Common.Interfaces.DB.Repositories.Accounts;
using Domain.User;
using FluentResults;
using Infrastructure.Errors;

namespace Application.Commands.Accounts.Authenticate;

public class AuthenticateCommand : ICommand<AuthenticateCommandRequest, AuthenticateCommandResponse>
{
    private readonly IPasswordService passwordService;
    private readonly IJwtService jwtService;
    private readonly IAccountRepository accountRepository;

    public AuthenticateCommand(
        IPasswordService passwordService,
        IJwtService jwtService,
        IAccountRepository accountRepository)
    {
        this.passwordService = passwordService;
        this.jwtService = jwtService;
        this.accountRepository = accountRepository;
    }


    public async Task<Result<AuthenticateCommandResponse>> Handle(AuthenticateCommandRequest request)
    {
        var user = await accountRepository.Query.Users.FindByEmail(request.Email);

        if (user == null)
            return new NotFoundError("User");

        if (user is {PasswordHash: null})
            return new BadRequestError("User is not signed up");
        
        if (!passwordService.IsPasswordCorrect(request.Password, user.PasswordHash.PasswordHash))
            return new BadRequestError("Email or password is incorrect");

        return Authenticate(user, request.IpAddress);
    }
    
    private Result<AuthenticateCommandResponse> Authenticate(User user, string ipAddress)
    {
        var jwtToken = jwtService.GenerateJwtToken(user);
        var refreshToken = jwtService.GenerateRefreshToken(user, ipAddress);

        var addRefreshTokenResult = accountRepository.RefreshTokens.AddAndSave(refreshToken);

        if (addRefreshTokenResult.IsFailed)
            return addRefreshTokenResult.ToResult();
        
        RemoveOldRefreshTokens(user);
        return new AuthenticateCommandResponse(user, jwtToken, refreshToken.Token);
    }
    
    private void RemoveOldRefreshTokens(User userEntity)
    {
        var tokensToDelete = userEntity.RefreshTokens
            .Where(refreshToken => !refreshToken.IsActive && jwtService.IsTokenExpired(refreshToken.Created))
            .ToList();
        
        accountRepository.RefreshTokens.DeleteRange(tokensToDelete);
        accountRepository.SaveChanges();
    }
}