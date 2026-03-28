using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class StoryGraphSaveUtility
{
    private StoryGraphView _targetGraphView;
    private StoryContainerSO _containerCache;

    public static StoryGraphSaveUtility GetInstance(StoryGraphView targetGraphView)
    {
        return new StoryGraphSaveUtility { _targetGraphView = targetGraphView };
    }

    public void SaveGraph(string filePath)
    {
        var existingAsset = AssetDatabase.LoadAssetAtPath<StoryContainerSO>(filePath);
        if (existingAsset == null)
        {
            existingAsset = ScriptableObject.CreateInstance<StoryContainerSO>();
            AssetDatabase.CreateAsset(existingAsset, filePath);
        }

        // Just pass the hard drive file to the memory packer
        SaveToContainer(existingAsset);

        EditorUtility.SetDirty(existingAsset);
        AssetDatabase.SaveAssets();
    }

    public void LoadGraph(string filePath)
    {
        var existingAsset = AssetDatabase.LoadAssetAtPath<StoryContainerSO>(filePath);
        if (existingAsset == null) return;

        // Just pass the hard drive file to the memory unpacker
        LoadFromContainer(existingAsset);
    }

    // Packs the graph into any container
    public void SaveToContainer(StoryContainerSO container)
    {
        container.Characters = new List<CharacterProfile>(_targetGraphView.Characters);
        container.Variables = new List<VariableSaveData>(_targetGraphView.Variables);
        container.NodeLinks.Clear();
        container.StoryNodes.Clear();

        var connectedPorts = _targetGraphView.edges.ToList().Where(x => x.input.node != null).ToArray();
        foreach (var edge in connectedPorts)
        {
            var outputNode = edge.output.node as BaseStoryNode;
            var inputNode = edge.input.node as BaseStoryNode;

            container.NodeLinks.Add(new StoryLinkData
            {
                BaseNodeGUID = outputNode.GUID,
                BasePortName = edge.output.portName,
                TargetNodeGUID = inputNode.GUID,
                TargetPortName = edge.input.portName
            });
        }

        var nodes = _targetGraphView.nodes.ToList().Cast<BaseStoryNode>().ToList();
        foreach (var node in nodes)
        {
            container.StoryNodes.Add(node.GetSaveData());
        }
    }

    // Unpacks the graph from any container
    public void LoadFromContainer(StoryContainerSO container)
    {
        _containerCache = container;
        _targetGraphView.Characters = new List<CharacterProfile>(container.Characters);
        _targetGraphView.Variables = new List<VariableSaveData>(container.Variables);
        ClearGraph();
        GenerateNodes();
        ConnectNodes();
    }

    private void ClearGraph()
    {
        foreach (var node in _targetGraphView.nodes.ToList())
            _targetGraphView.RemoveElement(node);

        foreach (var edge in _targetGraphView.edges.ToList())
            _targetGraphView.RemoveElement(edge);
    }

    private void GenerateNodes()
    {
        foreach (var nodeData in _containerCache.StoryNodes)
        {
            BaseStoryNode tempNode = null;

            // Creates the correct node
            if (nodeData is StartNodeData) { tempNode = new StartNode(); }
            else if (nodeData is MessageNodeData) { tempNode = new MessageNode(); }
            else if (nodeData is ChoiceNodeData) { tempNode = new ChoiceNode(); }
            else if (nodeData is TimerNodeData) { tempNode = new TimerNode(); }
            else if (nodeData is ConditionNodeData) { tempNode = new ConditionNode(); }
            else if (nodeData is EventNodeData) { tempNode = new EventNode(); }
            else if (nodeData is WaitUntilNodeData) { tempNode = new WaitUntilNode(); }
            else if (nodeData is SetVariableNodeData) { tempNode = new SetVariableNode(); }
            else if (nodeData is VariableNodeData) { tempNode = new VariableNode(); }
            else if (nodeData is CompareNodeData) { tempNode = new CompareNode(); }
            else if (nodeData is MathNodeData) { tempNode = new MathNode(); }
            else if (nodeData is LogicNodeData) { tempNode = new LogicNode(); }
            else if (nodeData is NotNodeData) { tempNode = new NotNode(); }

            if (tempNode != null)
            {
                tempNode.Initialize(Vector2.zero, null, _targetGraphView);
                tempNode.LoadSaveData(nodeData);
                tempNode.Draw(_targetGraphView.EdgeListener);

                if (tempNode is ChoiceNode cNode)
                {
                    cNode.RestoreChoices(_targetGraphView.EdgeListener);
                }

                _targetGraphView.AddElement(tempNode);
            }
        }
    }

    private void ConnectNodes()
    {
        var allNodes = _targetGraphView.nodes.ToList().Cast<BaseStoryNode>().ToList();

        foreach (var node in allNodes)
        {
            var connections = _containerCache.NodeLinks.Where(x => x.BaseNodeGUID == node.GUID).ToList();
            foreach (var connection in connections)
            {
                var targetNode = allNodes.FirstOrDefault(x => x.GUID == connection.TargetNodeGUID);
                if (targetNode != null)
                {
                    // Search the ports by the 'portName' property
                    Port outputPort = node.outputContainer.Query<Port>().ToList().FirstOrDefault(p => p.portName == connection.BasePortName);

                    Port inputPort = targetNode.inputContainer.Query<Port>().ToList().FirstOrDefault(p => p.portName == connection.TargetPortName);

                    if (outputPort != null && inputPort != null)
                    {
                        LinkNodes(outputPort, inputPort);
                    }
                }
            }
        }
    }

    private void LinkNodes(Port output, Port input)
    {
        var tempEdge = new Edge { output = output, input = input };
        tempEdge?.input.Connect(tempEdge);
        tempEdge?.output.Connect(tempEdge);
        _targetGraphView.AddElement(tempEdge);
    }
}