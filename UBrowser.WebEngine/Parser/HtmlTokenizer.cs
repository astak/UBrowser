using System.Text.RegularExpressions;

namespace UBrowser.WebEngine.Parser;

public class HtmlTokenizer
{
  public List<Token> Tokenize(string html)
  {
    var tokens = new List<Token>();
    var rawTokens = Regex.Matches(html, @"<!--.*?-->|<[^>]+>|[^<]+");
    foreach (Match rawToken in rawTokens)
    {
      var token = rawToken.Value;
      if (token.StartsWith("<!--"))
      {
        var commentToken = ParseComment(token);
        if (commentToken != null)
        {
          tokens.Add(commentToken);
        }
      }
      else if (token.StartsWith("</"))
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

  private Token? ParseComment(string rawToken)
  {
    var pattern = @"^<!--\s*(?<comment>.*?)\s*-->$";
    var match = Regex.Match(rawToken, pattern);

    if (!match.Success)
      return null;

    var commentContent = match.Groups["comment"].Value;
    return new Token(TokenType.Comment, commentContent);
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
    var index = 0;

    while (index < attributesPart.Length)
    {
      SkipWhitespace(ref index, attributesPart);

      var name = ExtractAttributeName(ref index, attributesPart);
      if (string.IsNullOrEmpty(name)) break;

      var value = ExtractAttributeValue(ref index, attributesPart);
      if (string.IsNullOrEmpty(value)) continue;

      attributes[name] = value;
    }

    return attributes.ToArray();
  }

  private void SkipWhitespace(ref int index, string input)
  {
    while (index < input.Length && char.IsWhiteSpace(input[index]))
      index++;
  }

  private string ExtractAttributeName(ref int index, string input)
  {
    var nameStart = index;

    while (index < input.Length && !char.IsWhiteSpace(input[index]) && input[index] != '=')
      index++;

    return input.Substring(nameStart, index - nameStart);
  }

  private string ExtractQuotedValue(ref int index, string input, char quoteChar)
  {
    var valueStart = ++index;
    index = input.IndexOf(quoteChar, index);
    if (index == -1)
      throw new FormatException("Unmatched quite in attribute value");
    string value = input.Substring(valueStart, index - valueStart);
    index++;
    return value;
  }

  private string ExtractUnquotedValue(ref int index, string input)
  {
    var valueStart = index;

    while (index < input.Length && !char.IsWhiteSpace(input[index]) && input[index] != '>')
      index++;

    return input.Substring(valueStart, index - valueStart);
  }

  private string? ExtractAttributeValue(ref int index, string input)
  {
    SkipWhitespace(ref index, input);

    if (index >= input.Length || input[index] != '=')
      return null;

    index++;
    SkipWhitespace(ref index, input);

    if (index >= input.Length)
      return null;

    var quoteChar = input[index];
    if (quoteChar == '"' || quoteChar == '\'')
    {
      return ExtractQuotedValue(ref index, input, quoteChar);
    }
    else
    {
      return ExtractUnquotedValue(ref index, input);
    }
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
