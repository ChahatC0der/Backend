using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Infrastructure.Persistence;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolERP.Application.Common.Behaviors;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly AppDbContext _dbContext;

    public TransactionBehavior(AppDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // 🔥 Sirf Commands ke liye transaction apply karo
        if (request is not ICommandBase)
        {
            return await next();
        }

        // Agar already transaction chal raha hai toh avoid karo
        if (_dbContext.Database.CurrentTransaction != null)
        {
            return await next();
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var response = await next();
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return response;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}