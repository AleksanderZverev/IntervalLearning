using IntervalLearningApi.Services.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace IntervalLearningApi.Controllers;

[Authorize]
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly UserService userService;

    public UsersController(UserService userService)
    {
        this.userService = userService;
    }

    [HttpGet("me")]
    public IActionResult Me() => Ok();

    [HttpGet]
    public IActionResult GetAll()
    {
        var users = userService.GetAll();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var user = userService.GetById(id);
        return Ok(user);
    }
}