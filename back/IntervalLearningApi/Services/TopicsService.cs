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

    public Task<List<TopicEntity>> GetAll(long parentCourseId, int page, int count)
    {
        var toSkip = (page - 1) * count;

        return db.Topics
            .Where(x => x.ParentCourseId == parentCourseId)
            .Skip(toSkip)
            .Take(count)
            .ToListAsync();
    }

    public async Task<TopicEntity?> Get(long name) => await db.Topics.FindAsync(name);

    public async Task<(string?, string?)> GetLink(long id)
    {
        var course = await db.Courses.FindAsync(id);

        if (course == null)
            return (null, "Course not found");

        return (course.Link, null) ;
    }

    public async Task<(CourseEntity? course, string? error)> Delete(long id)
    {
        var course = await db.Courses.FindAsync(id);

        if (course == null)
            return (null, "Course not found");

        db.Courses.Remove(course);

        try
        {
            await db.SaveChangesAsync();
            return (course, null);
        }
        catch
        {
            return (null, "Unknown error");
        }
    }
}