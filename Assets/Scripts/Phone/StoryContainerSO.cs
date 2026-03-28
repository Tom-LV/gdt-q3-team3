using System;
using System.Collections.Generic;
using UnityEngine;

// CHARACTERS

[Serializable]
public class CharacterProfile
{
    public string GUID;
    public string CharacterName = "New Character";
    public Color ChatColor = Color.white;
}


// NODES

[Serializable]
public abstract class BaseNodeData
{
    public string GUID;
    public Vector2 Position;
}

[Serializable]
public class ConditionNodeData : BaseNodeData
{
    public string VariableName = "HasKey";
    public string Operator = "==";
    public string Value = "True";
}

[Serializable]
public class EventNodeData : BaseNodeData
{
    public string EventName = "PlaySound";
    public string Parameter = "Explosion";
}

[Serializable]
public class WaitUntilNodeData : BaseNodeData
{
    public string VariableName = "HasKey";
    public string Operator = "==";
    public string Value = "True";
}

[Serializable]
public class SetVariableNodeData : BaseNodeData
{
    public string VariableName = "HasKey";
    public string Operator = "==";
    public string Value = "True";
}

[Serializable]
public class StartNodeData : BaseNodeData { }

[Serializable]
public class MessageNodeData : BaseNodeData
{
    public string CharacterGUID;
    public string MessageText;
    public string FallbackDelay = "0";
}

[Serializable]
public class ChoiceSaveData
{
    public string PortID;
    public string ChoiceText;
}

[Serializable]
public class ChoiceNodeData : BaseNodeData
{
    public List<ChoiceSaveData> Choices = new List<ChoiceSaveData>();
}

[Serializable]
public class TimerNodeData : BaseNodeData {
    public float WaitTime;
}

[Serializable]
public class VariableNodeData : BaseNodeData
{
    public string VariableName;
}

[Serializable]
public class CompareNodeData : BaseNodeData
{
    public string Operator = "==";
    public string FallbackA = "0";
    public string FallbackB = "0";
}

[Serializable]
public class MathNodeData : BaseNodeData
{
    public string Operator = "+";
    public string FallbackA = "0";
    public string FallbackB = "0";
}


[Serializable]
public class StoryLinkData
{
    public string BaseNodeGUID;
    public string BasePortName;
    public string TargetNodeGUID;
    public string TargetPortName;
}

[Serializable]
public class LogicNodeData : BaseNodeData
{
    public string Operator = "AND";
    public string FallbackA = "False";
    public string FallbackB = "False";
}

[Serializable]
public class NotNodeData : BaseNodeData
{
    public string Fallback = "False";
}

public enum VariableType { Bool, Number }

[System.Serializable]
public class VariableSaveData
{
    public string GUID;
    public string Name = "New Variable";
    public VariableType Type = VariableType.Bool;
    public string Value = "";
}

// THE CONTAINER

[CreateAssetMenu(menuName = "Story/Story Container", fileName = "New Story")]
public class StoryContainerSO : ScriptableObject
{
    public List<CharacterProfile> Characters = new List<CharacterProfile>();
    public List<VariableSaveData> Variables = new List<VariableSaveData>();

    public List<StoryLinkData> NodeLinks = new List<StoryLinkData>();

    [SerializeReference]
    public List<BaseNodeData> StoryNodes = new List<BaseNodeData>();
}