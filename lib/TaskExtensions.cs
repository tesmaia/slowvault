using System;

namespace SlowVault.Lib;

public static class TaskExtensions
{
    public static T GetSync<T>(
        this Task<T> task,
        int spinWaitMs = 10,
        int maxWaitTimeMs = 1_000_000_000
    )
    {
        var waitTimeMs = 0;
        while (waitTimeMs < maxWaitTimeMs)
        {
            if (task.IsCompleted)
            {
                if (task.IsCompletedSuccessfully)
                    return task.Result;

                if (task.IsFaulted)
                {
                    var endUserException = task.Exception.InnerExceptions.FirstOrDefault(x =>
                        x is EndUserException
                    );
                    if (endUserException != null)
                    {
                        throw endUserException;
                    }
                    throw task.Exception;
                }
                break;
            }
            Thread.Sleep(spinWaitMs);
            waitTimeMs += spinWaitMs;
        }

        throw new Exception("SpinWait time exceeded. Operation canceled.");
    }
}
