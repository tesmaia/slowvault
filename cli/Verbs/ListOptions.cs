using System;
using System.Threading.Tasks;
using CommandLine;
using SlowVault.Lib;

namespace SlowVault.Cli.Verbs;

[Verb("list", HelpText = "List all the keys stored in the vault.")]
public class ListOptions : IExecutableOptions
{
    [Option(
        'f',
        "file",
        Required = false,
        HelpText = "File path of the vault, may also be defined as environment variable SLOWVAULT_PATH"
    )]
    public string? FileName { get; set; }

    [Option(
        'p',
        "password",
        Required = false,
        HelpText = "Password for the vault, optional, will prompt if not given."
    )]
    public string Password { get; set; }

    public async Task<string?> Execute(VaultIO vaultIO)
    {
        (var vault, var _, var _) = await vaultIO.OpenVault(this.FileName, this.Password);

        Console.WriteLine($"Listing {vault.Items.Count} keys in the vault:");
        foreach (var item in vault.Items)
        {
            Console.WriteLine(item.Key);
        }

        return null;
    }
}
