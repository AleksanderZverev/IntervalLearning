using Domain.Collection;

namespace Application.Commands.Collections.GetCanStartCollections;

public record GetCanStartCollectionsResponse(
    int TotalCollections, 
    List<Collection> CanStartCollections
);