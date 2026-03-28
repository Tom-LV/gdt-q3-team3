using System;
using System.Collections.Generic;
using System.Linq; // Added for the List searches!
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class StorySearchWindow : ScriptableObject, ISearchWindowProvider
{
    private StoryGraphView _graphView;
    private EditorWindow _window;
    private Action _onNodeModified;
    private Port _draggedPort;

    public void Init(StoryGraphView graphView, EditorWindow window, Action onNodeModified)
    {
        _graphView = graphView;
        _window = window;
        _onNodeModified = onNodeModified;
    }

    public void SetDroppedPort(Port port)
    {
        _draggedPort = port;
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        var tree = new List<SearchTreeEntry>
        {
            new SearchTreeGroupEntry(new GUIContent("Create Elements"), 0)
        };

        // Figure out what kind of wire we are holding (if any)
        Type pType = _draggedPort?.portType;
        Direction pDir = _draggedPort != null ? _draggedPort.direction : Direction.Input;

        // --- FLOW NODES ---
        // Only show these if right-clicking empty space, or dragging a white Flow wire
        if (pType == null || pType == typeof(FlowPort))
        {
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Dialogue Nodes"), 1));
            tree.Add(new SearchTreeEntry(new GUIContent("Message Node")) { userData = "MessageNode", level = 2 });
            tree.Add(new SearchTreeEntry(new GUIContent("Choice Node")) { userData = "ChoiceNode", level = 2 });

            tree.Add(new SearchTreeGroupEntry(new GUIContent("Utility Nodes"), 1));
            tree.Add(new SearchTreeEntry(new GUIContent("Delay Node")) { userData = "TimerNode", level = 2 });
            tree.Add(new SearchTreeEntry(new GUIContent("Event Node")) { userData = "EventNode", level = 2 });
        }

        // --- LOGIC / VARIABLE NODES ---
        // These accept Flow OR Data depending on the direction!
        if (pType == null || pType == typeof(FlowPort) || pType == typeof(BoolPort) || pType == typeof(NumberPort))
        {
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Logic Nodes"), 1));

            // Condition & Wait Until accept Flow, OR incoming Boolean wires
            if (pType == null || pType == typeof(FlowPort) || (pDir == Direction.Output && pType == typeof(BoolPort)))
            {
                tree.Add(new SearchTreeEntry(new GUIContent("Branch Node")) { userData = "ConditionNode", level = 2 });
                tree.Add(new SearchTreeEntry(new GUIContent("Wait Until Node")) { userData = "WaitUntilNode", level = 2 });
            }

            if (pType == null || pType == typeof(BoolPort))
            {
                tree.Add(new SearchTreeEntry(new GUIContent("Logic Node (AND/OR)")) { userData = "LogicNode", level = 2 });
                tree.Add(new SearchTreeEntry(new GUIContent("NOT Node")) { userData = "NotNode", level = 2 });
            }

            // Set Variable accepts Flow, OR incoming Data wires
            if (pType == null || pType == typeof(FlowPort) || (pDir == Direction.Output && (pType == typeof(BoolPort) || pType == typeof(NumberPort))))
            {
                tree.Add(new SearchTreeGroupEntry(new GUIContent("Variable Nodes"), 1));
                tree.Add(new SearchTreeEntry(new GUIContent("Set Variable Node")) { userData = "SetVariableNode", level = 2 });

            }

            if (pType == null || pType == typeof(NumberPort) || (pDir == Direction.Input && pType == typeof(BoolPort)))
            {
                tree.Add(new SearchTreeGroupEntry(new GUIContent("Math & Compare"), 1));
                tree.Add(new SearchTreeEntry(new GUIContent("Compare Node")) { userData = "CompareNode", level = 2 });

                // Math node strictly deals in numbers
                if (pType == null || pType == typeof(NumberPort))
                {
                    tree.Add(new SearchTreeEntry(new GUIContent("Math Node")) { userData = "MathNode", level = 2 });
                }

            }
        }

        return tree;
    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        Vector2 windowMousePosition = context.screenMousePosition - _window.position.position;
        Vector2 graphMousePosition = _graphView.contentViewContainer.WorldToLocal(windowMousePosition);
        Vector2 finalPosition = graphMousePosition - new Vector2(100, 75);

        switch (SearchTreeEntry.userData)
        {
            case "MessageNode":
                MessageNode msgNode = _graphView.CreateMessageNode(finalPosition, _onNodeModified);
                ConnectSpawnedNode(msgNode);
                return true;
            case "ChoiceNode":
                ChoiceNode choiceNode = _graphView.CreateChoiceNode(finalPosition, _onNodeModified);
                ConnectSpawnedNode(choiceNode);
                return true;
            case "TimerNode":
                TimerNode timerNode = _graphView.CreateTimerNode(finalPosition, _onNodeModified);
                ConnectSpawnedNode(timerNode);
                return true;
            case "ConditionNode":
                ConditionNode condNode = _graphView.CreateConditionNode(finalPosition, _onNodeModified);
                ConnectSpawnedNode(condNode);
                return true;
            case "EventNode":
                EventNode eventNode = _graphView.CreateEventNode(finalPosition, _onNodeModified);
                ConnectSpawnedNode(eventNode);
                return true;
            case "WaitUntilNode":
                WaitUntilNode waitNode = _graphView.CreateWaitUntilNode(finalPosition, _onNodeModified);
                ConnectSpawnedNode(waitNode);
                return true;
            case "SetVariableNode":
                SetVariableNode setVariableNode = _graphView.CreateSetVariableNode(finalPosition, _onNodeModified);
                ConnectSpawnedNode(setVariableNode);
                return true;
            case "CompareNode":
                CompareNode compareNode = _graphView.CreateCompareNode(finalPosition, _onNodeModified);
                ConnectSpawnedNode(compareNode);
                return true;
            case "MathNode":
                MathNode mathNode = _graphView.CreateMathNode(finalPosition, _onNodeModified);
                ConnectSpawnedNode(mathNode);
                return true;
            case "LogicNode":
                LogicNode logicNode = _graphView.CreateLogicNode(finalPosition, _onNodeModified);
                ConnectSpawnedNode(logicNode);
                return true;
            case "NotNode":
                NotNode notNode = _graphView.CreateNotNode(finalPosition, _onNodeModified);
                ConnectSpawnedNode(notNode);
                return true;

            default:
                return false;
        }
    }

    private void ConnectSpawnedNode(BaseStoryNode newNode)
    {
        if (_draggedPort != null)
        {
            // Figure out which container to search in
            VisualElement targetContainer = _draggedPort.direction == Direction.Output
                ? newNode.inputContainer
                : newNode.outputContainer;

            // FIX: Find the first port that matches the EXACT SAME TYPE as the dragged port!
            Port targetPort = targetContainer.Query<Port>().ToList().FirstOrDefault(p => p.portType == _draggedPort.portType);

            if (targetPort == null)
            {
                // No compatible port exists on this node, so we cancel the auto-connect
                _draggedPort = null;
                return;
            }

            List<Edge> edgesToDelete = new List<Edge>();

            if (_draggedPort.capacity == Port.Capacity.Single)
                edgesToDelete.AddRange(_draggedPort.connections);

            if (targetPort.capacity == Port.Capacity.Single)
                edgesToDelete.AddRange(targetPort.connections);

            if (edgesToDelete.Count > 0)
            {
                _graphView.DeleteElements(edgesToDelete);
            }

            Edge newEdge = new Edge
            {
                output = _draggedPort.direction == Direction.Output ? _draggedPort : targetPort,
                input = _draggedPort.direction == Direction.Output ? targetPort : _draggedPort
            };

            newEdge.input.Connect(newEdge);
            newEdge.output.Connect(newEdge);
            _graphView.AddElement(newEdge);

            _onNodeModified?.Invoke();

            _draggedPort = null;
        }
    }
}