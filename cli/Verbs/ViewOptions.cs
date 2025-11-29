using System;
using CommandLine;
using SlowVault.Lib;

namespace SlowVault.Cli.Verbs;

[Verb(
    "view",
    HelpText = "View the options set for a key, can also be used to check if a key exists"
)]
public class ViewOptions : IExecutableOptions
{
    [Option(
        'f',
        "file",
        Required = false,
        HelpText = "File path of the vault, may also be defined as environment variable SLOWVAULT_PATH"
    )]
    public string FileName { get; set; } = string.Empty;

    [Option(
        'p',
        "password",
        Required = false,
        HelpText = "Password for the vault, optional, will prompt if not given."
    )]
    public string Password { get; set; } = string.Empty;

    [Value(0, HelpText = "The key to fetch from the vault (options only)")]
    public string? Key { get; set; }

    public async Task<string?> Execute(VaultIO vaultIO)
    {
        (var vault, var _, var _) = await vaultIO.OpenVault(this.FileName, this.Password);

        var entry = vault.Items.FirstOrDefault(x => x.Key == this.Key);
        if (entry == null)
        {
            return $"Entry with key {this.Key} was not found";
        }

        Console.WriteLine($"Key: {entry.Key}");
        Console.WriteLine($"Delay: {entry.Delay}s");
        Console.WriteLine($"Available for: {entry.Available}s");
        Console.WriteLine($"Clear after: {entry.ClearAfter}s");
        Console.WriteLine($"Lock after copy: {entry.LockAfterCopy}");
        Console.WriteLine($"Prompt password again: {entry.PromptAgain}");

        string PrintTime(int minutes)
        {
            return $"{(minutes < 0 ? "-" : "")}{TimeSpan.FromMinutes(minutes):h\\:mm}";
        }

        Console.WriteLine(
            $"Timelocked to: [{string.Join(";", entry.TimeAvailable?.Select(x => $"{PrintTime(x.Item1)}-{PrintTime(x.Item2)}") ?? [])}]"
        );

        return null;
    }
}
