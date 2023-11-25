using Application;

namespace IntervalLearningApi.Infrastructure.CommandManager;

public class CommandManager
{
    private readonly IServiceProvider serviceProvider;

    public CommandManager(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public TCommand GetCommand<TCommand>()
        where TCommand : ICommand
    {
        return serviceProvider.GetRequiredService<TCommand>();
    }
}