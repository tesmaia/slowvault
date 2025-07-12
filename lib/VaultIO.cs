using System;
using System.Threading.Tasks;
using static SlowVault.Lib.VaultExtensions;

namespace SlowVault.Lib;

public class VaultIO
{
    public required string DefaultFilePath { get; init; }
    public required Func<string[], Task<string>> FileNameProvider { get; init; }
    public required Func<Task<string>> PasswordProvider { get; init; }
    public required Func<string, Task<string>> UserSecretProvider { get; init; }

    public async Task<(Vault, string, string)> OpenVault(
        string fileName,
        string password,
        string vaultName = ""
    )
    {
        if (!string.IsNullOrEmpty(vaultName))
        {
            var vaultPaths = Directory
                .GetFiles(DefaultFilePath, "*.svlt", System.IO.SearchOption.TopDirectoryOnly)
                .Select(x => Path.GetFileNameWithoutExtension(x))
                .ToArray();
            if (vaultPaths.Contains(vaultName))
            {
                fileName = Path.Combine(DefaultFilePath, vaultName + ".svlt");
            }
        }

        if (string.IsNullOrEmpty(fileName))
        {
            string[] vaultPaths = Directory.GetFiles(
                DefaultFilePath,
                "*.svlt",
                System.IO.SearchOption.TopDirectoryOnly
            );

            fileName = await FileNameProvider(vaultPaths);
        }

        if (string.IsNullOrEmpty(password))
            password = await PasswordProvider();

        var vault = Load(fileName, password);
        if (vault == null)
        {
            throw new EndUserException("ERROR Failure to open vault");
        }
        return (vault, fileName, password);
    }

    public static Vault Load(string fileName, string password)
    {
        if (File.Exists(fileName))
        {
            var bytes = File.ReadAllBytes(fileName);
            if (!string.IsNullOrEmpty(password))
            {
                bytes = DecryptData(bytes, password);
                if (bytes == null)
                {
                    throw new EndUserException("ERROR Invalid password or corrupt file");
                }
            }

            var jsonString = System.Text.Encoding.UTF8.GetString(bytes);
            var vault = Try(() => System.Text.Json.JsonSerializer.Deserialize<Vault>(jsonString));

            if (vault == null || vault.Magic != Vault.MAGIC)
                throw new EndUserException(
                    "ERROR Invalid password or corrupt file (magic valiation failed)"
                );

            return vault;
        }

        throw new EndUserException($"ERROR {fileName} does not exist");
    }

    public static T? Try<T>(Func<T> function)
    {
        try
        {
            return function();
        }
        catch
        {
            return default;
        }
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
}
