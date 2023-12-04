using Application.Commands.Accounts.Authenticate;
using Application.Commands.Accounts.AuthenticateByOldToken;
using Application.Commands.Accounts.RefreshToken;
using Application.Commands.Accounts.Register;
using Application.Commands.Accounts.RevokeToken;
using Domain.Common.ValueObjects;
using Domain.Common.ValueObjects.Text.SingleLine;
using Domain.Language.ValueObjects;
using Domain.User.ValueObjects;
using FluentResults;
using FluentResults.Extensions;
using Infrastructure.BoundedContexts.Accounts.Jwt;
using IntervalLearningApi.Constants;
using IntervalLearningApi.Controllers.Accounts.Requests.Authenticate;
using IntervalLearningApi.Controllers.Accounts.Requests.Register;
using IntervalLearningApi.Controllers.Accounts.Requests.RevokeToken;
using IntervalLearningApi.Extensions;
using IntervalLearningApi.Infrastructure.CommandManager;
using IntervalLearningApi.Infrastructure.ValidatorResolver;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntervalLearningApi.Controllers.Accounts;

[Authorize]
[ApiController]
[Route(ApiRoutes.Accounts.BasePath)]
public class AuthenticationController : ControllerBase
{
    private const string RefreshTokenKey = "refreshToken";
    private const string JwtTokenKey = "jwtToken";

    private readonly ValidatorResolver validatorResolver;
    private readonly CommandManager commandManager;
    private readonly IMapper mapper;
    private readonly JwtSettings jwtSettings;

    public AuthenticationController(
        ValidatorResolver validatorResolver,
        CommandManager commandManager,
        JwtSettings jwtSettings,
        IMapper mapper)
    {
        this.validatorResolver = validatorResolver;
        this.commandManager = commandManager;
        this.mapper = mapper;
        this.jwtSettings = jwtSettings;
    }

    [AllowAnonymous]
    [HttpPost(ApiRoutes.Accounts.Register)]
    public Task<ActionResult> Register(RegisterRequest req)
    {
        return validatorResolver.Validate(req)
            .Bind(() => commandManager
                .GetCommand<RegisterAccountCommand>()
                .Handle(new RegisterAccountRequest(
                    EmailAddress.Create(req.Email).Value,
                    MediumSingleLineString.Create(req.Password).Value,
                    UserName.Create(req.FirstName, req.LastName).Value,
                    LanguageId.Create(req.SuggestLanguageId).Value,
                    GetSourceIpAddress()
                )))
            .ToActionResultAsync();
    }

    [AllowAnonymous]
    [HttpPost(ApiRoutes.Accounts.Authenticate)]
    public Task<ActionResult<AuthenticateResponse>> Authenticate(AuthenticateRequest model)
    {
        return validatorResolver.Validate(model)
            .Bind(() => commandManager
                .GetCommand<AuthenticateCommand>()
                .Handle(new AuthenticateCommandRequest(
                    EmailAddress.Create(model.Email).Value,
                    MediumSingleLineString.Create(model.Password).Value,
                    GetSourceIpAddress())))
            .Bind(auth =>
            {
                SetRefreshTokenCookie(auth.RefreshToken);
                SetJwtTokenCookie(auth.JwtToken);
                return mapper.Map<AuthenticateResponse>(auth).ToResult();
            })
            .ToActionResultAsync();
    }

    [AllowAnonymous]
    [HttpPost(ApiRoutes.Accounts.RefreshToken)]
    public async Task<ActionResult<AuthenticateResponse>> RefreshToken()
    {
        var jwtToken = GetJwtToken(Request);
        var refreshToken = GetRefreshToken(Request);

        if (string.IsNullOrEmpty(refreshToken))
            return BadRequest();

        if (!string.IsNullOrEmpty(jwtToken))
        {
            var reAuthResult = await commandManager
                .GetCommand<AuthenticateByOldTokenCommand>()
                .Handle(new AuthenticateByOldTokenRequest(jwtToken, refreshToken));

            if (reAuthResult.IsSuccess)
            {
                return mapper.Map<AuthenticateResponse>(reAuthResult.Value);
            }
        }

        var authResult = await commandManager
            .GetCommand<RefreshTokenCommand>()
            .Handle(new RefreshTokenCommandRequest(refreshToken, GetSourceIpAddress()));

        if (authResult.IsFailed)
            return authResult.ToErrorActionResult();

        var auth = authResult.Value;
        SetRefreshTokenCookie(auth.RefreshToken);
        SetJwtTokenCookie(auth.JwtToken);
        return mapper.Map<AuthenticateResponse>(auth);
    }

    [HttpPost(ApiRoutes.Accounts.RevokeToken)]
    public async Task<IActionResult> RevokeToken(RevokeTokenRequest req)
    {
        var validationResult = validatorResolver.Validate(req);

        if (validationResult.IsFailed)
        {
            return validationResult.ToErrorActionResult();
        }
            
        var token = req.Token ?? GetRefreshToken(Request);

        if (string.IsNullOrEmpty(token))
            return BadRequest("Token is required");

        var result = await commandManager
            .GetCommand<RevokeTokenCommand>()
            .Handle(new RevokeTokenCommandRequest(token, GetSourceIpAddress()));
        
        return result.ToActionResult();
    }

    private static string? GetJwtToken(HttpRequest req) => req.Cookies[JwtTokenKey];
    private static string? GetRefreshToken(HttpRequest req) => req.Cookies[RefreshTokenKey];

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(jwtSettings.RefreshTokenTTLInDays)
        };

        Response.Cookies.Append(RefreshTokenKey, refreshToken, cookieOptions);
    }

    private void SetJwtTokenCookie(string jwtToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddMinutes(jwtSettings.JwtTokenTTLInMinutes)
        };

        Response.Cookies.Append(JwtTokenKey, jwtToken, cookieOptions);
    }

    private string GetSourceIpAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
            return Request.Headers["X-Forwarded-For"];
        else
            return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? string.Empty;
    }
}