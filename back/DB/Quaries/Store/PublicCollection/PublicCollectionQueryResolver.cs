using Application.Common.Interfaces.DB.Queries.Store.PublicCollection;
using DB.Models.ValueObjects;
using Domain.Collection;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DB.Resolvers.Store.PublicCollection;

public class PublicCollectionQueryResolver : IPublicCollectionQueryResolver
{
    private readonly ApplicationContext db;

    public PublicCollectionQueryResolver(ApplicationContext db)
    {
        this.db = db;
    }

    public async Task<List<Collection>> Search(ThemeId themeId, string searchName, int skip, int take)
    {
        var lowerSearchName = searchName.ToLowerInvariant().Trim();
        return await db.Collections
            .Where(c => c.ThemeId == themeId 
                        && EF.Functions.ILike(c.Title, $"{lowerSearchName}%")
                        && c.IsPublic)
            .Skip(skip)
            .Take(take)
            .Include(c => c.CollectionPublicationEntity)
            .Include(c => c.ParentUser)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<Collection?> Find(UserId userId, CollectionId collectionId)
    {
        var collection = await db.Collections
            .Include(c => c.CollectionPublicationEntity)
            .SingleOrDefaultAsync(c => c.ParentUserId == userId && c.Id == collectionId);
        
        return collection is not {IsPublic: true} 
            ? null 
            : collection;
    }
}