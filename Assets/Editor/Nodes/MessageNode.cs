using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;

public class MessageNode : BaseStoryNode
{
    public string MessageText;
    public string CharacterGUID;

    private StoryGraphView _graphView;
    private DropdownField _characterDropdown;
    public string FallbackDelay = "0";

    public override void Initialize(Vector2 position, System.Action onNodeModified, StoryGraphView graphView = null)
    {
        base.Initialize(position, onNodeModified, graphView);
        _graphView = graphView;

        if (_graphView != null)
        {
            _graphView.OnCharactersUpdated += RefreshDropdown;

            this.RegisterCallback<DetachFromPanelEvent>(evt =>
            {
                if (_graphView != null)
                {
                    _graphView.OnCharactersUpdated -= RefreshDropdown;
                }
            });
        }
    }

    public override void Draw(StoryEdgeConnectorListener edgeListener)
    {
        title = "Send Phone Message";

        // The character dropdown
        _characterDropdown = new DropdownField("Character");
        _characterDropdown.RegisterValueChangedCallback(evt =>
        {
            if (_graphView == null) return;
            int newIndex = _graphView.Characters.FindIndex(c => c.CharacterName == evt.newValue);
            if (newIndex >= 0)
            {
                CharacterGUID = _graphView.Characters[newIndex].GUID;
                OnNodeModified?.Invoke();
            }
        });

        RefreshDropdown(); // Populate it
        mainContainer.Add(_characterDropdown);

        // Ports
        Port inputPort = GeneratePort(Direction.Input, "In", edgeListener, Port.Capacity.Multi, typeof(FlowPort));
        inputContainer.Add(inputPort);

        Port delayPort = GenerateDataPort(Direction.Input, "Delay", edgeListener, Port.Capacity.Single, typeof(NumberPort), FallbackDelay, val => FallbackDelay = val);
        inputContainer.Add(delayPort);

        Port outputPort = GeneratePort(Direction.Output, "Out", edgeListener, Port.Capacity.Single, typeof(FlowPort));
        outputContainer.Add(outputPort);

        TextField textField = new TextField(string.Empty);
        textField.RegisterValueChangedCallback(evt => { MessageText = evt.newValue; OnNodeModified?.Invoke(); });
        textField.SetValueWithoutNotify(MessageText);
        textField.multiline = true;
        textField.style.maxWidth = 200;
        textField.style.whiteSpace = WhiteSpace.Normal;
        mainContainer.Add(textField);

        RefreshExpandedState();
        RefreshPorts();
    }

    // Instantly updates the dropdown when the panel changes
    private void RefreshDropdown()
    {
        if (_graphView == null || _characterDropdown == null) return;

        // Temporarily mute the OnValueChanged callback so we don't accidentally trigger an Undo Snapshot while we are currently doing an Undo
        _characterDropdown.SetValueWithoutNotify("Updating...");

        if (_graphView.Characters.Count > 0)
        {
            _characterDropdown.choices = _graphView.Characters.ConvertAll(c => c.CharacterName);

            int index = _graphView.Characters.FindIndex(c => c.GUID == CharacterGUID);
            if (index >= 0)
            {
                _characterDropdown.SetValueWithoutNotify(_characterDropdown.choices[index]);
            }
            else
            {
                CharacterGUID = _graphView.Characters[0].GUID;
                _characterDropdown.SetValueWithoutNotify(_characterDropdown.choices[0]);
            }
        }
        else
        {
            _characterDropdown.choices = new List<string> { "No Characters Added" };
            _characterDropdown.SetValueWithoutNotify("No Characters Added");
        }
    }

    public override BaseNodeData GetSaveData()
    {
        return new MessageNodeData { GUID = this.GUID, Position = this.GetPosition().position, MessageText = this.MessageText, CharacterGUID = this.CharacterGUID, FallbackDelay = this.FallbackDelay };
    }

    public override void LoadSaveData(BaseNodeData data)
    {
        if (data is MessageNodeData msgData)
        {
            this.GUID = msgData.GUID;
            this.MessageText = msgData.MessageText;
            this.CharacterGUID = msgData.CharacterGUID;
            this.FallbackDelay = msgData.FallbackDelay;
            SetPosition(new Rect(msgData.Position, UnityEngine.Vector2.zero));
        }
    }
}