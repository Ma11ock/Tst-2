using Quake;

namespace Quake.ConsoleCommandTests;

public class ConsoleParsingTests
{
    [Fact]
    public void IsParsed_InputIsSay_HasSay()
    {
        CommandParser parser = new CommandParser();

        parser.Command = "say";

        Assert.True(parser.ArgV[0] == "say");
    }

    [Fact]
    public void IsParsed_InputIsSay_Has1Say()
    {
        CommandParser parser = new CommandParser();

        parser.Command = "1say";

        Assert.True(parser.ArgV[0] == "1say");
    }

    [Fact]
    public void IsParsed_InputIsSay_HasSayWithHello()
    {
        CommandParser parser = new CommandParser();

        parser.Command = "say hello";

        Assert.True(parser.ArgV[0] == "say");
        Assert.True(parser.ArgV[1] == "hello");
    }
}
