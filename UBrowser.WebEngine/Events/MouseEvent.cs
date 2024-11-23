namespace UBrowser.WebEngine.Events;

public sealed class MouseEvent : BaseEvent
{
  public MouseEvent(string type, DateTime timestamp, int x, int y, MouseButtons button) : base(type, timestamp)
  {
    X = x;
    Y = y;
    Button = button;
  }
  public int X { get; }
  public int Y { get; }
  public MouseButtons Button { get; }
}

public enum MouseButtons { Left, Right, None }
