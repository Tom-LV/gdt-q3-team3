using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class StoryGraphView : GraphView
{
    public StoryEdgeConnectorListener EdgeListener;

    // The Graph's internal memory
    public List<CharacterProfile> Characters = new List<CharacterProfile>();
    public Action OnCharactersUpdated;

    public List<VariableSaveData> Variables = new List<VariableSaveData>();
    public Action OnVariablesUpdated;
    public Action OnGraphModified;
    private VisualElement _variableListContainer;

    private Blackboard _characterWindow;
    private VisualElement _characterListContainer;

    public StoryGraphView()
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        RegisterCallback<DragUpdatedEvent>(OnDragUpdated, TrickleDown.TrickleDown);
        RegisterCallback<DragPerformEvent>(OnDragPerform, TrickleDown.TrickleDown);

        GridBackground grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        List<Port> compatiblePorts = new List<Port>();

        ports.ForEach((port) =>
        {
            // Type check
            if (startPort != port && startPort.node != port.node && startPort.direction != port.direction && startPort.portType == port.portType)
            {
                bool isAlreadyConnected = false;

                foreach (Edge edge in startPort.connections)
                {
                    if ((edge.input == startPort && edge.output == port) || (edge.output == startPort && edge.input == port))
                    {
                        isAlreadyConnected = true;
                        break;
                    }
                }

                if (!isAlreadyConnected) compatiblePorts.Add(port);
            }
        });

        return compatiblePorts;
    }

    // NODE SPAWNERS
    public void GenerateEntryPointNode()
    {
        StartNode startNode = new StartNode();
        startNode.Initialize(new Vector2(100, 200), null);
        startNode.GUID = "START_NODE_GUID"; // Hardcoded so the Save Utility always finds it
        startNode.Draw(EdgeListener);
        AddElement(startNode);
    }

    public MessageNode CreateMessageNode(Vector2 position, Action onNodeModified)
    {
        MessageNode node = new MessageNode();
        node.Initialize(position, onNodeModified, this);
        node.Draw(EdgeListener);
        AddElement(node);
        onNodeModified?.Invoke();
        return node;
    }

    public ChoiceNode CreateChoiceNode(Vector2 position, Action onNodeModified)
    {
        ChoiceNode node = new ChoiceNode();
        node.Initialize(position, onNodeModified, this);
        node.Draw(EdgeListener);

        node.AddChoicePort("Yes", Guid.NewGuid().ToString(), EdgeListener);
        node.AddChoicePort("No", Guid.NewGuid().ToString(), EdgeListener);

        AddElement(node);
        onNodeModified?.Invoke();
        return node;
    }

    public TimerNode CreateTimerNode(Vector2 position, System.Action onNodeModified)
    {
        TimerNode node = new TimerNode();
        node.Initialize(position, onNodeModified, this);
        node.Draw(EdgeListener);
        AddElement(node);
        onNodeModified?.Invoke();
        return node;
    }

    public ConditionNode CreateConditionNode(Vector2 position, System.Action onNodeModified)
    {
        ConditionNode node = new ConditionNode();
        node.Initialize(position, onNodeModified, this);
        node.Draw(EdgeListener);
        AddElement(node);
        onNodeModified?.Invoke();
        return node;
    }

    public EventNode CreateEventNode(Vector2 position, System.Action onNodeModified)
    {
        EventNode node = new EventNode();
        node.Initialize(position, onNodeModified, this);
        node.Draw(EdgeListener);
        AddElement(node);
        onNodeModified?.Invoke();
        return node;
    }

    public WaitUntilNode CreateWaitUntilNode(Vector2 position, System.Action onNodeModified)
    {
        WaitUntilNode node = new WaitUntilNode();
        node.Initialize(position, onNodeModified, this);
        node.Draw(EdgeListener);
        AddElement(node);
        onNodeModified?.Invoke();
        return node;
    }

    public SetVariableNode CreateSetVariableNode(Vector2 position, System.Action onNodeModified)
    {
        SetVariableNode node = new SetVariableNode();
        node.Initialize(position, onNodeModified, this);
        node.Draw(EdgeListener);
        AddElement(node);
        onNodeModified?.Invoke();
        return node;
    }

    public CompareNode CreateCompareNode(Vector2 position, System.Action onNodeModified)
    {
        CompareNode node = new CompareNode();
        node.Initialize(position, onNodeModified, this);
        node.Draw(EdgeListener);
        AddElement(node);
        onNodeModified?.Invoke();
        return node;
    }

    public MathNode CreateMathNode(Vector2 position, System.Action onNodeModified)
    {
        MathNode node = new MathNode();
        node.Initialize(position, onNodeModified, this);
        node.Draw(EdgeListener);
        AddElement(node);
        onNodeModified?.Invoke();
        return node;
    }

    public VariableNode CreateVariableNode(Vector2 position, System.Action onNodeModified)
    {
        VariableNode node = new VariableNode();
        node.Initialize(position, onNodeModified, this);
        node.Draw(EdgeListener);
        AddElement(node);
        onNodeModified?.Invoke();
        return node;
    }

    public LogicNode CreateLogicNode(Vector2 position, System.Action onNodeModified)
    {
        LogicNode node = new LogicNode();
        node.Initialize(position, onNodeModified, this);
        node.Draw(EdgeListener);
        AddElement(node);
        onNodeModified?.Invoke();
        return node;
    }

    public NotNode CreateNotNode(Vector2 position, System.Action onNodeModified)
    {
        NotNode node = new NotNode();
        node.Initialize(position, onNodeModified, this);
        node.Draw(EdgeListener);
        AddElement(node);
        onNodeModified?.Invoke();
        return node;
    }

    public void OpenCharacterWindow()
    {
        if (_characterWindow == null)
        {
            BuildCharacterWindow();
        }
        _characterWindow.style.display = DisplayStyle.Flex;
    }

    public void CloseCharacterWindow()
    {
        if (_characterWindow != null)
        {
            _characterWindow.style.display = DisplayStyle.None;
        }
    }

    private void BuildCharacterWindow()
    {
        _characterWindow = new Blackboard(this)
        {
            title = "Characters",
        };

        _characterWindow.SetPosition(new Rect(10, 30, 320, 300));

        // Wire up the native "+" button in the Blackboard header
        _characterWindow.addItemRequested = (blackboard) =>
        {
            Characters.Add(new CharacterProfile { GUID = Guid.NewGuid().ToString(), CharacterName = "New Character" });
            RefreshCharacterWindow();
            OnCharactersUpdated?.Invoke();
        };

        // The container that holds our character rows
        _characterListContainer = new VisualElement();
        _characterListContainer.style.paddingBottom = 10;
        _characterListContainer.style.paddingLeft = 10;
        _characterListContainer.style.paddingRight = 10;
        _characterListContainer.style.paddingTop = 10;
        _characterWindow.Add(_characterListContainer);

        RefreshCharacterWindow();
        Add(_characterWindow);

        _characterWindow.Add(new Label("Variables") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 20, marginLeft = 5 } });

        Button addVarBtn = new Button(() => {
            Variables.Add(new VariableSaveData { GUID = Guid.NewGuid().ToString() });
            RefreshVariableWindow();
            OnVariablesUpdated?.Invoke();
        })
        { text = "+ Add Variable" };
        _characterWindow.Add(addVarBtn);

        _variableListContainer = new VisualElement { style = { paddingBottom = 10, paddingLeft = 10, paddingRight = 10 } };
        _characterWindow.Add(_variableListContainer);
        RefreshVariableWindow();
    }

    public void RefreshCharacterWindow()
    {
        if (_characterListContainer == null) return;
        _characterListContainer.Clear();

        foreach (var character in Characters)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginBottom = 6;
            row.style.alignItems = Align.Center;

            TextField nameField = new TextField();
            nameField.value = character.CharacterName;
            nameField.style.flexGrow = 1;
            nameField.RegisterValueChangedCallback(evt =>
            {
                character.CharacterName = evt.newValue;
                OnCharactersUpdated?.Invoke();
            });

            ColorField colorField = new ColorField();
            colorField.value = character.ChatColor;
            colorField.style.width = 60;
            colorField.style.marginLeft = 5;
            colorField.RegisterValueChangedCallback(evt =>
            {
                character.ChatColor = evt.newValue;
                OnCharactersUpdated?.Invoke();
            });

            Button deleteBtn = new Button(() =>
            {
                Characters.Remove(character);
                RefreshCharacterWindow();
                OnCharactersUpdated?.Invoke();
            })
            { text = "-" };

            deleteBtn.style.width = 25;
            deleteBtn.style.marginLeft = 5;

            row.Add(nameField);
            row.Add(colorField);
            row.Add(deleteBtn);
            _characterListContainer.Add(row);
        }
    }

    private void OnDragUpdated(DragUpdatedEvent evt)
    {
        if (UnityEditor.DragAndDrop.GetGenericData("DraggedVariable") != null)
        {
            UnityEditor.DragAndDrop.visualMode = UnityEditor.DragAndDropVisualMode.Link;
            evt.StopPropagation();
        }
    }

    private void OnDragPerform(DragPerformEvent evt)
    {
        if (UnityEditor.DragAndDrop.GetGenericData("DraggedVariable") is string varName)
        {
            UnityEditor.DragAndDrop.AcceptDrag();

            Vector2 localPos = contentViewContainer.WorldToLocal(evt.mousePosition);

            GenericMenu menu = new GenericMenu();

            // Option 1: Get Variable
            menu.AddItem(new GUIContent($"Get {varName}"), false, () => {
                var node = CreateVariableNode(localPos, OnGraphModified);
                node.VariableName = varName;
                node.RebuildPort();
            });

            // Option 2: Set Variable
            menu.AddItem(new GUIContent($"Set {varName}"), false, () => {
                var node = CreateSetVariableNode(localPos, OnGraphModified);
                node.VariableName = varName;
                node.RefreshDropdown();
            });

            menu.ShowAsContext();
            evt.StopPropagation();
        }
    }

    public void RefreshVariableWindow()
    {
        if (_variableListContainer == null) return;
        _variableListContainer.Clear();

        foreach (var variable in Variables)
        {
            VisualElement row = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 6, alignItems = Align.Center } };

            Label dragHandle = new Label("=") { style = { fontSize = 20, marginRight = 5, color = Color.gray } };
            dragHandle.tooltip = "Drag me to the canvas!";
            dragHandle.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == 0)
                {
                    UnityEditor.DragAndDrop.PrepareStartDrag();
                    UnityEditor.DragAndDrop.SetGenericData("DraggedVariable", variable.Name);

                    // Trick Unity into accepting a non-asset drag
                    UnityEditor.DragAndDrop.objectReferences = new UnityEngine.Object[0];
                    UnityEditor.DragAndDrop.paths = new string[0]; // Add this empty array too!

                    UnityEditor.DragAndDrop.StartDrag(variable.Name);

                    // CRUCIAL: Stop the Blackboard window from trying to drag itself!
                    evt.StopPropagation();
                }
            });

            TextField nameField = new TextField { value = variable.Name, style = { flexGrow = 1 } };
            nameField.RegisterValueChangedCallback(evt =>
            {
                string oldName = variable.Name;
                variable.Name = evt.newValue;

                // CASCADE FIX: Update all nodes referencing this variable before refreshing!
                foreach (var node in nodes.ToList())
                {
                    if (node is VariableNode vNode && vNode.VariableName == oldName) vNode.VariableName = variable.Name;
                    if (node is SetVariableNode sNode && sNode.VariableName == oldName) sNode.VariableName = variable.Name;
                }

                OnVariablesUpdated?.Invoke();
            });

            UnityEngine.UIElements.EnumField typeField = new UnityEngine.UIElements.EnumField(variable.Type) { style = { width = 60 } };
            typeField.RegisterValueChangedCallback(evt => { variable.Type = (VariableType)evt.newValue; OnVariablesUpdated?.Invoke(); });

            Button deleteBtn = new Button(() => { Variables.Remove(variable); RefreshVariableWindow(); OnVariablesUpdated?.Invoke(); }) { text = "-" };
            deleteBtn.style.width = 25; deleteBtn.style.marginLeft = 5;

            row.Add(dragHandle); row.Add(nameField); row.Add(typeField); row.Add(deleteBtn);
            _variableListContainer.Add(row);
        }
    }
}