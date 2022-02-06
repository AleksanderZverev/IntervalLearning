using IntervalLearningApi.Helpers;
using IntervalLearningApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IntervalLearningApi.Controllers
{
    [Route("api/authorize")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly JwtSettings jwtSettings;

        public AccountController(JwtSettings jwtSettings)
        {
            this.jwtSettings = jwtSettings;
        }

        private IEnumerable<Users> logins = new List<Users>()
        {
            new()
            {
                Id = Guid.NewGuid(),
                EmailId = "adminakp@gmail.com",
                UserName = "Admin",
                Password = "Admin",
            },
            new()
            {
                Id = Guid.NewGuid(),
                EmailId = "adminakp@gmail.com",
                UserName = "User1",
                Password = "Admin",
            }
        };

        //TODO: after test 2 authorizations
        //[ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult GetToken(UserLogins userLogins)
        {
            try
            {
                var token = new UserTokens();
                var valid = logins.Any(x => x.UserName.Equals(userLogins.UserName, StringComparison.OrdinalIgnoreCase));

                if (valid)
                {
                    var user = logins.FirstOrDefault(x =>
                        x.UserName.Equals(userLogins.UserName, StringComparison.OrdinalIgnoreCase));
                    token = JwtHelpers.GenTokenKey(new UserTokens()
                    {
                        EmailId = user.EmailId,
                        GuidId = Guid.NewGuid(),
                        UserName = user.UserName,
                        Id = user.Id,
                    }, jwtSettings);
                }
                else
                {
                    return BadRequest("wrong password");
                }

                return Ok(token);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Get List of UserAccounts
        /// </summary>
        /// <returns>List Of UserAccounts</returns>
        [HttpGet]
        [Authorize]
        public IActionResult GetList()
        {
            return Ok(logins);
        }
    }
}
