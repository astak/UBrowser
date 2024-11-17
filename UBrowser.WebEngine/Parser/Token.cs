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
  public const string TagNameDiv = "div";
  public const string TagNameSpan = "span";
  public const string TagNameParagraph = "p";
  public const string TagNameImage = "img";
}
