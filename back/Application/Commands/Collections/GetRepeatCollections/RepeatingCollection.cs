using Domain.Collection;

namespace Application.Commands.Collections.GetRepeatCollections;

public record RepeatingCollection(Collection Collection)
{
    public int CardsToRepeatCount { get; set; }
};