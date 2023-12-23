using Application.Common.Accounts.PasswordService;
using Infrastructure.Accounts.Passwords;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DI;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordService, PasswordsService>();
    }
}