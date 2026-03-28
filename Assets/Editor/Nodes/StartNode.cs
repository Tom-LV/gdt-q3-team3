using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class StartNode : BaseStoryNode
{
    public override void Draw(StoryEdgeConnectorListener edgeListener)
    {
        title = "START";
        capabilities &= ~Capabilities.Deletable;
        capabilities &= ~Capabilities.Copiable;

        Port outputPort = GeneratePort(Direction.Output, "Out", edgeListener, Port.Capacity.Single, typeof(FlowPort));
        outputContainer.Add(outputPort);

        RefreshExpandedState();
        RefreshPorts();
    }

    public override BaseNodeData GetSaveData()
    {
        return new StartNodeData
        {
            GUID = this.GUID,
            Position = this.GetPosition().position
        };
    }

    public override void LoadSaveData(BaseNodeData data)
    {
        if (data is StartNodeData startData)
        {
            this.GUID = startData.GUID;
            SetPosition(new Rect(startData.Position, Vector2.zero));
        }
    }
}