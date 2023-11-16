using DB.Models.ValueObjects;
using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.Common.ValueObjects;
using Domain.User.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DB;

internal static class Converters
{
    public static readonly ValueConverter<UserId, long> UserId = new(
        from => from.Value,
        userId => Domain.User.ValueObjects.UserId.Create(userId).Value
    );

    public static ValueConverter<Counter, short> Counter = new(
        from => (short)from.Value,
        count => Domain.Common.ValueObjects.Counter.Create(count).Value
    );

    public static ValueConverter<CollectionId, short> CollectionId = new(
        from => from.Value,
        collectionId => Domain.Collection.ValueObjects.CollectionId.Create(collectionId).Value
    );

    public static ValueConverter<CardId, short> CardId = new(
        from => from.Value,
        cardId => Domain.Card.ValueObjects.CardId.Create(cardId).Value
    );

    public static ValueConverter<CardText, string> CardText = new(
        from => from.Value,
        cardText => Domain.Card.ValueObjects.CardText.Create(cardText).Value
    );

    public static ValueConverter<CardDescription, string> CardDescription = new(
        from => from.Value,
        cardText => Domain.Card.ValueObjects.CardDescription.Create(cardText).Value
    );

    public static ValueConverter<CardExample, string> CardExample = new(
        from => from.Value,
        cardText => Domain.Card.ValueObjects.CardExample.Create(cardText).Value
    );

    public static ValueConverter<ThemeId, short> ThemeId = new(
        from => from.Value,
        id => Models.ValueObjects.ThemeId.Create(id).Value
    );

    public static ValueConverter<ScheduleId, short> ScheduleId = new(
        from => from.Value,
        id => Models.ValueObjects.ScheduleId.Create(id).Value
    );

    public static ValueConverter<ScheduleShortDescription, string> ScheduleShortDescription = new(
        d => d.Value,
        s => Models.ValueObjects.ScheduleShortDescription.Create(s).Value);
    
    public static ValueConverter<ScheduleLongDescription, string> ScheduleLongDescription = new(
        d => d.Value,
        s => Models.ValueObjects.ScheduleLongDescription.Create(s).Value);

    public static ValueConverter<TFrom?, TTo?> ToNullable<TFrom, TTo>(this ValueConverter<TFrom, TTo> converter)
        where TFrom : class
        where TTo : class
    {
        return new(
            domainModel => domainModel == null
                ? null
                : (TTo?)converter.ConvertToProvider.Invoke(domainModel),
            databaseModel => databaseModel == null
                ? null
                : (TFrom?)converter.ConvertFromProvider.Invoke(databaseModel)
        );
    }
    
    public static ValueConverter<TFrom?, string> ToEmptyString<TFrom>(this ValueConverter<TFrom, string> converter)
        where TFrom : class
    {
        return new(
            domainModel => (string)(domainModel == null
                ? string.Empty
                : converter.ConvertToProvider.Invoke(domainModel) ?? string.Empty),
            databaseModel => string.IsNullOrEmpty(databaseModel)
                ? null
                : (TFrom?)converter.ConvertFromProvider.Invoke(databaseModel)
        );
    }
    
    public static ValueConverter<List<TFrom>, List<TTo>> ToArray<TFrom, TTo>(this ValueConverter<TFrom, TTo> converter)
        where TFrom : class
        where TTo : class
    {
        return new(
            from => from.Select(f => (TTo)converter.ConvertToProvider.Invoke(f)).ToList(),
            cardText => cardText.Select(t => (TFrom)converter.ConvertFromProvider.Invoke(t)).ToList()
        );
    }
}