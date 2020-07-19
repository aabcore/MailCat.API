using System;

namespace MailCat.API.Results
{
    public abstract class TypedResult<T>
    {
    }

    public class FailedTypedResult<T> : TypedResult<T>
    {
        public FailedTypedResult(Exception e)
        {
            Error = e;
        }

        public Exception Error { get; set; }
    }

    public class NotFoundTypedResult<T> : TypedResult<T>
    {
    }

    public class SuccessfulTypedResult<T> : TypedResult<T>
    {
        public SuccessfulTypedResult(T value)
        {
            Value = value;
        }

        public T Value { get; set; }
    }

    public class BadRequestTypedResult<T> : TypedResult<T>
    {
        public BadRequestTypedResult(string message)
        {
            Problem = new BadRequestOutDto(message);
        }

        public BadRequestTypedResult(BadRequestOutDto badRequestOutDto)
        {
            Problem = badRequestOutDto;
        }

        public BadRequestOutDto Problem { get; set; }
    }
    public class BadRequestOutDto
    {
        public BadRequestOutDto(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
    }
}