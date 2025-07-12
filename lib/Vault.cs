namespace SlowVault.Lib;

public class Vault
{
    //magic identifies the file format
    public const string MAGIC = "SLVLT";

    //file version, just in case I change my mind
    const string FILE_VERSION = "0.0.1";

    public string Magic { get; set; } = MAGIC;

    public string FileVersion { get; set; } = FILE_VERSION;

    public List<VaultEntry> Items { get; set; } = new List<VaultEntry>();
}
