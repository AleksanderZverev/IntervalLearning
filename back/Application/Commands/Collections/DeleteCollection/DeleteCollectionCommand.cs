using DomainServices.DB.Repositories.Study;
using FluentResults;
using FluentResults.Extensions;
using GlobalTools.Errors;
using GlobalTools.Extensions;

namespace Application.Commands.Collections.DeleteCollection;

public class DeleteCollectionCommand : ICommand<DeleteCollectionCommandRequest>
{
    private readonly IStudyRepository studyRepository;

    public DeleteCollectionCommand(IStudyRepository studyRepository)
    {
        this.studyRepository = studyRepository;
    }

    public async Task<Result> Handle(DeleteCollectionCommandRequest request)
    {
        var (userId, collectionId) = request;
        return await studyRepository.Query.Collections.Find(userId, collectionId)
            .ToResultAsync()
            .ErrorIfNull(new NotFoundError("Collection"))
            .Bind(async collection =>
            {
                var containsCards = await studyRepository.Query.Cards.ContainsAny(userId, collectionId);

                if (containsCards)
                {
                    return new BadRequestError("Collection is not empty");
                }

                collection.Delete();
                return studyRepository.Collections.DeleteAndSave(collection).ToResult();
            });
    }
}