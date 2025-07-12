using System;
using SlowVault.Lib;

namespace SlowVault.Cli.Verbs;

public interface IExecutableOptions
{
    Task<string?> Execute(VaultIO vaultIO);
}
