using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using CommandLine;
using SlowVault.Lib;

namespace SlowVault.Cli.Verbs;

[Verb("add", HelpText = "Add a new key to the vault")]
public class AddOptions : IExecutableOptions
{
    [Option('f', "file-name", Required = false, HelpText = "File name of the vault.")]
    public string? FileName { get; set; }

    [Option(
        'p',
        "password",
        Required = false,
        HelpText = "Password for the vault, optional, will prompt if not given."
    )]
    public string? Password { get; set; }

    [Value(0, HelpText = "The key to add to the vault")]
    public string? Key { get; set; }

    [Value(1, HelpText = "The value to add to the vault")]
    public string? Value { get; set; }

    [Option(
        'd',
        "delay",
        Required = false,
        HelpText = "Set the unlock delay in seconds, defaults to 300 (5 minutes)"
    )]
    public int Delay { get; set; } = 300;

    [Option(
        'a',
        "available",
        Required = false,
        HelpText = "Set how long a key remains unlocked after the delay has passed, defaults to 300 (5 minutes)"
    )]
    public int AvailableFor { get; set; } = 300;

    [Option(
        'c',
        "clear-clipboard",
        Required = false,
        HelpText = "Set how long after a value is copied to the clipboard that the clipboard needs to be cleared, defaults to 60 (1 minute)"
    )]
    public int ClearClipboardAfter { get; set; } = 60;

    [Option(
        'u',
        "no-lock-on-copy",
        Required = false,
        HelpText = "Don't lock the key immediately after copying or printing"
    )]
    public bool NoLockOnCopy { get; set; }

    [Option(
        'r',
        "prompt-again",
        Required = false,
        HelpText = "Prompt for password again when copying or printing, defaults to false. Will not ask this when deleting a key."
    )]
    public bool PromptPasswordAgain { get; set; } = false;

    [Option(
        'n',
        "no-overwrite",
        Required = false,
        HelpText = "If the key already exists, deny the attempt. Takes precedence over --overwrite."
    )]
    public bool NoOverwrite { get; set; }

    [Option(
        'o',
        "overwrite",
        Required = false,
        HelpText = "If the key already exists, always overwrite"
    )]
    public bool Overwrite { get; set; }

    [Option(
        't',
        "time",
        Required = false,
        HelpText = "Only allow access during this time block (format H:mm-H:mm as a from-to block, multiple allowed separated with ;)"
    )]
    public string? Time { get; set; }

    public async Task<string?> Execute(VaultIO vaultIO)
    {
        Trace.Assert(this.FileName != null);
        Trace.Assert(this.Password != null);

        (var vault, var filename, var password) = await vaultIO.OpenVault(
            this.FileName,
            this.Password
        );

        var entry = vault.Items.FirstOrDefault(x => x.Key == this.Key);
        if (entry == null)
        {
            entry = new VaultEntry();
            vault.Items.Add(entry);
        }
        else
        {
            if (this.NoOverwrite)
            {
                throw new EndUserException(
                    "Key already exists and --no-overwrite is enabled. Aborting."
                );
            }
            if (!this.Overwrite)
            {
                do
                {
                    Console.Write("Key already exists. Overwrite value? (Y/n): ");
                    var response = Console.ReadLine() ?? "";
                    Console.WriteLine();
                    if (response.Trim().Equals("n", StringComparison.InvariantCultureIgnoreCase))
                    {
                        throw new EndUserException("Aborting.");
                    }

                    if (response.Trim().Equals("y", StringComparison.InvariantCultureIgnoreCase))
                    {
                        break;
                    }
                } while (true);
            }
        }

        entry.Key = this.Key;
        entry.Delay = this.Delay;
        entry.Available = this.AvailableFor;
        entry.ClearAfter = this.ClearClipboardAfter;
        entry.LockAfterCopy = !this.NoLockOnCopy;
        entry.PromptAgain = this.PromptPasswordAgain;
        entry.TimeAvailable = ParseTimeBlocks(this.Time);

        if (string.IsNullOrEmpty(this.Value))
        {
            var response = await vaultIO.UserSecretProvider("Please enter the value: ");
            this.Value = response;
        }

        entry.Value = this.Value;

        VaultIO.Save(vault, filename, password);

        return "Entry added";
    }

    public Tuple<int, int>[]? ParseTimeBlocks(string? timeBlockString)
    {
        if (timeBlockString == null)
            return null;
        var entries = timeBlockString.Split(";");
        List<Tuple<int, int>> result = [];
        foreach (var entry in entries)
        {
            var words = entry.Split("-");
            if (words.Length == 2)
            {
                var leftValid = TimeSpan.TryParseExact(
                    words[0],
                    "h\\:mm",
                    CultureInfo.InvariantCulture,
                    out var tsLeft
                );
                var rightValid = TimeSpan.TryParseExact(
                    words[1],
                    "h\\:mm",
                    CultureInfo.InvariantCulture,
                    out var tsRight
                );
                if (leftValid && rightValid)
                {
                    if (tsLeft < tsRight)
                    {
                        result.Add(
                            Tuple.Create((int)tsLeft.TotalMinutes, (int)tsRight.TotalMinutes)
                        );
                    }
                    else
                    {
                        result.Add(Tuple.Create((int)tsLeft.TotalMinutes, 1440));
                        result.Add(Tuple.Create(0, (int)tsRight.TotalMinutes));
                    }
                }
            }
        }

        return result.ToArray();
    }
}
