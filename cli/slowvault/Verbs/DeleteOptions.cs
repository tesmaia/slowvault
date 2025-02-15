using System;
using CommandLine;

namespace SlowVault;


[Verb("delete", HelpText="Delete a key from the vault, needs to be unlocked")]
public class DeleteOptions 
{
    
    [Option('f', "file", Required = false, HelpText = "File path of the vault, may also be defined as environment variable SLOWVAULT_PATH")]
    public string? FileName { get; set; }

    [Option('p', "password", Required = false, HelpText = "Password for the vault, optional, will prompt if not given.")]
    public string Password { get; set; }

    [Value(0, HelpText = "The key to delete from the vault (options only)")]
    public string? Key { get; set; }

    public string Execute() 
    { 
        (var vault, var filename, var password) = Program.OpenVault(this.FileName, this.Password);

        var entry = vault.Items.FirstOrDefault(x => x.Key == this.Key);
        if(entry == null)
        {
            return $"Entry with key {this.Key} was not found";
        }

        vault.Items.Remove(entry);
        VaultExtensions.Save(vault, filename, password);

        return $"Key {this.Key} removed";
    }

}