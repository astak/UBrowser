using System.Diagnostics;
using UBrowser.WebEngine.DOM;
using UBrowser.WebEngine.JS;

namespace UBrowser.WebEngine.Tests;

public class DOMBindingTests
{
  [Fact]
  public void GetElementById_WithoutBinding_ShouldThrowInvalidOperationException()
  {
    var binding = new DOMBinding();

    Assert.Throws<InvalidOperationException>(() => binding.GetElementById("test"));
  }

  [Fact]
  public void QuerySelector_WithoutBinding_ShouldThrowInvalidOperationException()
  {
    var binding = new DOMBinding();

    Assert.Throws<InvalidOperationException>(() => binding.QuerySelector("test"));
  }

  [Fact]
  public void QuerySelectorAll_WithoutBinding_ShouldThrowInvalidOperationException()
  {
    var binding = new DOMBinding();

    Assert.Throws<InvalidOperationException>(() => binding.QuerySelectorAll("test"));
  }

  [Fact]
  public void GetElementById_ShouldReturnCorrectElement()
  {
    var root = new DOMNode("root")
    {
      Attributes = new Dictionary<string, string> { { "id", "root" } }
    };
    var child = new DOMNode("child")
    {
      Attributes = new Dictionary<string, string> { { "id", "child" } }
    };
    root.Children.Add(child);

    var binding = new DOMBinding();
    binding.BindDOM(root);

    var result = binding.GetElementById("child");

    Assert.Equal(child, result);
  }

  [Fact]
  public void GetElementById_NonExistentId_ShouldReturnNull()
  {
    var root = new DOMNode("root");
    var binding = new DOMBinding();
    binding.BindDOM(root);

    var result = binding.GetElementById("nonexistent");

    Assert.Null(result);
  }

  [Fact]
  public void QuerySelector_ShouldReturnElementByTagName()
  {
    var root = new DOMNode("div");
    var child = new DOMNode("span");
    root.Children.Add(child);

    var binding = new DOMBinding();
    binding.BindDOM(root);

    var result = binding.QuerySelector("span");

    Assert.Equal(child, result);
  }

  [Fact]
  public void QuerySelector_ShouldReturnElementByClassName()
  {
    var root = new DOMNode("root");
    var child = new DOMNode("root")
    {
      Attributes = new Dictionary<string, string> { { "class", "test-class" } }
    };
    root.Children.Add(child);

    var binding = new DOMBinding();
    binding.BindDOM(root);

    var result = binding.QuerySelector(".test-class");

    Assert.Equal(child, result);
  }

  [Fact]
  public void QuerySelector_ShouldReturnElementByIdSelector()
  {
    var root = new DOMNode("root");
    var child = new DOMNode("child")
    {
      Attributes = new Dictionary<string, string> { { "id", "test-id" } }
    };
    root.Children.Add(child);

    var binding = new DOMBinding();
    binding.BindDOM(root);

    var result = binding.QuerySelector("#test-id");

    Assert.Equal(child, result);
  }

  [Fact]
  public void QuerySelector_ShouldReturnFirstMatchingElement()
  {
    var root = new DOMNode("div");
    var child1 = new DOMNode("span");
    var child2 = new DOMNode("span");
    root.Children.Add(child1);
    root.Children.Add(child2);

    var binding = new DOMBinding();
    binding.BindDOM(root);

    var result = binding.QuerySelector("span");

    Assert.Equal(child1, result); // Должен вернуть первый элемент.
  }

  [Fact]
  public void QuerySelectorAll_ShouldReturnAllMatchingElements()
  {
    var root = new DOMNode("div");
    var child1 = new DOMNode("span");
    var child2 = new DOMNode("span");
    root.Children.Add(child1);
    root.Children.Add(child2);

    var binding = new DOMBinding();
    binding.BindDOM(root);

    var results = binding.QuerySelectorAll("span");

    Assert.Equal(2, results.Count);
    Assert.Contains(child1, results);
    Assert.Contains(child2, results);
  }

  [Fact]
  public void GetElementById_EmptyDOM_ShouldReturnNull()
  {
    var root = new DOMNode("root");
    var binding = new DOMBinding();
    binding.BindDOM(root);

    var result = binding.GetElementById("any-id");

    Assert.Null(result);
  }

  [Fact]
  public void QuerySelector_InvalidSelector_ShouldReturnNull()
  {
    var root = new DOMNode("root");
    var binding = new DOMBinding();
    binding.BindDOM(root);

    var result = binding.QuerySelector("$invalid");

    Assert.Null(result);
  }

  [Fact]
  public void GetElementById_LargeDOM_PerformanceTest()
  {
    // Arrange
    const int numberOfNodes = 100_000;
    var root = new DOMNode("root");
    var targetNode = new DOMNode("target") { Attributes = new Dictionary<string, string> { { "id", "target" } } };

    // Создаем большое дерево
    var current = root;
    for (int i = 0; i < numberOfNodes - 1; i++)
    {
      var child = new DOMNode("child");
      current.Children.Add(child);
      current = child;
    }
    current.Children.Add(targetNode);

    var binding = new DOMBinding();
    binding.BindDOM(root);

    // Act
    var stopwatch = Stopwatch.StartNew();
    var result = binding.GetElementById("target");
    stopwatch.Stop();

    // Assert
    Assert.Equal(targetNode, result);
    Assert.True(stopwatch.ElapsedMilliseconds < 100, "Поиск элемента должен занимать менее 100 мс.");
  }
}
