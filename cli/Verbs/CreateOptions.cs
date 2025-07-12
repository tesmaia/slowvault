using System;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using SlowVault.Lib;

namespace SlowVault.Cli.Verbs;

[Verb("create", HelpText = "Create a new vault at the given location.")]
public class CreateOptions : IExecutableOptions
{
    [Value(0, HelpText = "Name of the vault")]
    public string? Name { get; set; }

    [Option(
        "path",
        Required = false,
        HelpText = "Where to store the vault. Defaults to current directory or what is defined in SLOWVAULT_PATH."
    )]
    public string? Path { get; set; }

    [Option(
        'p',
        "password",
        Required = false,
        HelpText = "Password for the vault. When ommitted you will be prompted."
    )]
    public string? Password { get; set; }

    public async Task<string?> Execute(VaultIO vaultIO)
    {
        if (Name == null)
        {
            throw new EndUserException("ERROR Please enter a name for the vault");
        }

        var path = Path ?? vaultIO.DefaultFilePath;
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        var fileName = System.IO.Path.Combine(path, Name + ".svlt");
        if (File.Exists(fileName))
        {
            throw new EndUserException("A vault with this name already exists at this location");
        }

        if (string.IsNullOrEmpty(Password))
        {
            Password = await vaultIO.PasswordProvider();
        }

        var vault = new Vault();
        VaultIO.Save(vault, fileName, Password);
        return "Vault created";
    }
}
