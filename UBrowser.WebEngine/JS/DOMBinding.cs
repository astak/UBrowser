using UBrowser.WebEngine.DOM;

namespace UBrowser.WebEngine.JS;

/// <summary>
/// Класс для связывания JavaScript Engine с DOM-деревом.
/// </summary>
public class DOMBinding
{
  private DOMNode? _domTree;

  /// <summary>
  /// Привязывает DOM-дерево к DOMBinding.
  /// </summary>
  /// <param name="domTree">Ссылка на DOM-дерево.</param>
  /// <exception cref="ArgumentNullException"></exception>
  public void BindDOM(DOMNode domTree)
  {
    _domTree = domTree ?? throw new ArgumentNullException(nameof(domTree));
  }

  /// <summary>
  /// Возвращает элемент DOM по его ID.
  /// </summary>
  /// <param name="id">Идентификатор элемента.</param>
  /// <returns>Элемент DOM или null, если элемент не найден.</returns>
  /// <exception cref="InvalidOperationException"></exception>
  public DOMNode? GetElementById(string id)
  {
    if (_domTree == null)
      throw new InvalidOperationException("DOMTree is not bound.");

    return SearchById(_domTree.Root, id);
  }

  /// <summary>
  /// Возвращает первый элемент, соответствующий CSS-селектору.
  /// </summary>
  /// <param name="selector">CSS-селектор.</param>
  /// <returns>Найденный элемент или null.</returns>
  /// <exception cref="InvalidOperationException"></exception>
  public DOMNode? QuerySelector(string selector)
  {
    if (_domTree == null)
      throw new InvalidOperationException("DOMTree is not bound.");

    return SearchBySelector(_domTree.Root, selector);
  }

  /// <summary>
  /// Выполняет поиск всех элементов, соответствующих CSS-селектору.
  /// </summary>
  /// <param name="selector">CSS-селектор.</param>
  /// <returns>Список подходящих элементов.</returns>
  /// <exception cref="InvalidOperationException"></exception>
  public List<DOMNode> QuerySelectorAll(string selector)
  {
    if (_domTree == null)
      throw new InvalidOperationException("DOMTree is not bound.");

    var results = new List<DOMNode>();
    SearchAllBySelector(_domTree.Root, selector, results);
    return results;
  }

  private DOMNode? SearchById(DOMNode? root, string id)
  {
    if (root == null) return null;

    var queue = new Queue<DOMNode>();
    queue.Enqueue(root);

    while (queue.Count > 0)
    {
      var node = queue.Dequeue();

      if (node.Attributes.TryGetValue("id", out var nodeId) && nodeId == id)
        return node;

      foreach (var child in node.Children)
      {
        queue.Enqueue(child);
      }
    }

    return null;
  }

  private DOMNode? SearchBySelector(DOMNode? root, string selector)
  {
    if (root == null) return null;

    var queue = new Queue<DOMNode>();
    queue.Enqueue(root);

    while (queue.Count > 0)
    {
      var node = queue.Dequeue();

      if (MatchesSelector(node, selector))
        return node;

      foreach (var child in root.Children)
      {
        queue.Enqueue(child);
      }
    }

    return null;
  }

  private void SearchAllBySelector(DOMNode? root, string selector, List<DOMNode> results)
  {
    if (root == null) return;

    var queue = new Queue<DOMNode>();
    queue.Enqueue(root);

    while (queue.Count > 0)
    {
      var node = queue.Dequeue();

      if (MatchesSelector(node, selector))
        results.Add(node);

      foreach (var child in node.Children)
      {
        queue.Enqueue(child);
      }
    }
  }

  private bool MatchesSelector(DOMNode node, string selector)
  {
    if (selector.StartsWith("#") && node.Attributes.TryGetValue("id", out var id))
    {
      return id == selector.Substring(1);
    }
    if (selector.StartsWith(".") && node.Attributes.TryGetValue("class", out var classList))
    {
      return classList.Split(' ').Contains(selector.Substring(1));
    }
    return node.TagName == selector;
  }
}
