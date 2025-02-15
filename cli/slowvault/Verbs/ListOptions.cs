using System;
using CommandLine;

namespace SlowVault;


[Verb("list", HelpText="List all the keys stored in the vault.")]
public class ListOptions
{
    
    [Option('f', "file", Required = false, HelpText = "File path of the vault, may also be defined as environment variable SLOWVAULT_PATH")]
    public string? FileName { get; set; }

    [Option('p', "password", Required = false, HelpText = "Password for the vault, optional, will prompt if not given.")]
    public string Password { get; set; }

    public string Execute() 
    { 
        (var vault, var _, var _) = Program.OpenVault(this.FileName, this.Password);

        Console.WriteLine($"Listing {vault.Items.Count} keys in the vault:");
        foreach(var item in vault.Items)
        {
            Console.WriteLine(item.Key);
        }

        return null;
    }

}