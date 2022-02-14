using DB.Models;
using IntervalLearningApi.Models;

namespace IntervalLearningApi.Controllers.Users;

public interface IAuthenticationService
{
    AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress);
    AuthenticateResponse RefreshToken(string token, string ipAddress);
    void RevokeToken(string token, string ipAddress);
    IEnumerable<UserEntity> GetAll();
    UserEntity GetById(int id);
}