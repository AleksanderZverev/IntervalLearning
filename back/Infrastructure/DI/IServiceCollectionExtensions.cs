using DomainServices.BoundedContext.Accounts.PasswordService;
using Infrastructure.BoundedContexts.Accounts.Passwords;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DI;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordService, PasswordsService>();
    }
}