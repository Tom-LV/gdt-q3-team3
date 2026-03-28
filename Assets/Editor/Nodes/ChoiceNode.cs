using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements; // Required for EnumField
using UnityEngine;
using UnityEngine.UIElements;

public class ChoiceNode : BaseStoryNode
{
    private List<ChoiceSaveData> _choices = new List<ChoiceSaveData>();

    // --- EXPIRATION VARIABLES ---
    public ChoiceExpirationType ExpirationType = ChoiceExpirationType.None;
    public string FallbackTimer = "5";

    private Port _timerPort;
    private Port _conditionPort;
    private Port _expiredOutPort;

    private StoryEdgeConnectorListener _edgeListener;

    public override void Draw(StoryEdgeConnectorListener edgeListener)
    {
        _edgeListener = edgeListener;

        title = "Player Choice";
        titleContainer.style.backgroundColor = new Color(0.8f, 0.6f, 0.1f, 0.8f);

        // Input Port
        Port inputPort = GeneratePort(Direction.Input, "In", edgeListener, Port.Capacity.Multi, typeof(FlowPort));
        inputContainer.Add(inputPort);

        // --- EXPIRATION DROPDOWN ---
        EnumField expirationDropdown = new EnumField("Expiration", ExpirationType);
        expirationDropdown.RegisterValueChangedCallback(evt =>
        {
            ExpirationType = (ChoiceExpirationType)evt.newValue;
            RefreshExpirationPorts();
            OnNodeModified?.Invoke();
        });
        mainContainer.Add(expirationDropdown);

        // Add Choice Button
        Button addChoiceBtn = new Button(() => { AddChoicePort("New Choice", Guid.NewGuid().ToString(), edgeListener); });
        addChoiceBtn.text = "+ Add Choice";
        titleButtonContainer.Add(addChoiceBtn);

        RefreshExpirationPorts(); // Draw expiration ports if needed
        RefreshExpandedState();
        RefreshPorts();
    }

    private void RefreshExpirationPorts()
    {
        // 1. Clean up existing expiration ports using manual disconnection
        DisconnectAndRemovePort(_timerPort, inputContainer);
        _timerPort = null;

        DisconnectAndRemovePort(_conditionPort, inputContainer);
        _conditionPort = null;

        DisconnectAndRemovePort(_expiredOutPort, outputContainer);
        _expiredOutPort = null;

        // 2. Generate the correct Input Port (Timer or Condition)
        if (ExpirationType == ChoiceExpirationType.Timer)
        {
            _timerPort = GenerateDataPort(Direction.Input, "Time Limit", _edgeListener, Port.Capacity.Single, typeof(NumberPort), FallbackTimer, val => FallbackTimer = val);
            inputContainer.Add(_timerPort);
        }
        else if (ExpirationType == ChoiceExpirationType.Condition)
        {
            _conditionPort = GenerateDataPort(Direction.Input, "Force Expiry", _edgeListener, Port.Capacity.Single, typeof(BoolPort), "False", null);
            inputContainer.Add(_conditionPort);
        }

        // 3. Always add the "Expired" output Flow port if we aren't set to "None"
        if (ExpirationType != ChoiceExpirationType.None)
        {
            _expiredOutPort = GeneratePort(Direction.Output, "Expired", _edgeListener, Port.Capacity.Single, typeof(FlowPort));
            outputContainer.Add(_expiredOutPort);
        }

        RefreshExpandedState();
        RefreshPorts();
    }

    // Helper method to safely delete wires without needing _graphView
    private void DisconnectAndRemovePort(Port port, VisualElement container)
    {
        if (port == null) return;

        if (port.connected)
        {
            var edgesToDisconnect = new List<Edge>(port.connections);
            foreach (Edge edge in edgesToDisconnect)
            {
                edge.input?.Disconnect(edge);
                edge.output?.Disconnect(edge);
                edge.RemoveFromHierarchy();
            }
        }

        if (container.Contains(port))
        {
            container.Remove(port);
        }
    }

    // --- YOUR EXACT ORIGINAL CHOICE UI GENERATOR ---
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
                // Make sure we convert to a list to avoid collection modification errors
                var edgesToDisconnect = new List<Edge>(generatedPort.connections);
                foreach (Edge edge in edgesToDisconnect)
                {
                    edge.input.Disconnect(edge);
                    edge.RemoveFromHierarchy();
                }
            }
            outputContainer.Remove(generatedPort);

            if (_expiredOutPort != null && outputContainer.Contains(_expiredOutPort))
            {
                outputContainer.Add(_expiredOutPort);
            }

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

        if (_expiredOutPort != null && outputContainer.Contains(_expiredOutPort))
        {
            outputContainer.Add(_expiredOutPort);
        }

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
            Choices = new List<ChoiceSaveData>(this._choices),

            // Add Expiration State
            ExpirationType = this.ExpirationType,
            FallbackTimer = this.FallbackTimer
        };
    }

    public override void LoadSaveData(BaseNodeData data)
    {
        if (data is ChoiceNodeData choiceData)
        {
            this.GUID = choiceData.GUID;
            SetPosition(new Rect(choiceData.Position, Vector2.zero));
            this._choices = new List<ChoiceSaveData>(choiceData.Choices);

            // Restore Expiration State
            this.ExpirationType = choiceData.ExpirationType;
            this.FallbackTimer = choiceData.FallbackTimer;
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