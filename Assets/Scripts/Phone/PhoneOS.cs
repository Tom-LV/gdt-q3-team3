using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.Audio;

public class PhoneOS : MonoBehaviour
{
    public static PhoneOS Instance; // Singleton so any script can text the player

    private UIDocument phoneDoc;

    [Header("Player Reference")]
    [Tooltip("Drag your player here so we can change settings live!")]
    public PlayerControls playerControls;

    [Header("Audio Settings")]
    public AudioMixer mainMixer;

    // --- SCREENS ---
    private VisualElement homeScreen;
    private VisualElement archiveScreen;
    private VisualElement settingsScreen;
    private VisualElement chatScreen;

    // --- HOME BUTTONS ---
    private Button btnResetRoom;
    private Button btnResetWorld;
    private Button btnArchive;
    private Button btnSettings;
    private Button btnChat;
    private Button btnExit;

    // --- BACK BUTTONS ---
    private Button btnBackArchive;
    private Button btnBackSettings;
    private Button btnBackChat;

    // --- APP CONTENT ---
    private Label textCodes;
    private ScrollView chatHistory;
    private ScrollView settingsContainer;

    private void Awake()
    {
        // Set up the Singleton so we can call PhoneOS.Instance.ReceiveMessage() from anywhere
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    private void Start()
    {
        float linearVolume = PlayerPrefs.GetFloat("Vol_Master", 100f) / 100f;
        float decibelVolume = 20f * Mathf.Log10(linearVolume);

        mainMixer.SetFloat("MasterVolume", decibelVolume);
        linearVolume = PlayerPrefs.GetFloat("Vol_Music", 100f) / 100f;
        decibelVolume = 20f * Mathf.Log10(linearVolume);
        mainMixer.SetFloat("MusicVolume", decibelVolume);
        linearVolume = PlayerPrefs.GetFloat("Vol_Sfx", 100f) / 100f;
        decibelVolume = 20f * Mathf.Log10(linearVolume);
        mainMixer.SetFloat("SfxVolume", decibelVolume);

        PhoneOS.Instance.ReceiveMessage("Unknown", "Hi, I see that something went wrong...", Color.crimson);
        PhoneOS.Instance.ReceiveMessage("Unknown", "You should look around", Color.crimson);
        PhoneOS.Instance.ReceiveMessage("Unknown", "RUN", Color.yellowNice);

    }

    private void OnEnable()
    {
        phoneDoc = GetComponent<UIDocument>();
        VisualElement root = phoneDoc.rootVisualElement;

        // 1. QUERY SCREENS
        homeScreen = root.Q<VisualElement>("HomeScreen");
        settingsScreen = root.Q<VisualElement>("SettingsScreen");
        archiveScreen = root.Q<VisualElement>("ArchiveScreen");
        chatScreen = root.Q<VisualElement>("ChatScreen");

        // 2. QUERY BUTTONS & CONTROLS
        btnResetRoom = root.Q<Button>("Btn_ResetRoom");
        btnResetWorld = root.Q<Button>("Btn_ResetLevel");
        btnArchive = root.Q<Button>("Btn_Archive");
        btnSettings = root.Q<Button>("Btn_Settings");
        btnChat = root.Q<Button>("Btn_Chat");
        btnExit = root.Q<Button>("Btn_Exit");

        btnBackSettings = root.Q<Button>("Btn_Back_Settings");
        btnBackArchive = root.Q<Button>("Btn_Back_Archive");
        btnBackChat = root.Q<Button>("Btn_Back_Chat");

        settingsContainer = root.Q<ScrollView>("SettingsContainer");
        textCodes = root.Q<Label>("Text_Codes");
        chatHistory = root.Q<ScrollView>("ChatHistory");

        // 3. ASSIGN HOME BUTTON CLICKS
        btnResetRoom?.RegisterCallback<ClickEvent>(ev => App_ResetRoom());
        btnResetWorld?.RegisterCallback<ClickEvent>(ev => App_ResetWorld());
        btnSettings?.RegisterCallback<ClickEvent>(ev => OpenScreen(settingsScreen, SetupSettingsApp));
        btnArchive?.RegisterCallback<ClickEvent>(ev => OpenScreen(archiveScreen, SetupArchiveApp));
        btnChat?.RegisterCallback<ClickEvent>(ev => OpenScreen(chatScreen));
        btnExit?.RegisterCallback<ClickEvent>(ev => ExitGame());

        // 4. ASSIGN BACK BUTTON CLICKS
        btnBackSettings?.RegisterCallback<ClickEvent>(ev => OpenScreen(homeScreen));
        btnBackArchive?.RegisterCallback<ClickEvent>(ev => OpenScreen(homeScreen));
        btnBackChat?.RegisterCallback<ClickEvent>(ev => OpenScreen(homeScreen));

        // Start on Home Screen and hide everything else
        OpenScreen(homeScreen);
    }

    // --- SCREEN NAVIGATION ENGINE ---
    private void OpenScreen(VisualElement screenToShow, System.Action onScreenOpened = null)
    {
        // Safely hide all screens first
        if (homeScreen != null) homeScreen.style.display = DisplayStyle.None;
        if (archiveScreen != null) archiveScreen.style.display = DisplayStyle.None;
        if (settingsScreen != null) settingsScreen.style.display = DisplayStyle.None;
        if (chatScreen != null) chatScreen.style.display = DisplayStyle.None;

        // Show the requested screen
        if (screenToShow != null)
        {
            screenToShow.style.display = DisplayStyle.Flex;
        }

        // Run any specific setup code for that app (like loading save data)
        onScreenOpened?.Invoke();
    }

    // --- APP LOGIC ---

    private void ExitGame()
    {
        Application.Quit();
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
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    private void SetupSettingsApp()
    {
        if (settingsContainer == null) return;

        // Wipe it clean every time we open the app so it doesn't duplicate!
        settingsContainer.Clear();

        // --- AUDIO SETTINGS ---
        AddHeader("AUDIO");

        AddSlider("Master Volume", 0.0001f, 100f, PlayerPrefs.GetFloat("Vol_Master", 100f), val => {
            PlayerPrefs.SetFloat("Vol_Master", val);

            // 2. Convert linear 0-100 to logarithmic Decibels (-80dB to 0dB)
            if (mainMixer != null)
            {
                float linearVolume = val / 100f;
                float decibelVolume = 20f * Mathf.Log10(linearVolume);

                // 3. Send the dB value to the Exposed Parameter on the Mixer
                mainMixer.SetFloat("MasterVolume", decibelVolume);
            }
        });

        AddSlider("Music Volume", 0.0001f, 100f, PlayerPrefs.GetFloat("Vol_Music", 80f), val => {

            // 1. Save the 0-100 value for the UI slider
            PlayerPrefs.SetFloat("Vol_Music", val);

            // 2. Convert linear 0-100 to logarithmic Decibels (-80dB to 0dB)
            if (mainMixer != null)
            {
                float linearVolume = val / 100f;
                float decibelVolume = 20f * Mathf.Log10(linearVolume);

                // 3. Send the dB value to the Exposed Parameter on the Mixer
                mainMixer.SetFloat("MusicVolume", decibelVolume);
            }
        });

        AddSlider("SFX Volume", 0.0001f, 100f, PlayerPrefs.GetFloat("Vol_Sfx", 80f), val => {

            // 1. Save the 0-100 value for the UI slider
            PlayerPrefs.SetFloat("Vol_Sfx", val);

            // 2. Convert linear 0-100 to logarithmic Decibels (-80dB to 0dB)
            if (mainMixer != null)
            {
                float linearVolume = val / 100f;
                float decibelVolume = 20f * Mathf.Log10(linearVolume);

                // 3. Send the dB value to the Exposed Parameter on the Mixer
                mainMixer.SetFloat("SfxVolume", decibelVolume);
            }
        });

        AddHeader("VISUALS");
        AddToggle("Fullscreen", PlayerPrefs.GetInt("Fullscreen", 1) == 1, isToggled =>
        {
            if (isToggled) Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
            else Screen.fullScreenMode = FullScreenMode.Windowed;
            PlayerPrefs.SetInt("Fullscreen", isToggled ? 1 : 0);
        });
        AddToggle("v-sync", PlayerPrefs.GetInt("vSync", 1) == 1, isToggled =>
        {
            QualitySettings.vSyncCount = isToggled ? 1 : 0;
            PlayerPrefs.SetInt("vSync", isToggled ? 1 : 0);
        });

        // --- CONTROLS SETTINGS ---
        AddHeader("CONTROLS");

        AddSlider("Look Sensitivity", 0.05f, 0.5f, PlayerPrefs.GetFloat("MouseSensitivity", 0.1f), OnSensitivityChanged);
        AddToggle("Invert X-Axis", PlayerPrefs.GetInt("InvertX", 0) == 1, isToggled =>
        {
            PlayerPrefs.SetInt("InvertX", isToggled ? 1 : 0);
        });
        AddToggle("Invert Y-Axis", PlayerPrefs.GetInt("InvertY", 0) == 1, isToggled =>
        {
            PlayerPrefs.SetInt("InvertY", isToggled ? 1 : 0);
        });


        //AddKeybind("Jump", "Space", key => {
        //    Debug.Log("Waiting for player to press a key to bind Jump...");
        //    // Keybind logic goes here!
        //});
    }

    private void SetupArchiveApp()
    {
        if (textCodes == null) return;

        // Fetch codes from our TimeLoopSave script
        // string fireCode = TimeLoopSave.GetCode("Fire Room");
        // textCodes.text = $"<b>FIRE SECTOR:</b> {fireCode}";
    }

    private void OnSensitivityChanged(float newValue)
    {
        // Save to hard drive
        PlayerPrefs.SetFloat("MouseSensitivity", newValue);
        PlayerPrefs.Save();

        // Push live update to player script
        if (playerControls != null)
        {
            playerControls.SetSensitivity(newValue);
        }
    }

    // --- CHAT API ---
    public void ReceiveMessage(string senderName, string messageBody)
    {
        // rgb(0, 200, 255) is the default Cyan from our CSS
        ReceiveMessage(senderName, messageBody, new Color(0f, 0.78f, 1f));
    }
    public void ReceiveMessage(string senderName, string messageBody, Color nameColor)
    {
        if (chatHistory == null) return;

        // 1. Create container & bubble
        VisualElement container = new VisualElement();
        container.AddToClassList("chat-bubble-container");

        VisualElement bubble = new VisualElement();
        bubble.AddToClassList("chat-bubble");

        // 2. Create Sender text and APPLY THE CUSTOM COLOR
        Label senderLabel = new Label(senderName);
        senderLabel.AddToClassList("chat-sender-name");

        // THIS IS THE MAGIC LINE: It overrides the USS stylesheet!
        senderLabel.style.color = new StyleColor(nameColor);

        // 3. Create Message text
        Label bodyLabel = new Label(messageBody);
        bodyLabel.AddToClassList("chat-message-text");

        // 4. Assemble
        bubble.Add(senderLabel);
        bubble.Add(bodyLabel);
        container.Add(bubble);

        // 5. Add to screen
        chatHistory.Add(container);
    }

    private void AddHeader(string title)
    {
        Label header = new Label(title);
        header.AddToClassList("settings-header");

        // Add a bit of side margin to align with sliders
        header.style.marginLeft = new Length(7.5f, LengthUnit.Percent);

        settingsContainer.Add(header);
    }

    private void AddSlider(string labelText, float min, float max, float initialValue, System.Action<float> onChanged)
    {
        Slider slider = new Slider(labelText, min, max);
        slider.value = initialValue;

        // Use our beautiful Jumbo slider CSS
        slider.AddToClassList("unity-slider");

        slider.RegisterValueChangedCallback(ev => onChanged?.Invoke(ev.newValue));
        settingsContainer.Add(slider);
    }

    private void AddToggle(string labelText, bool initialState, System.Action<bool> onChanged)
    {
        VisualElement row = new VisualElement();
        row.AddToClassList("settings-row");

        Label label = new Label(labelText);
        label.AddToClassList("settings-row-label");

        // Using a stylized Button instead of the tiny default checkbox
        Button toggleBtn = new Button();
        toggleBtn.AddToClassList("settings-action-btn");

        // Local state tracker
        bool isOn = initialState;

        // Function to update visuals
        void UpdateVisuals()
        {
            toggleBtn.text = isOn ? "ON" : "OFF";
            toggleBtn.RemoveFromClassList("toggle-on");
            toggleBtn.RemoveFromClassList("toggle-off");
            toggleBtn.AddToClassList(isOn ? "toggle-on" : "toggle-off");
        }

        UpdateVisuals(); // Set initial look

        // Click event
        toggleBtn.clicked += () => {
            isOn = !isOn; // Flip state
            UpdateVisuals();
            onChanged?.Invoke(isOn);
        };

        row.Add(label);
        row.Add(toggleBtn);
        settingsContainer.Add(row);
    }

    private void AddKeybind(string actionName, string currentKey, System.Action<string> onRebindClick)
    {
        VisualElement row = new VisualElement();
        row.AddToClassList("settings-row");

        Label label = new Label(actionName);
        label.AddToClassList("settings-row-label");

        Button bindBtn = new Button();
        bindBtn.text = currentKey;
        bindBtn.AddToClassList("settings-action-btn");
        bindBtn.AddToClassList("keybind-btn");

        bindBtn.clicked += () => {
            bindBtn.text = "..."; // Visual feedback that it's listening
            onRebindClick?.Invoke(currentKey);
        };

        row.Add(label);
        row.Add(bindBtn);
        settingsContainer.Add(row);
    }
}