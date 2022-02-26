using IntervalLearningApi.Models;
using IntervalLearningApi.Services.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace IntervalLearningApi.Controllers;

[Authorize]
[ApiController]
[Route("api/authentication")]
public class AuthenticationController : ControllerBase
{
    private const string RefreshTokenKey = "refreshToken";

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
        var response = authService.Register(req, GetSourceIpAddress());
        SetRefreshTokenCookie(response.RefreshToken);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("authenticate")]
    public ActionResult<AuthenticateResponse> Authenticate(AuthenticateRequest model)
    {
        var response = authService.Authenticate(model, GetSourceIpAddress());
        SetRefreshTokenCookie(response.RefreshToken);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public IActionResult RefreshToken()
    {
        var refreshToken = GetRefreshToken(Request);

        if (string.IsNullOrEmpty(refreshToken))
            return BadRequest();

        var response = authService.RefreshToken(refreshToken, GetSourceIpAddress());
        SetRefreshTokenCookie(response.RefreshToken);
        return Ok(response);
    }

    [HttpPost("revoke-token")]
    public IActionResult RevokeToken(RevokeTokenRequest req)
    {
        var token = req.Token ?? GetRefreshToken(Request);

        if (string.IsNullOrEmpty(token))
            return BadRequest("Token is required");

        authService.RevokeToken(token, GetSourceIpAddress());
        return Ok();
    }

    [HttpGet("{id}/refresh-tokens")]
    public IActionResult GetRefreshTokens(int id)
    {
        var user = userService.GetById(id);
        return Ok(user.RefreshTokens);
    }

    private static string? GetRefreshToken(HttpRequest req) => req.Cookies[RefreshTokenKey];

    private void SetRefreshTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(jwtSettings.RefreshTokenTTLInDays)
        };

        Response.Cookies.Append(RefreshTokenKey, token, cookieOptions);
    }

    private string GetSourceIpAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
            return Request.Headers["X-Forwarded-For"];
        else
            return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
    }
}