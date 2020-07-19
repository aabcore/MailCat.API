namespace MailCat.API.Results
{
    public abstract class EarlyReturnResult<T>
    {
    }

    public class ReturnEarly_EarlyReturnResult<T> : EarlyReturnResult<T>
    {
        public ReturnEarly_EarlyReturnResult(T earlyReturnValue)
        {
            EarlyReturnValue = earlyReturnValue;
        }
        public T EarlyReturnValue { get; set; }
    }

    public class Continue_EarlyReturnResult<T> : EarlyReturnResult<T>
    {
    }
}