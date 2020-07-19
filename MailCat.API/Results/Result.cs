using System;

namespace MailCat.API.Results
{
    public abstract class Result
    {
    }

    public class SuccessResult: Result{}

    public class FailedResult : Result
    {
        public Exception Exception { get; set; }

        public FailedResult(Exception exception)
        {
            Exception = exception;
        }
    }
}
