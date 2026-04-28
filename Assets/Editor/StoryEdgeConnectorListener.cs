using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class StoryEdgeConnectorListener : IEdgeConnectorListener
{
    private StorySearchWindow _searchWindow;
    private EditorWindow _window;

    public StoryEdgeConnectorListener(StorySearchWindow searchWindow, EditorWindow window)
    {
        _searchWindow = searchWindow;
        _window = window;
    }

    public void OnDropOutsidePort(Edge edge, Vector2 position)
    {
        Port draggedPort = edge.output != null ? edge.output : edge.input;
        _searchWindow.SetDroppedPort(draggedPort);

        Vector2 screenMousePosition = position + _window.position.position;

        SearchWindow.Open(new SearchWindowContext(screenMousePosition), _searchWindow);
    }

    public void OnDrop(GraphView graphView, Edge edge)
    {
        List<Edge> edgesToDelete = new List<Edge>();

        // Check Input Port rules
        if (edge.input != null && edge.input.capacity == Port.Capacity.Single)
            edgesToDelete.AddRange(edge.input.connections);

        // Check Output Port rules
        if (edge.output != null && edge.output.capacity == Port.Capacity.Single)
            edgesToDelete.AddRange(edge.output.connections);

        // Native deletion registers with the Undo System
        if (edgesToDelete.Count > 0)
        {
            graphView.DeleteElements(edgesToDelete);
        }

        // Complete the new connection
        edge.input?.Connect(edge);
        edge.output?.Connect(edge);
        graphView.AddElement(edge);

        // THE FIX: Tell the graph that an edge was successfully connected!
        if (graphView is StoryGraphView storyGraph)
        {
            storyGraph.OnGraphModified?.Invoke();
        }
    }
}