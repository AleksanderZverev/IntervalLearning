using AutoMapper;
using IntervalLearningApi.Extensions;
using IntervalLearningApi.Models.Courses;
using IntervalLearningApi.Models.Pagination;
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
    public async Task<ActionResult<Course>> Create([FromBody] CreateCourseRequest request)
    {
        var (course, error) = await coursesService.Create(
            HttpContext.GetUserId(),
            mapper.Map<CreateCourseParameters>(request));

        return course != null
            ? mapper.Map<Course>(course)
            : BadRequest(error);
    }
    
    [HttpPatch("{courseId:long}")]
    public async Task<ActionResult<Course>> Patch(long courseId, [FromBody] PatchCourseRequest request)
    {
        var (course, error) = await coursesService.Patch(
            HttpContext.GetUserId(),
            courseId,
            mapper.Map<PatchCourseParameters>(request));

        return course != null
            ? mapper.Map<Course>(course)
            : BadRequest(error);
    }

    [HttpGet("{courseId:long}")]
    public async Task<ActionResult<Course>> Get(long courseId)
    {
        var course = await coursesService.Get(courseId);

        return course != null
            ? mapper.Map<Course>(course)
            : NotFound();
    }

    [HttpGet]
    public async Task<ActionResult<SearchResult<Course>>> Search([FromQuery] string? name, [FromQuery] int page, [FromQuery] int count)
    {
        var (courses, totalCount) = await coursesService.Search(name?.ToLower(), page, count);

        return new SearchResult<Course>
        {
            FoundItems = courses.Select(mapper.Map<Course>).ToList(),
            TotalCount = totalCount
        };
    }

    [HttpDelete("{courseId:long}")]
    public async Task<ActionResult<Course>> Delete(long courseId)
    {
        var (course, error) = await coursesService.Delete(
            HttpContext.GetUserId(),
            courseId);

        return course != null 
            ? mapper.Map<Course>(course) 
            : BadRequest(error);
    }
}