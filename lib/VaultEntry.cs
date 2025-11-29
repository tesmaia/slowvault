using System;

namespace SlowVault.Lib;

public class VaultEntry
{
    public string? Key { get; set; }
    public int Delay { get; set; }
    public int Available { get; set; }
    public int ClearAfter { get; set; }
    public bool LockAfterCopy { get; set; }
    public bool PromptAgain { get; set; }
    public Tuple<int, int>[]? TimeAvailable { get; set; }

    public string? Value { get; set; }

    public bool CheckTimeLock(DateTime now)
    {
        if (TimeAvailable == null || TimeAvailable.Length == 0)
            return false;

        var nowMinutes = (int)now.TimeOfDay.TotalMinutes;

        return !TimeAvailable.Any(x => x.Item1 <= nowMinutes && x.Item2 >= nowMinutes);
    }
}
