namespace UBrowser.WebEngine.DOM;

public class DOMNodeGeometry
{
  public float X { get; set; } = 0;
  public float Y { get; set; } = 0;
  public float Width { get; set; } = 0;
  public float Height { get; set; } = 0;
  public float Padding { get; set; } = 0;
  public float Margin { get; set; } = 0;
  public float BorderWidth { get; set; } = 0;

  public float OuterWidth => Width + 2 * (Margin + Padding + BorderWidth);
  public float OuterHeight => Height + 2 * (Margin + Padding + BorderWidth);

  public bool ContainsPoint(float x, float y)
  {
    return x >= X && x <= X + Width && y >= Y && y <= Y + Height;
  }
}
