namespace SlowVault;

public class DecryptionException : Exception
{
    public DecryptionException(string message = "Decryption failed")
        : base(message) { }
}
