using System.Security.Claims;
using DB;
using IntervalLearningApi.Models;
using IntervalLearningApi.Services.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace IntervalLearningApi;

public class JwtMiddleware
{
    private const string BearerPrefix = "Bearer ";

    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment env;
    private readonly JwtSettings _jwtSettings;

    public JwtMiddleware(
        RequestDelegate next, 
        IOptions<JwtSettings> appSettings,  
        IWebHostEnvironment env)
    {
        _next = next;
        this.env = env;
        _jwtSettings = appSettings.Value;
    }

    public async Task Invoke(HttpContext context, ApplicationContext db, IJwtService jwtService)
    {
        var authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        if (authorizationHeader == null || !authorizationHeader.StartsWith(BearerPrefix))
        {
            context.User = new ClaimsPrincipal();
            await _next(context);
            return;
        }

        var principal = new ClaimsPrincipal();

        var securityToken = authorizationHeader[BearerPrefix.Length..];

        var customIdentity = await ValidateCustom(securityToken, db, jwtService);

        if (customIdentity != null)
        {
            principal.AddIdentity(customIdentity);
        }

        context.User = principal;
        await _next(context);
    }

    private static async Task<ClaimsIdentity?> ValidateCustom(string securityToken, ApplicationContext db, IJwtService jwtService)
    {
        var userId = jwtService.ValidateJwtToken(securityToken);

        if (userId == null)
            return null;

        var user = await db.Users.FindAsync(userId);

        if (user == null)
            return null;

        var claims = JwtService.GetClaims(user);
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