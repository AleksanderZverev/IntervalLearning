using DB;
using DB.Models;
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

    public async Task<(TopicCollectionEntity? topicCollection, string? error)> Create(
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
    
    public async Task<(TopicCollectionEntity? topicCollection, string? error)> Patch(
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

    public Task<List<TopicCollectionEntity>> SearchByName(long courseId, long topicId, string? name, int page, int count)
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

    public async Task<(TopicCollectionEntity? topicCollection, string? error)> Delete(
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
}