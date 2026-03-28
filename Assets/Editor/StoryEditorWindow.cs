using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class StoryEditorWindow : EditorWindow
{
    private StoryGraphView graphView;
    private StorySearchWindow searchWindow;

    // State Tracking
    [SerializeField] private string currentFilePath = "";
    [SerializeField] private string _serializedGraphState = ""; // Unity Native Undo Target

    private bool isGraphDirty = false;
    private string _lastAppliedJson = "";

    // Safety Locks
    private bool _isRestoring = false;
    private bool _isInitializing = false;
    private bool _savePending = false;

    [MenuItem("Tools/Story Editor")]
    public static void OpenStoryWindow()
    {
        StoryEditorWindow window = GetWindow<StoryEditorWindow>("Story Editor");
        window.UpdateTitle();
    }

    [OnOpenAsset(1)]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        var asset = EditorUtility.EntityIdToObject(instanceID) as StoryContainerSO;
        if (asset != null)
        {
            StoryEditorWindow window = GetWindow<StoryEditorWindow>("Story Editor");
            string path = AssetDatabase.GetAssetPath(asset);
            window.LoadGraphFromFile(path);
            return true;
        }
        return false;
    }

    private void OnEnable()
    {
        _isInitializing = true;

        ConstructGraphView();
        GenerateToolbar();
        SetupShortcuts();
        Undo.undoRedoPerformed += OnUndoRedo;

        if (!string.IsNullOrEmpty(currentFilePath))
        {
            LoadGraphFromFile(currentFilePath);
            _isInitializing = false;
        }
        else
        {
            // Brand new graph (no file). Generate default baseline.
            var temp = ScriptableObject.CreateInstance<StoryContainerSO>();
            StoryGraphSaveUtility.GetInstance(graphView).SaveToContainer(temp);
            _serializedGraphState = EditorJsonUtility.ToJson(temp);
            _lastAppliedJson = _serializedGraphState;
            DestroyImmediate(temp);
            Undo.ClearUndo(this);
            _isInitializing = false;
        }
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(graphView);
        Undo.undoRedoPerformed -= OnUndoRedo;
    }

    private void ConstructGraphView()
    {
        graphView = new StoryGraphView { name = "Story Graph" };
        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);

        searchWindow = ScriptableObject.CreateInstance<StorySearchWindow>();
        searchWindow.Init(graphView, this, MarkAsDirty);

        var edgeListener = new StoryEdgeConnectorListener(searchWindow, this);
        graphView.EdgeListener = edgeListener;

        graphView.nodeCreationRequest = context =>
        {
            searchWindow.SetDroppedPort(null);
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        };

        graphView.GenerateEntryPointNode();

        graphView.graphViewChanged = (changes) =>
        {
            MarkAsDirty();
            return changes;
        };
        graphView.OnCharactersUpdated += MarkAsDirty;
        graphView.OnVariablesUpdated += MarkAsDirty;
        graphView.OnGraphModified = MarkAsDirty;

        graphView.RegisterCallback<MouseUpEvent>(evt => MarkAsDirty(), TrickleDown.TrickleDown);
    }

    private void GenerateToolbar()
    {
        Toolbar toolbar = new Toolbar();

        toolbar.Add(new Button(SaveGraph) { text = "Save" });
        toolbar.Add(new Button(SaveAsGraph) { text = "Save As..." });

        Button btnLoad = new Button(() =>
        {
            string absolutePath = EditorUtility.OpenFilePanel("Load Story Graph", "Assets", "asset");
            if (!string.IsNullOrEmpty(absolutePath))
            {
                string unityPath = FileUtil.GetProjectRelativePath(absolutePath);
                LoadGraphFromFile(unityPath);
            }
        })
        { text = "Open File" };
        toolbar.Add(btnLoad);

        ToolbarSpacer spacer = new ToolbarSpacer();
        spacer.style.flexGrow = 1;
        toolbar.Add(spacer);

        ToolbarToggle btnCharacters = new ToolbarToggle();
        Texture2D characterIcon = EditorGUIUtility.IconContent("Toolbar Plus More").image as Texture2D;

        if (characterIcon != null)
        {
            btnCharacters.Add(new Image { image = characterIcon });
            btnCharacters.tooltip = "Toggle Character Inspector";
        }
        else
        {
            btnCharacters.text = "Characters";
        }

        btnCharacters.RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue) graphView.OpenCharacterWindow();
            else graphView.CloseCharacterWindow();
        });

        toolbar.Add(btnCharacters);
        rootVisualElement.Add(toolbar);
    }

    private void SetupShortcuts()
    {
        rootVisualElement.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.keyCode == KeyCode.S && (evt.ctrlKey || evt.commandKey))
            {
                SaveGraph();
                evt.StopPropagation();
            }
        });
    }

    private void SaveGraph()
    {
        if (string.IsNullOrEmpty(currentFilePath)) SaveAsGraph();
        else
        {
            StoryGraphSaveUtility.GetInstance(graphView).SaveGraph(currentFilePath);
            isGraphDirty = false;
            UpdateTitle();

            // Sync the baseline after saving manually!
            _serializedGraphState = EditorJsonUtility.ToJson(AssetDatabase.LoadAssetAtPath<StoryContainerSO>(currentFilePath));
            _lastAppliedJson = _serializedGraphState;
        }
    }

    private void SaveAsGraph()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Story Graph", "NewStory", "asset", "Choose where to save.");
        if (!string.IsNullOrEmpty(path))
        {
            currentFilePath = path;
            SaveGraph();
        }
    }

    public void LoadGraphFromFile(string path)
    {
        _isRestoring = true;
        currentFilePath = path;

        // 1. YOUR FIX: Get the actual file from disk
        var asset = AssetDatabase.LoadAssetAtPath<StoryContainerSO>(path);
        if (asset != null)
        {
            // 2. Set the baseline directly from the file!
            _serializedGraphState = EditorJsonUtility.ToJson(asset);
            _lastAppliedJson = _serializedGraphState;

            // 3. Now push it into the UI
            StoryGraphSaveUtility.GetInstance(graphView).LoadFromContainer(asset);
        }

        ReattachNodeEvents();
        graphView.RefreshCharacterWindow();
        graphView.RefreshVariableWindow();

        isGraphDirty = false;
        UpdateTitle();
        Undo.ClearUndo(this); // Wipe any boot-up undo steps

        _isRestoring = false;
    }

    private void MarkAsDirty()
    {
        if (_isRestoring || _isInitializing || graphView == null) return;

        if (!_savePending)
        {
            _savePending = true;

            rootVisualElement.schedule.Execute(() =>
            {
                _savePending = false;
                if (graphView == null) return;

                var temp = ScriptableObject.CreateInstance<StoryContainerSO>();
                StoryGraphSaveUtility.GetInstance(graphView).SaveToContainer(temp);
                string currentJson = EditorJsonUtility.ToJson(temp);
                DestroyImmediate(temp);

                if (currentJson != _serializedGraphState)
                {
                    if (!isGraphDirty) { isGraphDirty = true; UpdateTitle(); }

                    // Record the PREVIOUS state into the Undo Stack
                    Undo.RecordObject(this, "Story Graph Edit");

                    // Update to the NEW state
                    _serializedGraphState = currentJson;
                    _lastAppliedJson = currentJson;

                    EditorUtility.SetDirty(this);
                }
            }).ExecuteLater(10);
        }
    }

    private void OnUndoRedo()
    {
        if (_serializedGraphState == _lastAppliedJson) return;

        _isRestoring = true;

        var temp = ScriptableObject.CreateInstance<StoryContainerSO>();
        EditorJsonUtility.FromJsonOverwrite(_serializedGraphState, temp);
        StoryGraphSaveUtility.GetInstance(graphView).LoadFromContainer(temp);

        _lastAppliedJson = _serializedGraphState;
        DestroyImmediate(temp);

        ReattachNodeEvents();
        graphView.RefreshCharacterWindow();
        graphView.RefreshVariableWindow();

        isGraphDirty = true;
        UpdateTitle();

        _isRestoring = false;
    }

    private void ReattachNodeEvents()
    {
        foreach (var node in graphView.nodes.ToList())
        {
            if (node is BaseStoryNode baseNode)
                baseNode.OnNodeModified = MarkAsDirty;
        }
    }

    private void UpdateTitle()
    {
        string fileName = string.IsNullOrEmpty(currentFilePath) ? "New Story" : Path.GetFileNameWithoutExtension(currentFilePath);
        titleContent = new GUIContent($"{fileName} {(isGraphDirty ? "*" : "")}");
    }
}