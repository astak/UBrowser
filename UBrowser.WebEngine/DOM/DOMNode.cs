using UBrowser.WebEngine.Events;

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
  public DOMNodeGeometry Geometry { get; set; } = new DOMNodeGeometry();

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

  private readonly EventHandlerRegistry _eventHandlers = new();

  public void AddEventListener(string eventType, Action<IEvent> handler, bool capture)
  {
    _eventHandlers.AddEventListener(eventType, handler, capture);
  }

  public void RemoveEventListener(string eventType, Action<IEvent> handler, bool capture)
  {
    _eventHandlers.RemoveEventListener(eventType, handler, capture);
  }

  public IReadOnlyList<EventHandlerCommand> GetEventHandlers(string eventType, bool capture)
  {
    return _eventHandlers.GetHandlers(eventType, capture);
  }

  public void DispatchEvent(IEvent eventObj)
  {
    if (eventObj.Target == null) return;

    var path = BuildPathToTarget(eventObj.Target);

    // 1. Capturing phase
    for (int i = path.Count - 1; i >= 0; i--)
    {
      if (eventObj.IsPropagationStopped) return;

      eventObj.Phase = EventPhase.Capturing;
      var node = path[i];
      node.InvokeHandlers(eventObj, capture: true);
    }

    // 2. Target phase
    if (eventObj.IsPropagationStopped) return;

    eventObj.Phase = EventPhase.AtTarget;
    eventObj.Target.InvokeHandlers(eventObj, capture: true);

    if (eventObj.IsPropagationStopped) return;

    eventObj.Phase = EventPhase.AtTarget;
    eventObj.Target.InvokeHandlers(eventObj, capture: false);

    // 3. Bubbling phase
    for (int i = 0; i < path.Count; i++)
    {
      if (eventObj.IsPropagationStopped) return;

      eventObj.Phase = EventPhase.Bubbling;
      var node = path[i];
      node.InvokeHandlers(eventObj, capture: false);
    }
  }

  private List<DOMNode> BuildPathToTarget(DOMNode target)
  {
    var path = new List<DOMNode>();
    var current = target.Parent;
    while (current != null)
    {
      path.Add(current);
      if (current == this) break;
      current = current.Parent;
    }

    return path;
  }

  private void InvokeHandlers(IEvent eventObj, bool capture)
  {
    var commands = GetEventHandlers(eventObj.Type, capture);
    foreach (var command in commands)
    {
      command.Execute(eventObj);
    }
  }

  public override string ToString() => TagName;
}

public static class SpecialNodeNames
{
  public const string Root = "Document";
  public const string Text = "#text";
}
