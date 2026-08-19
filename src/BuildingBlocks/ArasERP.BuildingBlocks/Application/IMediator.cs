namespace ArasERP.BuildingBlocks.Application;

public interface IMediator
{
    Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand;

    Task<TResult> SendAsync<TQuery, TResult>(
        TQuery query,
        CancellationToken cancellationToken = default
    )
        where TQuery : IQuery<TResult>;
}
