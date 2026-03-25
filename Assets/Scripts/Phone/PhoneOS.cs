using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PhoneOS : MonoBehaviour
{
    private UIDocument phoneDoc;

    [Header("Player Reference")]
    [Tooltip("Drag your player here so we can change settings live!")]
    public PlayerControls playerControls;

    // Screens
    private VisualElement homeScreen;
    private VisualElement settingsScreen;

    // Home Buttons
    private Button btnResetRoom;
    private Button btnResetWorld;
    private Button btnSettings;

    // Back Buttons
    private Button btnBackSettings;

    // App Content
    private Slider sliderSensitivity;

    private void OnEnable()
    {
        phoneDoc = GetComponent<UIDocument>();
        VisualElement root = phoneDoc.rootVisualElement;

        // 1. QUERY SCREENS
        homeScreen = root.Q<VisualElement>("HomeScreen");
        settingsScreen = root.Q<VisualElement>("SettingsScreen");

        // 2. QUERY BUTTONS & CONTROLS
        btnResetRoom = root.Q<Button>("Btn_ResetRoom");
        btnResetWorld = root.Q<Button>("Btn_ResetLevel");
        btnSettings = root.Q<Button>("Btn_Settings");

        btnBackSettings = root.Q<Button>("Btn_Back_Settings");

        sliderSensitivity = root.Q<Slider>("Slider_Sensitivity");

        // 3. ASSIGN BUTTON CLICKS
        btnResetRoom?.RegisterCallback<ClickEvent>(ev => App_ResetRoom());
        btnResetWorld?.RegisterCallback<ClickEvent>(ev => App_ResetWorld());
        btnSettings?.RegisterCallback<ClickEvent>(ev => OpenScreen(settingsScreen, SetupSettingsApp));

        // Assign Back Buttons
        btnBackSettings?.RegisterCallback<ClickEvent>(ev => OpenScreen(homeScreen));

        // 4. SETUP SLIDER EVENT (Live updates!)
        if (sliderSensitivity != null)
        {
            // Load saved sensitivity, default to 15
            sliderSensitivity.value = PlayerPrefs.GetFloat("MouseSensitivity", 0.1f);

            // Listen for changes
            sliderSensitivity.RegisterValueChangedCallback(ev => OnSensitivityChanged(ev.newValue));
        }

        // Start on Home Screen
        OpenScreen(homeScreen);
    }

    // --- SCREEN NAVIGATION ENGINE ---
    private void OpenScreen(VisualElement screenToShow, System.Action onScreenOpened = null)
    {
        // Hide everything
        homeScreen.style.display = DisplayStyle.None;
        settingsScreen.style.display = DisplayStyle.None;

        // Show the requested screen
        if (screenToShow != null)
        {
            screenToShow.style.display = DisplayStyle.Flex;
        }

        // Run any specific setup code for that app
        onScreenOpened?.Invoke();
    }

    // --- APP SPECIFIC LOGIC ---

    private void App_ResetRoom()
    {
        if (CheckpointManager.Instance != null) CheckpointManager.Instance.ReloadCheckpoint();
        PhoneController.ClosePhoneFromButton();
    }

    private void App_ResetWorld()
    {
        PhoneController.isGamePaused = false;
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    private void SetupSettingsApp()
    {
        // The slider visually updates itself, so nothing needed here right now!
    }

    private void OnSensitivityChanged(float newValue)
    {
        // Save to hard drive
        PlayerPrefs.SetFloat("MouseSensitivity", newValue);
        PlayerPrefs.Save();

        // Push live update to player script!
        if (playerControls != null)
        {
            playerControls.SetSensitivity(newValue);
        }
    }
}