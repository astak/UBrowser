namespace UBrowser.WebEngine.Parser;

public class Token
{
  public Token(TokenType type, string name, params KeyValuePair<string, string>[] attributes)
  {
    Type = type;
    Name = name;
    Attributes = new Dictionary<string, string>(attributes);
  }

  public TokenType Type { get; }
  public string Name { get; }
  public Dictionary<string, string> Attributes { get; }
}

public enum TokenType
{
  StartTag,
  EndTag,
  SelfClosingTag,
  Text,
  Comment,
}

public static class TagNames
{
  public const string Div = "div";
  public const string Span = "span";
  public const string Paragraph = "p";
  public const string Image = "img";
  public const string Bold = "b";
}
