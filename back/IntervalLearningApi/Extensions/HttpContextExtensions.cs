using System.Security.Claims;

namespace IntervalLearningApi.Extensions
{
    public static class HttpContextExtensions
    {
        public static long GetUserId(this HttpContext context)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                   throw new NotSupportedException("UserId not found");

            return long.Parse(userId);
        }
    }
}
