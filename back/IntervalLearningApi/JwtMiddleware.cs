using System.Security.Claims;
using Application.Common.Accounts.JwtService;
using Application.Common.Interfaces.DB.Queries.Accounts;
using DB;
using DB.Quaries.Accounts;
using Domain.User.ValueObjects;
using Infrastructure.BoundedContexts.Accounts.Jwt;
using IntervalLearningApi.Services.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IntervalLearningApi;

public class JwtMiddleware
{
    private const string BearerPrefix = "Bearer ";

    private readonly RequestDelegate _next;
    private readonly JwtSettings _jwtSettings;

    public JwtMiddleware(
        RequestDelegate next, 
        JwtSettings appSettings)
    {
        _next = next;
        _jwtSettings = appSettings;
    }

    public async Task Invoke(HttpContext context, IAccountQueryRepository accountRep, IJwtService jwtService)
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

        var customIdentity = await ValidateCustom(securityToken, accountRep, jwtService);

        if (customIdentity != null)
        {
            principal.AddIdentity(customIdentity);
        }

        context.User = principal;
        await _next(context);
    }

    private static async Task<ClaimsIdentity?> ValidateCustom(string securityToken, IAccountQueryRepository accountRep, IJwtService jwtService)
    {
        var userIdResult = jwtService.ValidateJwtToken(securityToken);

        if (userIdResult.IsFailed)
            return null;

        var userId = userIdResult.Value;
        var user = await accountRep.Users.Find(userId);

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