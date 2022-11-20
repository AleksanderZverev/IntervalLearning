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
    public ActionResult<Course> Create([FromBody] CreateCourseRequest request)
    {
        var (course, error) = coursesService.CreateOrEdit(
            new CreateOrPatchCourse
            {
                Name = request.Name,
                UsersGroupIds = request.UsersGroupIds
            },
            null);

        return course != null
            ? ToCourse(course)
            : BadRequest(error);
    }
    
    [HttpPatch("{courseId:long}")]
    public ActionResult<Course> Patch(long courseId, [FromBody] PatchCourseRequest request)
    {
        var (course, error) = coursesService.CreateOrEdit(
            new CreateOrPatchCourse
            {
                Name = request.Name
            },
            courseId);

        return course != null
            ? ToCourse(course)
            : BadRequest(error);
    }

    [HttpGet]
    public async Task<ActionResult<List<Course>>> GetAll([FromQuery] int page, [FromQuery] int count)
    {
        var courses = await coursesService.Search(null, page, count);
        return courses.Select(ToCourse).ToList();
    }

    [HttpGet("search/{name}")]
    public async Task<ActionResult<List<Course>>> Search(string name, [FromQuery] int page, [FromQuery] int count)
    {
        var courses = await coursesService.Search(name, page, count);
        return courses.Select(ToCourse).ToList();
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<Course>> Delete(long id)
    {
        var (course, error) = await coursesService.Delete(id);
        return course != null ? ToCourse(course) : BadRequest(error);
    }

    public Course ToCourse(CourseEntity courseEntity) =>
        new(courseEntity.Id, courseEntity.Name, courseEntity.Link, courseEntity.UsersGroupIds);
}

public class CreateCourseRequest
{
    public string Name { get; set; }
    public List<long> UsersGroupIds { get; set; }
}

public class PatchCourseRequest
{
    public string Name { get; set; }
}