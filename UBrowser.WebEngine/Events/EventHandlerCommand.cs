namespace UBrowser.WebEngine.Events;

public class EventHandlerCommand
{
  public EventHandlerCommand(Action<IEvent> handler, bool capture)
  {
    Handler = handler;
    Capture = capture;
  }

  public bool IsActive { get; private set; } = true;
  public bool Capture { get; }
  public Action<IEvent> Handler { get; }

  public void Execute(IEvent eventObj)
  {
    if (IsActive)
    {
      try
      {
        Handler(eventObj);
      } 
      catch (Exception ex)
      {
        Console.WriteLine($"Exception in evnet handler: {ex}");
      }
    }
  }

  public void Deactivate()
  {
    IsActive = false;
  }
}
