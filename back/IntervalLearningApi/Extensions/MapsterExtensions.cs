using System.Linq.Expressions;
using Mapster;

namespace IntervalLearningApi.Extensions;

public static class MapsterExtensions
{
    public static TypeAdapterSetter<string?, TDestination?> MapWhenNotNullOrEmpty<TDestination>(
        this TypeAdapterSetter<string?, TDestination?> setter,
        Func<string?, TDestination> converterFactory)
        where TDestination : class
    {
        setter.MapWith(stringValue => !string.IsNullOrEmpty(stringValue) ? converterFactory(stringValue) : null);
        setter.IgnoreNullValues(true);
        return setter;
    }
}