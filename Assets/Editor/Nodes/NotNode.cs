using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class NotNode : BaseStoryNode
{
    public string Fallback = "False";

    public override void Draw(StoryEdgeConnectorListener edgeListener)
    {
        title = "NOT";
        titleContainer.style.backgroundColor = new Color(0.8f, 0.4f, 0.2f, 0.8f);

        // Input (Bool - Red Diamond)
        Port inputPort = GenerateDataPort(Direction.Input, "In", edgeListener, Port.Capacity.Single, typeof(BoolPort), Fallback, val => Fallback = val);
        inputContainer.Add(inputPort);

        // Output (Bool - Red Diamond)
        Port outputPort = GeneratePort(Direction.Output, "Out", edgeListener, Port.Capacity.Multi, typeof(BoolPort));
        outputContainer.Add(outputPort);

        RefreshExpandedState();
        RefreshPorts();
    }

    public override BaseNodeData GetSaveData() => new NotNodeData { GUID = this.GUID, Position = this.GetPosition().position, Fallback = this.Fallback };
    public override void LoadSaveData(BaseNodeData data) { if (data is NotNodeData d) { GUID = d.GUID; Fallback = d.Fallback; SetPosition(new Rect(d.Position, Vector2.zero)); } }
}