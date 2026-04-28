using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class PhoneOS : MonoBehaviour
{
    public static PhoneOS Instance;

    private UIDocument _phoneDoc;
    private List<PhoneApp> _installedApps = new List<PhoneApp>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    private void OnEnable()
    {
        _phoneDoc = GetComponent<UIDocument>();
        VisualElement root = _phoneDoc.rootVisualElement;

        // Find all PhoneApp scripts attached to this GameObject
        _installedApps = GetComponents<PhoneApp>().ToList();

        // Boot them up
        foreach (var app in _installedApps)
        {
            app.Initialize(root, this);
            app.Close(); // Make sure everything starts hidden
        }

        // Open the Home Screen by default
        OpenApp<HomeApp>();
    }

    // OS ROUTING API

    public void OpenApp<T>() where T : PhoneApp
    {
        // Close all apps
        foreach (var app in _installedApps) { app.Close(); }

        // Find and open the requested app
        T appToOpen = GetApp<T>();
        if (appToOpen != null) { appToOpen.Open(); }
    }

    public T GetApp<T>() where T : PhoneApp
    {
        return _installedApps.OfType<T>().FirstOrDefault();
    }
}