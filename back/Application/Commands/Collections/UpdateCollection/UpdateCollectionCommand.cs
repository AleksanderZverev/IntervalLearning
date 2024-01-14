using Domain.Collection;
using Domain.Collection.ValueObjects;
using DomainServices.DB.Repositories.Study;
using FluentResults;
using FluentResults.Extensions;
using GlobalTools.Errors;
using GlobalTools.Extensions;

namespace Application.Commands.Collections.UpdateCollection;

public class UpdateCollectionCommand : ICommand<UpdateCollectionCommandRequest, Collection>
{
    private readonly IStudyRepository studyRepository;

    public UpdateCollectionCommand(
        IStudyRepository studyRepository)
    {
        this.studyRepository = studyRepository;
    }

    public async Task<Result<Collection>> Handle(UpdateCollectionCommandRequest request)
    {
        return await studyRepository.Query.Collections
            .Find(request.ParentUserId, request.CollectionId)
            .ToResultAsync()
            .ErrorIfNull(new NotFoundError(nameof(Collection)))
            .Bind(collection =>
            {
                collection.Title = CollectionTitle.Create(request.Title).Value;
                collection.ThemeId = request.ThemeId;
                collection.IsDefaultBackSide = request.IsDefaultBackSide;
                return studyRepository.Collections.UpdateAndSave(collection);
            });
    }
}