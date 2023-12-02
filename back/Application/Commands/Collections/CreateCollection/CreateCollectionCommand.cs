using Application.Common.Interfaces.DB.Repositories.Study;
using Domain.Collection;
using Domain.Collection.ValueObjects;
using FluentResults;

namespace Application.Commands.Collections.CreateCollection;

public class CreateCollectionCommand : ICommand<CreateCollectionCommandRequest, Collection>
{
    private readonly IStudyRepository studyRepository;

    public CreateCollectionCommand(
        IStudyRepository studyRepository)
    {
        this.studyRepository = studyRepository;
    }

    public Task<Result<Collection>> Handle(CreateCollectionCommandRequest request)
    {
        var collectionId = studyRepository.Collections.GetUniqueId(new(request.ParentUserId)).Value;

        var newCollectionResult = Collection.Create(
            request.ParentUserId,
            collectionId,
            CollectionTitle.Create(request.Title).Value,
            request.ThemeId);

        if (newCollectionResult.IsFailed)
            return Task.FromResult<Result<Collection>>(new Error("Creation error"));

        var newCollection = newCollectionResult.Value;
        newCollection.IsDefaultBackSide = request.IsDefaultBackSide;
        
        return Task.FromResult(studyRepository.Collections.Add(newCollection));
    }
}