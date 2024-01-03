using Application.Commands.Accounts.Authenticate;
using DomainServices.BoundedContext.Accounts.JwtService;
using DomainServices.DB.Repositories.Accounts;
using FluentResults;
using FluentResults.Extensions;
using GlobalTools;
using GlobalTools.Errors;
using GlobalTools.Extensions;

namespace Application.Commands.Accounts.AuthenticateByOldToken;

public class AuthenticateByOldTokenCommand : ICommand<AuthenticateByOldTokenRequest, AuthenticateCommandResponse>
{
    private readonly IAccountRepository accountRepository;
    private readonly IDateTimeProvider dateTimeProvider;
    private readonly IJwtService jwtService;

    public AuthenticateByOldTokenCommand(
        IAccountRepository accountRepository,
        IDateTimeProvider dateTimeProvider,
        IJwtService jwtService)
    {
        this.accountRepository = accountRepository;
        this.dateTimeProvider = dateTimeProvider;
        this.jwtService = jwtService;
    }

    public async Task<Result<AuthenticateCommandResponse>> Handle(AuthenticateByOldTokenRequest request)
    {
        var userIdResult = jwtService.ValidateJwtToken(request.JwtToken, dateTimeProvider.UtcNow.AddMinutes(5));

        if (userIdResult.IsFailed)
            return userIdResult.ToResult();

        return await accountRepository.Query.Users.Find(userIdResult.Value)
            .ToResultAsync()
            .ErrorIfNull(new InternalError())
            .Bind(user => new AuthenticateCommandResponse(user, request.JwtToken, request.RefreshToken).ToResult());
    }
}