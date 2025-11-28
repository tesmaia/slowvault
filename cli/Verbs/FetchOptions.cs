//#define FASTTIME
//#define WAYLANDKDE

using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CommandLine;
using SlowVault.Lib;

namespace SlowVault.Cli.Verbs;

[Verb("fetch", HelpText = "Fetch a secret from the vault.")]
public class FetchOptions : IExecutableOptions
{
    [Option(
        'f',
        "file",
        Required = false,
        HelpText = "File path of the vault, may also be defined as environment variable SLOWVAULT_PATH"
    )]
    public string FileName { get; set; } = string.Empty;

    [Option(
        'v',
        "from",
        Required = false,
        HelpText = "Name of the vault, without extension, requires the vault to be placed in the SLOWVAULT_PATH directory"
    )]
    public string VaultName { get; set; } = string.Empty;

    [Option(
        'p',
        "password",
        Required = false,
        HelpText = "Password for the vault, optional, will prompt if not given."
    )]
    public string Password { get; set; } = string.Empty;

    [Value(0, HelpText = "The key to fetch from the vault")]
    public string? Key { get; set; }

    static bool locked = false;
    static string? valueOnClipboard;

    static DateTime? _firstCall;

    DateTime GetUtcNow()
    {
#if FASTTIME
        if (!_firstCall.HasValue)
            _firstCall = DateTime.UtcNow;

        return _firstCall.Value + (20 * (DateTime.UtcNow - _firstCall.Value));
#endif
        return DateTime.UtcNow;
    }

    public async Task<string?> Execute(VaultIO vaultIO)
    {
        (var vault, var _, var _) = await vaultIO.OpenVault(
            this.FileName,
            this.Password,
            VaultName
        );

        var entry = vault.Items.FirstOrDefault(x => x.Key == this.Key);
        if (entry == null)
        {
            return $"Entry with key {this.Key} was not found";
        }

        var target = GetUtcNow().AddSeconds(entry.Delay);
        Console.Write($"Unlocking entry after {entry.Delay}s");
        var spinnerSequence = @"/-\|";
        var spinnerPos = 0;

        var spinnerSupported = true;
        while (target > GetUtcNow())
        {
            System.Threading.Thread.Sleep(200);
            spinnerPos = (++spinnerPos) % spinnerSequence.Length;
            if (spinnerSupported)
                spinnerSupported = Program.ClearCurrentConsoleLine();

            if (spinnerSupported)
            {
                int delayRemaining = (int)Math.Round((target - GetUtcNow()).TotalSeconds);
                Console.Write(
                    $"Unlocking entry after {delayRemaining}s {spinnerSequence[spinnerPos]}"
                );
            }
            else
            {
                if (spinnerPos == 0)
                {
                    Console.Write(".");
                }
            }
        }

        Program.ClearCurrentConsoleLine();

        var available = GetUtcNow().AddSeconds(entry.Available);

        Console.WriteLine($"You can obtain the value until {available.ToLocalTime():HH:mm:ss}");
        string? response = null;
        System.Threading.Timer? timer = new System.Threading.Timer(
            PrintAvailabilityExpired,
            "timer",
            dueTime: entry.Available * 1000
#if FASTTIME
                / 20
#endif
            ,
            Timeout.Infinite
        );
        Console.WriteLine($"Use print (p) or copy (c) command. Use exit (q) to quit the session");
        while (true)
        {
            Console.Write("> ");
            response = Console.ReadLine()?.ToLower() ?? string.Empty;

            if (response == "exit" || response == "q")
            {
                break;
            }

            if (GetUtcNow() > available)
            {
                Console.WriteLine("Availability expired. Use q to exit.");
                Console.Write("> ");
                break;
            }

            if (response == "copy" || response == "c")
            {
                if (locked)
                {
                    Console.WriteLine("Access denied. Value is locked.");
                    break;
                }
                else
                {
                    Console.WriteLine("Value copied to clipboard");
                    TextCopy.ClipboardService.SetText(entry!.Value ?? string.Empty);
                    valueOnClipboard = entry!.Value;

                    if (timer != null)
                    {
                        timer.Change(Timeout.Infinite, Timeout.Infinite);
                        timer.Dispose();
                        timer = null;
                    }
                    Console.WriteLine($"The value will be cleared after {entry.ClearAfter}s");
                    timer = new System.Threading.Timer(
                        ClearClipboardAndExit,
                        "timer",
                        entry.ClearAfter * 1000,
                        Timeout.Infinite
                    );

                    if (entry.LockAfterCopy)
                        locked = true;
                }
            }
            else if (response == "print" || response == "p")
            {
                if (locked)
                {
                    Console.WriteLine("Access denied. Value is locked.");
                    break;
                }
                Console.WriteLine(entry.Value);
                if (entry.LockAfterCopy)
                    locked = true;
            }
        }

        ClearClipboard(null);
        return null;
    }

    static void PrintAvailabilityExpired(object? arg)
    {
        Console.WriteLine();
        Console.WriteLine("Availability expired. Use q to exit.");
        Console.Write("> ");
    }
    
    static void ClearClipboardAndExit(object? arg)
    {
        ClearClipboard(null);
        Thread.Sleep(500);
        System.Environment.Exit(0);
    }

    static void ClearClipboard(object? arg)
    {
        if (string.IsNullOrWhiteSpace(valueOnClipboard))
            return;
        try
        {
            var clipboard = TextCopy.ClipboardService.GetText();
            if (clipboard != null && valueOnClipboard.Trim() == clipboard.Trim())
            {
                TextCopy.ClipboardService.SetText("");
#if WAYLANDKDE
                KdeClear();
#endif
                Console.WriteLine();
                Console.WriteLine("Clipboard cleared");
                if ((arg as string) == "timer")
                    Console.Write("> ");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }


    static void KdeClear()
    {
        SlowVault.Lib.TextCopy.BashRunner.Run(
            "qdbus6 org.kde.klipper /klipper clearClipboardHistory"
        );
    }
}
