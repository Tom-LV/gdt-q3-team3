using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

public class ConditionNode : BaseStoryNode
{
    public string FallbackValue = "False";

    public override void Draw(StoryEdgeConnectorListener edgeListener)
    {
        title = "Branch / Condition";
        titleContainer.style.backgroundColor = new Color(0.6f, 0.2f, 0.6f, 0.8f);

        // Flow Ports
        Port inputPort = GeneratePort(Direction.Input, "In", edgeListener, Port.Capacity.Multi, typeof(FlowPort));
        inputContainer.Add(inputPort);

        Port truePort = GeneratePort(Direction.Output, "True", edgeListener, Port.Capacity.Single, typeof(FlowPort));
        truePort.portColor = new Color(0.2f, 0.8f, 0.2f);
        outputContainer.Add(truePort);

        Port falsePort = GeneratePort(Direction.Output, "False", edgeListener, Port.Capacity.Single, typeof(FlowPort));
        falsePort.portColor = new Color(0.8f, 0.2f, 0.2f);
        outputContainer.Add(falsePort);

        // Data Port (Red Diamond)
        Port conditionDataPort = GenerateDataPort(Direction.Input, "Condition", edgeListener, Port.Capacity.Single, typeof(BoolPort), FallbackValue, (val) => FallbackValue = val);
        inputContainer.Add(conditionDataPort);

        RefreshExpandedState();
        RefreshPorts();
    }

    // We store the fallback in the "Value" slot of your old save data so we don't have to rewrite the SO right now
    public override BaseNodeData GetSaveData() => new ConditionNodeData { GUID = this.GUID, Position = this.GetPosition().position, Value = this.FallbackValue };
    public override void LoadSaveData(BaseNodeData data)
    {
        if (data is ConditionNodeData condData) { this.GUID = condData.GUID; this.FallbackValue = condData.Value; SetPosition(new Rect(condData.Position, Vector2.zero)); }
    }
}