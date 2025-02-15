using System;
using CommandLine;

namespace SlowVault;


[Verb("view", HelpText="View the options set for a key, can also be used to check if a key exists")]
public class ViewOptions
{
    
    [Option('f', "file", Required = false, HelpText = "File path of the vault, may also be defined as environment variable SLOWVAULT_PATH")]
    public string? FileName { get; set; }

    [Option('p', "password", Required = false, HelpText = "Password for the vault, optional, will prompt if not given.")]
    public string Password { get; set; }

    [Value(0, HelpText = "The key to fetch from the vault (options only)")]
    public string? Key { get; set; }

    public string Execute() 
    { 
        (var vault, var _, var _) = Program.OpenVault(this.FileName, this.Password);

        var entry = vault.Items.FirstOrDefault(x => x.Key == this.Key);
        if(entry == null)
        {
            return $"Entry with key {this.Key} was not found";
        }

        Console.WriteLine($"Key: {entry.Key}");
        Console.WriteLine($"Delay: {entry.Delay}s");
        Console.WriteLine($"Available for: {entry.Available}s");
        Console.WriteLine($"Clear after: {entry.ClearAfter}s");
        Console.WriteLine($"Lock after copy: {entry.LockAfterCopy}");
        Console.WriteLine($"Prompt password again: {entry.PromptAgain}");

        return null;
    }

}