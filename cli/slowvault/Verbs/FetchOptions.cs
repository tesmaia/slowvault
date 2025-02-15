using System;
using CommandLine;

namespace SlowVault;

[Verb("fetch", HelpText="Fetch a secret from the vault.")]
public class FetchOptions 
{
    
    [Option('f', "file", Required = false, HelpText = "File path of the vault, may also be defined as environment variable SLOWVAULT_PATH")]
    public string? FileName { get; set; }

    [Option('p', "password", Required = false, HelpText = "Password for the vault, optional, will prompt if not given.")]
    public string Password { get; set; }

    [Value(0, HelpText = "The key to fetch from the vault")]
    public string? Key { get; set; }


    static bool locked = false;
    static string valueOnClipboard;

    public string Execute() 
    { 
        (var vault, var _, var _) = Program.OpenVault(this.FileName, this.Password);

        var entry = vault.Items.FirstOrDefault(x => x.Key == this.Key);
        if(entry == null)
        {
            return $"Entry with key {this.Key} was not found";
        }

        var target = DateTime.UtcNow.AddSeconds(entry.Delay);
        Console.Write($"Unlocking entry after {entry.Delay}s");
        var spinnerSequence = @"/-\|";
        var spinnerPos = 0;

        while(target > DateTime.UtcNow)
        {
            System.Threading.Thread.Sleep(200);
            Program.ClearCurrentConsoleLine();
            spinnerPos = (++spinnerPos) % spinnerSequence.Length;
            int delayRemaining = (int)Math.Round((target - DateTime.UtcNow).TotalSeconds);
            Console.Write($"Unlocking entry after {delayRemaining}s {spinnerSequence[spinnerPos]}");
        }

         Program.ClearCurrentConsoleLine();

        var available = DateTime.UtcNow.AddSeconds(entry.Available);

        Console.WriteLine($"You can obtain the value until {available.ToLocalTime():HH:mm:ss}");
        string response = null;
        System.Threading.Timer timer = null;
        Console.WriteLine($"Use print (p) or copy (c) command. Use exit (q) to quit the session");
        while(true)
        {
            Console.Write("> ");
            response = Console.ReadLine().ToLower();

            if(DateTime.UtcNow > available)
            {
                Console.WriteLine("Availability expired");
                break;
            }

            if(response == "copy" || response == "c")
            {
                if(locked)
                {
                    Console.WriteLine("Access denied. Value is locked.");
                    break;
                }
                else
                {
                    Console.WriteLine("Value copied to clipboard");
                    TextCopy.ClipboardService.SetText(entry.Value);
                    valueOnClipboard = entry.Value;
                    if(timer != null)
                    {
                        timer.Change(Timeout.Infinite, Timeout.Infinite);
                        timer.Dispose();
                        timer = null;
                    }
                    Console.WriteLine($"The value will be cleared after {entry.ClearAfter}s");
                    timer = new System.Threading.Timer(ClearClipboard, "timer", entry.ClearAfter * 1000, Timeout.Infinite);

                    if(entry.LockAfterCopy) 
                        locked = true;
                }
            }
            else if(response == "print"|| response == "p")
            {
                if(locked)
                {
                    Console.WriteLine("Access denied. Value is locked.");
                    break;
                }
                Console.WriteLine(entry.Value);
                if(entry.LockAfterCopy) 
                    locked = true;
            }
            else if(response == "exit"|| response == "q")
            {
                break;
            }

        }

        ClearClipboard(null);
        return null;

    }

    static void ClearClipboard(object arg)
    {
        if(valueOnClipboard == null) return;
        try
        {
            var clipboard = TextCopy.ClipboardService.GetText();
            if(valueOnClipboard.Trim() == clipboard.Trim())
            {
                TextCopy.ClipboardService.SetText("");
                Console.WriteLine();
                Console.WriteLine("Clipboard cleared");
                if(arg == "timer")
                    Console.Write("> ");
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        
    }



}
