namespace UBrowser.WebEngine.DOM;

public class DOMNode
{
  public DOMNode(string tagName)
  {
    TagName = tagName;
  }

  public string TagName { get; }
  public DOMNode? Parent { get; set; }
  public List<DOMNode> Children { get; set; } = new List<DOMNode>();
  public CSSStyle Style { get; set; } = new CSSStyle();
  public string? InnerText { get; set; }
  public Dictionary<string, string>? Attributes { get; init; }

  public DOMNode Root
  {
    get
    {
      var current = this;
      while (current.Parent != null){
        current = current.Parent;
      }
      return current;
    }
  }

  public void AddChild(DOMNode child)
  {
    child.Parent = this;
    Children.Add(child);
  }

  public void RemoveChild(DOMNode child)
  {
    Children.Remove(child);
    child.Parent = null;
  }
}

public static class SpecialNodeNames
{
  public const string Root = "Document";
  public const string Text = "#text";
}
