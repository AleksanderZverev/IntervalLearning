using DB.Models;
using IntervalLearningApi.Models.Courses;
using IntervalLearningApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntervalLearningApi.Controllers;

[Route("api/courses")]
[Authorize]
[ApiController]
public class CoursesController : ControllerBase
{
    private readonly CoursesService coursesService;

    public CoursesController(CoursesService coursesService)
    {
        this.coursesService = coursesService;
    }

    [HttpPost]
    public ActionResult<Course> CreateCourse([FromBody] CreateCourseRequest request)
    {
        var (course, error) = coursesService.CreateOrEdit(
            new CreateOrPatchCourse
            {
                Name = request.Name,
                UsersGroupIds = request.UsersGroupId
            },
            request.CourseId);

        return course != null
            ? ToCourse(course)
            : BadRequest(error);
    }
    
    public Course ToCourse(CourseEntity courseEntity)
    {
        return new Course(courseEntity.Id, courseEntity.Name, courseEntity.UsersGroupIds);
    }
}

public class CreateCourseRequest
{
    public long CourseId;
    public string Name;
    public List<long> UsersGroupId;
}