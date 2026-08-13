using FluentValidation;
using MediatR;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
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

        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var genericArg = typeof(TResponse).GetGenericArguments()[0];

            // 🔥 FIX: .GetMethods() se saare "Failure" methods lo, phir SIRF generic wala chuno
            var method = typeof(Result)
                .GetMethods()
                .Single(m => m.Name == nameof(Result.Failure) && m.IsGenericMethodDefinition)
                .MakeGenericMethod(genericArg);

            var result = method.Invoke(null, new object[] { error });
            return (TResponse)result!;
        }

        return (TResponse)(object)Result.Failure(error);
    }
}