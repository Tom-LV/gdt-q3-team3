using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

public class EventNode : BaseStoryNode
{
    public string EventName = "PlaySound";
    public string Parameter = "Explosion";

    public override void Draw(StoryEdgeConnectorListener edgeListener)
    {
        title = "Trigger Event";
        titleContainer.style.backgroundColor = new Color(0.8f, 0.3f, 0.1f, 0.8f); // Orange

        // Ports
        Port inputPort = GeneratePort(Direction.Input, "In", edgeListener, Port.Capacity.Multi, typeof(FlowPort));
        inputContainer.Add(inputPort);

        Port outputPort = GeneratePort(Direction.Output, "Out", edgeListener, Port.Capacity.Single, typeof(FlowPort));
        outputContainer.Add(outputPort);

        // UI Fields
        TextField eventField = new TextField("Event");
        eventField.value = EventName;
        eventField.RegisterValueChangedCallback(evt => { EventName = evt.newValue; OnNodeModified?.Invoke(); });
        mainContainer.Add(eventField);

        TextField paramField = new TextField("Parameter");
        paramField.value = Parameter;
        paramField.RegisterValueChangedCallback(evt => { Parameter = evt.newValue; OnNodeModified?.Invoke(); });
        mainContainer.Add(paramField);

        RefreshExpandedState();
        RefreshPorts();
    }

    public override BaseNodeData GetSaveData()
    {
        return new EventNodeData { GUID = this.GUID, Position = this.GetPosition().position, EventName = this.EventName, Parameter = this.Parameter };
    }

    public override void LoadSaveData(BaseNodeData data)
    {
        if (data is EventNodeData eventData)
        {
            this.GUID = eventData.GUID;
            this.EventName = eventData.EventName;
            this.Parameter = eventData.Parameter;
            SetPosition(new Rect(eventData.Position, Vector2.zero));
        }
    }
}