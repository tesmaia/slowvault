using System.Collections.Generic;
using System.IO;

namespace SlowVault;

public class Vault
{
    public const string MAGIC = "SLVLT";
    const string FILE_VERSION = "0.0.1";

    /// <summary>
    /// Serialization constructor
    /// </summary>
    public Vault() {

    }

    /// <summary>
    /// Create a new vault object
    /// </summary>
    public static Vault Create() 
    {
        return new Vault
        {
            Magic = MAGIC,
            FileVersion = FILE_VERSION
        };
    }


    public string Magic { get; set; } = MAGIC;

    public string FileVersion { get; set; } = FILE_VERSION;


    public List<VaultEntry> Items { get; set; } = new List<VaultEntry>();

    

}

public class VaultEntry
{
    public string? Key { get; set; }
    public int Delay { get; set; }
    public int Available { get; set; }
    public int ClearAfter { get; set; }
    public bool LockAfterCopy { get; set; }
    public bool PromptAgain { get; set; }

    public string? Value { get; set; }
}