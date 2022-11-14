using DB;
using DB.Models;

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

    public (TopicEntity? course, string? error) CreateOrEdit(CreateOrPatchCourse item, long? courseId)
    {
        var course = courseId == null
            ? new CourseEntity()
            : db.Courses.Find(courseId);

        if (course == null)
            return (null, "Course not found");

        var entry = db.Entry(course);
        entry.CurrentValues.SetValues(item);

        try
        {
            db.SaveChanges();
            return (course, null);
        }
        catch
        {
            return (null, "Unknown error");
        }
    }

    public Task<List<CourseEntity>> GetAll(int page, int count)
    {
        var toSkip = (page - 1) * count;

        return db.Courses
            .OrderByDescending(c => c.Name)
            .Skip(toSkip)
            .Take(count)
            .ToListAsync();
    }

    public async Task<CourseEntity?> Get(string name) => await db.Courses.FindAsync(name);

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