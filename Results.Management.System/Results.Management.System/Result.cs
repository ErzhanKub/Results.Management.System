
using Newtonsoft.Json;

namespace Results.Management.System
{
    public abstract class Result
    {
        protected Result() { }
        public bool IsSuccess { get; protected set; }
        public bool IsFailure => !IsSuccess;
        public List<Error> Errors { get; protected set; } = new();

        public static Result<TValue> Success<TValue>() => new();
        public static Result<TValue> Success<TValue>(TValue value) => new(value);
        public static Result<TValue> Warn<TValue>(string message) => new(new Error(message));
        public static Result<TValue> Fail<TValue>(string message, string code) => new(new Error(message, code));
        public static Result<TValue> Fail<TValue>(Error error) => new(error);
        public static Result<TValue> Fail<TValue>(List<Error> errors) => new(errors);
        public static Result<TValue> FromException<TValue>(Exception ex) => new(new Error(ex.Message));

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
            HasValue = true;
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

        public List<Result<TValue>> FilterSuccessfulResults(List<Result<TValue>> results)
        {
            List<Result<TValue>> successfulResults = new();

            foreach (var result in results)
            {
                if (result.IsSuccess)
                {
                    successfulResults.Add(result);
                }
            }
            return successfulResults;
        }

        public List<Result<TValue>> FilterFailedResults(List<Result<TValue>> results)
        {
            List<Result<TValue>> failedResults = new();

            foreach (var result in results)
            {
                if (result.IsFailure)
                {
                    failedResults.Add(result);
                }
            }
            return failedResults;
        }

        public string ConvertResultToJson(Result<TValue> result)
        {
            var resultData = new
            {
                result.IsSuccess,
                result.Value,
                Errors = result.Errors.Select(error => new { error.Message, error.Code })
            };

            var jsonString = JsonConvert.SerializeObject(resultData);

            return jsonString;
        }

        public void ClearErrors()
        {
            Errors.Clear();
            IsSuccess = true;
        }

        public void AddErrors(List<Error> errors)
        {
            IsSuccess = false;
            Errors.AddRange(errors);
        }

        public List<Error> GetErrorsWithCode(string code)
        {
            return Errors.Where(error => error.Code == code).ToList();
        }

        public void RemoveErrorsWithCode(string code)
        {
            Errors.RemoveAll(error => error.Code == code);
            if (!Errors.Any())
                IsSuccess = true;
        }

        public void ResetValue()
        {
            Value = default;
            HasValue = false;
        }

        public Result<TNewValue> Map<TNewValue>(Func<TValue, TNewValue> transform)
        {
            if (IsSuccess)
            {
                return Success(transform(Value));
            }
            else
            {
                return Fail<TNewValue>(Errors);
            }
        }

        public void OnSuccess(Action<TValue> action)
        {
            if (IsSuccess)
            {
                action(Value);
            }
        }

        public void OnFailure(Action<List<Error>> action)
        {
            if (IsFailure)
            {
                action(Errors);
            }
        }

        public Result<(TValue, TOther)> CombineWith<TOther>(Result<TOther> other)
        {
            if (IsSuccess && other.IsSuccess)
            {
                return Success((Value, other.Value));
            }
            else
            {
                var combinedErrors = new List<Error>(Errors);
                combinedErrors.AddRange(other.Errors);
                return Fail<(TValue, TOther)>(combinedErrors);
            }
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