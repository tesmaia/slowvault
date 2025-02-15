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
            var vaultPaths = Directory.GetFiles(DefaultFilePath, "*.svlt", System.IO.SearchOption.TopDirectoryOnly);
            for(int i = 0; i < vaultPaths.Length; i++)
            {
                var name = Path.GetFileNameWithoutExtension(vaultPaths[i]);
                Console.WriteLine($"{i+1}) {name}");
            }
            while(true)
            {
                Console.Write("Choose one of the above vaults to open: ");
                var response = Console.ReadLine();
                if(int.TryParse(response, out var responseIdx))
                {
                    if(responseIdx >= 1 && responseIdx <= vaultPaths.Length)
                    {
                        var path = vaultPaths[responseIdx - 1];
                        fileName = path;
                        break;
                    }
                    else
                    {
                        throw new EndUserException("ERROR Vault does not exist");
                    }
                }
                else
                {
                    ClearCurrentConsoleLine();
                }
            }
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

    internal static void ClearCurrentConsoleLine()
    {
        int currentLineCursor = Console.CursorTop;
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(new string(' ', Console.WindowWidth)); 
        Console.SetCursorPosition(0, currentLineCursor);
    }

}