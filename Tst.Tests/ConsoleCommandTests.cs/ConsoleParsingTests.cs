using Quake;

namespace Quake.ConsoleCommandTests;

public class ConsoleParsingTests
{
    [Fact]
    public void IsParsed_InputIsSay_HasSay()
    {
        CommandParser parser = new CommandParser();

        parser.Command = "say";

        Assert.Equal(parser.ArgV[0], "say");
        Assert.Equal(parser.ArgC, 1);
    }

    [Fact]
    public void IsParsed_InputIsSay_Has1Say()
    {
        CommandParser parser = new CommandParser();

        parser.Command = "1say";

        Assert.Equal(parser.ArgV[0], "1say");
        Assert.Equal(parser.ArgC, 1);
    }

    [Fact]
    public void IsParsed_InputIsSay_HasSayWithHello()
    {
        CommandParser parser = new CommandParser();

        parser.Command = "say hello";

        Assert.Equal(parser.ArgV[0], "say");
        Assert.Equal(parser.ArgV[1], "hello");
        Assert.Equal(parser.ArgC, 2);
    }

    [Fact]
    public void IsParsed_InputIsSay_HasSayWithSinglelineCommentWithoutHello()
    {
        CommandParser parser = new CommandParser();

        parser.Command = "say // hello";

        Assert.Equal(parser.ArgV[0], "say");
        Assert.Equal(parser.ArgC, 1);
    }

    [Fact]
    public void IsParsed_InputIsSay_HasSayWithMultilineCommentAndHello()
    {
        CommandParser parser = new CommandParser();

        parser.Command = "say /* hello */ hello";

        Assert.Equal(parser.ArgV[0], "say");
        Assert.Equal(parser.ArgV[1], "hello");
        Assert.Equal(parser.ArgC, 2);
    }

    [Fact]
    public void IsParsed_InputIsInvalidSayWithHello_ThrowsCommandParsingException()
    {
        CommandParser parser = new CommandParser();


        Assert.Throws<CommandParsingException>(() => parser.Command = "say /* hello");
    }

    [Fact]
    public void IsParsed_InputIsInvalidSayWithQuoteHello_ThrowsCommandParsingException()
    {
        CommandParser parser = new CommandParser();


        Assert.Throws<CommandParsingException>(() => parser.Command = "say ” hello");
    }
}
