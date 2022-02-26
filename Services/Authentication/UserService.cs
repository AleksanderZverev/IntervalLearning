using DB;
using DB.Models;
using IntervalLearningApi.Models;
using IntervalLearningApi.Services.Jwt;
using Microsoft.Extensions.Options;

namespace IntervalLearningApi.Services.Authentication;

public class UserService
{
    private readonly ApplicationContext db;
    private readonly IJwtService jwtService;
    private readonly JwtSettings _jwtSettings;

    public UserService(
        ApplicationContext db,
        IJwtService jwtService,
        IOptions<JwtSettings> appSettings)
    {
        this.db = db;
        this.jwtService = jwtService;
        _jwtSettings = appSettings.Value;
    }

    public IEnumerable<UserEntity> GetAll()
    {
        return db.Users;
    }

    public UserEntity GetById(int id)
    {
        var user = db.Users.Find(id);
        if (user == null) throw new KeyNotFoundException("UserEntity not found");
        return user;
    }
}