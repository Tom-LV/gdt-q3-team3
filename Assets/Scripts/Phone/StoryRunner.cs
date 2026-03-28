using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StoryRunner : MonoBehaviour
{
    public static StoryRunner Instance { get; private set; }

    [Header("Story Asset")]
    public StoryContainerSO StoryGraph;


    // --- EXTERNAL EVENTS ---
    // Other scripts (like AudioManagers or QuestManagers) can listen to this!
    public event Action<string> OnStoryEvent;

    public NotificationGlow notificationGlow;

    // --- RUNTIME MEMORY (BLACKBOARD) ---
    private Dictionary<string, float> _numberVariables = new Dictionary<string, float>();
    private Dictionary<string, bool> _boolVariables = new Dictionary<string, bool>();

    // Internal State
    private BaseNodeData _currentNode;
    private bool _isWaitingForInput = false;
    private string _selectedChoicePortID = "";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (StoryGraph != null)
        {
            Invoke(nameof(BeginStory), 0.5f);
        }
    }

    private void BeginStory()
    {
        InitializeVariables();
        StartCoroutine(RunStory());
    }

    // --- EXTERNAL VARIABLE API (For Inventory, Quests, outside code) ---
    public void SetVariable(string varName, float value) { _numberVariables[varName] = value; }
    public void SetVariable(string varName, bool value) { _boolVariables[varName] = value; }
    public float GetNumber(string varName) => _numberVariables.ContainsKey(varName) ? _numberVariables[varName] : 0f;
    public bool GetBool(string varName) => _boolVariables.ContainsKey(varName) ? _boolVariables[varName] : false;

    private void InitializeVariables()
    {
        _numberVariables.Clear();
        _boolVariables.Clear();

        // Load default starting values from the Asset
        foreach (var v in StoryGraph.Variables)
        {
            if (v.Type == VariableType.Number) _numberVariables[v.Name] = 0f;
            else if (v.Type == VariableType.Bool) _boolVariables[v.Name] = false;
        }
    }

    private IEnumerator RunStory()
    {
        var startNode = StoryGraph.StoryNodes.FirstOrDefault(n => n is StartNodeData);
        if (startNode == null) yield break;

        _currentNode = GetNextNode(startNode.GUID, "Out");

        // The Main Game Loop
        while (_currentNode != null)
        {
            if (_currentNode is MessageNodeData msgNode)
            {
                // 1. Evaluate the dynamic delay port FIRST!
                float customDelay = ResolveNumber(msgNode.GUID, "Delay", msgNode.FallbackDelay);
                if (customDelay > 0) yield return new WaitForSeconds(customDelay);

                // 2. Process the message UI
                yield return StartCoroutine(ProcessMessageNode(msgNode));
                _currentNode = GetNextNode(_currentNode.GUID, "Out");
            }
            else if (_currentNode is ChoiceNodeData choiceNode)
            {
                yield return StartCoroutine(ProcessChoiceNode(choiceNode));
                // _currentNode is updated inside ProcessChoiceNode
            }
            else if (_currentNode is TimerNodeData timerNode)
            {
                // Resolve the math/number plugged into the Timer
                float waitTime = ResolveNumber(timerNode.GUID, "Wait Time", timerNode.WaitTime.ToString());
                yield return new WaitForSeconds(waitTime);
                _currentNode = GetNextNode(_currentNode.GUID, "Out");
            }
            else if (_currentNode is ConditionNodeData condNode)
            {
                // Resolve all the boolean logic plugged into the Condition
                bool conditionPassed = ResolveBool(condNode.GUID, "Condition", condNode.Value);
                string portToFollow = conditionPassed ? "True" : "False";
                _currentNode = GetNextNode(_currentNode.GUID, portToFollow);
            }
            else if (_currentNode is WaitUntilNodeData waitNode)
            {
                // Pause the loop entirely until the boolean logic becomes true!
                yield return new WaitUntil(() => ResolveBool(waitNode.GUID, "Condition", waitNode.Value));
                _currentNode = GetNextNode(_currentNode.GUID, "Out");
            }
            else if (_currentNode is SetVariableNodeData setVarNode)
            {
                // Update the Blackboard memory
                var variableMeta = StoryGraph.Variables.FirstOrDefault(v => v.Name == setVarNode.VariableName);
                if (variableMeta != null)
                {
                    if (variableMeta.Type == VariableType.Bool)
                        _boolVariables[setVarNode.VariableName] = ResolveBool(setVarNode.GUID, "Value", setVarNode.Value);
                    else
                        _numberVariables[setVarNode.VariableName] = ResolveNumber(setVarNode.GUID, "Value", setVarNode.Value);
                }
                _currentNode = GetNextNode(_currentNode.GUID, "Out");
            }
            else if (_currentNode is EventNodeData eventNode)
            {
                // Broadcast to outside scripts
                Debug.Log($"[STORY EVENT FIRED]: {eventNode.EventName}");
                OnStoryEvent?.Invoke(eventNode.EventName);
                _currentNode = GetNextNode(_currentNode.GUID, "Out");
            }
            else
            {
                Debug.LogWarning($"Hit an unknown or pure data node type! ({_currentNode.GetType()})");
                break;
            }
        }

        Debug.Log("Reached the end of the story graph!");
    }

    // --- NODE PROCESSING LOGIC ---

    private IEnumerator ProcessMessageNode(MessageNodeData msgNode)
    {
        // Note: Check if your class uses CharacterID or CharacterGUID and match it here!
        var character = StoryGraph.Characters.FirstOrDefault(c => c.GUID == msgNode.CharacterGUID);

        string charName = character != null ? character.CharacterName : "Unknown";
        Color charColor = character != null ? character.ChatColor : Color.white;

        if (notificationGlow != null) notificationGlow.TriggerGlow();
        // Send it directly to your UI Toolkit PhoneOS!
        PhoneOS.Instance.GetApp<ChatApp>().ReceiveMessage(charName, msgNode.MessageText, charColor);

        // Add a slight artificial delay based on message length so it feels like texting
        float readTime = Mathf.Clamp(msgNode.MessageText.Length * 0.05f, 1.0f, 3.0f);
        yield return new WaitForSeconds(readTime);
    }

    private IEnumerator ProcessChoiceNode(ChoiceNodeData choiceNode)
    {
        _selectedChoicePortID = "";
        _isWaitingForInput = true;

        PhoneOS.Instance.GetApp<ChatApp>().ShowChoices(choiceNode.Choices, (portID) =>
        {
            _selectedChoicePortID = portID;
            _isWaitingForInput = false;
        });

        yield return new WaitUntil(() => !_isWaitingForInput);

        _currentNode = GetNextNode(_currentNode.GUID, _selectedChoicePortID);
    }

    // --- GRAPH NAVIGATION HELPER ---
    private BaseNodeData GetNextNode(string currentNodeGUID, string portName)
    {
        var link = StoryGraph.NodeLinks.FirstOrDefault(x => x.BaseNodeGUID == currentNodeGUID && x.BasePortName == portName);
        if (link != null)
        {
            return StoryGraph.StoryNodes.FirstOrDefault(n => n.GUID == link.TargetNodeGUID);
        }
        return null;
    }

    // =========================================================================
    // --- DATA RESOLUTION (RECURSIVE MATH AND LOGIC) ---
    // =========================================================================

    private float ResolveNumber(string targetNodeGUID, string portName, string fallback)
    {
        var link = StoryGraph.NodeLinks.FirstOrDefault(l => l.TargetNodeGUID == targetNodeGUID && l.TargetPortName == portName);
        if (link == null) return float.TryParse(fallback, out float f) ? f : 0f;

        var sourceNode = StoryGraph.StoryNodes.FirstOrDefault(n => n.GUID == link.BaseNodeGUID);

        if (sourceNode is VariableNodeData varNode) return GetNumber(varNode.VariableName);
        if (sourceNode is MathNodeData mathNode)
        {
            float a = ResolveNumber(mathNode.GUID, "A", mathNode.FallbackA);
            float b = ResolveNumber(mathNode.GUID, "B", mathNode.FallbackB);
            switch (mathNode.Operator)
            {
                case "+": return a + b;
                case "-": return a - b;
                case "*": return a * b;
                case "÷": return b != 0 ? a / b : 0;
                default: return 0;
            }
        }
        return 0f;
    }

    private bool ResolveBool(string targetNodeGUID, string portName, string fallback)
    {
        var link = StoryGraph.NodeLinks.FirstOrDefault(l => l.TargetNodeGUID == targetNodeGUID && l.TargetPortName == portName);
        if (link == null) return bool.TryParse(fallback, out bool b) ? b : false;

        var sourceNode = StoryGraph.StoryNodes.FirstOrDefault(n => n.GUID == link.BaseNodeGUID);

        if (sourceNode is VariableNodeData varNode) return GetBool(varNode.VariableName);
        if (sourceNode is NotNodeData notNode) return !ResolveBool(notNode.GUID, "In", notNode.Fallback);

        if (sourceNode is LogicNodeData logicNode)
        {
            bool a = ResolveBool(logicNode.GUID, "A", logicNode.FallbackA);
            bool b = ResolveBool(logicNode.GUID, "B", logicNode.FallbackB);
            return logicNode.Operator == "AND" ? (a && b) : (a || b);
        }

        if (sourceNode is CompareNodeData compNode)
        {
            float a = ResolveNumber(compNode.GUID, "A", compNode.FallbackA);
            float b = ResolveNumber(compNode.GUID, "B", compNode.FallbackB);
            switch (compNode.Operator)
            {
                case "==": return Mathf.Approximately(a, b);
                case "!=": return !Mathf.Approximately(a, b);
                case ">": return a > b;
                case "<": return a < b;
                case ">=": return a >= b;
                case "<=": return a <= b;
                default: return false;
            }
        }
        return false;
    }
}