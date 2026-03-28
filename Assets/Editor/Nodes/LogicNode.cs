using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class LogicNode : BaseStoryNode
{
    public string Operator = "AND";
    public string FallbackA = "False";
    public string FallbackB = "False";

    public override void Draw(StoryEdgeConnectorListener edgeListener)
    {
        title = "Logic (AND/OR)";
        titleContainer.style.backgroundColor = new Color(0.6f, 0.4f, 0.2f, 0.8f);

        // Input A (Bool - Red Diamond)
        Port portA = GenerateDataPort(Direction.Input, "A", edgeListener, Port.Capacity.Single, typeof(BoolPort), FallbackA, val => FallbackA = val);
        inputContainer.Add(portA);

        // Operator Dropdown
        DropdownField opField = new DropdownField(new List<string> { "AND", "OR" }, Operator);
        opField.RegisterValueChangedCallback(evt => { Operator = evt.newValue; OnNodeModified?.Invoke(); });
        opField.style.maxWidth = 60;

        VisualElement row = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.Center, paddingBottom = 5, paddingTop = 5 } };
        row.Add(opField);
        mainContainer.Add(row);

        // Input B (Bool - Red Diamond)
        Port portB = GenerateDataPort(Direction.Input, "B", edgeListener, Port.Capacity.Single, typeof(BoolPort), FallbackB, val => FallbackB = val);
        inputContainer.Add(portB);

        // Output (Bool - Red Diamond)
        Port outputPort = GeneratePort(Direction.Output, "Result", edgeListener, Port.Capacity.Multi, typeof(BoolPort));
        outputContainer.Add(outputPort);

        RefreshExpandedState();
        RefreshPorts();
    }

    public override BaseNodeData GetSaveData() => new LogicNodeData { GUID = this.GUID, Position = this.GetPosition().position, Operator = this.Operator, FallbackA = this.FallbackA, FallbackB = this.FallbackB };
    public override void LoadSaveData(BaseNodeData data) { if (data is LogicNodeData d) { GUID = d.GUID; Operator = d.Operator; FallbackA = d.FallbackA; FallbackB = d.FallbackB; SetPosition(new Rect(d.Position, Vector2.zero)); } }
}