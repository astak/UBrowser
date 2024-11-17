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
    var expectedToken = new Token(TokenType.StartTag, TagNames.TagNameDiv);

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
    var expectedToken = new Token(TokenType.EndTag, TagNames.TagNameDiv);

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
      new Token(TokenType.EndTag, TagNames.TagNameDiv),
      new Token(TokenType.EndTag, TagNames.TagNameSpan),
      new Token(TokenType.EndTag, TagNames.TagNameParagraph),
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
    var expectedToken = new Token(TokenType.EndTag, TagNames.TagNameDiv);
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
    result[0].Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.TagNameParagraph));
    result[1].Should().BeEquivalentTo(new Token(TokenType.Text, "Hello "));
    result[2].Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.TagNameSpan));
    result[3].Should().BeEquivalentTo(new Token(TokenType.Text, "nice"));
    result[4].Should().BeEquivalentTo(new Token(TokenType.EndTag, TagNames.TagNameSpan));
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
      new Token(TokenType.StartTag, TagNames.TagNameDiv),
      new Token(TokenType.EndTag, TagNames.TagNameDiv),
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
    var expectedToken = new Token(TokenType.SelfClosingTag, TagNames.TagNameImage, attributes.ToArray());
    var tokenizer = new HtmlTokenizer();

    //Act
    var result = tokenizer.Tokenize(inputHtml);

    //Assert
    result.Should().HaveCount(1);
    result.First().Should().BeEquivalentTo(expectedToken);
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

  [Fact]
  public void Tokenizer_ShouldHandleNestedTags()
  {
    //Arrange
    string inputHtml = "<div><p>Hello</p></div>";
    var tokenizer = new HtmlTokenizer();

    //Act
    var result = tokenizer.Tokenize(inputHtml);

    //Assert
    result.Should().HaveCount(5);
    result[0].Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.TagNameDiv));
    result[1].Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.TagNameParagraph));
    result[2].Should().BeEquivalentTo(new Token(TokenType.Text, "Hello"));
    result[3].Should().BeEquivalentTo(new Token(TokenType.EndTag, TagNames.TagNameParagraph));
    result[4].Should().BeEquivalentTo(new Token(TokenType.EndTag, TagNames.TagNameDiv));
  }

  [Fact]
  public void Tokenizer_ShouldHandleTagWithOneAttribute()
  {
    //Arrange
    string inputHtml = "<div id=\"main\">";
    var tokenizer = new HtmlTokenizer();

    //Act
    var result = tokenizer.Tokenize(inputHtml);

    //Assert
    result.Should().HaveCount(1);
    result[0].Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.TagNameDiv), options => options.Excluding(t => t.Attributes));
    result[0].Attributes.Should().ContainKey("id").WhoseValue.Should().Be("main");
  }

  [Fact]
  public void Tokenizer_ShouldHandleTagWithMultipleAttributes()
  {
    //Arrange
    string inputHtml = "<img src=\"image.jpg\" style=\"width: 100px\" />";
    var tokenizer = new HtmlTokenizer();

    //Act
    var result = tokenizer.Tokenize(inputHtml);

    //Assert
    result.Should().HaveCount(1);
    result[0].Should().BeEquivalentTo(new Token(TokenType.SelfClosingTag, TagNames.TagNameImage), options => options.Excluding(t => t.Attributes));
    result[0].Attributes.Should().ContainKey("src").WhoseValue.Should().Be("image.jpg");
    result[0].Attributes.Should().ContainKey("style").WhoseValue.Should().Be("width: 100px");
  }

  [Fact]
  public void Tokenizer_ShouldIgnoreAttributeWithoutValue()
  {
    //Arrange
    var inputHtml = "<div id>";
    var tokenizer = new HtmlTokenizer();

    //Act
    var result = tokenizer.Tokenize(inputHtml);

    //Assert
    result.Should().HaveCount(1);
    result[0].Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.TagNameDiv), options => options.Excluding(t => t.Attributes));
    result[0].Attributes.Should().BeEmpty();
  }

  [Fact]
  public void Tokenizer_ShouldHandleAttributeWithoutQuotes()
  {
    //Arrange
    var inputHtml = "<div id=main>";
    var tokenizer = new HtmlTokenizer();

    //Act
    var result = tokenizer.Tokenize(inputHtml);

    //Assert
    result.Should().HaveCount(1);
    result[0].Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.TagNameDiv), options => options.Excluding(t => t.Attributes));
    result[0].Attributes.Should().ContainKey("id").WhoseValue.Should().Be("main");
  }

  [Fact]
  public void Tokenizer_ShouldHandlePlainTextBetweenTags()
  {
    //Arrange
    var inputHtml = "<div>Hello</div>";
    var tokenizer = new HtmlTokenizer();

    //Act
    var result = tokenizer.Tokenize(inputHtml);

    //Assert
    result.Should().HaveCount(3);
    result[0].Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.TagNameDiv));
    result[1].Should().BeEquivalentTo(new Token(TokenType.Text, "Hello"));
    result[2].Should().BeEquivalentTo(new Token(TokenType.EndTag, TagNames.TagNameDiv));
  }

  [Fact]
  public void Tokenizer_ShouldHandleTextWithSpaces()
  {
    //Arrange
    string inputHtml = "<div>   Hello World   </div>";
    var tokenizer = new HtmlTokenizer();

    //Act
    var result = tokenizer.Tokenize(inputHtml);

    //Assert
    result.Should().HaveCount(3);
    result[0].Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.TagNameDiv));
    result[1].Should().BeEquivalentTo(new Token(TokenType.Text, "   Hello World   "));
    result[2].Should().BeEquivalentTo(new Token(TokenType.EndTag, TagNames.TagNameDiv));
  }

  [Fact]
  public void Tokenizer_ShouldHandleSimpleComments()
  {
    //Arrange
    var inputHtml = "<!-- This is a comment -->";
    var tokenizer = new HtmlTokenizer();

    //Act
    var result = tokenizer.Tokenize(inputHtml);

    //Assert
    result.Should().HaveCount(1);
    result[0].Should().BeEquivalentTo(new Token(TokenType.Comment, "This is a comment"));
  }
}