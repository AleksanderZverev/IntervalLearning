using System.Linq.Expressions;
using DB;
using DB.Models;
using IntervalLearningApi.Models.Topics;
using Microsoft.EntityFrameworkCore;

namespace IntervalLearningApi.Services;

public class TopicsService
{
    private readonly ILogger<CardsService> logger;
    private readonly IWebHostEnvironment env;
    private readonly ApplicationContext db;

    public TopicsService(ILogger<CardsService> logger,
        IWebHostEnvironment env,
        ApplicationContext db)
    {
        this.logger = logger;
        this.env = env;
        this.db = db;
    }

    public (TopicEntity? course, string? error) CreateOrEdit(CreateOrPatchTopic item, long? courseId)
    {
        var topic = courseId == null
            ? new TopicEntity()
            : db.Topics.Find(courseId);

        if (topic == null)
            return (null, "Course not found");

        var entry = db.Entry(topic);
        entry.CurrentValues.SetValues(item);

        try
        {
            db.SaveChanges();
            return (topic, null);
        }
        catch
        {
            return (null, "Unknown error");
        }
    }

    public Task<List<TopicEntity>> GetAll(long parentCourseId, int page, int count) =>
        SearchByCondition(x => x.ParentCourseId == parentCourseId, page, count);

    public async Task<TopicEntity?> Get(long name) => await db.Topics.FindAsync(name);

    public Task<List<TopicEntity>> SearchByName(long parentCourseId, string name, int page, int count) =>
        SearchByCondition(x => x.ParentCourseId == parentCourseId && x.Name == name, page, count);

    public async Task<(TopicEntity? course, string? error)> Delete(long id)
    {
        var topicEntity = await db.Topics.FindAsync(id);

        if (topicEntity == null)
            return (null, "Course not found");

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

    private Task<List<TopicEntity>> SearchByCondition(Expression<Func<TopicEntity, bool>> condition, int page, int count)
    {
        var toSkip = (page - 1) * count;

        return db.Topics
            .Where(condition)
            .Skip(toSkip)
            .Take(count)
            .ToListAsync();
    }
}