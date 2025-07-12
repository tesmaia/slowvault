using System.Collections.Generic;
using System.IO;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace SlowVault.Lib;

public static class VaultExtensions
{
    public static byte[] Encrypt(string plaintext, string password)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(plaintext);
        return EncryptData(bytes, password);
    }

    const int seed = 23213;

    public static byte[] EncryptData(byte[] data, string password)
    {
        if (string.IsNullOrEmpty(password))
            return data;

        var salt = new byte[20];
        Random rng = new Random(seed);
        rng.NextBytes(salt);

        var passwordBytes = new Rfc2898DeriveBytes(
            password: password,
            salt: salt,
            iterations: 1000,
            hashAlgorithm: HashAlgorithmName.SHA1
        );
        Aes encryptor = Aes.Create();
        encryptor.Key = passwordBytes.GetBytes(32);
        encryptor.IV = passwordBytes.GetBytes(16);
        using var memstream = new MemoryStream();
        using var cryptoStream = new CryptoStream(
            memstream,
            encryptor.CreateEncryptor(),
            CryptoStreamMode.Write
        );

        cryptoStream.Write(data, 0, data.Length);
        cryptoStream.FlushFinalBlock();

        var result = memstream.ToArray();
        return result;
    }

    public static string Decrypt(byte[] data, string? password)
    {
        var bytes = DecryptData(data, password);
        if (bytes == null)
            throw new DecryptionException();
        return System.Text.Encoding.UTF8.GetString(bytes);
    }

    public static byte[]? DecryptData(byte[] data, string? password)
    {
        if (string.IsNullOrEmpty(password))
            return data;
        try
        {
            var salt = new byte[20];
            Random rng = new Random(seed);
            rng.NextBytes(salt);

            var passwordBytes = new Rfc2898DeriveBytes(
                password: password,
                salt: salt,
                iterations: 1000,
                hashAlgorithm: HashAlgorithmName.SHA1
            );
            Aes encryptor = Aes.Create();
            encryptor.Key = passwordBytes.GetBytes(32);
            encryptor.IV = passwordBytes.GetBytes(16);
            using var memstream = new MemoryStream();
            using var cryptoStream = new CryptoStream(
                memstream,
                encryptor.CreateDecryptor(),
                CryptoStreamMode.Write
            );

            cryptoStream.Write(data, 0, data.Length);
            cryptoStream.FlushFinalBlock();

            return memstream.ToArray();
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            return null;
        }
    }
}
