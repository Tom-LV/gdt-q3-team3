using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using System;

public class VariableNode : BaseStoryNode
{
    public string VariableName;

    private StoryGraphView _graphView;
    private Port _outputPort;
    private StoryEdgeConnectorListener _edgeListener;

    public override void Initialize(Vector2 position, Action onNodeModified, StoryGraphView graphView = null)
    {
        base.Initialize(position, onNodeModified, graphView);
        _graphView = graphView;
        if (_graphView != null)
        {
            _graphView.OnVariablesUpdated += RefreshNode;
            this.RegisterCallback<DetachFromPanelEvent>(evt => _graphView.OnVariablesUpdated -= RefreshNode);
        }
    }

    public override void Draw(StoryEdgeConnectorListener edgeListener)
    {
        _edgeListener = edgeListener;

        // 1. Completely hide the default boxy Node layout
        titleContainer.style.display = DisplayStyle.None;

        // 2. Style the main container to look like a rounded pill
        mainContainer.style.backgroundColor = new Color(0.18f, 0.18f, 0.18f, 0.9f);
        mainContainer.style.borderTopLeftRadius = 15;
        mainContainer.style.borderTopRightRadius = 15;
        mainContainer.style.borderBottomLeftRadius = 15;
        mainContainer.style.borderBottomRightRadius = 15;
        mainContainer.style.paddingLeft = 5;
        mainContainer.style.paddingRight = 5;
        mainContainer.style.paddingTop = 2;
        mainContainer.style.paddingBottom = 2;

        RebuildPort();
    }

    public void RebuildPort()
    {
        if (_graphView == null || outputContainer == null) return;

        var selectedVar = _graphView.Variables.Find(v => v.Name == VariableName);
        if (selectedVar == null) return;

        Type expectedPortType = selectedVar.Type == VariableType.Bool ? typeof(BoolPort) : typeof(NumberPort);

        // SMART REBUILD: If the port is already the correct shape/type, just update the text!
        if (_outputPort != null && _outputPort.portType == expectedPortType)
        {
            _outputPort.portName = VariableName;
            Label portLabel = _outputPort.contentContainer.Q<Label>("type");
            if (portLabel != null) portLabel.text = VariableName;

            RefreshExpandedState();
            return; // Stop here so wires don't break!
        }

        // Clean up the old wire ONLY if the Type (Bool/Number) changed
        if (_outputPort != null)
        {
            if (_outputPort.connected) _graphView.DeleteElements(_outputPort.connections);
            if (outputContainer.Contains(_outputPort)) outputContainer.Remove(_outputPort);
        }

        _outputPort = GeneratePort(Direction.Output, VariableName, _edgeListener, Port.Capacity.Multi, expectedPortType);

        Label newPortLabel = _outputPort.contentContainer.Q<Label>("type");
        if (newPortLabel != null)
        {
            newPortLabel.style.fontSize = 14;
            newPortLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            newPortLabel.style.marginRight = 5;
        }

        outputContainer.Add(_outputPort);

        RefreshPorts();
        RefreshExpandedState();
    }

    private void RefreshNode()
    {
        if (_graphView.Variables.Find(v => v.Name == VariableName) == null)
        {
            _graphView.RemoveElement(this);
            return;
        }
        RebuildPort();
    }

    public override BaseNodeData GetSaveData() => new VariableNodeData { GUID = this.GUID, Position = this.GetPosition().position, VariableName = this.VariableName };

    public override void LoadSaveData(BaseNodeData data)
    {
        if (data is VariableNodeData varData)
        {
            this.GUID = varData.GUID;
            this.VariableName = varData.VariableName;
            SetPosition(new Rect(varData.Position, Vector2.zero));
        }
    }
}