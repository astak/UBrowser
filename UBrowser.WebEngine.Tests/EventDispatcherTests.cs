using FluentAssertions;
using Moq;
using System.Diagnostics;
using UBrowser.WebEngine.DOM;
using UBrowser.WebEngine.Events;

namespace UBrowser.WebEngine.Tests;

public class EventDispatcherTests
{
  [Fact]
  public void AddEventListener_Should_Register_Handler_For_Event()
  {
    //Arrange
    var node = new DOMNode("div");

    Action<IEvent> clickHandler = e => { };

    //Act
    node.AddEventListener("click", clickHandler, false);

    //Assert
    var commands = node.GetEventHandlers("click", false);
    commands.Should().ContainSingle(command => command.Handler == clickHandler);
  }

  [Fact]
  public void RemoveEventListener_Should_Unregister_Handler()
  {
    //Arrange
    var node = new DOMNode("div");
    var dispatcher = new EventDispatcher(node);

    Action<IEvent> clickHandler = e => { };
    node.AddEventListener("click", clickHandler, false);

    //Act
    node.RemoveEventListener("click", clickHandler, false);

    //Assert
    var handlers = node.GetEventHandlers("click", false);
    handlers.Should().BeEmpty();
  }

  [Fact]
  public void DispatchEvent_Should_Invoke_Registered_Handlers()
  {
    //Arrange
    var node = new DOMNode("div");

    var mockHandler = new Mock<Action<IEvent>>();
    node.AddEventListener("click", mockHandler.Object, capture: false);

    var clickEvent = new MouseEvent("click", DateTime.Now, 10, 20, MouseButtons.Left)
    {
      Target = node
    };

    //Act
    node.DispatchEvent(clickEvent);

    //Assert
    mockHandler.Verify(handler => handler(clickEvent), Times.Once,
      "the click handler should be invoked exactly once with the correct event");
    mockHandler.VerifyNoOtherCalls();
  }

  [Fact]
  public void DispatchEvent_Should_Invoke_Handler_On_Target_Node()
  {
    //Arrange
    var rootNode = new DOMNode("div");
    var childNode = new DOMNode("span");
    var grandChildNode = new DOMNode("a");

    rootNode.AddChild(childNode);
    childNode.AddChild(grandChildNode);

    var mockHandler = new Mock<Action<IEvent>>();
    grandChildNode.AddEventListener("click", mockHandler.Object, capture: false);

    var clickEvent = new MouseEvent("click", DateTime.Now, 10, 20, MouseButtons.Left)
    {
      Target = grandChildNode
    };

    //Act
    rootNode.DispatchEvent(clickEvent);

    //Assert
    mockHandler.Verify(handler => handler(clickEvent), Times.Once,
      "the click handler should be invoked exactly once with the correct event");
    mockHandler.VerifyNoOtherCalls();
  }

  [Fact]
  public void DispatchEvent_Should_Invoke_Capturing_Handlers_Only_In_Order()
  {
    //Arrange
    var rootNode = new DOMNode("div");
    var childNode = new DOMNode("span");
    var grandChildNode = new DOMNode("a");

    rootNode.AddChild(childNode);
    childNode.AddChild(grandChildNode);

    var capturingHandlerCalls = new List<(string Node, EventPhase Phase)>();
    var bubblingHandlerCalls = new List<(string Node, EventPhase Phase)>();

    // Добавляем обработчики с capture: true
    var rootCaptureHandler = new Mock<Action<IEvent>>();
    rootCaptureHandler.Setup(h => h(It.IsAny<IEvent>())).Callback<IEvent>(e => capturingHandlerCalls.Add(("root", e.Phase)));
    rootNode.AddEventListener("click", rootCaptureHandler.Object, capture: true);

    var childCaptureHandler = new Mock<Action<IEvent>>();
    childCaptureHandler.Setup(h => h(It.IsAny<IEvent>())).Callback<IEvent>(e => capturingHandlerCalls.Add(("child", e.Phase)));
    childNode.AddEventListener("click", childCaptureHandler.Object, capture: true);

    var grandChildCaptureHandler = new Mock<Action<IEvent>>();
    grandChildCaptureHandler.Setup(h => h(It.IsAny<IEvent>())).Callback<IEvent>(e => capturingHandlerCalls.Add(("grandchild", e.Phase)));
    grandChildNode.AddEventListener("click", grandChildCaptureHandler.Object, capture: true);

    // Добавляем обработчики с capture: false (должны быть игнорированы в этой фазе)
    var rootBubbleHandler = new Mock<Action<IEvent>>();
    rootBubbleHandler.Setup(h => h(It.IsAny<IEvent>())).Callback<IEvent>(e => bubblingHandlerCalls.Add(("root", e.Phase)));
    rootNode.AddEventListener("click", rootBubbleHandler.Object, capture: false);

    var childBubbleHandler = new Mock<Action<IEvent>>();
    childBubbleHandler.Setup(h => h(It.IsAny<IEvent>())).Callback<IEvent>(e => bubblingHandlerCalls.Add(("child", e.Phase)));
    childNode.AddEventListener("click", childBubbleHandler.Object, capture: false);

    var grandChildBubbleHandler = new Mock<Action<IEvent>>();
    grandChildBubbleHandler.Setup(h => h(It.IsAny<IEvent>())).Callback<IEvent>(e => bubblingHandlerCalls.Add(("grandchild", e.Phase)));
    grandChildNode.AddEventListener("click", grandChildBubbleHandler.Object, capture: false);

    var clickEvent = new MouseEvent("click", DateTime.Now, 10, 20, MouseButtons.Left)
    {
      Target = grandChildNode
    };

    //Act
    rootNode.DispatchEvent(clickEvent);

    //Assert
    capturingHandlerCalls.Should().Equal(("root", EventPhase.Capturing), ("child", EventPhase.Capturing), ("grandchild", EventPhase.AtTarget));
    bubblingHandlerCalls.Should().Equal(("grandchild", EventPhase.AtTarget), ("child", EventPhase.Bubbling), ("root", EventPhase.Bubbling));

    rootCaptureHandler.Verify(h => h(clickEvent), Times.Once, "The root capturing handler should be called once.");
    childCaptureHandler.Verify(h => h(clickEvent), Times.Once, "The child capturing handler should be called once.");
    grandChildCaptureHandler.Verify(h => h(clickEvent), Times.Once, "The grandchild capturing handler should be called once.");

    rootBubbleHandler.Verify(h => h(clickEvent), Times.Once, "The root bubbling handler should be called once.");
    childBubbleHandler.Verify(h => h(clickEvent), Times.Once, "The child bubbling handler should be called once.");
    grandChildBubbleHandler.Verify(h => h(clickEvent), Times.Once, "the grandchild bubbling handle should be once.");
  }

  [Fact]
  public void DispatchEvent_Should_Invoke_All_Handlers_On_Same_Node_For_All_Phases()
  {
    //Arrange
    var node = new DOMNode("div");

    var invokedHandlers = new List<string>();

    // Регистрация обработчиков для фазы capturing
    var capturingHandler1 = new Mock<Action<IEvent>>();
    capturingHandler1.Setup(h => h(It.IsAny<IEvent>())).Callback(() => invokedHandlers.Add("capturingHandler1"));
    node.AddEventListener("click", capturingHandler1.Object, capture: true);

    var capturingHandler2 = new Mock<Action<IEvent>>();
    capturingHandler2.Setup(h => h(It.IsAny<IEvent>())).Callback(() => invokedHandlers.Add("capturingHandler2"));
    node.AddEventListener("click", capturingHandler2.Object, capture: true);

    // Регистрация обработчиков для фазы bubbling
    var bubblingHandler1 = new Mock<Action<IEvent>>();
    bubblingHandler1.Setup(h => h(It.IsAny<IEvent>())).Callback(() => invokedHandlers.Add("bubblingHandler1"));
    node.AddEventListener("click", bubblingHandler1.Object, capture: false);

    var bubblingHandler2 = new Mock<Action<IEvent>>();
    bubblingHandler2.Setup(h => h(It.IsAny<IEvent>())).Callback(() => invokedHandlers.Add("bubblingHandler2"));
    node.AddEventListener("click", bubblingHandler2.Object, capture: false);

    var clickEvent = new MouseEvent("click", DateTime.Now, 10, 20, MouseButtons.Left)
    {
      Target = node
    };

    //Act
    node.DispatchEvent(clickEvent);

    //Assert
    // Проверяем порядок вызова обработчиков
    invokedHandlers.Should().Equal("capturingHandler1", "capturingHandler2", "bubblingHandler1", "bubblingHandler2");

    // Проверяем, что каждый обработчик был вызван ровно один раз
    capturingHandler1.Verify(h => h(clickEvent), Times.Once, "CapturingHandler1 should be called once.");
    capturingHandler2.Verify(h => h(clickEvent), Times.Once, "CapturingHandler2 should be called once.");
    bubblingHandler1.Verify(h => h(clickEvent), Times.Once, "BubblingHandler1 should be called once.");
    bubblingHandler2.Verify(h => h(clickEvent), Times.Once, "BubblingHandler2 should be called once.");
  }

  [Fact]
  public void DispatchEvent_Should_Stop_Propagation_In_Capturing_Phase()
  {
    // Arrange
    var rootNode = new DOMNode("div");
    var childNode = new DOMNode("span");
    var grandChildNode = new DOMNode("a");

    rootNode.AddChild(childNode);
    childNode.AddChild(grandChildNode);

    var invokedHandlers = new List<string>();

    var rootHandler = new Mock<Action<IEvent>>();
    rootHandler.Setup(h => h(It.IsAny<IEvent>())).Callback<IEvent>(e => invokedHandlers.Add("root"));
    rootNode.AddEventListener("click", rootHandler.Object, capture: true);

    var childHandler = new Mock<Action<IEvent>>();
    childHandler.Setup(h => h(It.IsAny<IEvent>())).Callback<IEvent>(e =>
    {
      invokedHandlers.Add("child");
      e.StopPropagation();
    });
    childNode.AddEventListener("click", childHandler.Object, capture: true);

    var grandChildHandler = new Mock<Action<IEvent>>();
    grandChildHandler.Setup(h => h(It.IsAny<IEvent>())).Callback<IEvent>(e => invokedHandlers.Add("grandchild"));
    grandChildNode.AddEventListener("click", grandChildHandler.Object, capture: true);

    var clickEvent = new MouseEvent("click", DateTime.Now, 10, 20, MouseButtons.Left)
    {
      Target = grandChildNode
    };

    // Act
    rootNode.DispatchEvent(clickEvent);

    // Assert
    // Проверяем, что захват остановился на childNode
    invokedHandlers.Should().ContainInOrder("root", "child");
    invokedHandlers.Should().NotContain("grandChild");

    // Проверяем, что обработчики вызваны ровно один раз
    rootHandler.Verify(h => h(clickEvent), Times.Once);
    childHandler.Verify(h => h(clickEvent), Times.Once);
    grandChildHandler.Verify(h => h(clickEvent), Times.Never);
  }

  [Fact]
  public void DispatchEvent_Shuld_Stop_Propagation_In_Bubbling_Phase()
  {
    // Arrange
    var rootNode = new DOMNode("div");
    var childNode = new DOMNode("span");
    var grandChildNode = new DOMNode("a");

    rootNode.AddChild(childNode);
    childNode.AddChild(grandChildNode);

    var invokedHandlers = new List<string>();

    var rootHandler = new Mock<Action<IEvent>>();
    rootHandler.Setup(h => h(It.IsAny<IEvent>())).Callback<IEvent>(e => invokedHandlers.Add("root"));
    rootNode.AddEventListener("click", rootHandler.Object, capture: false);

    var childHandler = new Mock<Action<IEvent>>();
    childHandler.Setup(h => h(It.IsAny<IEvent>())).Callback<IEvent>(e =>
    {
      invokedHandlers.Add("child");
      e.StopPropagation();
    });
    childNode.AddEventListener("click", childHandler.Object, capture: false);

    var grandChildHandler = new Mock<Action<IEvent>>();
    grandChildHandler.Setup(h => h(It.IsAny<IEvent>())).Callback<IEvent>(e => invokedHandlers.Add("grandchild"));
    grandChildNode.AddEventListener("click", grandChildHandler.Object, capture: false);

    var clickEvent = new MouseEvent("click", DateTime.Now, 10, 20, MouseButtons.Left)
    {
      Target = grandChildNode
    };

    // Act
    rootNode.DispatchEvent(clickEvent);

    //Assert
    // Проверяем, что захват остановился на childNode
    invokedHandlers.Should().ContainInOrder("grandchild", "child");
    invokedHandlers.Should().NotContain("root");

    // Проверяем, что обработчики вызваны ровно один раз
    grandChildHandler.Verify(h => h(clickEvent), Times.Once);
    childHandler.Verify(h => h(clickEvent), Times.Once);
    rootHandler.Verify(h => h(clickEvent), Times.Never);
  }

  [Fact]
  public void DispatchEvent_Should_Complete_Without_Handlers()
  {
    // Arrange
    var rootNode = new DOMNode("div");
    var childNode = new DOMNode("span");
    var grandChildNode = new DOMNode("a");

    rootNode.AddChild(childNode);
    childNode.AddChild(grandChildNode);

    var clickEvent = new MouseEvent("click", DateTime.Now, 10, 20, MouseButtons.Left)
    {
      Target = grandChildNode
    };

    // Act
    var exception = Record.Exception(() => rootNode.DispatchEvent(clickEvent));

    // Assert
    exception.Should().BeNull("DispatchEvent should not throw an exception for events without handlers.");
  }

  [Fact]
  public void DispatchEvent_Should_Handle_Removal_Of_Handler_During_Execution()
  {
    // Arrange
    var node = new DOMNode("div");

    var invokedHandlers = new List<string>();

    Action<IEvent> firstHandler = e => invokedHandlers.Add("firstHandler");
    Action<IEvent> secondHandler = e => invokedHandlers.Add("secondHandler");
    Action<IEvent> thirdHandler = e => invokedHandlers.Add("thirdHandler");

    // Обработчик, который удаляет другой обработчик
    Action<IEvent> removingHandler = e =>
    {
      invokedHandlers.Add("removingHandler");
      node.RemoveEventListener("click", secondHandler, capture: false);
    };

    node.AddEventListener("click", firstHandler, capture: false);
    node.AddEventListener("click", removingHandler, capture: false);
    node.AddEventListener("click", secondHandler, capture: false);
    node.AddEventListener("click", thirdHandler, capture: false);

    var clickEvent = new MouseEvent("click", DateTime.Now, 10, 20, MouseButtons.Left)
    {
      Target = node
    };

    // Act
    node.DispatchEvent(clickEvent);

    // Assert
    invokedHandlers.Should().Equal("firstHandler", "removingHandler", "thirdHandler");
  }

  [Fact]
  public void DispatchEvent_Should_Handle_Nested_Event_Dispatch_Correctly()
  {
    // Arrange
    var rootNode = new DOMNode("div");
    var childNode = new DOMNode("span");
    rootNode.AddChild(childNode);

    var handlerOrder = new List<string>();

    // Обработчик на корневом узле для фазы захвата
    var rootCaptureHandler = new Mock<Action<IEvent>>();
    rootCaptureHandler.Setup(h => h(It.IsAny<IEvent>()))
      .Callback(() => handlerOrder.Add("root-capture"));
    rootNode.AddEventListener("click", rootCaptureHandler.Object, capture: true);

    // Обработчик на дочернем узле для фазы всплытия
    var childBubbleHandler = new Mock<Action<IEvent>>();
    childBubbleHandler.Setup(h => h(It.IsAny<IEvent>()))
      .Callback(() =>
      {
        handlerOrder.Add("child-bubble");

        // Вложенный вызов DispatchEvent
        var nestedEvent = new MouseEvent("nested-click", DateTime.Now, 10, 20, MouseButtons.Left)
        {
          Target = rootNode
        };
        rootNode.DispatchEvent(nestedEvent);
      });
    childNode.AddEventListener("click", childBubbleHandler.Object, capture: false);

    // Обработчик для вложенного события
    var nestedHandler = new Mock<Action<IEvent>>();
    nestedHandler.Setup(h => h(It.IsAny<IEvent>()))
      .Callback(() => handlerOrder.Add("nested-handler"));
    rootNode.AddEventListener("nested-click", nestedHandler.Object, capture: true);

    var clickEvent = new MouseEvent("click", DateTime.Now, 10, 20, MouseButtons.Left)
    {
      Target = childNode
    };

    // Act
    rootNode.DispatchEvent(clickEvent);

    handlerOrder.Should().Equal(
      "root-capture",  // Обработчик на корневом узле (фаза захвата)
      "child-bubble",  // Обработчик на дочернем узле (фаза всплытия)
      "nested-handler" // Обработчик вложенного события
    );

    rootCaptureHandler.Verify(h => h(clickEvent), Times.Once);
    childBubbleHandler.Verify(h => h(clickEvent), Times.Once);
    nestedHandler.Verify(h => h(It.Is<MouseEvent>(e => e.Type == "nested-click")), Times.Once);
  }

  [Fact]
  public void DispatchEvent_Should_Bubble_Up_To_Dispatching_Node_Only()
  {
    // Arrange
    var rootNode = new DOMNode("html");
    var bodyNode = new DOMNode("body");
    var containerNode = new DOMNode("div");
    var buttonNode = new DOMNode("button");

    // Формируем дерево DOM
    rootNode.AddChild(bodyNode);
    bodyNode.AddChild(containerNode);
    containerNode.AddChild(buttonNode);

    var handlerOrder = new List<string>();

    // Обработчики для фазы всплытия
    var rootHandler = new Mock<Action<IEvent>>();
    rootHandler.Setup(h => h(It.IsAny<IEvent>()))
      .Callback(() => handlerOrder.Add("root"));
    rootNode.AddEventListener("click", rootHandler.Object, capture: false);

    var bodyHandler = new Mock<Action<IEvent>>();
    bodyHandler.Setup(h => h(It.IsAny<IEvent>()))
      .Callback(() => handlerOrder.Add("body"));
    bodyNode.AddEventListener("click", bodyHandler.Object, capture: false);

    var containerHandler = new Mock<Action<IEvent>>();
    containerHandler.Setup(h => h(It.IsAny<IEvent>()))
      .Callback(() => handlerOrder.Add("div"));
    containerNode.AddEventListener("click", containerHandler.Object, capture: false);

    var buttonHandler = new Mock<Action<IEvent>>();
    buttonHandler.Setup(h => h(It.IsAny<IEvent>()))
      .Callback(() => handlerOrder.Add("button"));
    buttonNode.AddEventListener("click", buttonHandler.Object, capture: false);

    // Событие для всплытия
    var clickEvent = new MouseEvent("click", DateTime.Now, 10, 20, MouseButtons.Left)
    {
      Target = buttonNode
    };

    // Act
    containerNode.DispatchEvent(clickEvent);

    // Assert
    handlerOrder.Should().Equal(
      "button", // Обработчик вызывается на целевом узле
      "div"     // Событие всплывает к родителю
    );

    buttonHandler.Verify(h => h(clickEvent), Times.Once, "The button handler should be called once.");
    containerHandler.Verify(h => h(clickEvent), Times.Once, "The container handler should be called once.");
    bodyHandler.Verify(h => h(clickEvent), Times.Never, "The event should not propagate beyond the container node.");
    rootHandler.Verify(h => h(clickEvent), Times.Never, "The event should not propagate beyond the container ndoe.");
  }

  [Fact]
  public void EventDispatcher_Should_Find_Target_Node_By_Coordinates()
  {
    // Arrange
    var root = new DOMNode("root") { Geometry = new DOMNodeGeometry { X = 0, Y = 0, Width = 200, Height = 200 } };
    var child1 = new DOMNode("child1") { Geometry = new DOMNodeGeometry { X = 10, Y = 10, Width = 80, Height = 80 } };
    var child2 = new DOMNode("child2") { Geometry = new DOMNodeGeometry { X = 100, Y = 10, Width = 80, Height = 80 } };

    root.AddChild(child1);
    root.AddChild(child2);

    var targetNode = new DOMNode("target") { Geometry = new DOMNodeGeometry { X = 15, Y = 15, Width = 50, Height = 50 } };
    child1.AddChild(targetNode);

    var mockHandler = new Mock<Action<IEvent>>();
    targetNode.AddEventListener("click", mockHandler.Object, capture: false);

    var eventDispatcher = new EventDispatcher(root);

    var mouseEvent = new MouseEvent("click", DateTime.Now, 20, 20, MouseButtons.Left);

    // Act
    eventDispatcher.DispatchEvent(mouseEvent);

    // Assert
    mockHandler.Verify(handler => handler(mouseEvent), Times.Once,
      "The handler should be called on the target node determined by the coordinates.");
    mouseEvent.Target.Should().Be(targetNode, "The target node should be correctly identified by the coordinates.");
  }

  [Fact]
  public void DOM_Handlers_Should_Work_Correctly_When_DOM_Is_Updated_During_Event_Processing()
  {
    // Arrange
    var rootNode = new DOMNode("root");
    var childNode = new DOMNode("child");
    var grandChildNode = new DOMNode("grandchild");

    rootNode.AddChild(childNode);
    childNode.AddChild(grandChildNode);

    var invokedHandlers = new List<string>();

    // Обработчик на childNode удаляет grandChildNode из DOM
    childNode.AddEventListener("customEvent", evt =>
    {
      invokedHandlers.Add("childHandler");
      childNode.RemoveChild(grandChildNode); // Обновление DOM
    }, capture: false);

    // Обработчик на grandChildNode
    grandChildNode.AddEventListener("customEvent", evt => invokedHandlers.Add("grandChildHandler"), capture: false);

    // Обработчик на rootNode
    rootNode.AddEventListener("customEvent", evt => invokedHandlers.Add("rootHandler"), capture: false);

    var customEvent = new CustomEvent("customEvent", DateTime.Now)
    {
      Target = grandChildNode
    };

    // Act
    rootNode.DispatchEvent(customEvent);

    //Assert
    invokedHandlers.Should().Equal("grandChildHandler", "childHandler", "rootHandler");
  }

  [Fact]
  public void MouseEvent_Should_Associate_With_Node_At_Specified_Coordinates()
  {
    // Arrange
    var rootNode = new DOMNode("root") { Geometry = new DOMNodeGeometry { X = 0, Y = 0, Width = 200, Height = 200 } };
    var childNode = new DOMNode("child") { Geometry = new DOMNodeGeometry { X = 50, Y = 50, Width = 100, Height = 100 } };
    var grandChildNode = new DOMNode("grandchild") { Geometry = new DOMNodeGeometry { X = 75, Y = 75, Width = 50, Height = 50 } };

    rootNode.AddChild(childNode);
    childNode.AddChild(grandChildNode);

    var hitNode = (DOMNode?)null;

    grandChildNode.AddEventListener("mousemove", evt => hitNode = evt.Target, capture: false);

    var mouseEvent = new MouseEvent("mousemove", DateTime.Now, x: 80, y: 80, MouseButtons.None)
    {
      Target = grandChildNode
    };

    // Act
    rootNode.DispatchEvent(mouseEvent);

    // Assert
    hitNode.Should().Be(grandChildNode, because: "grandChildNode is under the specified coordinates (80, 80).");
  }

  [Fact]
  public void MouseEvents_Should_Be_Correctly_Processed()
  {
    // Arrange
    var rootNode = new DOMNode("root") { Geometry = new DOMNodeGeometry { X = 0, Y = 0, Width = 200, Height = 200 } };
    var childNode = new DOMNode("child") { Geometry = new DOMNodeGeometry { X = 50, Y = 50, Width = 100, Height = 100 } };

    rootNode.AddChild(childNode);

    var invokedHandlers = new List<string>();

    rootNode.AddEventListener("click", evt => invokedHandlers.Add("rootClick"), capture: false);
    rootNode.AddEventListener("mousedown", evt => invokedHandlers.Add("rootMouseDown"), capture: false);
    rootNode.AddEventListener("mouseup", evt => invokedHandlers.Add("rootMouseUp"), capture: false);

    childNode.AddEventListener("click", evt => invokedHandlers.Add("childClick"), capture: false);
    childNode.AddEventListener("mousedown", evt => invokedHandlers.Add("childMouseDown"), capture: false);
    childNode.AddEventListener("mouseup", evt => invokedHandlers.Add("childMouseUp"), capture: false);

    // Act
    var mouseDownEvent = new MouseEvent("mousedown", DateTime.Now, x: 75, y: 75, MouseButtons.Left) { Target = childNode };
    var mouseUpEvent = new MouseEvent("mouseup", DateTime.Now, x: 75, y: 75, MouseButtons.Left) { Target = childNode };
    var clickEvent = new MouseEvent("click", DateTime.Now, x: 75, y: 75, MouseButtons.Left) { Target = childNode };

    rootNode.DispatchEvent(mouseDownEvent);
    rootNode.DispatchEvent(mouseUpEvent);
    rootNode.DispatchEvent(clickEvent);

    // Assert
    invokedHandlers.Should().Equal(
      "childMouseDown", "rootMouseDown",
      "childMouseUp", "rootMouseUp",
      "childClick", "rootClick");
  }

  [Fact]
  public void KeyboardEvents_Should_Be_Correctly_Processed()
  {
    // Arrange
    var rootNode = new DOMNode("root");
    var childNode = new DOMNode("child");

    rootNode.AddChild(childNode);

    var invokedHandlers = new List<string>();

    rootNode.AddEventListener("keydown", evt => invokedHandlers.Add("rootKeyDown"), capture: false);
    rootNode.AddEventListener("keyup", evt => invokedHandlers.Add("rootKeyUp"), capture: false);

    childNode.AddEventListener("keydown", evt => invokedHandlers.Add("childKeyDown"), capture: false);
    childNode.AddEventListener("keyup", evt => invokedHandlers.Add("childKeyUp"), capture: false);

    // Act
    var keyDownEvent = new KeyboardEvent("keydown", DateTime.Now, key: "A", KeyCode.A, ctrlKey: false, shiftKey: false, altKey: false, metaKey: false) { Target = childNode };
    var keyUpEvent = new KeyboardEvent("keyup", DateTime.Now, key: "A", KeyCode.A, ctrlKey: false, shiftKey: false, altKey: false, metaKey: false) { Target = childNode };

    rootNode.DispatchEvent(keyDownEvent);
    rootNode.DispatchEvent(keyUpEvent);

    //Assert
    invokedHandlers.Should().Equal(
      "childKeyDown", "rootKeyDown",
      "childKeyUp", "rootKeyUp");
  }

  [Fact]
  public void KeyboardEvent_Modifiers_Should_Be_Correctly_Recognized()
  {
    //Arrange
    var node = new DOMNode("input");
    KeyboardEvent? capturedEvent = null;

    node.AddEventListener("keydown", evt => capturedEvent = (KeyboardEvent)evt, capture: false);

    var keyboardEvent = new KeyboardEvent("keydown", DateTime.Now, key: "C", KeyCode.C, ctrlKey: true, shiftKey: true, altKey: false, metaKey: false) { Target = node };

    //Act
    node.DispatchEvent(keyboardEvent);

    //Assert
    capturedEvent.Should().NotBeNull(because: "the keyboard event hsould be captured by the handler.");
    capturedEvent!.Key.Should().Be("C", because: "the 'key' property should match the dispatched event.");
    capturedEvent.CtrlKey.Should().BeTrue(because: "the CtrlKey flag should be true in the dispatched event.");
    capturedEvent.ShiftKey.Should().BeTrue(because: "the ShiftKey flag should be true in the dispatched event.");
    capturedEvent.AltKey.Should().BeFalse(because: "the AltKey falg should be false in the dispatched event.");
  }

  [Fact]
  public void DispatchEvent_Should_Not_Throw_When_No_Handlers_Are_Registered()
  {
    //Arrange
    var node = new DOMNode("div");

    //  Создаем событие, для которого нет обработчиков
    var keyboardEvent = new KeyboardEvent("keydown", DateTime.Now, key: "A", KeyCode.A, ctrlKey: false, shiftKey: false, altKey: false, metaKey: false) { Target = node };
    var mouseEvent = new MouseEvent("click", DateTime.Now, x: 50, y: 50, MouseButtons.Left) { Target = node };

    //Act & Assert
    FluentActions.Invoking(() => node.DispatchEvent(keyboardEvent))
      .Should().NotThrow(because: "dispatching a keyboard event with not handlers should not throw an exception.");

    FluentActions.Invoking(() => node.DispatchEvent(mouseEvent))
      .Should().NotThrow(because: "dispatching a mouse event with no handlers should not throw an exception.");
  }

  [Fact]
  public void EventDispatcher_Should_Handle_Large_DOM_Tree_Efficiently()
  {
    // Arrange
    const int treeDepth = 10;
    const int childrenPerNode = 5;
    var rootNode = CreateLargeDOMTree(treeDepth, childrenPerNode);

    var eventHandler = new Mock<Action<IEvent>>();
    rootNode.AddEventListener("click", eventHandler.Object, capture: true);

    var mouseEvent = new MouseEvent("click", DateTime.Now, 10, 20, MouseButtons.Left)
    {
      Target = rootNode.Children[2].Children[1]
    };

    var stopwatch = Stopwatch.StartNew();

    // Act
    rootNode.DispatchEvent(mouseEvent);

    stopwatch.Stop();

    // Assert
    eventHandler.Verify(h => h(mouseEvent), Times.Once, "Event handler should be called exactly once.");
    stopwatch.ElapsedMilliseconds.Should().BeLessThan(100, "dispatching the event on a large DOM tree should be performand.");
  }

  private DOMNode CreateLargeDOMTree(int treeDepth, int childrenPerNode)
  {
    var root = new DOMNode("root");
    AddChildren(root, treeDepth - 1, childrenPerNode);
    return root;
  }

  private void AddChildren(DOMNode node, int depth, int childrenPerNode)
  {
    if (depth <= 0) return;

    for (int i = 0; i < childrenPerNode; i++)
    {
      var child = new DOMNode($"child_{depth}_{i}");
      node.AddChild(child);
      AddChildren(child, depth - 1, childrenPerNode);
    }
  }

  [Fact]
  public void EventDispatcher_Should_Handle_Bulk_Handler_Operations()
  {
    //Arrange
    const int handlerCount = 10000;
    var node = new DOMNode("testNode");
    var handlers = new List<Action<IEvent>>();

    for (int i = 0; i < handlerCount; i++)
    {
      handlers.Add(_ => { });
    }

    var stopwatch = Stopwatch.StartNew();

    // Act: Add handlers
    foreach (var handler in handlers)
    {
      node.AddEventListener("click", handler, capture: false);
    }
    stopwatch.Stop();
    var addTime = stopwatch.ElapsedMilliseconds;

    stopwatch.Restart();

    // Act: Remove handlers
    foreach (var handler in handlers)
    {
      node.RemoveEventListener("click", handler, capture: false);
    }
    stopwatch.Stop();
    var removeTime = stopwatch.ElapsedMilliseconds;

    // Assert
    addTime.Should().BeLessThan(500, "adding handlers in bulk should be efficiend.");
    removeTime.Should().BeLessThan(500, "removing handlers in bulk should be efficient.");
    GC.GetTotalMemory(forceFullCollection: true).Should().BeLessThan(50 * 1024 * 1024, "memory usage should not increase significantly after handler removal.");
  }

  [Fact]
  public void AddEventListener_Should_Throw_Exception_For_Invalid_EventType()
  {
    //Arrange
    var node = new DOMNode("testNode");

    //Act
    Action action = () => node.AddEventListener(null!, _ => { }, capture: false);

    //Assert
    action.Should().Throw<ArgumentNullException>("event type cannot be null.");

    //Act with empty event type
    action = () => node.AddEventListener(string.Empty, _ => { }, capture: false);

    //Assert
    action.Should().Throw<ArgumentException>("event type cannot be an empty string.");
  }

  [Fact]
  public void DispatchEvent_Should_Not_Break_Other_Handlers_If_One_Handler_Throws_Exception()
  {
    //Arrange
    var node = new DOMNode("testNode");

    var throwingHandler = new Mock<Action<IEvent>>();
    throwingHandler.Setup(h => h(It.IsAny<IEvent>())).Throws<InvalidOperationException>();
    node.AddEventListener("click", throwingHandler.Object, capture: false);

    var validHandler = new Mock<Action<IEvent>>();
    node.AddEventListener("click", validHandler.Object, capture: false);

    var clickEvent = new MouseEvent("click", DateTime.Now, 10, 20, MouseButtons.Left)
    {
      Target = node
    };

    //Act
    Action action = () => node.DispatchEvent(clickEvent);

    //Assert
    action.Should().NotThrow("exceptions in handlers should not break the event dispatch process.");
    validHandler.Verify(h => h(clickEvent), Times.Once, "the valid handler should still be called.");
    throwingHandler.Verify(h => h(clickEvent), Times.Once, "the throwing handler should be invoked once.");
  }
}

public class CustomEvent : BaseEvent
{
  public CustomEvent(string type, DateTime timestamp) : base(type, timestamp)
  {
  }
}
