using UnityEngine;
using UnityEngine.UIElements;

public abstract class PhoneApp : MonoBehaviour
{
    protected PhoneOS OS;
    protected VisualElement Root;
    protected VisualElement ScreenContainer; // The main UI visual element for this app
    protected bool isOpen;

    // Called by the OS when the phone boots up
    public virtual void Initialize(VisualElement root, PhoneOS os)
    {
        this.Root = root;
        this.OS = os;
    }

    // Turns the app screen on
    public virtual void Open()
    {
        isOpen = true;
        if (ScreenContainer != null)
        {
            OnOpen();
            ScreenContainer.style.display = DisplayStyle.Flex;
        }
            
    }

    // Turns the app screen off
    public virtual void Close()
    {
        isOpen = false;
        if (ScreenContainer != null)
        {
            OnClose();
            ScreenContainer.style.display = DisplayStyle.None;
        }
            
    }

    public virtual void OnOpen()
    {

    }

    public virtual void OnClose()
    {

    }
}