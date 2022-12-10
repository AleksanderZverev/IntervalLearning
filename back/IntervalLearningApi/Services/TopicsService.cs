using DB;
using DB.Models;
using IntervalLearningApi.Models.Topics;
using Microsoft.EntityFrameworkCore;

namespace IntervalLearningApi.Services;

public class TopicsService
{
    private readonly ApplicationContext db;

    public TopicsService(ApplicationContext db)
    {
        this.db = db;
    }

    public async Task<(TopicEntity? course, string? error)> Create(long userId, long courseId, CreateTopicParameters parameters)
    {
        var course = await db.Courses.FindAsync(courseId);
        if (course == null)
            return (null, "Course not found");
        if (!course.AdminIds.Contains(userId))
            return (null, $"User can't perform operation because {userId} is not admin of current course");

        var topic = new TopicEntity
        {
            ParentCourseId = courseId,
            Name = parameters.Name,
            Theory = parameters.Theory
        };
        await db.Topics.AddAsync(topic);

        try
        {
            await db.SaveChangesAsync();
            return (topic, null);
        }
        catch
        {
            return (null, "Unknown error");
        }
    }
    
    public async Task<(TopicEntity? course, string? error)> Patch(long userId, long courseId, long topicId, PatchTopicParameters parameters)
    {
        var course = await db.Courses.FindAsync(courseId);
        if (course == null)
            return (null, "Course not found");
        if (!course.AdminIds.Contains(userId))
            return (null, $"User can't perform operation because {userId} is not admin of current course");

        var topic = await db.Topics.FindAsync(courseId, topicId);
        if (topic == null)
            return (null, "Topic not found");

        var entry = db.Entry(topic);
        entry.CurrentValues.SetValues(parameters);

        try
        {
            await db.SaveChangesAsync();
            return (topic, null);
        }
        catch
        {
            return (null, "Unknown error");
        }
    }

    public async Task<TopicEntity?> Get(long courseId, long topicId) =>
        await db.Topics.FindAsync(courseId, topicId);

    public Task<List<TopicEntity>> SearchByName(long courseId, string? name, int page, int count)
    {
        var toSkip = (page - 1) * count;

        var query = db.Topics.Where(x => x.ParentCourseId == courseId);

        if (name != null)
            query = query.Where(x => x.Name.ToLower().StartsWith(name));

        return query
            .Skip(toSkip)
            .Take(count)
            .ToListAsync();
    }

    public async Task<(TopicEntity? course, string? error)> Delete(long userId, long courseId, long topicId)
    {
        var course = await db.Courses.FindAsync(courseId);
        if (course == null)
            return (null, "Course not found");
        if (!course.AdminIds.Contains(userId))
            return (null, $"User can't perform operation because {userId} is not admin of current course");

        var topicEntity = await db.Topics.FindAsync(courseId, topicId);

        if (topicEntity == null)
            return (null, "Topic not found");

        db.Topics.Remove(topicEntity);

        try
        {
            await db.SaveChangesAsync();
            return (topicEntity, null);
        }
        catch
        {
            return (null, "Unknown error");
        }
    }
}