using FluentAssertions;
using UBrowser.WebEngine.Parser;

namespace UBrowser.WebEngine.Tests;

public class HtmlTokenizerTests
{
  [Fact]
  public void Tokenize_ShouldReturnStartTagToken_ForSimpleOpenTag()
  {
    //Arrange
    string inputHtml = "<div>";
    var expectedToken = new Token(TokenType.StartTag, Token.TagNameDiv);

    var tokenizer = new HtmlTokenizer();

    //Act
    var result = tokenizer.Tokenize(inputHtml);

    //Assert
    result.Should().HaveCount(1);
    result.First().Should().BeEquivalentTo(expectedToken);
  }

  [Fact]
  public void Tokenize_ShouldReturnEndTagToken_ForSimpleCloseTag()
  {
    //Arrange
    string inputHtml = "</div>";
    var expectedToken = new Token(TokenType.EndTag, Token.TagNameDiv);

    var tokenizer = new HtmlTokenizer();

    //Act
    var result = tokenizer.Tokenize(inputHtml);

    //Assert
    result.Should().HaveCount(1);
    result.First().Should().BeEquivalentTo(expectedToken);
  }

  [Fact]
  public void Tokenize_ShouldIgnoreInvalidCloseTag()
  {
    //Arrange
    string inputHtml = "</>";
    var tokenizer = new HtmlTokenizer();

    //Act
    var result = tokenizer.Tokenize(inputHtml);

    //Assert
    result.Should().BeEmpty();
  }

  [Fact]
  public void Tokenize_ShouldHandleMultipleCloseTags()
  {
    //Arrange
    string inputHtml = "</div></span></p>";
    var expectedTokens = new List<Token>
    {
      new Token(TokenType.EndTag, Token.TagNameDiv),
      new Token(TokenType.EndTag, Token.TagNameSpan),
      new Token(TokenType.EndTag, Token.TagNameParagraph),
    };
    var tokenizer = new HtmlTokenizer();

    //Act
    var result = tokenizer.Tokenize(inputHtml);

    //Assert
    result.Should().BeEquivalentTo(expectedTokens);
  }

  [Fact]
  public void Tokenizer_ShouldHandleCloseTagWithWhiteSpace()
  {
    //Arrange
    string inputHtml = "</div >";
    var expectedToken = new Token(TokenType.EndTag, Token.TagNameDiv);
    var tokenizer = new HtmlTokenizer();

    //Act
    var result = tokenizer.Tokenize(inputHtml);

    //Assert
    result.Should().HaveCount(1);
    result.First().Should().BeEquivalentTo(expectedToken);
  }

  [Fact]
  public void Tokenizer_ShouldPreserveTextNodesBetweenTags()
  {
    //Arrange
    string inputHtml = "<p>Hello <span>nice</span> world</p>";
    var tokenizer = new HtmlTokenizer();

    //Act
    var result = tokenizer.Tokenize(inputHtml);

    //Assert
    result.Should().HaveCount(7);
    result[0].Should().BeEquivalentTo(new Token(TokenType.StartTag, Token.TagNameParagraph));
    result[1].Should().BeEquivalentTo(new Token(TokenType.Text, "Hello "));
    result[2].Should().BeEquivalentTo(new Token(TokenType.StartTag, Token.TagNameSpan));
    result[3].Should().BeEquivalentTo(new Token(TokenType.Text, "nice"));
    result[4].Should().BeEquivalentTo(new Token(TokenType.EndTag, Token.TagNameSpan));
    result[5].Should().BeEquivalentTo(new Token(TokenType.Text, " world"));
    result[6].Should().BeEquivalentTo(new Token(TokenType.EndTag, "p"));
  }

  [Fact]
  public void Tokenize_ShouldHandleStartAndEndTags()
  {
    //Arrange
    string inputHtml = "<div></div>";
    var expectedTokens = new List<Token>
    {
      new Token(TokenType.StartTag, Token.TagNameDiv),
      new Token(TokenType.EndTag, Token.TagNameDiv),
    };
    var tokenizer = new HtmlTokenizer();

    //Act
    var result = tokenizer.Tokenize(inputHtml);

    //Assert
    result.Should().HaveCount(2);
    result.Should().BeEquivalentTo(expectedTokens);
  }

  [Fact]
  public void Tokenize_ShouldReturnSelfClosingTagToken_WithAttributes()
  {
    //Arrange
    string inputHtml = "<img src=\"image.jpg\" />";
    var attributes = new Dictionary<string, string>
    {
      { "src", "image.jpg" }
    };
    var expectedToken = new Token(TokenType.SelfClosingTag, Token.TagNameImage, attributes.ToArray());
    var tokenizer = new HtmlTokenizer();

    //Act
    var result = tokenizer.Tokenize(inputHtml);

    //Assert
    result.Should().HaveCount(1);
    result.First().Should().BeEquivalentTo(expectedToken);
  }

  [Fact]
  public void Tokenizer_ShouldHandleTextInsideTags()
  {
    //Arrange
    string inputHtml = "<p>Hello World</p>";
    var tokenizer = new HtmlTokenizer();

    //Act
    var result = tokenizer.Tokenize(inputHtml);

    //Assert
    result.Should().HaveCount(3);
    result[0].Should().BeEquivalentTo(new Token(TokenType.StartTag, Token.TagNameParagraph));
    result[1].Should().BeEquivalentTo(new Token(TokenType.Text, "Hello World"));
    result[2].Should().BeEquivalentTo(new Token(TokenType.EndTag, Token.TagNameParagraph));
  }

  [Fact]
  public void Tokenizer_ShouldIgnoreEmptyTextNodes()
  {
    //Arrange
    string inputHtml = "    ";
    var tokenizer = new HtmlTokenizer();

    //Act
    var result = tokenizer.Tokenize(inputHtml);

    //Assert
    result.Should().BeEmpty();
  }
}