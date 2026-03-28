using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;

public class SetVariableNode : BaseStoryNode
{
    public string VariableName = "";
    public string FallbackValue = ""; // We store as string to handle both float and bool easily

    private StoryGraphView _graphView;
    private DropdownField _varDropdown;

    private Port _dataInputPort;
    private VisualElement _fallbackUI;
    private StoryEdgeConnectorListener _edgeListener;

    public override void Initialize(Vector2 position, System.Action onNodeModified, StoryGraphView graphView = null)
    {
        base.Initialize(position, onNodeModified, graphView);
        _graphView = graphView;

        if (_graphView != null)
        {
            _graphView.OnVariablesUpdated += RefreshDropdown;
            this.RegisterCallback<DetachFromPanelEvent>(evt => _graphView.OnVariablesUpdated -= RefreshDropdown);
        }
    }

    public override void Draw(StoryEdgeConnectorListener edgeListener)
    {
        _edgeListener = edgeListener;
        title = "Set Variable";
        titleContainer.style.backgroundColor = new Color(0.1f, 0.6f, 0.4f, 0.8f);

        // 1. FLOW PORTS
        Port inputFlowPort = GeneratePort(Direction.Input, "In", edgeListener, Port.Capacity.Multi, typeof(FlowPort));
        inputContainer.Add(inputFlowPort);

        Port outputFlowPort = GeneratePort(Direction.Output, "Out", edgeListener, Port.Capacity.Single, typeof(FlowPort));
        outputContainer.Add(outputFlowPort);

        // 2. VARIABLE SELECTION
        _varDropdown = new DropdownField("Variable");
        _varDropdown.RegisterValueChangedCallback(evt =>
        {
            VariableName = evt.newValue;
            OnNodeModified?.Invoke();
            RebuildDataPort(); // Rebuild the diamond port when variable changes!
        });
        mainContainer.Add(_varDropdown);

        RefreshDropdown();
        RefreshExpandedState();
        RefreshPorts();
    }

    public void RefreshDropdown()
    {
        if (_graphView == null || _varDropdown == null) return;
        _varDropdown.SetValueWithoutNotify("Updating...");

        if (_graphView.Variables.Count > 0)
        {
            _varDropdown.choices = _graphView.Variables.ConvertAll(v => v.Name);
            int index = _graphView.Variables.FindIndex(v => v.Name == VariableName);

            if (index >= 0) _varDropdown.SetValueWithoutNotify(_varDropdown.choices[index]);
            else
            {
                VariableName = _graphView.Variables[0].Name;
                _varDropdown.SetValueWithoutNotify(_varDropdown.choices[0]);
            }
        }
        else
        {
            _varDropdown.choices = new List<string> { "No Variables" };
            _varDropdown.SetValueWithoutNotify("No Variables");
        }

        RebuildDataPort();
    }

    private void RebuildDataPort()
    {
        if (_graphView == null || inputContainer == null) return;

        var selectedVar = _graphView.Variables.Find(v => v.Name == VariableName);
        if (selectedVar == null) return;

        System.Type expectedPortType = selectedVar.Type == VariableType.Bool ? typeof(BoolPort) : typeof(NumberPort);

        // SMART REBUILD: If the port is already the correct shape/type, leave it alone!
        if (_dataInputPort != null && _dataInputPort.portType == expectedPortType)
        {
            return; // Stop here so wires don't break!
        }

        if (_dataInputPort != null)
        {
            if (_dataInputPort.connected) _graphView.DeleteElements(_dataInputPort.connections);
            if (inputContainer.Contains(_dataInputPort)) inputContainer.Remove(_dataInputPort);
        }

        _dataInputPort = GenerateDataPort(Direction.Input, "Value", _edgeListener, Port.Capacity.Single, expectedPortType, FallbackValue, (newVal) => { FallbackValue = newVal; });
        inputContainer.Add(_dataInputPort);

        RefreshPorts();
        RefreshExpandedState();
    }

    public override BaseNodeData GetSaveData() => new SetVariableNodeData { GUID = this.GUID, Position = this.GetPosition().position, VariableName = this.VariableName, Value = this.FallbackValue };

    public override void LoadSaveData(BaseNodeData data)
    {
        if (data is SetVariableNodeData eventData)
        {
            this.GUID = eventData.GUID;
            this.VariableName = eventData.VariableName; // Fixed a copy/paste bug from your old file here!
            this.FallbackValue = eventData.Value;
            SetPosition(new Rect(eventData.Position, Vector2.zero));
        }
    }
}