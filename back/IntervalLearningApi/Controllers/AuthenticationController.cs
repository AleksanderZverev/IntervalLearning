using IntervalLearningApi.Models;
using IntervalLearningApi.Services.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace IntervalLearningApi.Controllers;

[Authorize]
[ApiController]
[Route("api/accounts")]
public class AuthenticationController : ControllerBase
{
    private const string RefreshTokenKey = "refreshToken";
    private const string JwtTokenKey = "jwtToken";

    private readonly IAuthenticationService authService;
    private readonly UserService userService;
    private readonly JwtSettings jwtSettings;

    public AuthenticationController(
        IOptions<JwtSettings> jwtSettings,
        IAuthenticationService authService, 
        UserService userService)
    {
        this.authService = authService;
        this.userService = userService;
        this.jwtSettings = jwtSettings.Value;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public IActionResult Register(RegisterRequest req)
    {
        var (ok, errorMessage) = authService.Register(req, GetSourceIpAddress());
        return ok ? Ok() : BadRequest(errorMessage);
    }

    [AllowAnonymous]
    [HttpPost("authenticate")]
    public ActionResult<AuthenticateResponse> Authenticate(AuthenticateRequest model)
    {
        var (response, error) = authService.Authenticate(model, GetSourceIpAddress());

        if (response == null)
            return BadRequest(error);

        SetRefreshTokenCookie(response.RefreshToken);
        SetJwtTokenCookie(response.JwtToken);
        return response;
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
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

        var (response, error) = authService.RefreshToken(refreshToken, GetSourceIpAddress());

        if (response == null)
            return BadRequest(error);

        SetRefreshTokenCookie(response.RefreshToken);
        SetJwtTokenCookie(response.JwtToken);
        return response;
    }

    [HttpPost("revoke-token")]
    public IActionResult RevokeToken(RevokeTokenRequest req)
    {
        var token = req.Token ?? GetRefreshToken(Request);

        if (string.IsNullOrEmpty(token))
            return BadRequest("Token is required");

        var (ok, error) = authService.RevokeToken(token, GetSourceIpAddress());
        return ok ? Ok() : BadRequest(error);
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
            return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
    }
}