using UBrowser.WebEngine.DOM;

namespace UBrowser.WebEngine.Parser;

public class HtmlParser
{
  private readonly HtmlTokenizer _tokenizer = new HtmlTokenizer();
  private DOMNode? _currentNode;

  public DOMNode Parse(string html)
  {
    _tokenizer.Reset(html);

    var domTree = new DOMNode(SpecialNodeNames.Root);
    _currentNode = domTree;

    Token? token;
    while ((token = _tokenizer.GetNextToken()) != null)
    {
      switch (token.Type)
      {
        case TokenType.StartTag:
          HandleStartTag(token);
          break;
        case TokenType.EndTag:
          HandleEndTag(token);
          break;
        case TokenType.SelfClosingTag:
          HandleSelfClosingTag(token);
          break;
        case TokenType.Text:
          HandleText(token);
          break;
      }
    }

    return domTree;
  }

  private void HandleStartTag(Token token)
  {
    var elementNode = new DOMNode(token.Name)
    {
      Attributes = token.Attributes
    };

    _currentNode?.AddChild(elementNode);
    _currentNode = elementNode;
  }

  private void HandleEndTag(Token token)
  {
    if (_currentNode?.TagName == token.Name)
    {
      _currentNode = _currentNode.Parent ?? _currentNode;
    }
  }

  private void HandleSelfClosingTag(Token token)
  {
    var selfClosingNode = new DOMNode(token.Name)
    {
      Attributes = token.Attributes
    };

    _currentNode?.AddChild(selfClosingNode);
  }

  private void HandleText(Token token)
  {
    var textNode = new DOMNode(SpecialNodeNames.Text)
    {
      InnerText = token.Name
    };

    _currentNode?.AddChild(textNode);
  }
}
