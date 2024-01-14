using FluentAssertions.Equivalency;
using IntervalLearningApi.Controllers.Study.Cards.DTOs;
using IntervalLearningApi.Controllers.Study.Collections.DTOs;
using Newtonsoft.Json;

namespace IntervalLearningApi.IntegrationTests.Common.Extensions;

public static class BaseExtensions
{
    public static TResponse? ToResponseDto<TResponse>(this HttpResponseMessage? response)
        where TResponse : class
        => ToResponseDtoAsync<TResponse>(response).GetAwaiter().GetResult();
    
    public static async Task<TResponse?> ToResponseDtoAsync<TResponse>(this HttpResponseMessage? response)
        where TResponse : class
    {
        if (response is not { IsSuccessStatusCode: true })
            return null;

        var responseJson = await response.Content.ReadAsStringAsync();
        
        if (responseJson == null)
            return null;
        
        return JsonConvert.DeserializeObject<TResponse>(responseJson);
    }

    public static EquivalencyAssertionOptions<CardDto> ForCard(this EquivalencyAssertionOptions<CardDto> options)
    {
        options.Using<DateTime>(ctx => 
                ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromMilliseconds(100)))
            .WhenTypeIs<DateTime>();
        
        return options;
    }
    
    public static EquivalencyAssertionOptions<CollectionDto> ForCollection(this EquivalencyAssertionOptions<CollectionDto> options)
    {
        options.Using<DateTime>(ctx => 
                ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromMilliseconds(100)))
            .WhenTypeIs<DateTime>();
        
        return options;
    }
}