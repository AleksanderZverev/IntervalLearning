using System.Security.Claims;
using DB;
using DB.Models;
using Google.Apis.Auth;
using IntervalLearningApi.Controllers.Users;
using IntervalLearningApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace IntervalLearningApi;

public class JwtMiddleware
{
    private const string BearerPrefix = "Bearer ";

    private readonly RequestDelegate _next;
    private readonly JwtSettings _jwtSettings;
    private readonly GoogleSettings _googleSettings;

    public JwtMiddleware(RequestDelegate next, IOptions<JwtSettings> appSettings, IOptions<GoogleSettings> googleSettings)
    {
        _next = next;
        _jwtSettings = appSettings.Value;
        _googleSettings = googleSettings.Value;
    }

    public async Task Invoke(HttpContext context, ApplicationContext db, IJwtUtils jwtUtils)
    {
        var authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        if (authorizationHeader == null || !authorizationHeader.StartsWith(BearerPrefix))
        {
            await _next(context);
            return;
        }

        var principal = new ClaimsPrincipal();

        var securityToken = authorizationHeader[BearerPrefix.Length..];

        var customIdentity = await ValidateCustom(securityToken, db, jwtUtils);
        if (customIdentity != null)
        {
            principal.AddIdentity(customIdentity);
        }
        else
        {
            var googleIdentity = ValidateGoogle(securityToken, db);
            principal.AddIdentity(googleIdentity);
        }

        context.User = principal;
        await _next(context);
    }

    private static async Task<ClaimsIdentity?> ValidateCustom(string securityToken, ApplicationContext db, IJwtUtils jwtUtils)
    {
        var userId = jwtUtils.ValidateJwtToken(securityToken);

        if (userId == null)
            return null;

        var user = await db.Users.FindAsync(userId);

        if (user == null)
            return null;

        var claims = JwtUtils.GetClaims(user);
        return new ClaimsIdentity(claims);
    }

    private ClaimsIdentity ValidateGoogle(string securityToken, ApplicationContext db)
    {
        GoogleJsonWebSignature.Payload payload;

        try
        {
            payload = GoogleJsonWebSignature.ValidateAsync(securityToken,
                new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new[] {_googleSettings.ClientId},
                }).Result;
        }
        catch
        {
            return null;
        }

        var user = db.Users.SingleOrDefault(u => u.Email == payload.Email);

        if (user == null)
        {
            user = new UserEntity()
            {
                Email = payload.Email,
                EmailConfirmed = payload.EmailVerified,
                FirstName = payload.GivenName,
                LastName = payload.FamilyName,
            };
            db.Users.Add(user);
        }

        var claims = JwtUtils.GetClaims(user);
        return new ClaimsIdentity(claims);
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
        if (allowAnonymous)
            return;

        var identity = context.HttpContext.User.Identity;

        if (identity == null)
            context.Result = new UnauthorizedResult();
    }
}