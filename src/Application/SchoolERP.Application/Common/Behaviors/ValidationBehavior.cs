using FluentValidation;
using MediatR;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result  // 🔥 Ensures TResponse is always Result or Result<T>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

        if (failures.Count == 0)
            return await next();

        var errorMessage = string.Join(" | ", failures.Select(f => f.ErrorMessage));
        var error = Error.Validation(errorMessage);

        // 🔥 SAFE CAST using Reflection (Your Result class has Failure<T> method)
        // Case 1: Agar TResponse = Result<T> (Generic)
        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var genericArg = typeof(TResponse).GetGenericArguments()[0]; // T
            // Call: Result.Failure<T>(Error error)
            var method = typeof(Result)
                .GetMethod(nameof(Result.Failure))!
                .MakeGenericMethod(genericArg);

            var result = method.Invoke(null, new object[] { error });
            return (TResponse)result!;
        }

        // Case 2: TResponse = Result (Non-Generic)
        return (TResponse)(object)Result.Failure(error);
    }
}