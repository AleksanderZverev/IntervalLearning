using DB;
using DB.Models;
using IntervalLearningApi.Models.Courses;

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

    public (CourseEntity? course, string? error) CreateOrEdit(CreateOrPatchCourse item, long? courseId)
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
}