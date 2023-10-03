
namespace Results.Management.System
{
    public abstract class Result
    {
        protected Result() { }
        public bool IsSuccess { get; protected set; }
        public bool IsFailure => !IsSuccess;
        public List<Error> Errors { get; protected set; } = new();

        public static Result<TValue> Ok<TValue>() => new();
        public static Result<TValue> Ok<TValue>(TValue value) => new(value);
        public static Result<TValue> Bad<TValue>(string message) => new(new Error(message));
        public static Result<TValue> Bad<TValue>(string message, string code) => new(new Error(message, code));
        public static Result<TValue> Bad<TValue>(Error error) => new(error);
        public static Result<TValue> Bad<TValue>(List<Error> errors) => new(errors);
    }

    public class Result<TValue> : Result
    {
        public TValue? Value { get; private set; }
        public bool HasValue { get; private set; }
        internal Result()
        {
            IsSuccess = true;
            Value = default;
            HasValue = false;
        }

        internal Result(TValue value)
        {
            IsSuccess = true;
            Value = value;
            HasValue = false;
        }

        internal Result(Error error)
        {
            IsSuccess = false;
            Value = default;
            HasValue = false;
            Errors.Add(error);
        }

        internal Result(List<Error> errors)
        {
            IsSuccess = false;
            Value = default;
            HasValue = false;
            Errors = errors;
        }

        public static implicit operator Result<TValue>(TValue value) => new(value);
        public static implicit operator Result<TValue>(Error error) => new(error);
        public static implicit operator Result<TValue>(List<Error> errors) => new(errors);

        public void Deconstruct(out bool isSuccsess, out TValue? value, out List<Error> errors)
        {
            isSuccsess = IsSuccess;
            value = Value;
            errors = Errors;
        }
    }

    public class Error
    {
        public string Message { get; private set; }
        public string Code { get; private set; }
        public Error(string message, string code)
        {
            Message = message;
            Code = code;
        }

        public Error(string message)
        {
            Message = message;
            Code = string.Empty;
        }
    }
}