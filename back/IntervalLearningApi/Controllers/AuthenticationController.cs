using IntervalLearningApi.Constants;
using IntervalLearningApi.Extensions;
using IntervalLearningApi.Models;
using IntervalLearningApi.Services.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace IntervalLearningApi.Controllers;

[Authorize]
[ApiController]
[Route(ApiRoutes.Accounts.BasePath)]
public class AuthenticationController : ControllerBase
{
    private const string RefreshTokenKey = "refreshToken";
    private const string JwtTokenKey = "jwtToken";

    private readonly IAuthenticationService authService;
    private readonly UserService userService;
    private readonly JwtSettings jwtSettings;

    public AuthenticationController(
        JwtSettings jwtSettings,
        IAuthenticationService authService, 
        UserService userService)
    {
        this.authService = authService;
        this.userService = userService;
        this.jwtSettings = jwtSettings;
    }

    [AllowAnonymous]
    [HttpPost(ApiRoutes.Accounts.Register)]
    public IActionResult Register(RegisterRequest req)
    {
        var result = authService.Register(req, GetSourceIpAddress());
        return result.ToActionResult();
    }

    [AllowAnonymous]
    [HttpPost(ApiRoutes.Accounts.Authenticate)]
    public ActionResult<AuthenticateResponse> Authenticate(AuthenticateRequest model)
    {
        var authResult = authService.Authenticate(model, GetSourceIpAddress());

        if (authResult.IsFailed)
            return authResult.ToErrorActionResult();

        var auth = authResult.Value;
        SetRefreshTokenCookie(auth.RefreshToken);
        SetJwtTokenCookie(auth.JwtToken);
        return auth;
    }

    [AllowAnonymous]
    [HttpPost(ApiRoutes.Accounts.RefreshToken)]
    public ActionResult<AuthenticateResponse> RefreshToken()
    {
        var jwtToken = GetJwtToken(Request);
        var refreshToken = GetRefreshToken(Request);

        if (string.IsNullOrEmpty(refreshToken))
            return BadRequest();

        var oldResponse = string.IsNullOrEmpty(jwtToken)
            ? null
            : authService.TryAuthenticateByOldToken(jwtToken, refreshToken);

        if (oldResponse != null)
            return Ok(oldResponse);

        var authResult = authService.RefreshToken(refreshToken, GetSourceIpAddress());

        if (authResult.IsFailed)
            return authResult.ToErrorActionResult();

        var auth = authResult.Value;
        SetRefreshTokenCookie(auth.RefreshToken);
        SetJwtTokenCookie(auth.JwtToken);
        return auth;
    }

    [HttpPost(ApiRoutes.Accounts.RevokeToken)]
    public IActionResult RevokeToken(RevokeTokenRequest req)
    {
        var token = req.Token ?? GetRefreshToken(Request);

        if (string.IsNullOrEmpty(token))
            return BadRequest("Token is required");

        var result = authService.RevokeToken(token, GetSourceIpAddress());
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