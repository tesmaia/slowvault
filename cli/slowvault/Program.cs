using System;
using CommandLine;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography;

namespace SlowVault;

internal class Program
{
    internal static string DefaultFilePath = null;
    private static void Main(string[] args)
    {
        try
        {
            var directory = Environment.GetEnvironmentVariable("SLOWVAULT_PATH");
            DefaultFilePath = directory ?? Directory.GetCurrentDirectory();
            var result = Parser.Default.ParseArguments<
                AddOptions, 
                CreateOptions,
                FetchOptions,
                DeleteOptions,
                ViewOptions,
                ListOptions
                 >(args);

            var parsed = result as Parsed<object>;
            if(parsed != null) {
                var finalResult = parsed.Value switch {
                    (CreateOptions opt) => opt.Execute(),
                    (AddOptions opt) => opt.Execute(),
                    (FetchOptions opt) => opt.Execute(),
                    (DeleteOptions opt) => opt.Execute(),
                    (ViewOptions opt) => opt.Execute(),
                    (ListOptions opt) => opt.Execute(),
                    _ => "ERROR"
                };

                if(finalResult != null)
                    Console.WriteLine(finalResult);
            }
        }
        catch (EndUserException ex)
        {
            Console.WriteLine(ex.Message);
        }

    }


    internal static (Vault, string, string) OpenVault(string fileName, string password)
    {
        if(string.IsNullOrEmpty(fileName))
        {
            throw new EndUserException("ERROR Not yet implemented");
        }
        if(string.IsNullOrEmpty(password))
            password = ConsoleReadPassword();

        var vault = VaultExtensions.Load(fileName, password);
        if(vault == null) 
        {
             throw new EndUserException("ERROR Failure to open vault");
        }
        return (vault, fileName, password);
    }

    public static string HandleAdd(AddOptions options)
    {
        (var vault, var filename, var password) = OpenVault(options.FileName, options.Password);

        var entry = vault.Items.FirstOrDefault(x => x.Key == options.Key);
        if(entry == null)
        {
            entry = new VaultEntry();
            vault.Items.Add(entry);
        }

        entry.Key = options.Key;
        entry.Delay = options.Delay;
        entry.Available = options.AvailableFor;
        entry.ClearAfter = options.ClearClipboardAfter;
        entry.LockAfterCopy = !options.NoLockOnCopy;
        entry.PromptAgain = options.PromptPasswordAgain;
        entry.Value = options.Value;

        VaultExtensions.Save(vault, filename, password);

        return "Entry added";
    }

    internal static string ConsoleReadPassword(bool hidePrompt = false)
    {
        if(!hidePrompt)
        {
            Console.Write("Please enter the password:");
        }
        string password = null;
        while (true)
        {
            var key = System.Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                break;
            }
            password += key.KeyChar;
        }
        return password;
    }




}