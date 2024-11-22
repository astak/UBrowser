using FluentAssertions;
using UBrowser.WebEngine.DOM;
using UBrowser.WebEngine.Parser;

namespace UBrowser.WebEngine.Tests;

public class HtmlParserTests
{
  [Fact]
  public void Parse_ShouldHandleSimpleDocument()
  {
    //Arrange
    string inputHtml = "<div>Hello</div>";
    var parser = new HtmlParser();

    //Act
    var domTree = parser.Parse(inputHtml);

    //Assert
    domTree.Should().NotBeNull();
    var root = domTree.Root;
    root.Should().NotBeNull();
    root.TagName.Should().Be(SpecialNodeNames.Root);

    root.Children.Should().HaveCount(1);
    var divNode = root.Children[0];
    divNode.Should().NotBeNull();
    divNode.TagName.Should().Be(TagNames.Div);

    divNode.Children.Should().HaveCount(1);
    var textNode = divNode.Children[0];
    textNode.Should().NotBeNull();
    textNode.InnerText.Should().Be("Hello");
  }

  [Fact]
  public void Parse_ShouldHandleComplexNestedStructures()
  {
    //Arrange
    string inputHtml = "<div><p><span>Text</span></p></div>";
    var parser = new HtmlParser();

    //Act
    var domTree = parser.Parse(inputHtml);

    //Assert
    domTree.Should().NotBeNull();
    var root = domTree.Root;
    root.Should().NotBeNull();
    root.TagName.Should().Be(SpecialNodeNames.Root);

    root.Children.Should().HaveCount(1);
    var divNode = root.Children[0];
    divNode.Should().NotBeNull();
    divNode.TagName.Should().Be(TagNames.Div);

    divNode.Children.Should().HaveCount(1);
    var pNode = divNode.Children[0];
    pNode.Should().NotBeNull();
    pNode.TagName.Should().Be(TagNames.Paragraph);

    pNode.Children.Should().HaveCount(1);
    var span = pNode.Children[0];
    span.Should().NotBeNull();
    span.TagName.Should().Be(TagNames.Span);

    span.Children.Should().HaveCount(1);
    var textNode = span.Children[0];
    textNode.Should().NotBeNull();
    textNode.InnerText.Should().Be("Text");
  }

  [Fact]
  public void Parse_ShouldHandleSelfClosingTags()
  {
    //Arrange
    string inputHtml = "<div><img src=\"image.jpg\" /></div>";
    var parser = new HtmlParser();

    //Act
    var domTree = parser.Parse(inputHtml);

    //Assert
    domTree.Should().NotBeNull();
    var root = domTree.Root;
    root.Should().NotBeNull();
    root.TagName.Should().Be(SpecialNodeNames.Root);

    root.Children.Should().HaveCount(1);
    var divNode = root.Children[0];
    divNode.Should().NotBeNull();
    divNode.TagName.Should().Be(TagNames.Div);

    divNode.Children.Should().HaveCount(1);
    var imgNode = divNode.Children[0];
    imgNode.Should().NotBeNull();
    imgNode.TagName.Should().Be(TagNames.Image);
    imgNode.Attributes.Should().ContainKey("src")
      .WhoseValue.Should().Be("image.jpg");

    imgNode.Children.Should().BeEmpty();
  }

  [Fact]
  public void Parse_ShouldHandleTextNodesWithSpaces()
  {
    //Arrange
    string inputHtml = "<div> Text with <b>bold</b> content </div>";
    var parser = new HtmlParser();

    //Act
    var treeNode = parser.Parse(inputHtml);

    //Assert
    treeNode.Should().NotBeNull();
    var root = treeNode.Root;
    root.Should().NotBeNull();
    root.TagName.Should().Be(SpecialNodeNames.Root);
    root.Children.Should().HaveCount(1);

    var divNode = root.Children[0];
    divNode.Should().NotBeNull();
    divNode.TagName.Should().Be(TagNames.Div);
    divNode.Children.Should().HaveCount(3);

    var firstTextNode = divNode.Children[0];
    firstTextNode.Should().NotBeNull();
    firstTextNode.TagName.Should().Be(SpecialNodeNames.Text);
    firstTextNode.InnerText.Should().Be(" Text with ");

    var secondTextNode = divNode.Children[2];
    secondTextNode.Should().NotBeNull();
    secondTextNode.TagName.Should().Be(SpecialNodeNames.Text);
    secondTextNode.InnerText.Should().Be(" content ");
  }

  [Fact]
  public void Parse_ShouldHandleErroneousDocuments()
  {
    //Arrange
    string inputHtml = "<div><span></div>";
    var parser = new HtmlParser();

    //Act
    var treeNode = parser.Parse(inputHtml);

    //Assert
    treeNode.Should().NotBeNull();
    var root = treeNode.Root;
    root.Should().NotBeNull();
    root.TagName.Should().Be(SpecialNodeNames.Root);
    root.Children.Should().HaveCount(1);

    var divNode = root.Children[0];
    divNode.Should().NotBeNull();
    divNode.TagName.Should().Be(TagNames.Div);
    divNode.Children.Should().HaveCount(1);

    var spanNode = divNode.Children[0];
    spanNode.Should().NotBeNull();
    spanNode.TagName.Should().Be(TagNames.Span);
    spanNode.Children.Should().BeEmpty();
  }

  [Fact]
  public void Parse_ShouldHandleErroneousDocuments_2()
  {
    //Arrange
    string inputHtml = "<div><span>Text";
    var parser = new HtmlParser();

    //Act
    var treeNode = parser.Parse(inputHtml);

    //Assert
    treeNode.Should().NotBeNull();
    var root = treeNode.Root;
    root.Should().NotBeNull();
    root.TagName.Should().Be(SpecialNodeNames.Root);
    root.Children.Should().HaveCount(1);

    var divNode = root.Children[0];
    divNode.Should().NotBeNull();
    divNode.TagName.Should().Be(TagNames.Div);
    divNode.Children.Should().HaveCount(1);

    var spanNode = divNode.Children[0];
    spanNode.Should().NotBeNull();
    spanNode.TagName.Should().Be(TagNames.Span);
    spanNode.Children.Should().HaveCount(1);

    var textNode = spanNode.Children[0];
    textNode.Should().NotBeNull();
    textNode.TagName.Should().Be(SpecialNodeNames.Text);
    textNode.InnerText.Should().Be("Text");
    textNode.Children.Should().BeEmpty();
  }

  [Fact]
  public void Parser_ShouldHandleHtmlWithoutRootTag()
  {
    //Arrange
    var inputHtml = "Hello <b>world</b>";
    var parser = new HtmlParser();

    //Act
    var treeNode = parser.Parse(inputHtml);

    //Assert
    treeNode.Should().NotBeNull();
    var root = treeNode.Root;
    root.Should().NotBeNull();
    root.TagName.Should().Be(SpecialNodeNames.Root);
    root.Children.Should().HaveCount(2);

    var helloTextNode = root.Children[0];
    helloTextNode.Should().NotBeNull();
    helloTextNode.TagName.Should().Be(SpecialNodeNames.Text);
    helloTextNode.InnerText.Should().Be("Hello ");
    helloTextNode.Children.Should().BeEmpty();

    var boldNode = root.Children[1];
    boldNode.Should().NotBeNull();
    boldNode.TagName.Should().Be(TagNames.Bold);
    boldNode.Children.Should().HaveCount(1);

    var worldTextNode = boldNode.Children[0];
    worldTextNode.Should().NotBeNull();
    worldTextNode.TagName.Should().Be(SpecialNodeNames.Text);
    worldTextNode.InnerText.Should().Be("world");
    worldTextNode.Children.Should().BeEmpty();
  }

  [Fact]
  public void Parser_ShouldHandleNestedAttributes()
  {
    //Arrange
    var inputHtml = "<div style=\"color:red; font-size:14px;\"><p class=\"intro\">Hello</p></div>";
    var parser = new HtmlParser();

    //Act
    var treeNode = parser.Parse(inputHtml);

    //Assert
    treeNode.Should().NotBeNull();
    var root = treeNode.Root;
    root.Should().NotBeNull();
    root.TagName.Should().Be(SpecialNodeNames.Root);
    root.Children.Should().HaveCount(1);

    var divNode = root.Children[0];
    divNode.Should().NotBeNull();
    divNode.TagName.Should().Be(TagNames.Div);
    divNode.Attributes.Should().ContainKey("style")
      .WhoseValue.Should().Be("color:red; font-size:14px;");
    divNode.Children.Should().HaveCount(1);

    var pNode = divNode.Children[0];
    pNode.Should().NotBeNull();
    pNode.TagName.Should().Be(TagNames.Paragraph);
    pNode.Attributes.Should().ContainKey("class")
      .WhoseValue.Should().Be("intro");
    pNode.Children.Should().HaveCount(1);

    var textNode = pNode.Children[0];
    textNode.Should().NotBeNull();
    textNode.TagName.Should().Be(SpecialNodeNames.Text);
    textNode.InnerText.Should().Be("Hello");
    textNode.Children.Should().HaveCount(0);
  }

  [Fact]
  public void Parser_ShouldHandleHtmlWithComments()
  {
    //Arrange
    var inputHtml = "<div><!-- This is a comment --><p>Content</p></div>";
    var parser = new HtmlParser();

    //Act
    var treeNode = parser.Parse(inputHtml);

    //Assert
    treeNode.Should().NotBeNull();
    var root = treeNode.Root;
    root.Should().NotBeNull();
    root.TagName.Should().Be(SpecialNodeNames.Root);
    root.Children.Should().HaveCount(1);

    var divNode = root.Children[0];
    divNode.Should().NotBeNull();
    divNode.TagName.Should().Be(TagNames.Div);
    divNode.Children.Should().HaveCount(1);

    var pNode = divNode.Children[0];
    pNode.Should().NotBeNull();
    pNode.TagName.Should().Be(TagNames.Paragraph);
    pNode.Children.Should().HaveCount(1);

    var textNode = pNode.Children[0];
    textNode.Should().NotBeNull();
    textNode.TagName.Should().Be(SpecialNodeNames.Text);
    textNode.InnerText.Should().Be("Content");
    textNode.Children.Should().BeEmpty();
  }

  [Fact]
  public void Parser_ShouldHandleEmptyLine()
  {
    //Arrange
    var inputHtml = string.Empty;
    var parser = new HtmlParser();

    //Act
    var treeNode = parser.Parse(inputHtml);

    //Assert
    treeNode.Should().NotBeNull();
    var root = treeNode.Root;
    root.Should().NotBeNull();
    root.TagName.Should().Be(SpecialNodeNames.Root);
    root.Children.Should().BeEmpty();
  }

  [Fact]
  public void Parser_ShouldIgnoreInvalidTags()
  {
    //Arrange
    var inputHtml = "<div><></div>";
    var parser = new HtmlParser();

    //Act
    var treeNode = parser.Parse(inputHtml);

    //Assert
    treeNode.Should().NotBeNull();
    var root = treeNode.Root;
    root.Should().NotBeNull();
    root.TagName.Should().Be(SpecialNodeNames.Root);
    root.Children.Should().HaveCount(1);

    var divNode = root.Children[0];
    divNode.Should().NotBeNull();
    divNode.TagName.Should().Be(TagNames.Div);
    divNode.Children.Should().BeEmpty();
  }

  [Fact]
  public void Parser_ShouldHandleTextOutsideTags()
  {
    //Arrange
    var inputHtml = "Text <div>Inside</div> More text";
    var parser = new HtmlParser();

    //Act
    var treeNode = parser.Parse(inputHtml);

    //Assert
    treeNode.Should().NotBeNull();
    var root = treeNode.Root;
    root.Should().NotBeNull();
    root.TagName.Should().Be(SpecialNodeNames.Root);
    root.Children.Should().HaveCount(3);

    var textNode = root.Children[0];
    textNode.Should().NotBeNull();
    textNode.TagName.Should().Be(SpecialNodeNames.Text);
    textNode.InnerText.Should().Be("Text ");
    textNode.Children.Should().BeEmpty();

    var divNode = root.Children[1];
    divNode.Should().NotBeNull();
    divNode.TagName.Should().Be(TagNames.Div);
    divNode.Children.Should().HaveCount(1);

    var insideNode = divNode.Children[0];
    insideNode.Should().NotBeNull();
    insideNode.TagName.Should().Be(SpecialNodeNames.Text);
    insideNode.InnerText.Should().Be("Inside");
    insideNode.Children.Should().BeEmpty();

    var moreTextNode = root.Children[2];
    moreTextNode.Should().NotBeNull();
    moreTextNode.TagName.Should().Be(SpecialNodeNames.Text);
    moreTextNode.InnerText.Should().Be(" More text");
    moreTextNode.Children.Should().BeEmpty();
  }

  [Fact]
  public void Parser_ShouldHandleErrorsAndRecoverTags()
  {
    //Arrange
    var inputHtml = "<div><span></div>";
    var parser = new HtmlParser();

    //Act
    var treeNode = parser.Parse(inputHtml);

    //Assert
    treeNode.Should().NotBeNull();
    var root = treeNode.Root;
    root.Should().NotBeNull();
    root.TagName.Should().Be(SpecialNodeNames.Root);
    root.Children.Should().HaveCount(1);

    var divNode = root.Children[0];
    divNode.Should().NotBeNull();
    divNode.TagName.Should().Be(TagNames.Div);
    divNode.Children.Should().HaveCount(1);

    var spanNode = divNode.Children[0];
    spanNode.Should().NotBeNull();
    spanNode.TagName.Should().Be(TagNames.Span);
    spanNode.Children.Should().BeEmpty();
  }
}
