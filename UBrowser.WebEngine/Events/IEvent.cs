using UBrowser.WebEngine.DOM;

namespace UBrowser.WebEngine.Events;

public interface IEvent
{
  string Type { get; }
  DateTime Timestamp { get; }
  DOMNode? Target { get; set; }
  EventPhase Phase { get; set; }
  bool IsPropagationStopped { get; }

  void StopPropagation();
}

public enum EventPhase { Capturing, AtTarget, Bubbling }
