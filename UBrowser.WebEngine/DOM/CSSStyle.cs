namespace UBrowser.WebEngine.DOM;

public class CSSStyle
{
  public string? Color { get; set; }
  public string? BackgroundColor { get; set; }
  public string? FontSize { get; set; }
  public string? FontWeight { get; set; }
  public string? TextAlign { get; set; }
  public string? Margin { get; set; }
  public string? Padding { get; set; }
  public string? Border { get; set; }
  public string? Display { get; set; }

  public void ApplyStyle(string property, string value)
  {
    switch (property.ToLower())
    {
      case "color": Color = value; break;
      case "background-color": BackgroundColor = value; break;
      case "font-size": FontSize = value; break;
      case "text-align": TextAlign = value; break;
      case "margin": Margin = value; break;
      case "padding": Padding = value; break;
      case "border": Border = value; break;
      case "display": Display = value; break;
    }
  }
}
