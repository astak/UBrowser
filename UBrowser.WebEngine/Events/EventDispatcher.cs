using UBrowser.WebEngine.DOM;

namespace UBrowser.WebEngine.Events;

public class EventDispatcher
{
  private readonly DOMNode _root;

  public EventDispatcher(DOMNode root)
  {
    _root = root;
  }

  private readonly Queue<IEvent> _eventQueue = new();

  public void EnqueueEvent(IEvent eventObj)
  {
    _eventQueue.Enqueue(eventObj);
  }

  public void DispatchEvents()
  {
    while (_eventQueue.TryDequeue(out var eventObj))
    {
      DispatchEvent(eventObj);
    }
  }

  public void DispatchEvent(IEvent eventObj)
  {
    eventObj.Target = HitTest(eventObj);
    _root.DispatchEvent(eventObj);
  }

  private DOMNode? HitTest(IEvent eventObj)
  {
    if (eventObj is MouseEvent mouseEvent)
    {
      return FindNodeAtCoordinates(_root, mouseEvent.X, mouseEvent.Y);
    }

    return null;
  }

  private DOMNode? FindNodeAtCoordinates(DOMNode node, float x, float y)
  {
    if (!node.Geometry.ContainsPoint(x, y))
    {
      return null;
    }

    foreach (var child in node.Children)
    {
      var result = FindNodeAtCoordinates(child, x, y);
      if (result != null)
      {
        return result;
      }
    }
    return node;
  }
}
