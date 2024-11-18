using System.Net;

namespace UBrowser.WebEngine.Parser;

public class HtmlTokenizer
{
  public List<Token> Tokenize(string html)
  {
    var tokens = new List<Token>();
    var openTags = new Stack<string>();

    int i = 0;
    while (i < html.Length)
    {
      if (html[i] == '<')
      {
        // Определяем комментарий
        if (i + 4 <= html.Length && html.Substring(i, 4) == "<!--")
        {
          int commentEnd = html.IndexOf("-->", i + 4);
          if (commentEnd != -1)
          {
            string commentContent = html.Substring(i, commentEnd + 3 - i);
            var commentToken = ParseComment(commentContent);
            if (commentToken != null)
            {
              tokens.Add(commentToken);
            }
            i = commentEnd + 3;
            continue;
          }
        }

        // Определяем закрывающий тег
        if (i + 2 <= html.Length && html[i + 1] == '/')
        {
          int tagEnd = html.IndexOf('>', i + 2);
          if (tagEnd != -1)
          {
            string endTagContent = html.Substring(i, tagEnd + 1 - i);
            var endTagToken = ParseEndTag(endTagContent);
            if (endTagToken != null)
            {
              // Проверяем, есть ли соответствующий открывающий тэг
              if (openTags.Contains(endTagToken.Name))
              {
                // Закрываем открытые теги, если нужно
                while (openTags.Count > 0 && openTags.Peek() != endTagToken.Name)
                {
                  tokens.Add(new Token(TokenType.EndTag, openTags.Pop()));
                }
                if (openTags.Count > 0 && openTags.Peek() == endTagToken.Name)
                {
                  openTags.Pop();
                }
                tokens.Add(endTagToken);
              }
              // Иначе пропускаем закрывающий тэг
            }
            i = tagEnd + 1;
            continue;
          }
        }

        // Определяем открывающий или самозакрывающийся тег
        int tagEndIndex = html.IndexOf('>', i + 1);
        if (tagEndIndex != -1)
        {
          string tagContent = html.Substring(i, tagEndIndex + 1 - i);
          var tagToken = ParseStartOrSelfClosingTag(tagContent);
          if (tagToken != null)
          {
            tokens.Add(tagToken);
            if (tagToken.Type == TokenType.StartTag)
            {
              openTags.Push(tagToken.Name);
            }
          }
          i = tagEndIndex + 1;
          continue;
        }
      }
      else
      {
        // Определяем текст
        int nextTagIndex = html.IndexOf('<', i);
        if (nextTagIndex == -1) nextTagIndex = html.Length;

        string textContent = html.Substring(i, nextTagIndex - i);
        if (!string.IsNullOrEmpty(textContent.Trim()))
        {
          string decodedText = WebUtility.HtmlDecode(textContent);
          tokens.Add(new Token(TokenType.Text, decodedText));
        }
        i = nextTagIndex;
      }
    }

    // Закрываем оставшиеся открытые теги
    while (openTags.Count > 0)
    {
      tokens.Add(new Token(TokenType.EndTag, openTags.Pop()));
    }

    return tokens;
  }

  private Token? ParseComment(string rawToken)
  {
    // Убедимся, что строка начинается с `<!--` и заканчивается на `-->`
    if (!rawToken.StartsWith("<!--") || !rawToken.EndsWith("-->"))
      return null;

    // Убираем префикс и суффикс комментария
    var commentContent = rawToken.Substring(4, rawToken.Length - 7).Trim();

    return new Token(TokenType.Comment, commentContent);
  }

  private Token? ParseStartOrSelfClosingTag(string rawToken)
  {
    // Убираем угловые скобки
    var trimmed = rawToken.Trim('<', '>').Trim();
    if (string.IsNullOrEmpty(trimmed)) return null;

    // Проверяем, является ли тег самозакрывающимся
    var isSelfClosing = trimmed.EndsWith("/");
    if (isSelfClosing)
    {
      trimmed = trimmed.TrimEnd('/');
    }

    // Отделяем имя тега от атрибутов
    var spaceIndex = trimmed.IndexOf(' ');
    var tagName = spaceIndex >= 0 ? trimmed.Substring(0, spaceIndex) : trimmed;
    var attributesPart = spaceIndex >= 0 ? trimmed.Substring(spaceIndex + 1) : string.Empty;

    // Возвращаем соответствующий токен
    return isSelfClosing
        ? new Token(TokenType.SelfClosingTag, tagName, ParseAttributes(attributesPart))
        : new Token(TokenType.StartTag, tagName, ParseAttributes(attributesPart));
  }

  private Token? ParseEndTag(string rawToken)
  {
    // Убедимся, что строка начинается с `</` и заканчивается на `>`
    if (!rawToken.StartsWith("</") || !rawToken.EndsWith(">"))
      return null;

    // Убираем префикс `</` и суффикс `>`
    var tagName = rawToken.Substring(2, rawToken.Length - 3).Trim();

    // Проверяем, содержит ли тег только допустимые символы
    if (string.IsNullOrEmpty(tagName) || !IsValidTagName(tagName))
      return null;

    return new Token(TokenType.EndTag, tagName);
  }

  // Метод для проверки валидности имени тега
  private bool IsValidTagName(string tagName)
  {
    foreach (var c in tagName)
    {
      if (!char.IsLetterOrDigit(c))
        return false;
    }
    return true;
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
