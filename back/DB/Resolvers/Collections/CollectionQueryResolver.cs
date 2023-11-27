using Application.Common.Interfaces.Domain.Collections;
using DB.Models.ValueObjects;
using Domain.Collection;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace DB.Resolvers.Collections;

public class CollectionQueryResolver : ICollectionQueryResolver
{
    private readonly ApplicationContext db;

    public CollectionQueryResolver(ApplicationContext db)
    {
        this.db = db;
    }

    public Task<Collection?> Find(UserId userId, CollectionId collectionId)
    {
        return db.Collections.FindAsync(userId, collectionId).AsTask();
    }

    public async Task<List<Collection>> SearchPublicCollection(
        ThemeId themeId,
        string searchName,
        int skip,
        int take)
    {
        var lowerSearchName = searchName.ToLowerInvariant().Trim();
        return await db.Collections
            .Where(c => c.ThemeId == themeId 
                        && c.IsPublic 
                        && EF.Functions.ILike(c.Title, $"{lowerSearchName}%"))
            .Skip(skip)
            .Take(take)
            .Include(c => c.CollectionPublicationEntity)
            .Include(c => c.ParentUser)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<Result<List<Collection>>> SearchPrivate(
        UserId userId,
        ThemeId themeId, 
        string searchName,
        int skip,
        int take)
    {
        var lowerSearchName = searchName.ToLowerInvariant().Trim();
        return await db.Collections
            .Where(c => c.ParentUserId == userId
                        && c.ThemeId == themeId
                        && EF.Functions.ILike(c.Title, $"{lowerSearchName}%"))
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }
}