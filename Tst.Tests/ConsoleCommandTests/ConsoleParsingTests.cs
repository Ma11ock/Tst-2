using Quake;

namespace Quake.ConsoleCommandTests;

public class ConsoleParsingTests
{
    private string NthFrom(IEnumerable<ReadOnlyMemory<char>> argv, int where) => argv.ToArray()[where].ToString();

    [Fact]
    public void IsParsed_InputIsSay_HasSay()
    {
        CommandParser parser = new CommandParser();

        parser.Command = "say";

        Assert.Equal("say", NthFrom(parser.ArgV, 0));
        Assert.Equal(1, parser.ArgC);
    }

    [Fact]
    public void IsParsed_InputIs1Say_Has1Say()
    {
        CommandParser parser = new CommandParser();

        parser.Command = "1say";

        Assert.Equal("1say", NthFrom(parser.ArgV, 0));
        Assert.Equal(1, parser.ArgC);
    }

    [Fact]
    public void IsParsed_InputIsSay_HasSayWithHello()
    {
        CommandParser parser = new CommandParser();

        parser.Command = "say hello";

        Assert.Equal("say", NthFrom(parser.ArgV, 0));
        Assert.Equal("hello", NthFrom(parser.ArgV, 1));
        Assert.Equal(2, parser.ArgC);
    }

    [Fact]
    public void IsParsed_InputIsSay_HasSayWithSinglelineCommentWithoutHello()
    {
        CommandParser parser = new CommandParser();

        parser.Command = "say // hello";

        Assert.Equal("say", NthFrom(parser.ArgV, 0));
        Assert.Equal(1, parser.ArgC);
    }

    [Fact]
    public void IsParsed_InputIsSay_HasSayWithMultilineCommentAndHello()
    {
        CommandParser parser = new CommandParser();

        parser.Command = "say /* hello */ hello";

        Assert.Equal("say", NthFrom(parser.ArgV, 0));
        Assert.Equal("hello", NthFrom(parser.ArgV, 1));
        Assert.Equal(2, parser.ArgC);
    }

    [Fact]
    public void IsParsed_InputIsQuotedHelloWorld_HasSayWithHelloWorld()
    {
        CommandParser parser = new CommandParser();

        parser.Command = "say \"Hello World\"";

        Assert.Equal("say", NthFrom(parser.ArgV, 0));
        Assert.Equal("Hello World", NthFrom(parser.ArgV, 0));
        Assert.Equal(2, parser.ArgC);
    }

    [Fact]
    public void IsParsed_InputIsQuotedHelloWorldWithEscapedQuotedHello_HasSayWithQuotedHelloWorld()
    {
        CommandParser parser = new CommandParser();

        parser.Command = "say \"Hello \\\"World\\\"\"";

        Assert.Equal("say", NthFrom(parser.ArgV, 0));
        Assert.Equal("Hello \"World\"", NthFrom(parser.ArgV, 0));
        Assert.Equal(2, parser.ArgC);
    }

    [Fact]
    public void IsParsed_InputIsUnicodeQuotedHelloWorldWithEscapedQuotedHello_HasSayWithUnicodeQuotedHelloWorld()
    {
        CommandParser parser = new CommandParser();

        parser.Command = "say \"Hello \\“World\\”\"";

        Assert.Equal("say", NthFrom(parser.ArgV, 0));
        Assert.Equal("Hello “World“", NthFrom(parser.ArgV, 0));
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

    [Fact]
    public void IsParsed_InputIsInvalidSayWithImproperlyQuotedHello_ThrowsCommandParsingException()
    {
        CommandParser parser = new CommandParser();


        Assert.Throws<CommandParsingException>(() => parser.Command = "say \" hello");
        Assert.Throws<CommandParsingException>(() => parser.Command = "say ” hello");
    }
}
