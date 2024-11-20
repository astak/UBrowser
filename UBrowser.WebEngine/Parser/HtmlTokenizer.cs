using System.Net;

namespace UBrowser.WebEngine.Parser;

public class HtmlTokenizer
{
  private string _html = string.Empty;
  private int _currentIndex;

  public void Reset(string html)
  {
    _html = html;
    _currentIndex = 0;
  }

  public Token? GetNextToken()
  {
    if (_currentIndex >= _html.Length)
      return null;

    if (_html[_currentIndex] == '<')
    {
      if (_currentIndex + 4 <= _html.Length && _html.Substring(_currentIndex, 4) == "<!--")
        return ParseComment();

      if (_currentIndex + 2 <= _html.Length && _html[_currentIndex + 1] == '/')
        return ParseEndTag();

      return ParseStartOrSelfClosingTag();
    }

    return ParseTextNode();
  }

  private Token? ParseComment()
  {
    int commentEnd = _html.IndexOf("-->", _currentIndex + 4);
    if (commentEnd != -1)
    {
      string commentContent = _html.Substring(_currentIndex, commentEnd + 3 - _currentIndex);
      _currentIndex = commentEnd + 3;
      return new Token(TokenType.Comment, commentContent.Substring(4, commentContent.Length - 7).Trim());
    }

    // Пропускаем остаток строки, если комментарий не закрыт
    _currentIndex = _html.Length;
    return null;
  }

  private Token? ParseStartOrSelfClosingTag()
  {
    int tagEndIndex = _html.IndexOf('>', _currentIndex + 1);
    if (tagEndIndex != -1)
    {
      string tagContent = _html.Substring(_currentIndex, tagEndIndex + 1 - _currentIndex);
      _currentIndex = tagEndIndex + 1;

      var trimmed = tagContent.Trim('<', '>').Trim();
      if (string.IsNullOrEmpty(trimmed)) return null;

      var isSelfClosing = trimmed.EndsWith("/");
      if (isSelfClosing)
        trimmed = trimmed.TrimEnd('/');

      var spaceIndex = trimmed.IndexOf(' ');
      var tagName = spaceIndex >= 0 ? trimmed.Substring(0, spaceIndex) : trimmed;
      if (string.IsNullOrEmpty(tagName)) return null;

      var attributesPart = spaceIndex >= 0 ? trimmed.Substring(spaceIndex + 1) : string.Empty;

      return isSelfClosing
          ? new Token(TokenType.SelfClosingTag, tagName, ParseAttributes(attributesPart))
          : new Token(TokenType.StartTag, tagName, ParseAttributes(attributesPart));
    }

    // Пропускаем остаток строки, если тег не закрыт
    _currentIndex = _html.Length;
    return null;
  }

  private Token? ParseEndTag()
  {
    int tagEnd = _html.IndexOf('>', _currentIndex + 2);
    if (tagEnd != -1)
    {
      string endTagContent = _html.Substring(_currentIndex, tagEnd + 1 - _currentIndex);
      _currentIndex = tagEnd + 1;

      var tagName = endTagContent.Substring(2, endTagContent.Length - 3).Trim();
      return string.IsNullOrEmpty(tagName) ? null : new Token(TokenType.EndTag, tagName);
    }

    // Пропускаем остаток строки, если тег не закрыт
    _currentIndex = _html.Length;
    return null;
  }

  private Token? ParseTextNode()
  {
    int nextTagIndex = _html.IndexOf('<', _currentIndex);
    if (nextTagIndex == -1) nextTagIndex = _html.Length;

    string textContent = _html.Substring(_currentIndex, nextTagIndex - _currentIndex);
    _currentIndex = nextTagIndex;

    return !string.IsNullOrWhiteSpace(textContent)
        ? new Token(TokenType.Text, WebUtility.HtmlDecode(textContent))
        : null;
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
      var valueStart = ++index;
      index = input.IndexOf(quoteChar, index);
      if (index == -1) throw new FormatException("Unmatched quote in attribute value");

      var value = input.Substring(valueStart, index - valueStart);
      index++;
      return value;
    }
    else
    {
      var valueStart = index;

      while (index < input.Length && !char.IsWhiteSpace(input[index]) && input[index] != '>')
        index++;

      return input.Substring(valueStart, index - valueStart);
    }
  }
}
