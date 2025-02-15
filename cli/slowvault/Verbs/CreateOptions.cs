using System;
using System.IO;
using CommandLine;

namespace SlowVault;

[Verb("create", HelpText = "Create a new vault at the given location.")]
public class CreateOptions
{
    [Value(0, HelpText = "Name of the vault")]
    public string? Name { get; set; }

    [Option("path", Required = false, HelpText = "Where to store the vault. Defaults to current directory or what is defined in SLOWVAULT_PATH.")]
    public string? Path { get; set; }

    [Option('p', "password", Required = false, HelpText = "Password for the vault. When ommitted you will be prompted.")]
    public string? Password { get; set; }


    public string Execute()
    {
        if (Name == null) 
        {
            throw new EndUserException("ERROR Please enter a name for the vault");
        }

        var path = Path ?? Program.DefaultFilePath;
        if(!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        var fileName = System.IO.Path.Combine(path, Name + ".svlt");
        if(File.Exists(fileName))
        {
            throw new EndUserException("A vault with this name already exists at this location");
        }
        
        if(string.IsNullOrEmpty(Password))
        {
            Password = Program.ConsoleReadPassword();
        }

        var vault = new Vault();
        VaultExtensions.Save(vault, fileName, Password);
        return "Vault created";
    }
}