using System.Linq.Expressions;
using DB;
using DB.Models;
using IntervalLearningApi.Models;
using IntervalLearningApi.Models.Topics.TopicCollections;
using Microsoft.EntityFrameworkCore;

namespace IntervalLearningApi.Services;

public class TopicCollectionsService
{
    private readonly ApplicationContext db;

    public TopicCollectionsService(ApplicationContext db)
    {
        this.db = db;
    }

    public async Task<(TopicCollectionEntity? topicCollection, string? error)> CreateTopicCollection(
        long userId,
        long courseId,
        long topicId,
        CreateTopicCollectionParameters parameters)
    {
        var course = await db.Courses.FindAsync(courseId);
        if (course == null)
            return (null, "Course not found");
        if (!course.AdminIds.Contains(userId))
            return (null, $"User can't perform operation because {userId} is not admin of current course");
        
        var topic = await db.Topics.FindAsync(courseId, topicId);
        if (topic == null)
            return (null, "Topic not found");
        
        var topicCollection = new TopicCollectionEntity
        {
            ParentCourseId = courseId,
            ParentTopicId = topicId,
            Name = parameters.Name
        };
        await db.TopicCollections.AddAsync(topicCollection);

        try
        {
            await db.SaveChangesAsync();
            return (topicCollection, null);
        }
        catch
        {
            return (null, "Unknown error");
        }
    }
    
    public async Task<(TopicCollectionEntity? topicCollection, string? error)> PatchTopicCollection(
        long userId,
        long courseId,
        long topicId,
        long topicCollectionId,
        PatchTopicCollectionParameters parameters)
    {
        var course = await db.Courses.FindAsync(courseId);
        if (course == null)
            return (null, "Course not found");
        if (!course.AdminIds.Contains(userId))
            return (null, $"User can't perform operation because {userId} is not admin of current course");

        var topic = await db.Topics.FindAsync(courseId, topicId);
        if (topic == null)
            return (null, "Topic not found");
        
        var topicCollection = await db.TopicCollections.FindAsync(courseId, topicId, topicCollectionId);
        if (topicCollection == null)
            return (null, "Topic's collection not found");

        var entry = db.Entry(topicCollection);
        entry.CurrentValues.SetValues(parameters);

        try
        {
            await db.SaveChangesAsync();
            return (topicCollection, null);
        }
        catch
        {
            return (null, "Unknown error");
        }
    }

    public async Task<TopicCollectionEntity?> Get(long courseId, long topicId, long topicCollectionId) =>
        await db.TopicCollections.FindAsync(courseId, topicId, topicCollectionId);

    public Task<List<TopicCollectionEntity>> SearchTopicCollections(long courseId, long topicId, string? name, int page, int count)
    {
        var toSkip = (page - 1) * count;

        var query = db.TopicCollections.Where(x => x.ParentCourseId == courseId && x.ParentTopicId == topicId);

        if (name != null)
            query = query.Where(x => x.Name.ToLower().StartsWith(name));

        return query
            .Skip(toSkip)
            .Take(count)
            .ToListAsync();
    }

    public async Task<(TopicCollectionEntity? topicCollection, string? error)> DeleteTopicCollection(
        long userId,
        long courseId,
        long topicId,
        long topicCollectionId)
    {
        var course = await db.Courses.FindAsync(courseId);
        if (course == null)
            return (null, "Course not found");
        if (!course.AdminIds.Contains(userId))
            return (null, $"User can't perform operation because {userId} is not admin of current course");

        var topic = await db.Topics.FindAsync(courseId, topicId);
        if (topic == null)
            return (null, "Topic not found");
        
        var topicCollection = await db.TopicCollections.FindAsync(courseId, topicId, topicCollectionId);
        if (topicCollection == null)
            return (null, "Topic's collection not found");

        db.TopicCollections.Remove(topicCollection);

        try
        {
            await db.SaveChangesAsync();
            return (topicCollection, null);
        }
        catch
        {
            return (null, "Unknown error");
        }
    }

    public async Task<(TopicCardEntity? topicCard, string? error)> CreateTopicCard(
        long userId,
        long courseId,
        long topicId,
        long topicCollectionId,
        CreateTopicCardParameters parameters)
    {
        var course = await db.Courses.FindAsync(courseId);
        if (course == null)
            return (null, "Course not found");
        if (!course.AdminIds.Contains(userId))
            return (null, $"User can't perform operation because {userId} is not admin of current course");

        var topicCollection = await db.TopicCollections.FindAsync(courseId, topicId, topicCollectionId);
        if (topicCollection == null)
            return (null, "Topic's collection not found");

        var topicCard = new TopicCardEntity
        {
            RememberingText = parameters.RememberingText,
            PromptText = parameters.PromptText,
            MeaningText = parameters.MeaningText,
            Description = parameters.Description,
            Examples = parameters.Examples,
            ParentCourseId = courseId,
            ParentTopicId = topicId,
            ParentTopicCollectionId = topicCollectionId
        };
        await db.TopicCards.AddAsync(topicCard);

        try
        {
            await db.SaveChangesAsync();
            return (topicCard, null);
        }
        catch
        {
            return (null, "Unknown error");
        }
    }

    public async Task<(TopicCardEntity? topicCollection, string? error)> PatchTopicCard(
        long userId,
        long courseId,
        long topicId,
        long topicCollectionId,
        long topicCardId,
        PatchTopicCardParameters parameters)
    {
        var course = await db.Courses.FindAsync(courseId);
        if (course == null)
            return (null, "Course not found");
        if (!course.AdminIds.Contains(userId))
            return (null, $"User can't perform operation because {userId} is not admin of current course");

        var topicCard = await db.TopicCards.FindAsync(courseId, topicId, topicCollectionId, topicCardId);
        if (topicCard == null)
            return (null, "Topic's card not found");

        var entry = db.Entry(topicCard);
        entry.CurrentValues.SetValues(parameters);

        try
        {
            await db.SaveChangesAsync();
            return (topicCard, null);
        }
        catch
        {
            return (null, "Unknown error");
        }
    }

    public async Task<TopicCardEntity?> GetCard(long courseId, long topicId, long topicCollectionId, long topicCardId) =>
        await db.TopicCards.FindAsync(courseId, topicId, topicCollectionId, topicCardId);

    public async Task<List<TopicCardEntity>> SearchTopicCards(
        long courseId,
        long topicId,
        long topicCollectionId,
        string searchValue,
        SearchFieldType fieldType,
        int page,
        int count)
    {
        var skip = (page - 1) * count;

        return fieldType switch
        {
            SearchFieldType.RememberingText => await GetCards(c =>
                c.ParentCourseId == courseId
                && c.ParentTopicId == topicId
                && c.ParentTopicCollectionId == topicCollectionId
                && c.RememberingText.ToLower().StartsWith(searchValue), skip, count),
            SearchFieldType.PromptText => await GetCards(c =>
                c.ParentCourseId == courseId
                && c.ParentTopicId == topicId
                && c.ParentTopicCollectionId == topicCollectionId
                && c.PromptText.ToLower().StartsWith(searchValue), skip, count),
            SearchFieldType.MeaningText => await GetCards(c =>
                c.ParentCourseId == courseId
                && c.ParentTopicId == topicId
                && c.ParentTopicCollectionId == topicCollectionId
                && c.MeaningText.ToLower().StartsWith(searchValue), skip, count),
            _ => throw new ArgumentOutOfRangeException(nameof(fieldType), fieldType, null)
        };
    }

    public async Task<(TopicCardEntity? topicCollection, string? error)> DeleteTopicCard(
        long userId,
        long courseId,
        long topicId,
        long topicCollectionId,
        long topicCardId)
    {
        var course = await db.Courses.FindAsync(courseId);
        if (course == null)
            return (null, "Course not found");
        if (!course.AdminIds.Contains(userId))
            return (null, $"User can't perform operation because {userId} is not admin of current course");

        var topicCard = await db.TopicCards.FindAsync(courseId, topicId, topicCollectionId, topicCardId);
        if (topicCard == null)
            return (null, "Topic's card not found");

        db.TopicCards.Remove(topicCard);

        try
        {
            await db.SaveChangesAsync();
            return (topicCard, null);
        }
        catch
        {
            return (null, "Unknown error");
        }
    }

    private async Task<List<TopicCardEntity>> GetCards(Expression<Func<TopicCardEntity, bool>> condition, int skip, int take) =>
        await db.TopicCards
            .Where(condition)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
}