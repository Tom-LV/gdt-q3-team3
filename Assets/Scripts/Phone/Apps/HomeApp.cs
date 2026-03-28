using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class HomeApp : PhoneApp
{
    public override void Initialize(VisualElement root, PhoneOS os)
    {
        base.Initialize(root, os);
        ScreenContainer = root.Q<VisualElement>("HomeScreen");

        // Query Buttons
        Button btnResetRoom = root.Q<Button>("Btn_ResetRoom");
        Button btnResetWorld = root.Q<Button>("Btn_ResetLevel");
        Button btnArchive = root.Q<Button>("Btn_Archive");
        Button btnSettings = root.Q<Button>("Btn_Settings");
        Button btnChat = root.Q<Button>("Btn_Chat");
        Button btnExit = root.Q<Button>("Btn_Exit");

        // Routing
        btnSettings?.RegisterCallback<ClickEvent>(ev => OS.OpenApp<SettingsApp>());
        btnChat?.RegisterCallback<ClickEvent>(ev => OS.OpenApp<ChatApp>());
        // (Add Archive routing here when you build ArchiveApp.cs!)

        // System Actions
        btnExit?.RegisterCallback<ClickEvent>(ev => Application.Quit());
        btnResetRoom?.RegisterCallback<ClickEvent>(ev => App_ResetRoom());
        btnResetWorld?.RegisterCallback<ClickEvent>(ev => App_ResetWorld());
    }

    private void App_ResetRoom()
    {
        Debug.Log("Phone OS: Resetting Room...");
        if (CheckpointManager.Instance != null) CheckpointManager.Instance.ReloadCheckpoint();
        PhoneController.ClosePhoneFromButton();
    }

    private void App_ResetWorld()
    {
        Debug.Log("Phone OS: Resetting World...");
        PhoneController.isGamePaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}