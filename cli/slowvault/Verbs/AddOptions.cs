using System;
using CommandLine;

namespace SlowVault;

[Verb("add", HelpText="Add a new key to the vault")]
public class AddOptions 
{
    [Option('f', "file-name", Required = false, HelpText = "File name of the vault.")]
    public string FileName { get; set; }

    [Option('p', "password", Required = false, HelpText = "Password for the vault, optional, will prompt if not given.")]
    public string Password { get; set; }
    

    [Value(0, HelpText = "The key to add to the vault")]
    public string? Key { get; set; }

    [Value(1, HelpText = "The value to add to the vault")]
    public string? Value { get; set; }


    [Option('d', "delay", Required = false, HelpText = "Set the unlock delay in seconds, defaults to 300 (5 minutes)")]
    public int Delay { get; set; } = 300;

    [Option('a', "available", Required = false, HelpText = "Set how long a key remains unlocked after the delay has passed, defaults to 300 (5 minutes)")]
    public int AvailableFor { get; set; } = 300;

    [Option('c', "clear-clipboard", Required = false, HelpText = "Set how long after a value is copied to the clipboard that the clipboard needs to be cleared, defaults to 60 (1 minute)")]
    public int ClearClipboardAfter { get; set; } = 60;

    [Option('u', "no-lock-on-copy", Required = false, HelpText = "Don't lock the key immediately after copying or printing")]
    public bool NoLockOnCopy { get; set; }

    [Option('r', "prompt-again", Required = false, HelpText = "Prompt for password again when copying or printing, defaults to false. Will not ask this when deleting a key.")]
    public bool PromptPasswordAgain { get; set; } = false;

    [Option('n', "no-overwrite", Required = false, HelpText = "If the key already exists, deny the attempt. Takes precedence over --overwrite.")]
    public bool NoOverwrite { get; set; }

    [Option('o', "overwrite", Required = false, HelpText = "If the key already exists, always overwrite")]
    public bool Overwrite { get; set; }

    public string Execute()
    {
         (var vault, var filename, var password) = Program.OpenVault(this.FileName, this.Password);

        var entry = vault.Items.FirstOrDefault(x => x.Key == this.Key);
        if(entry == null)
        {
            entry = new VaultEntry();
            vault.Items.Add(entry);
        }
        else
        {
            if(this.NoOverwrite)
            {
                throw new EndUserException("Key already exists and --no-overwrite is enabled. Aborting.");
            }
            if(!this.Overwrite)
            {
                do
                {
                    Console.Write("Key already exists. Overwrite value? (Y/n): ");
                    var response = Console.ReadLine();
                    Console.WriteLine();
                    if(response.Trim().ToLower() == "n")
                    {
                        throw new EndUserException("Aborting.");
                    }
                    
                    if(response.Trim().ToLower() == "y")
                    {
                        break;
                    }
                } while(true);
            }
        }

        entry.Key = this.Key;
        entry.Delay = this.Delay;
        entry.Available = this.AvailableFor;
        entry.ClearAfter = this.ClearClipboardAfter;
        entry.LockAfterCopy = !this.NoLockOnCopy;
        entry.PromptAgain = this.PromptPasswordAgain;

        if(string.IsNullOrEmpty(this.Value))
        {
            Console.Write("Please enter the value: ");
            var response = Program.ConsoleReadPassword(hidePrompt: true);
            this.Value = response;
        }

        entry.Value = this.Value;

        VaultExtensions.Save(vault, filename, password);

        return "Entry added";
    }
}