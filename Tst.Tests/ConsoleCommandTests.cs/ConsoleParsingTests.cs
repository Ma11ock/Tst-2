using Quake;

namespace Quake.ConsoleCommandTests;

public class ConsoleParsingTests
{
    [Fact]
    public void IsParsed_InputIsSay_HasSay()
    {
        CommandParser parser = new CommandParser();

        parser.Command = "say";

        Assert.Equal("say", parser.ArgV[0]);
        Assert.Equal(1, parser.ArgC);
    }

    [Fact]
    public void IsParsed_InputIs1Say_Has1Say()
    {
        CommandParser parser = new CommandParser();

        parser.Command = "1say";

        Assert.Equal("1say", parser.ArgV[0]);
        Assert.Equal(1, parser.ArgC);
    }

    [Fact]
    public void IsParsed_InputIsSay_HasSayWithHello()
    {
        CommandParser parser = new CommandParser();

        parser.Command = "say hello";

        Assert.Equal("say", parser.ArgV[0]);
        Assert.Equal("hello", parser.ArgV[1]);
        Assert.Equal(2, parser.ArgC);
    }

    [Fact]
    public void IsParsed_InputIsSay_HasSayWithSinglelineCommentWithoutHello()
    {
        CommandParser parser = new CommandParser();

        parser.Command = "say // hello";

        Assert.Equal("say", parser.ArgV[0]);
        Assert.Equal(1, parser.ArgC);
    }

    [Fact]
    public void IsParsed_InputIsSay_HasSayWithMultilineCommentAndHello()
    {
        CommandParser parser = new CommandParser();

        parser.Command = "say /* hello */ hello";

        Assert.Equal("say", parser.ArgV[0]);
        Assert.Equal("hello", parser.ArgV[1]);
        Assert.Equal(2, parser.ArgC);
    }

    [Fact]
    public void IsParsed_InputIsQuotedHelloWorld_HasSayWithQuotedHelloWorld()
    {
        CommandParser parser = new CommandParser();

        parser.Command = "say \"Hello World\"";

        Assert.Equal(parser.ArgV[0], "say");
        Assert.Equal(parser.ArgV[1], "Hello World");
        Assert.Equal(2, parser.ArgC);
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
