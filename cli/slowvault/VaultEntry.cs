using System;

namespace SlowVault;

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
