using System.Security.Claims;
using Domain.User.ValueObjects;
using FluentResults;

namespace IntervalLearningApi.Extensions
{
    public static class HttpContextExtensions
    {
        public static Result<UserId> GetUserId(this HttpContext context)
        {
            var userIdString = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString))
            {
                return Result.Fail("User Id not found");
            }

            if (!long.TryParse(userIdString, out var userId))
                return Result.Fail("Incorrect user Id");

            return UserId.Create(userId);
        }
    }
}
