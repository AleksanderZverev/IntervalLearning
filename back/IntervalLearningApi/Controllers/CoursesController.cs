using AutoMapper;
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
    private readonly IMapper mapper;

    public CoursesController(CoursesService coursesService, IMapper mapper)
    {
        this.coursesService = coursesService;
        this.mapper = mapper;
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
            ? mapper.Map<Course>(course)
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
            ? mapper.Map<Course>(course)
            : BadRequest(error);
    }

    [HttpGet]
    public async Task<ActionResult<List<Course>>> GetAll([FromQuery] int page, [FromQuery] int count)
    {
        var courses = await coursesService.Search(null, page, count);
        return courses.Select(mapper.Map<Course>).ToList();
    }

    [HttpGet("search/{name}")]
    public async Task<ActionResult<List<Course>>> Search(string name, [FromQuery] int page, [FromQuery] int count)
    {
        var courses = await coursesService.Search(name, page, count);
        return courses.Select(mapper.Map<Course>).ToList();
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<Course>> Delete(long id)
    {
        var (course, error) = await coursesService.Delete(id);
        return course != null ? mapper.Map<Course>(course) : BadRequest(error);
    }
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