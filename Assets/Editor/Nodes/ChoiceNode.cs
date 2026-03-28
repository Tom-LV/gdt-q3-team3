using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

public class ChoiceNode : BaseStoryNode
{
    private List<ChoiceSaveData> _choices = new List<ChoiceSaveData>();

    public override void Draw(StoryEdgeConnectorListener edgeListener)
    {
        title = "Player Choice";
        titleContainer.style.backgroundColor = new Color(0.8f, 0.6f, 0.1f, 0.8f);

        // Input Port
        Port inputPort = GeneratePort(Direction.Input, "In", edgeListener, Port.Capacity.Multi, typeof(FlowPort));
        inputContainer.Add(inputPort);

        // Add Choice Button
        Button addChoiceBtn = new Button(() => { AddChoicePort("New Choice", Guid.NewGuid().ToString(), edgeListener); });
        addChoiceBtn.text = "+ Add Choice";
        titleButtonContainer.Add(addChoiceBtn);

        RefreshExpandedState();
        RefreshPorts();
    }

    public void AddChoicePort(string choiceText, string portID, StoryEdgeConnectorListener edgeListener)
    {
        Port generatedPort = GeneratePort(Direction.Output, portID, edgeListener, Port.Capacity.Single, typeof(FlowPort));

        Label portLabel = generatedPort.contentContainer.Q<Label>();
        if (portLabel != null) { portLabel.style.display = DisplayStyle.None; }

        // Track it in our local list
        var choiceData = new ChoiceSaveData { PortID = portID, ChoiceText = choiceText };
        _choices.Add(choiceData);

        VisualElement customContainer = new VisualElement();
        customContainer.style.flexDirection = FlexDirection.Row;
        customContainer.style.alignItems = Align.Center;

        Button deleteBtn = new Button(() =>
        {
            if (generatedPort.connected)
            {
                foreach (Edge edge in generatedPort.connections)
                {
                    edge.input.Disconnect(edge);
                    edge.RemoveFromHierarchy();
                }
            }
            outputContainer.Remove(generatedPort);
            _choices.Remove(choiceData);
            OnNodeModified?.Invoke();
            RefreshPorts();
        })
        { text = "X" };

        // Clean up the button look
        deleteBtn.style.backgroundColor = new Color(0.7f, 0.2f, 0.2f);
        deleteBtn.style.width = 20;
        deleteBtn.style.marginRight = 5;

        TextField choiceTextField = new TextField();
        choiceTextField.value = choiceText;
        choiceTextField.style.width = 150;
        choiceTextField.RegisterValueChangedCallback(evt =>
        {
            choiceData.ChoiceText = evt.newValue;
            OnNodeModified?.Invoke();
        });

        // Add them to the row layout, then add the row to the port
        customContainer.Add(deleteBtn);
        customContainer.Add(choiceTextField);
        generatedPort.contentContainer.Add(customContainer);

        outputContainer.Add(generatedPort);

        OnNodeModified?.Invoke();
        RefreshExpandedState();
        RefreshPorts();
    }

    // --- SAVE AND LOAD ---

    public override BaseNodeData GetSaveData()
    {
        return new ChoiceNodeData
        {
            GUID = this.GUID,
            Position = this.GetPosition().position,
            Choices = new List<ChoiceSaveData>(this._choices)
        };
    }

    public override void LoadSaveData(BaseNodeData data)
    {
        if (data is ChoiceNodeData choiceData)
        {
            this.GUID = choiceData.GUID;
            SetPosition(new Rect(choiceData.Position, Vector2.zero));
            this._choices = new List<ChoiceSaveData>(choiceData.Choices);
        }
    }

    public void RestoreChoices(StoryEdgeConnectorListener edgeListener)
    {
        var savedChoices = new List<ChoiceSaveData>(_choices);
        _choices.Clear();

        foreach (var choice in savedChoices)
        {
            AddChoicePort(choice.ChoiceText, choice.PortID, edgeListener);
        }
    }
}