using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

public class WaitUntilNode : BaseStoryNode
{
    public string FallbackValue = "False";

    public override void Draw(StoryEdgeConnectorListener edgeListener)
    {
        title = "Wait Until";
        titleContainer.style.backgroundColor = new Color(0.2f, 0.5f, 0.5f, 0.8f);

        Port inputPort = GeneratePort(Direction.Input, "In", edgeListener, Port.Capacity.Multi, typeof(FlowPort));
        inputContainer.Add(inputPort);

        Port outputPort = GeneratePort(Direction.Output, "Out", edgeListener, Port.Capacity.Single, typeof(FlowPort));
        outputContainer.Add(outputPort);

        // Data Port (Red Diamond)
        Port conditionDataPort = GenerateDataPort(Direction.Input, "Condition", edgeListener, Port.Capacity.Single, typeof(BoolPort), FallbackValue, (val) => FallbackValue = val);
        inputContainer.Add(conditionDataPort);

        RefreshExpandedState();
        RefreshPorts();
    }

    public override BaseNodeData GetSaveData() => new WaitUntilNodeData { GUID = this.GUID, Position = this.GetPosition().position, Value = this.FallbackValue };
    public override void LoadSaveData(BaseNodeData data)
    {
        if (data is WaitUntilNodeData waitData) { this.GUID = waitData.GUID; this.FallbackValue = waitData.Value; SetPosition(new Rect(waitData.Position, Vector2.zero)); }
    }
}