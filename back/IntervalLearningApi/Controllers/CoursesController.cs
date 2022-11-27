using AutoMapper;
using IntervalLearningApi.Models.Courses;
using IntervalLearningApi.Models.Requests;
using IntervalLearningApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntervalLearningApi.Controllers;

[Authorize]
[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly IMapper mapper;
    private readonly CoursesService coursesService;

    public CoursesController(IMapper mapper, CoursesService coursesService)
    {
        this.mapper = mapper;
        this.coursesService = coursesService;
    }

    [HttpPost]
    public ActionResult<Course> Create([FromBody] CreateCourseRequest request)
    {
        var (course, error) = coursesService.CreateOrEdit(mapper.Map<CreateOrPatchCourse>(request));

        return course != null
            ? mapper.Map<Course>(course)
            : BadRequest(error);
    }
    
    [HttpPatch("{courseId:long}")]
    public ActionResult<Course> Patch(long courseId, [FromBody] PatchCourseRequest request)
    {
        var (course, error) = coursesService.CreateOrEdit(mapper.Map<CreateOrPatchCourse>(request), courseId);

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

        return course != null 
            ? mapper.Map<Course>(course) 
            : BadRequest(error);
    }
}