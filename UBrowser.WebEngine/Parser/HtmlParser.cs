using UBrowser.WebEngine.DOM;
using UBrowser.WebEngine.Parser;

namespace UBrowser.WebEngine.HTMLParser;

public class HtmlParser
{
  private readonly HtmlTokenizer tokenizer = new HtmlTokenizer();
  private DOMTree domTree = new DOMTree(); 

  public DOMTree Parse(string html)
  {
    tokenizer.Reset(html);
    domTree = new DOMTree();

    Token token;
    while ((token = tokenizer.GetNextToken()) != null)
    {
      switch (token.Type)
      {
        case TokenType.StartTag:
          HandleStartTag(token.Name, token.Attributes);
          break;
        case TokenType.EndTag:
          HandleEndTag(token.Name);
          break;
        case TokenType.Text:
          HandleText(token.Attributes["data"]);
          break;
      }
    }

    return domTree;
  }

  private void HandleStartTag(string tagName, Dictionary<string, string> attributes)
  {
    var node = new DOMNode(tagName)
    {
      Attributes = attributes
    };
    domTree.AppendChild(node);
  }

  private void HandleEndTag(string tagName)
  {
    domTree.CloseCurrenNode();
  }

  private void HandleText(string text)
  {
    var textNode = new DOMNode("#text") { InnerText = text };
    domTree.AppendChild(textNode);
  }
}
