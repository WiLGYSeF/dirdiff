using Wilgysef.DirDiff.Cli.Logging;
using Shouldly;

namespace Wilgysef.DirDiff.Cli.Tests.LoggingTests;

public class ConsoleLogFormatterTest
{
    [Fact]
    public void Format_No_Items()
    {
        var formatter = new ConsoleLogFormatter();

        formatter.Format(new[]
        {
            new KeyValuePair<string, object>("{OriginalFormat}", "this is a test"),
        }).ShouldBe("this is a test");
    }

    [Theory]
    [InlineData("this is a {item message", "this is a {item message")]
    [InlineData("this is a item} message", "this is a item} message")]
    [InlineData("this is a {{item} message", "this is a {item} message")]
    [InlineData("this is a {item}} message", "this is a test} message")]
    public void Balance_Check(string format, string expected)
    {
        var formatter = new ConsoleLogFormatter();

        formatter.Format(new[]
        {
            new KeyValuePair<string, object>("item", "test"),
            new KeyValuePair<string, object>("{OriginalFormat}", format),
        }).ShouldBe(expected);
    }

    [Fact]
    public void One_Item()
    {
        var formatter = new ConsoleLogFormatter();

        formatter.Format(new[]
        {
            new KeyValuePair<string, object>("item", "test"),
            new KeyValuePair<string, object>("{OriginalFormat}", "this is a {item}"),
        }).ShouldBe("this is a test");
    }

    [Fact]
    public void Multiple_Item()
    {
        var formatter = new ConsoleLogFormatter();

        formatter.Format(new[]
        {
            new KeyValuePair<string, object>("item", "test"),
            new KeyValuePair<string, object>("abc", "def"),
            new KeyValuePair<string, object>("{OriginalFormat}", "this is a {item} {abc}!"),
        }).ShouldBe("this is a test def!");
    }
}
