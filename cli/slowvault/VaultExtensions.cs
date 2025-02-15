using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.IO;
using System.Security.Cryptography;


namespace SlowVault;

public static class VaultExtensions
 {
    public static Vault Load(string fileName, string password)
    {
        if (File.Exists(fileName))
        {
            var bytes = File.ReadAllBytes(fileName);
            if(!string.IsNullOrEmpty(password))
            {
                bytes = DecryptData(bytes, password);
                if(bytes == null)
                {
                    throw new EndUserException("ERROR Invalid password or corrupt file");
                }
            }
            var jsonString = System.Text.Encoding.UTF8.GetString(bytes);
            var vault = System.Text.Json.JsonSerializer.Deserialize<Vault>(jsonString);

            if(vault == null && vault.Magic != Vault.MAGIC)
                throw new EndUserException("ERROR Invalid password or corrupt file (magic valiation failed)");

            return vault;

        }

        throw new EndUserException($"ERROR {fileName} does not exist");
    }

    public static void Save(Vault vault, string filename, string password)
    {
        string jsonString;
        try
        {
            jsonString = System.Text.Json.JsonSerializer.Serialize<Vault>(vault);
        }
        catch
        {
            throw new EndUserException("ERROR Could not serialize the vault");
        }
        var bytes = System.Text.Encoding.UTF8.GetBytes(jsonString);
        var encrypted = EncryptData(bytes, password);

        File.WriteAllBytes(filename, encrypted);

    }

    public static byte[] Encrypt(string plaintext, string password)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(plaintext);
        return EncryptData(bytes, password);
    }

    const int seed = 23213;

    public static byte[] EncryptData(byte[] data, string password)
    {
        if(string.IsNullOrEmpty(password)) 
            return data;

        var salt = new byte[20];
        Random rng = new Random(seed);
        rng.NextBytes(salt);

        var passwordBytes = new Rfc2898DeriveBytes(password, salt);
        Aes encryptor = Aes.Create();
        encryptor.Key = passwordBytes.GetBytes(32);
        encryptor.IV = passwordBytes.GetBytes(16);
        using var memstream = new MemoryStream();
        using var cryptoStream = new CryptoStream(memstream, encryptor.CreateEncryptor(), 
            CryptoStreamMode.Write);

        cryptoStream.Write(data, 0, data.Length);
        cryptoStream.FlushFinalBlock();

        var result = memstream.ToArray();
        Console.WriteLine($"Encrypted. Cypher length: {result.Length}");
        return result;
    }

    public static string ToBase64(this byte[] data)
    {
        return Convert.ToBase64String(data);
    }

    public static byte[] DecodeBase64(this string text)
    {
        return Convert.FromBase64String(text);
    }

    public static string Decrypt(byte[] data, string? password)
    {
        var bytes = DecryptData(data, password);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }

    public static byte[] DecryptData(byte[] data, string? password)
    {
        if(string.IsNullOrEmpty(password)) 
            return data;

        Console.WriteLine($"Decrypting. Cypher length: {data.Length}");

        try
        {
            var salt = new byte[20];
            Random rng = new Random(seed);
            rng.NextBytes(salt);

            var passwordBytes = new Rfc2898DeriveBytes(password, salt);
            Aes encryptor = Aes.Create();
            encryptor.Key = passwordBytes.GetBytes(32);
            encryptor.IV = passwordBytes.GetBytes(16);
            using var memstream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memstream, encryptor.CreateDecryptor(),
                CryptoStreamMode.Write);


            cryptoStream.Write(data, 0, data.Length);
            cryptoStream.FlushFinalBlock();

            return memstream.ToArray();
        }
        catch(System.Security.Cryptography.CryptographicException)
        {
            throw;
            return null;
        }
    }
}