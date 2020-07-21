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


    public abstract class EarlyReturnResult<EarlyReturnT, ContinueT>
    {
    }

    public class ReturnEarly_EarlyReturnResult<EarlyReturnT, ContinueT> : EarlyReturnResult<EarlyReturnT, ContinueT>
    {
        public ReturnEarly_EarlyReturnResult(EarlyReturnT earlyReturnValue)
        {
            EarlyReturnValue = earlyReturnValue;
        }
        public EarlyReturnT EarlyReturnValue { get; set; }
    }

    public class Continue_EarlyReturnResult<EarlyReturnT, ContinueT> : EarlyReturnResult<EarlyReturnT, ContinueT>
    {
        public Continue_EarlyReturnResult(ContinueT continueValue)
        {
            ContinueValue = continueValue;
        }
        public ContinueT ContinueValue { get; set; }
    }
}