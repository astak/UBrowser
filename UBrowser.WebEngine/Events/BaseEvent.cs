using UBrowser.WebEngine.DOM;

namespace UBrowser.WebEngine.Events;

public class BaseEvent : IEvent
{
  protected BaseEvent(string type, DateTime timestamp)
  {
    Type = type;
    Timestamp = timestamp;
  }

  public string Type { get; }

  public DateTime Timestamp { get; }

  public DOMNode? Target { get; set; }
  public EventPhase Phase { get; set; }

  public bool IsPropagationStopped { get; private set; }

  public void StopPropagation()
  {
    IsPropagationStopped = true;
  }
}
