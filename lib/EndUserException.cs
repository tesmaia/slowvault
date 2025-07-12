namespace SlowVault;

public class EndUserException : Exception
{
    public EndUserException(string message)
        : base(message) { }
}
