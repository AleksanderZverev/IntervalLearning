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
        if (response == null)
            return null;
        
        var responseJson = await response.Content.ReadAsStringAsync();
        
        if (responseJson == null)
            return null;
        
        return JsonConvert.DeserializeObject<TResponse>(responseJson);
    }
}