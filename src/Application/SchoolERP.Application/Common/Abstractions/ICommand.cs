using MediatR;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Common.Abstractions;

// 👇 Marker interface (Non-generic) - Used by TransactionBehavior to detect commands
public interface ICommandBase { }

// 👇 Generic Command (Write operations - CUD)
public interface ICommand<TResponse> : IRequest<Result<TResponse>>, ICommandBase { }

// 👇 Generic Query (Read operations - R)
public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }