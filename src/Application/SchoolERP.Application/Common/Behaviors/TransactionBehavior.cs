using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;

namespace SchoolERP.Application.Common.Behaviors;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IApplicationDbContext _dbContext;

    public TransactionBehavior(IApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Sirf Commands ke liye
        if (request is not ICommandBase)
            return await next();

        if (_dbContext.HasActiveTransaction)
            return await next();

        await _dbContext.BeginTransactionAsync(cancellationToken);
        try
        {
            var response = await next();
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _dbContext.CommitTransactionAsync(cancellationToken);
            return response;
        }
        catch
        {
            await _dbContext.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}