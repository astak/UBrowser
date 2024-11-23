using UBrowser.WebEngine.DOM;

namespace UBrowser.WebEngine.Events;

public sealed class KeyboardEvent : BaseEvent
{
  public KeyboardEvent(string type, DateTime timestamp, string key, KeyCode keyCode, bool ctrlKey, bool shiftKey, bool altKey, bool metaKey) : base(type, timestamp)
  {
    Key = key;
    KeyCode = keyCode;
    CtrlKey = ctrlKey;
    ShiftKey = shiftKey;
    AltKey = altKey;
    MetaKey = metaKey;
  }

  // Символ клавиши (например, "A", "1", ";").
  public string Key { get; }

  // Код клавиши (например, KeyCode.A, KeyCode.Enter).
  public KeyCode KeyCode { get; }

  // Модификаторы.
  public bool CtrlKey { get; }
  public bool ShiftKey { get; }
  public bool AltKey { get; }
  public bool MetaKey { get; }
}

// Перечисление кодов клавиш.
public enum KeyCode
{
  // Основные клавиши.
  Backspace = 8,
  Tab = 9,
  Enter = 13,
  Shift = 16,
  Ctrl = 17,
  Alt = 18,
  Escape = 27,
  Space = 32,
  ArrowLeft = 37,
  ArrowUp = 38,
  ArrowRight = 39,
  ArrowDown = 40,

  // Буквы.
  A = 65, B = 66, C = 67, D = 68, E = 69, F = 70, G = 71,
  H = 72, I = 73, J = 74, K = 75, L = 76, M = 77, N = 78,
  O = 79, P = 80, Q = 81, R = 82, S = 83, T = 84, U = 85,
  V = 86, W = 87, X = 88, Y = 89, Z = 90,

  // Цифры.
  Digit0 = 48, Digit1 = 49, Digit2 = 50, Digit3 = 51,
  Digit4 = 52, Digit5 = 53, Digit6 = 54, Digit7 = 55,
  Digit8 = 56, Digit9 = 57
}
