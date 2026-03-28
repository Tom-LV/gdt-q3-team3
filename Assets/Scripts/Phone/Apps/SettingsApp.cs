using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Audio;

public class SettingsApp : PhoneApp
{
    [Header("Dependencies")]
    public PlayerControls playerControls;
    public AudioMixer mainMixer;

    private ScrollView settingsContainer;
    private bool _isPopulated = false; // Ensures we only build the UI once!

    public override void Initialize(VisualElement root, PhoneOS os)
    {
        base.Initialize(root, os);
        ScreenContainer = root.Q<VisualElement>("SettingsScreen");
        settingsContainer = root.Q<ScrollView>("SettingsContainer");

        Button btnBack = root.Q<Button>("Btn_Back_Settings");
        btnBack?.RegisterCallback<ClickEvent>(ev => OS.OpenApp<HomeApp>());

        ApplySavedAudio(); // Apply audio on boot without opening the screen
    }

    public override void Open()
    {
        base.Open();

        // Generate the UI elements the very first time the user opens the Settings app
        if (!_isPopulated)
        {
            PopulateSettingsUI();
            _isPopulated = true;
        }
    }

    private void ApplySavedAudio()
    {
        if (mainMixer == null) return;

        float master = 20f * Mathf.Log10(PlayerPrefs.GetFloat("Vol_Master", 100f) / 100f);
        float music = 20f * Mathf.Log10(PlayerPrefs.GetFloat("Vol_Music", 100f) / 100f);
        float sfx = 20f * Mathf.Log10(PlayerPrefs.GetFloat("Vol_Sfx", 100f) / 100f);

        mainMixer.SetFloat("MasterVolume", master);
        mainMixer.SetFloat("MusicVolume", music);
        mainMixer.SetFloat("SfxVolume", sfx);
    }

    private void PopulateSettingsUI()
    {
        if (settingsContainer == null) return;
        settingsContainer.Clear();

        // AUDIO
        AddHeader("AUDIO");
        AddSlider("Master Volume", 0.0001f, 100f, PlayerPrefs.GetFloat("Vol_Master", 100f), val => { UpdateVolume("Vol_Master", "MasterVolume", val); });
        AddSlider("Music Volume", 0.0001f, 100f, PlayerPrefs.GetFloat("Vol_Music", 80f), val => { UpdateVolume("Vol_Music", "MusicVolume", val); });
        AddSlider("SFX Volume", 0.0001f, 100f, PlayerPrefs.GetFloat("Vol_Sfx", 80f), val => { UpdateVolume("Vol_Sfx", "SfxVolume", val); });

        // VISUALS
        AddHeader("VISUALS");
        AddToggle("Fullscreen", PlayerPrefs.GetInt("Fullscreen", 1) == 1, isToggled =>
        {
            Screen.fullScreenMode = isToggled ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed;
            PlayerPrefs.SetInt("Fullscreen", isToggled ? 1 : 0);
        });

        // CONTROLS
        AddHeader("CONTROLS");
        AddSlider("Look Sensitivity", 0.05f, 0.5f, PlayerPrefs.GetFloat("MouseSensitivity", 0.1f), val =>
        {
            PlayerPrefs.SetFloat("MouseSensitivity", val);
            if (playerControls != null) playerControls.SetSensitivity(val);
        });
    }

    private void UpdateVolume(string prefKey, string mixerGroup, float val)
    {
        PlayerPrefs.SetFloat(prefKey, val);
        if (mainMixer != null)
        {
            mainMixer.SetFloat(mixerGroup, 20f * Mathf.Log10(val / 100f));
        }
    }

    // --- UI HELPERS ---
    private void AddHeader(string title)
    {
        Label header = new Label(title);
        header.AddToClassList("section-header");
        header.style.marginLeft = new Length(7.5f, LengthUnit.Percent);
        settingsContainer.Add(header);
    }

    private void AddSlider(string labelText, float min, float max, float initialValue, System.Action<float> onChanged)
    {
        Slider slider = new Slider(labelText, min, max) { value = initialValue };
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

        Button toggleBtn = new Button();
        toggleBtn.AddToClassList("settings-action-btn");

        bool isOn = initialState;

        void UpdateVisuals()
        {
            toggleBtn.text = isOn ? "ON" : "OFF";
            toggleBtn.RemoveFromClassList("toggle-on");
            toggleBtn.RemoveFromClassList("toggle-off");
            toggleBtn.AddToClassList(isOn ? "toggle-on" : "toggle-off");
        }

        UpdateVisuals();

        toggleBtn.clicked += () => {
            isOn = !isOn;
            UpdateVisuals();
            onChanged?.Invoke(isOn);
        };

        row.Add(label);
        row.Add(toggleBtn);
        settingsContainer.Add(row);
    }
}