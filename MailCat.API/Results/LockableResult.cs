using System;

namespace MailCat.API.Results
{
    public abstract class LockableResult<T>
    {
    }
    public class FailedLockableResult<T> : LockableResult<T>
    {
        public FailedLockableResult(Exception e)
        {
            Error = e;
        }

        public Exception Error { get; set; }
    }
    public class LockedLockableResult<T> : LockableResult<T>
    {
    }
    public class NotFoundLockableResult<T> : LockableResult<T>
    {
    }
    public class NotLockedLockableResult<T> : LockableResult<T>
    {
    }
    public class SuccessfulLockableResult<T> : LockableResult<T>
    {
        public SuccessfulLockableResult(T value)
        {
            Value = value;
        }

        public T Value { get; set; }
    }
    public class BadRequestLockableResult<T> : LockableResult<T>
    {
        public BadRequestLockableResult(string message)
        {
            Problem = new BadRequestOutDto(message);
        }

        public BadRequestLockableResult(BadRequestOutDto problem)
        {
            Problem = problem;
        }

        public BadRequestOutDto Problem { get; set; }
    }
}