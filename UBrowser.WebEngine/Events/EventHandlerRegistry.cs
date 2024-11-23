namespace UBrowser.WebEngine.Events;

public class EventHandlerRegistry
{
  private readonly Dictionary<string, List<EventHandlerCommand>> _eventHandlers = new();

  public void AddEventListener(string eventType, Action<IEvent> handler, bool capture = false)
  {
    if (eventType == null) throw new ArgumentNullException(nameof(eventType));
    if (eventType == string.Empty) throw new ArgumentException("Invalid event type");

    if (!_eventHandlers.ContainsKey(eventType))
    {
      _eventHandlers[eventType] = new List<EventHandlerCommand>();
    }
    var command = new EventHandlerCommand(handler, capture);
    _eventHandlers[eventType].Add(command);
  }

  public void RemoveEventListener(string eventType, Action<IEvent> handler, bool capture = false)
  {
    if (eventType == null) throw new ArgumentNullException(nameof(eventType));
    if (eventType == string.Empty) throw new ArgumentException("Invalid event type");

    if (!_eventHandlers.TryGetValue(eventType, out var commands)) return;
    var newCommands = new List<EventHandlerCommand>(commands.Count);
    foreach (var command in commands)
    {
      if (command.Handler == handler)
      {
        command.Deactivate();
      }
      else
      {
        newCommands.Add(command);
      }
    }
    if (newCommands.Count == 0)
    {
      _eventHandlers.Remove(eventType);
    }
    else
    {
      _eventHandlers[eventType] = newCommands;
    }
  }

  public IReadOnlyList<EventHandlerCommand> GetHandlers(string eventType, bool capture)
  {
    if (!_eventHandlers.TryGetValue(eventType, out var handlers))
    {
      return Array.Empty<EventHandlerCommand>();
    }

    return handlers.Where(h => h.Capture == capture).ToList().AsReadOnly();
  }
}
