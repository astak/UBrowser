
using System.Text.RegularExpressions;

namespace UBrowser.WebEngine.Parser;

public class HtmlTokenizer
{
  public List<Token>Tokenize(string html)
  {
    var tokens = new List<Token>();
    var closeTagPattern = @"<(?<isClose>/)?(?<tag>[a-zA-Z0-9]+)(?<attributes>[^>]*?)(?<isSelfClosing>/)?>";
    var match = Regex.Match(html, closeTagPattern);

    while (match.Success)
    {
      var isClose = match.Groups["isClose"].Success;
      var isSelfClosing = match.Groups["isSelfClosing"].Success;
      var name = match.Groups["tag"].Value;
      var attributesPart = match.Groups["attributes"].Value.Trim();

      if (!string.IsNullOrWhiteSpace(name))
      {
        if (isClose)
        {
          tokens.Add(new Token(TokenType.EndTag, name));
        }
        else if (isSelfClosing)
        {
          tokens.Add(new Token(TokenType.SelfClosingTag, name, ParseAttributes(attributesPart)));
        }
        else
        {
          tokens.Add(new Token(TokenType.StartTag, name, ParseAttributes(attributesPart)));
        }
      }
      match = match.NextMatch();
    }

    return tokens;
  }

  private KeyValuePair<string, string>[] ParseAttributes(string attributesPart)
  {
    var attributes = new Dictionary<string, string>();
    if (string.IsNullOrWhiteSpace(attributesPart))
      return attributes.ToArray();

    var attributePattern = @"(?<name>[a-zA-Z0-9\-]+)\s*=\s*""(?<value>[^""]*)""";
    var matches = Regex.Matches(attributesPart, attributePattern);

    foreach (Match match in matches)
    {
      var name = match.Groups["name"].Value;
      var value = match.Groups["value"].Value;

      if (!string.IsNullOrWhiteSpace(name))
      {
        attributes[name] = value;
      }
    }

    return attributes.ToArray();
  }

  internal Token GetNextToken()
  {
    throw new NotImplementedException();
  }

  internal void Reset(string html)
  {
    throw new NotImplementedException();
  }
}
