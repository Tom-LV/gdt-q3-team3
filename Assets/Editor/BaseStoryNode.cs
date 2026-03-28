using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

// 1. Dummy types to represent our different wires
public struct FlowPort { }
public struct BoolPort { }
public struct NumberPort { }

public abstract class BaseStoryNode : Node
{
    public string GUID;
    public Action OnNodeModified;

    public virtual void Initialize(Vector2 position, Action onNodeModified, StoryGraphView graphView = null)
    {
        GUID = Guid.NewGuid().ToString();
        SetPosition(new Rect(position, Vector2.zero));
        OnNodeModified = onNodeModified;
    }

    // 2. We now pass "Type portType" into this method!
    public Port GeneratePort(Direction portDirection, string portName, StoryEdgeConnectorListener edgeListener, Port.Capacity capacity, Type portType)
    {
        Port newPort = InstantiatePort(Orientation.Horizontal, portDirection, capacity, portType);
        newPort.portName = portName;

        if (edgeListener != null)
        {
            if (newPort.edgeConnector != null) newPort.RemoveManipulator(newPort.edgeConnector);
            newPort.AddManipulator(new EdgeConnector<Edge>(edgeListener));
        }

        // 3. Style the ports dynamically!
        if (portType == typeof(FlowPort))
        {
            newPort.portColor = Color.white;
        }
        else if (portType == typeof(BoolPort))
        {
            newPort.portColor = new Color(0.8f, 0.2f, 0.2f); // Red
            StylePortAsDiamond(newPort);
        }
        else if (portType == typeof(NumberPort))
        {
            newPort.portColor = new Color(0.2f, 0.8f, 0.2f); // Green
            StylePortAsDiamond(newPort);
        }

        return newPort;
    }

    private void StylePortAsDiamond(Port port)
    {
        VisualElement connector = port.Q("connector");
        if (connector != null)
        {
            // Remove the circle rounding
            connector.style.borderBottomLeftRadius = 0;
            connector.style.borderBottomRightRadius = 0;
            connector.style.borderTopLeftRadius = 0;
            connector.style.borderTopRightRadius = 0;
            // Rotate 45 degrees
            connector.style.rotate = new StyleRotate(new Rotate(45));
        }
    }

    // Add this inside BaseStoryNode.cs right below GeneratePort()

    public Port GenerateDataPort(Direction portDirection, string portName, StoryEdgeConnectorListener edgeListener, Port.Capacity capacity, Type portType, string initialFallbackValue, Action<string> onFallbackValueChanged)
    {
        // 1. Generate the diamond port
        Port newPort = GeneratePort(portDirection, portName, edgeListener, capacity, portType);

        VisualElement fallbackUI = null;

        // 2. Spawn the correct UI field based on the port type
        if (portType == typeof(BoolPort))
        {
            Toggle boolField = new Toggle();
            boolField.value = bool.TryParse(initialFallbackValue, out bool b) ? b : false;
            boolField.RegisterValueChangedCallback(evt => {
                onFallbackValueChanged?.Invoke(evt.newValue.ToString());
                OnNodeModified?.Invoke();
            });
            fallbackUI = boolField;
        }
        else if (portType == typeof(NumberPort))
        {
            FloatField numberField = new FloatField();
            numberField.style.width = 40;
            numberField.value = float.TryParse(initialFallbackValue, out float f) ? f : 0f;
            numberField.RegisterValueChangedCallback(evt => {
                onFallbackValueChanged?.Invoke(evt.newValue.ToString());
                OnNodeModified?.Invoke();
            });
            fallbackUI = numberField;
        }

        // 3. Attach it to the port and set up the auto-hide logic
        if (fallbackUI != null)
        {
            newPort.contentContainer.Add(fallbackUI);

            newPort.schedule.Execute(() =>
            {
                if (newPort != null && fallbackUI != null)
                {
                    fallbackUI.style.display = newPort.connected ? DisplayStyle.None : DisplayStyle.Flex;
                }
            }).Every(100);
        }

        return newPort;
    }

    public abstract void Draw(StoryEdgeConnectorListener edgeListener);
    public abstract BaseNodeData GetSaveData();
    public abstract void LoadSaveData(BaseNodeData data);
}