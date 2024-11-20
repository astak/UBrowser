using FluentAssertions;
using System.Diagnostics;
using System.Text;
using UBrowser.WebEngine.Parser;

namespace UBrowser.WebEngine.Tests;

public class HtmlTokenizerTests
{
  [Fact]
  public void Tokenize_ShouldReturnStartTagToken_ForSimpleOpenTag()
  {
    //Arrange
    var tokenizer = new HtmlTokenizer();
    tokenizer.Reset("<div>");

    //Act
    var token = tokenizer.GetNextToken();

    //Assert
    token.Should().NotBeNull();
    token.Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.Div));
  }

  [Fact]
  public void Tokenize_ShouldIgnoreInvalidCloseTag()
  {
    //Arrange
    var tokenizer = new HtmlTokenizer();
    tokenizer.Reset("</>");

    //Act
    var result = tokenizer.GetNextToken();

    //Assert
    result.Should().BeNull();
  }

  [Fact]
  public void Tokenizer_ShouldHandleCloseTagWithWhiteSpace()
  {
    //Arrange
    var tokenizer = new HtmlTokenizer();
    tokenizer.Reset("<div></div >");

    //Act
    var result0 = tokenizer.GetNextToken();
    var result1 = tokenizer.GetNextToken();

    //Assert
    result0.Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.Div));
    result1.Should().BeEquivalentTo(new Token(TokenType.EndTag, TagNames.Div));
  }

  [Fact]
  public void Tokenizer_ShouldPreserveTextNodesBetweenTags()
  {
    //Arrange
    var tokenizer = new HtmlTokenizer();
    tokenizer.Reset("<p>Hello <span>nice</span> world</p>");

    //Act
    var result = new Token?[7];
    result[0] = tokenizer.GetNextToken();
    result[1] = tokenizer.GetNextToken();
    result[2] = tokenizer.GetNextToken();
    result[3] = tokenizer.GetNextToken();
    result[4] = tokenizer.GetNextToken();
    result[5] = tokenizer.GetNextToken();
    result[6] = tokenizer.GetNextToken();

    //Assert
    result[0].Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.Paragraph));
    result[1].Should().BeEquivalentTo(new Token(TokenType.Text, "Hello "));
    result[2].Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.Span));
    result[3].Should().BeEquivalentTo(new Token(TokenType.Text, "nice"));
    result[4].Should().BeEquivalentTo(new Token(TokenType.EndTag, TagNames.Span));
    result[5].Should().BeEquivalentTo(new Token(TokenType.Text, " world"));
    result[6].Should().BeEquivalentTo(new Token(TokenType.EndTag, "p"));
  }

  [Fact]
  public void Tokenize_ShouldHandleStartAndEndTags()
  {
    //Arrange
    var expectedTokens = new List<Token>
    {
      new Token(TokenType.StartTag, TagNames.Div),
      new Token(TokenType.EndTag, TagNames.Div),
    };
    var tokenizer = new HtmlTokenizer();
    tokenizer.Reset("<div></div>");

    //Act
    var result = new Token?[2];
    result[0] = tokenizer.GetNextToken();
    result[1] = tokenizer.GetNextToken();

    //Assert
    result.Should().BeEquivalentTo(expectedTokens);
  }

  [Fact]
  public void Tokenize_ShouldReturnSelfClosingTagToken_WithAttributes()
  {
    //Arrange
    var attributes = new Dictionary<string, string>
    {
      { "src", "image.jpg" }
    };
    var expectedToken = new Token(TokenType.SelfClosingTag, TagNames.Image, attributes.ToArray());
    var tokenizer = new HtmlTokenizer();
    tokenizer.Reset("<img src=\"image.jpg\" />");

    //Act
    var result = tokenizer.GetNextToken();

    //Assert
    result.Should().BeEquivalentTo(expectedToken);
  }

  [Fact]
  public void Tokenizer_ShouldIgnoreEmptyTextNodes()
  {
    //Arrange
    var tokenizer = new HtmlTokenizer();
    tokenizer.Reset("    ");

    //Act
    var result = tokenizer.GetNextToken();

    //Assert
    result.Should().BeNull();
  }

  [Fact]
  public void Tokenizer_ShouldHandleNestedTags()
  {
    //Arrange
    var tokenizer = new HtmlTokenizer();
    tokenizer.Reset("<div><p>Hello</p></div>");

    //Act
    var result = new Token?[5];
    result[0] = tokenizer.GetNextToken();
    result[1] = tokenizer.GetNextToken();
    result[2] = tokenizer.GetNextToken();
    result[3] = tokenizer.GetNextToken();
    result[4] = tokenizer.GetNextToken();

    //Assert
    result[0].Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.Div));
    result[1].Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.Paragraph));
    result[2].Should().BeEquivalentTo(new Token(TokenType.Text, "Hello"));
    result[3].Should().BeEquivalentTo(new Token(TokenType.EndTag, TagNames.Paragraph));
    result[4].Should().BeEquivalentTo(new Token(TokenType.EndTag, TagNames.Div));
  }

  [Fact]
  public void Tokenizer_ShouldHandleTagWithOneAttribute()
  {
    //Arrange
    var tokenizer = new HtmlTokenizer();
    tokenizer.Reset("<div id=\"main\">");

    //Act
    var result = tokenizer.GetNextToken();

    //Assert
    result.Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.Div), options => options.Excluding(t => t.Attributes));
    result!.Attributes.Should().ContainKey("id").WhoseValue.Should().Be("main");
  }

  [Fact]
  public void Tokenizer_ShouldHandleTagWithMultipleAttributes()
  {
    //Arrange
    var tokenizer = new HtmlTokenizer();
    tokenizer.Reset("<img src=\"image.jpg\" style=\"width: 100px\" />");

    //Act
    var result = tokenizer.GetNextToken();

    //Assert
    result.Should().BeEquivalentTo(new Token(TokenType.SelfClosingTag, TagNames.Image), options => options.Excluding(t => t.Attributes));
    result!.Attributes.Should().ContainKey("src").WhoseValue.Should().Be("image.jpg");
    result.Attributes.Should().ContainKey("style").WhoseValue.Should().Be("width: 100px");
  }

  [Fact]
  public void Tokenizer_ShouldIgnoreAttributeWithoutValue()
  {
    //Arrange
    var tokenizer = new HtmlTokenizer();
    tokenizer.Reset("<div id>");

    //Act
    var result = tokenizer.GetNextToken();

    //Assert
    result.Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.Div), options => options.Excluding(t => t.Attributes));
    result!.Attributes.Should().BeEmpty();
  }

  [Fact]
  public void Tokenizer_ShouldHandleAttributeWithoutQuotes()
  {
    //Arrange
    var tokenizer = new HtmlTokenizer();
    tokenizer.Reset("<div id=main>");

    //Act
    var result = tokenizer.GetNextToken();

    //Assert
    result.Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.Div), options => options.Excluding(t => t.Attributes));
    result!.Attributes.Should().ContainKey("id").WhoseValue.Should().Be("main");
  }

  [Fact]
  public void Tokenizer_ShouldHandlePlainTextBetweenTags()
  {
    //Arrange
    var tokenizer = new HtmlTokenizer();
    tokenizer.Reset("<div>Hello</div>");

    //Act
    var result = new Token?[3];
    result[0] = tokenizer.GetNextToken();
    result[1] = tokenizer.GetNextToken();
    result[2] = tokenizer.GetNextToken();

    //Assert
    result[0].Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.Div));
    result[1].Should().BeEquivalentTo(new Token(TokenType.Text, "Hello"));
    result[2].Should().BeEquivalentTo(new Token(TokenType.EndTag, TagNames.Div));
  }

  [Fact]
  public void Tokenizer_ShouldHandleTextWithSpaces()
  {
    //Arrange
    var tokenizer = new HtmlTokenizer();
    tokenizer.Reset("<div>   Hello World   </div>");

    //Act
    var result = new Token?[3];
    result[0]= tokenizer.GetNextToken();
    result[1] = tokenizer.GetNextToken();
    result[2] = tokenizer.GetNextToken();

    //Assert
    result[0].Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.Div));
    result[1].Should().BeEquivalentTo(new Token(TokenType.Text, "   Hello World   "));
    result[2].Should().BeEquivalentTo(new Token(TokenType.EndTag, TagNames.Div));
  }

  [Fact]
  public void Tokenizer_ShouldHandleSimpleComments()
  {
    //Arrange
    var tokenizer = new HtmlTokenizer();
    tokenizer.Reset("<!-- This is a comment -->");

    //Act
    var result = tokenizer.GetNextToken();

    //Assert
    result.Should().BeEquivalentTo(new Token(TokenType.Comment, "This is a comment"));
  }

  [Fact]
  public void Tokenizer_ShouldHandleCommentsInsideTags()
  {
    //Arrange
    var tokenizer = new HtmlTokenizer();
    tokenizer.Reset("<div><!-- Comment inside --></div>");

    //Act
    var result = new Token?[3];
    result[0]= tokenizer.GetNextToken();
    result[1] = tokenizer.GetNextToken();
    result[2] = tokenizer.GetNextToken();

    //Assert
    result[0].Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.Div));
    result[1].Should().BeEquivalentTo(new Token(TokenType.Comment, "Comment inside"));
    result[2].Should().BeEquivalentTo(new Token(TokenType.EndTag, TagNames.Div));
  }

  [Fact]
  public void Tokenizer_ShouldHandleSimpleSpecialCharacters()
  {
    //Arrange
    var tokenizer = new HtmlTokenizer();
    tokenizer.Reset("<p>&lt;Hello&gt;</p>");

    //Act
    var result = new Token?[3];
    result[0]= tokenizer.GetNextToken();
    result[1] = tokenizer.GetNextToken();
    result[2] = tokenizer.GetNextToken();

    //Assert
    result[0].Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.Paragraph));
    result[1].Should().BeEquivalentTo(new Token(TokenType.Text, "<Hello>"));
    result[2].Should().BeEquivalentTo(new Token(TokenType.EndTag, TagNames.Paragraph));
  }

  [Fact]
  public void Tokenizer_ShouldHandleSeveralSpecialCharacters()
  {
    //Arrange
    var tokenizer = new HtmlTokenizer();
    tokenizer.Reset("<p>&lt;&amp;&gt;</p>");

    //Act
    var result = new Token?[3];
    result[0]= tokenizer.GetNextToken();
    result[1] = tokenizer.GetNextToken();
    result[2] = tokenizer.GetNextToken();

    //Assert
    result[0].Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.Paragraph));
    result[1].Should().BeEquivalentTo(new Token(TokenType.Text, "<&>"));
    result[2].Should().BeEquivalentTo(new Token(TokenType.EndTag, TagNames.Paragraph));
  }

  [Fact]
  public void Tokenizer_ShouldIgnoreTagWithoutName()
  {
    //Arrange
    var tokenizer = new HtmlTokenizer();
    tokenizer.Reset("<>");

    //Act
    var result = tokenizer.GetNextToken();

    //Assert
    result.Should().BeNull();
  }

  [Fact]
  public void Tokenizer_ShouldHandleLargeHTMLDocument()
  {
    //Arrange
    var builder = new StringBuilder();
    for (var i = 0; i < 10_000; i++)
    {
      builder.AppendLine("<div><p>Repeated text</p></div>");
    }
    var tokenizer = new HtmlTokenizer();
    tokenizer.Reset(builder.ToString());

    //Act
    var tokens = new List<Token>(10_000);
    var sw = Stopwatch.StartNew();
    var token = tokenizer.GetNextToken();
    while (token != null)
    {
      tokens.Add(token);
      token = tokenizer.GetNextToken();
    }
    sw.Stop();

    //Assert
    sw.ElapsedMilliseconds.Should().BeLessThan(100);
  }

  [Fact]
  public void Tokenizer_ShouldHandleEmptyLine()
  {
    //Arrange
    var tokenizer = new HtmlTokenizer();

    //Act
    var result = tokenizer.GetNextToken();

    //Assert
    result.Should().BeNull();
  }

  [Fact]
  public void Tokenizer_ShouldHandleOnlyTextWithoutTags()
  {
    //Arrange
    var tokenizer = new HtmlTokenizer();
    tokenizer.Reset("Hello World");

    //Act
    var result = tokenizer.GetNextToken();

    //Assert
    result.Should().BeEquivalentTo(new Token(TokenType.Text, "Hello World"));
  }
}