using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolERP.Domain.Shared.Results
{
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; }
        public object? Value { get; }
        public string? Message { get; } // 👈 Success message ke liye (Failure mein ignore)

        // Private constructor - Success path
        protected Result(bool isSuccess, Error error, object? value = null, string? message = null)
        {
            if (isSuccess && error != Error.None)
                throw new InvalidOperationException("Success result cannot have an error.");
            if (!isSuccess && error == Error.None)
                throw new InvalidOperationException("Failure result must have an error.");

            IsSuccess = isSuccess;
            Error = error;
            Value = value;
            Message = isSuccess ? message : null; // 👈 Failure mein Message null hi rahega
        }

        // ---- Static Factory Methods ----
        public static Result Success(object? value = null, string? message = null)
            => new(true, Error.None, value, message);

        public static Result Failure(Error error)
            => new(false, error);

        // ---- Generic Version ----
        public static Result<T> Success<T>(T value, string? message = null)
            => new(value, message);

        public static Result<T> Failure<T>(Error error)
            => new(error);
    }

    // ---- Generic Result<T> ----
    public class Result<T> : Result
    {
        public new T? Value => (T?)base.Value;

        public Result(T value, string? message = null)
            : base(true, Error.None, value, message) { }

        public Result(Error error)
            : base(false, error) { }

        // Implicit conversions
        public static implicit operator Result<T>(T value) => new(value);
        public static implicit operator Result<T>(Error error) => new(error);
    }

    // ---- Error Record (Unchanged) ----
    public record Error(string Code, string Message)
    {
        public static readonly Error None = new(string.Empty, string.Empty);
        public static Error NotFound(string entity, string id)
            => new($"NotFound", $"{entity} with id '{id}' was not found.");
        public static Error Conflict(string message)
            => new("Conflict", message);
        public static Error Validation(string message)
            => new("Validation", message);
        public static Error Unauthorized(string message = "Unauthorized access.")
            => new("Unauthorized", message);
    }
}
