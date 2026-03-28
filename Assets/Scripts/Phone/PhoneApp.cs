using UnityEngine;
using UnityEngine.UIElements;

public abstract class PhoneApp : MonoBehaviour
{
    protected PhoneOS OS;
    protected VisualElement Root;
    protected VisualElement ScreenContainer; // The main UI visual element for this app

    // Called by the OS when the phone boots up
    public virtual void Initialize(VisualElement root, PhoneOS os)
    {
        this.Root = root;
        this.OS = os;
    }

    // Turns the app screen on
    public virtual void Open()
    {
        if (ScreenContainer != null)
            ScreenContainer.style.display = DisplayStyle.Flex;
    }

    // Turns the app screen off
    public virtual void Close()
    {
        if (ScreenContainer != null)
            ScreenContainer.style.display = DisplayStyle.None;
    }
}