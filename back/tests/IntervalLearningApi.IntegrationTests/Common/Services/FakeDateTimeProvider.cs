using System.Collections.Concurrent;
using Domain.UnitTests.Common.Mocks.DateTimeProvider;
using Domain.User.ValueObjects;
using GlobalTools;
using IntervalLearningApi.Extensions;
using NUnit.Framework;

namespace IntervalLearningApi.IntegrationTests.Common.Services;

public class FakeDateTimeProvider : IDateTimeProvider
{
    private static ConcurrentDictionary<UserId, DateTimeProviderMock> userIdToCurrentDateTime = [];
    
    public DateTime UtcNow => GetCurrentDateTime();
    
    private readonly IHttpContextAccessor httpContextAccessor;

    public FakeDateTimeProvider(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    private DateTime GetCurrentDateTime()
    {
        if (httpContextAccessor.HttpContext == null)
        {
            TestContext.Out.WriteLine("Used DateTime.UtcNow (user was not authorized).");
            return DateTime.UtcNow;
        }

        var userIdResult = httpContextAccessor.HttpContext.GetUserId();

        if (userIdResult.IsFailed)
        {
            TestContext.Out.WriteLine("Used DateTime.UtcNow (user was not authorized).");
            TestContext.Error.WriteLine("Used DateTime.UtcNow (couldn't find a user id).");
            return DateTime.UtcNow;
        }

        if (userIdToCurrentDateTime.TryGetValue(userIdResult.Value, out var currentDateTimeProvider))
        {
            return currentDateTimeProvider.UtcNow;
        }
        
        return DateTime.UtcNow;
    }

    public static void SetUserDateTime(UserId userId, DateTime dateTime)
    {
        userIdToCurrentDateTime[userId] = new DateTimeProviderMock(dateTime);
    }
}