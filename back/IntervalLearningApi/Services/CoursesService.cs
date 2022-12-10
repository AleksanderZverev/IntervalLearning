using DB;
using DB.Models;
using IntervalLearningApi.Models.Courses;
using Microsoft.EntityFrameworkCore;

namespace IntervalLearningApi.Services;

public class CoursesService
{
    private readonly ApplicationContext db;

    public CoursesService(ApplicationContext db)
    {
        this.db = db;
    }

    public async Task<(CourseEntity? course, string? error)> Create(long userId, CreateCourseParameters parameters)
    {
        var course = new CourseEntity
        {
            Name = parameters.Name,
            Description = parameters.Description,
            IsPrivate = parameters.IsPrivate
        };
        course.AdminIds.Add(userId);
        await db.Courses.AddAsync(course);

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

    public async Task<(CourseEntity? course, string? error)> Patch(long userId, long courseId, PatchCourseParameters parameters)
    {
        var course = await db.Courses.FindAsync(courseId);

        if (course == null)
            return (null, "Course not found");
        if (!course.AdminIds.Contains(userId))
            return (null, $"User can't perform operation because {userId} is not admin of current course");

        var entry = db.Entry(course);
        entry.CurrentValues.SetValues(parameters);

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

    public async Task<CourseEntity?> Get(long courseId) => await db.Courses.FindAsync(courseId);

    public async Task<(List<CourseEntity>, int)> Search(string? name, int page, int count)
    {
        var skip = (page - 1) * count;

        var query = db.Courses
            .Where(x => !x.IsPrivate);

        if (name != null)
            query = query.Where(x => x.Name.ToLower().StartsWith(name));

        var foundItems = query
            .OrderByDescending(c => c.Name)
            .Skip(skip)
            .Take(count)
            .ToList();

        return (foundItems, await db.Courses.CountAsync());
    }

    public async Task<(CourseEntity? course, string? error)> Delete(long userId, long courseId)
    {
        var course = await db.Courses.FindAsync(courseId);

        if (course == null)
            return (null, "Course not found");
        if (!course.AdminIds.Contains(userId))
            return (null, $"User can't perform operation because {userId} is not admin of current course");

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