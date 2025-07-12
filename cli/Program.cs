using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using CommandLine;
using SlowVault.Cli.Verbs;
using SlowVault.Lib;

namespace SlowVault.Cli;

public class Program
{
    internal static string? DefaultFilePath = null;

    public static string[] ShellSplit(string arguments)
    {
        var list = new List<string>();
        var last = new List<char>();
        var quoted = false;
        foreach (var ch in arguments)
        {
            last.Add(ch);
            if (ch == '"')
            {
                quoted = !quoted;
            }

            if (!quoted && ch == ' ')
            {
                list.Add(new string(last.ToArray()).Trim().Trim('"'));
                last.Clear();
            }
        }
        if (last.Count != 0)
        {
            list.Add(new string(last.ToArray()).Trim().Trim('"'));
        }
        return list.ToArray();
    }

    private static void Main(string[] args)
    {
        if (args.Length == 0)
        {
#if DEBUG
            Console.WriteLine("Debug arguments");
            var readArgs = Console.ReadLine() ?? "";
            args = ShellSplit(readArgs);
#endif
        }
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

            var io = new VaultIO()
            {
                DefaultFilePath = DefaultFilePath,
                FileNameProvider = RequestFileFromCollection,
                PasswordProvider = ConsoleAskPassword,
                UserSecretProvider = ConsoleAskSecret,
            };

            var parsed = result as Parsed<object>;
            if (parsed != null)
            {
                Task<string?> resultTask = parsed.Value switch
                {
                    CreateOptions opt => opt.Execute(io),
                    AddOptions opt => opt.Execute(io),
                    FetchOptions opt => opt.Execute(io),
                    DeleteOptions opt => opt.Execute(io),
                    ViewOptions opt => opt.Execute(io),
                    ListOptions opt => opt.Execute(io),
                    _ => Task.FromResult<string?>("ERROR"),
                };

                string? finalResult = resultTask.GetSync();

                if (finalResult != null)
                    Console.WriteLine(finalResult);
            }
        }
        catch (EndUserException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private static Task<string> RequestFileFromCollection(string[] vaultPaths)
    {
        string fileName;

        for (int i = 0; i < vaultPaths.Length; i++)
        {
            var name = Path.GetFileNameWithoutExtension(vaultPaths[i]);
            Console.WriteLine($"{i + 1}) {name}");
        }
        while (true)
        {
            Console.Write("Choose one of the above vaults to open: ");
            var response = Console.ReadLine();
            if (int.TryParse(response, out var responseIdx))
            {
                if (responseIdx >= 1 && responseIdx <= vaultPaths.Length)
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

        return Task.FromResult(fileName);
    }

    private static async Task<string> ConsoleAskPassword()
    {
        return await ConsoleAskSecret("Please enter your password: ");
    }

    private static Task<string> ConsoleAskSecret(string prompt)
    {
        if (!string.IsNullOrEmpty(prompt))
            Console.Write(prompt);

        string password = "";
        while (true)
        {
            try
            {
                var key = System.Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                password += key.KeyChar;
            }
            catch (InvalidOperationException)
            {
                //fallback
                var pass = System.Console.ReadLine();
                password += pass;
                break;
            }
        }
        return Task.FromResult(password);
    }

    internal static bool ClearCurrentConsoleLine()
    {
        try
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
            return true;
        }
        catch
        {
            //return false to indicate that the console is not succesfully cleared
            return false;
        }
    }
}
