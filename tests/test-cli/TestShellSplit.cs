namespace SlowVault.Test;

[TestFixture]
public class TestShellSplit
{
    [Test]
    [TestCase("sd adfags \"sdsd\" \"sdsd  sdsds\"", "sd-adfags-sdsd-sdsd  sdsds")]
    public void Test1(string input, string joined)
    {
        var args = SlowVault.Cli.Program.ShellSplit(input);
        Assert.That(string.Join("-", args), Is.EqualTo(joined));
    }
}
