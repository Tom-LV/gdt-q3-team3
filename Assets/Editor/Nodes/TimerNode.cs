using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

public class TimerNode : BaseStoryNode
{
    public string FallbackWaitTime = "1.0";

    public override void Draw(StoryEdgeConnectorListener edgeListener)
    {
        title = "Delay";
        titleContainer.style.backgroundColor = new Color(0.2f, 0.4f, 0.6f, 0.8f);

        Port inputPort = GeneratePort(Direction.Input, "In", edgeListener, Port.Capacity.Multi, typeof(FlowPort));
        inputContainer.Add(inputPort);

        Port outputPort = GeneratePort(Direction.Output, "Out", edgeListener, Port.Capacity.Single, typeof(FlowPort));
        outputContainer.Add(outputPort);

        // Data Port (Green Diamond)
        Port timeDataPort = GenerateDataPort(Direction.Input, "Wait Time", edgeListener, Port.Capacity.Single, typeof(NumberPort), FallbackWaitTime, (val) => FallbackWaitTime = val);
        inputContainer.Add(timeDataPort);

        RefreshExpandedState();
        RefreshPorts();
    }

    public override BaseNodeData GetSaveData() => new TimerNodeData { GUID = this.GUID, Position = this.GetPosition().position, WaitTime = float.TryParse(FallbackWaitTime, out float f) ? f : 1f };
    public override void LoadSaveData(BaseNodeData data)
    {
        if (data is TimerNodeData timerData) { this.GUID = timerData.GUID; this.FallbackWaitTime = timerData.WaitTime.ToString(); SetPosition(new Rect(timerData.Position, Vector2.zero)); }
    }
}