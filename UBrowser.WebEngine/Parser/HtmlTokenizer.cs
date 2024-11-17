
using System.Text.RegularExpressions;

namespace UBrowser.WebEngine.Parser;

public class HtmlTokenizer
{
  public List<Token> Tokenize(string html)
  {
    var tokens = new List<Token>();
    var rawTokens = Regex.Matches(html, @"<[^>]+>|[^<]+");
    foreach (Match rawToken in rawTokens)
    {
      var token = rawToken.Value;
      if (token.StartsWith("</"))
      {
        var endTagToken = ParseEndTag(token);
        if (endTagToken != null)
        {
          tokens.Add(endTagToken);
        }
      }
      else if (token.StartsWith("<"))
      {
        var startTagToken = ParseStartOrSelfClosingTag(token);
        if (startTagToken != null)
        {
          tokens.Add(startTagToken);
        }
      }
      else
      {
        var trimmedText = token.Trim();
        if (!string.IsNullOrEmpty(trimmedText))
        {
          tokens.Add(new Token(TokenType.Text, token));
        }
      }
    }

    return tokens;
  }

  private Token? ParseStartOrSelfClosingTag(string rawToken)
  {
    var pattern = @"<(?<tag>[a-zA-Z0-9]+)(?<attributes>[^>]*?)(?<isSelfClosing>/)?>";
    var match = Regex.Match(rawToken, pattern);

    if (!match.Success)
      return null;

    var isSelfClosing = match.Groups["isSelfClosing"].Success;
    var tagName = match.Groups["tag"].Value;
    var attributesPart = match.Groups["attributes"].Value.Trim();

    if (isSelfClosing)
    {
      return new Token(TokenType.SelfClosingTag, tagName, ParseAttributes(attributesPart));
    }
    else
    {
      return new Token(TokenType.StartTag, tagName, ParseAttributes(attributesPart));
    }
  }

  private Token? ParseEndTag(string rawToken)
  {
    var closeTagPattern = @"^<\/(?<tag>[a-zA-Z0-9]+)\s*>$";
    var match = Regex.Match(rawToken, closeTagPattern);

    if (!match.Success)
      return null;

    var tagName = match.Groups["tag"].Value;
    return new Token(TokenType.EndTag, tagName);
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
