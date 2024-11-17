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
    var tokenizer = new HtmlTokenizer();

    //Act
    var result = tokenizer.Tokenize(inputHtml);

    //Assert
    result.Should().HaveCount(2);
    result[0].Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.Div));
    result[1].Should().BeEquivalentTo(new Token(TokenType.EndTag, TagNames.Div));
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
  public void Tokenizer_ShouldHandleCloseTagWithWhiteSpace()
  {
    //Arrange
    string inputHtml = "<div></div >";
    var tokenizer = new HtmlTokenizer();

    //Act
    var result = tokenizer.Tokenize(inputHtml);

    //Assert
    result.Should().HaveCount(2);
    result[0].Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.Div));
    result[1].Should().BeEquivalentTo(new Token(TokenType.EndTag, TagNames.Div));
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
    string inputHtml = "<div></div>";
    var expectedTokens = new List<Token>
    {
      new Token(TokenType.StartTag, TagNames.Div),
      new Token(TokenType.EndTag, TagNames.Div),
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
    var expectedToken = new Token(TokenType.SelfClosingTag, TagNames.Image, attributes.ToArray());
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
    string inputHtml = "<div id=\"main\">";
    var tokenizer = new HtmlTokenizer();

    //Act
    var result = tokenizer.Tokenize(inputHtml);

    //Assert
    result.Should().HaveCount(2);
    result[0].Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.Div), options => options.Excluding(t => t.Attributes));
    result[0].Attributes.Should().ContainKey("id").WhoseValue.Should().Be("main");
    result[1].Should().BeEquivalentTo(new Token(TokenType.EndTag, TagNames.Div));
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
    result[0].Should().BeEquivalentTo(new Token(TokenType.SelfClosingTag, TagNames.Image), options => options.Excluding(t => t.Attributes));
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
    result.Should().HaveCount(2);
    result[0].Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.Div), options => options.Excluding(t => t.Attributes));
    result[0].Attributes.Should().BeEmpty();
    result[1].Should().BeEquivalentTo(new Token(TokenType.EndTag, TagNames.Div));
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
    result.Should().HaveCount(2);
    result[0].Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.Div), options => options.Excluding(t => t.Attributes));
    result[0].Attributes.Should().ContainKey("id").WhoseValue.Should().Be("main");
    result[1].Should().BeEquivalentTo(new Token(TokenType.EndTag, TagNames.Div));
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
    result[0].Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.Div));
    result[1].Should().BeEquivalentTo(new Token(TokenType.Text, "Hello"));
    result[2].Should().BeEquivalentTo(new Token(TokenType.EndTag, TagNames.Div));
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
    result[0].Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.Div));
    result[1].Should().BeEquivalentTo(new Token(TokenType.Text, "   Hello World   "));
    result[2].Should().BeEquivalentTo(new Token(TokenType.EndTag, TagNames.Div));
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

  [Fact]
  public void Tokenizer_ShouldHandleCommentsInsideTags()
  {
    //Arrange
    var inputHtml = "<div><!-- Comment inside --></div>";
    var tokenizer = new HtmlTokenizer();

    //Act
    var result = tokenizer.Tokenize(inputHtml);

    //Assert
    result.Should().HaveCount(3);
    result[0].Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.Div));
    result[1].Should().BeEquivalentTo(new Token(TokenType.Comment, "Comment inside"));
    result[2].Should().BeEquivalentTo(new Token(TokenType.EndTag, TagNames.Div));
  }

  [Fact]
  public void Tokenizer_ShouldHandleSimpleSpecialCharacters()
  {
    //Arrange
    var inputHtml = "<p>&lt;Hello&gt;</p>";
    var tokenizer = new HtmlTokenizer();

    //Act
    var result = tokenizer.Tokenize(inputHtml);

    //Assert
    result.Should().HaveCount(3);
    result[0].Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.Paragraph));
    result[1].Should().BeEquivalentTo(new Token(TokenType.Text, "<Hello>"));
    result[2].Should().BeEquivalentTo(new Token(TokenType.EndTag, TagNames.Paragraph));
  }

  [Fact]
  public void Tokenizer_ShouldHandleSeveralSpecialCharacters()
  {
    //Arrange
    var inputHtml = "<p>&lt;&amp;&gt;</p>";
    var tokenizer = new HtmlTokenizer();

    //Act
    var result = tokenizer.Tokenize(inputHtml);

    //Assert
    result.Should().HaveCount(3);
    result[0].Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.Paragraph));
    result[1].Should().BeEquivalentTo(new Token(TokenType.Text, "<&>"));
    result[2].Should().BeEquivalentTo(new Token(TokenType.EndTag, TagNames.Paragraph));
  }

  [Fact]
  public void Tokenizer_ShouldAutomaticallyRestoreUnclosedTags()
  {
    //Arrange
    var inputHtml = "<div><p>Text</div>";
    var tokenizer = new HtmlTokenizer();

    //Act
    var result = tokenizer.Tokenize(inputHtml);

    //Assert
    result.Should().HaveCount(5);
    result[0].Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.Div));
    result[1].Should().BeEquivalentTo(new Token(TokenType.StartTag, TagNames.Paragraph));
    result[2].Should().BeEquivalentTo(new Token(TokenType.Text, "Text"));
    result[3].Should().BeEquivalentTo(new Token(TokenType.EndTag, TagNames.Paragraph));
    result[4].Should().BeEquivalentTo(new Token(TokenType.EndTag, TagNames.Div));
  }

  [Fact]
  public void Tokenizer_ShouldIgnoreClosingTagsWithoutOpeningOnes()
  {
    //Arrange
    var inputHtml = "</div>";
    var tokenizer = new HtmlTokenizer();

    //Act
    var result = tokenizer.Tokenize(inputHtml);

    //Assert
    result.Should().BeEmpty();
  }

  [Fact]
  public void Tokenizer_ShouldIgnoreTagWithoutName()
  {
    //Arrange
    var inputHtml = "<>";
    var tokenizer = new HtmlTokenizer();

    //Act
    var result = tokenizer.Tokenize(inputHtml);

    //Assert
    result.Should().BeEmpty();
  }
}