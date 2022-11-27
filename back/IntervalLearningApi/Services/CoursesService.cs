using DB;
using DB.Models;
using IntervalLearningApi.Models.Courses;
using Microsoft.EntityFrameworkCore;

namespace IntervalLearningApi.Services;

public class CoursesService
{
    private readonly ILogger<CardsService> logger;
    private readonly IWebHostEnvironment env;
    private readonly ApplicationContext db;

    public CoursesService(ILogger<CardsService> logger,
        IWebHostEnvironment env,
        ApplicationContext db)
    {
        this.logger = logger;
        this.env = env;
        this.db = db;
    }

    public (CourseEntity? course, string? error) CreateOrEdit(CreateOrPatchCourse item, long? courseId = null)
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

    public Task<List<CourseEntity>> Search(string? name, int page, int count)
    {
        var toSkip = (page - 1) * count;

        return db.Courses
            .Where(x => name == null || x.Name.ToLowerInvariant().StartsWith(name.ToLowerInvariant()))
            .OrderByDescending(c => c.Name)
            .Skip(toSkip)
            .Take(count)
            .ToListAsync();
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