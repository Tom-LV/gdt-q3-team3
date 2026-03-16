using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PhoneController : MonoBehaviour
{
    [Header("Phone Setup")]
    [Tooltip("Drag your 3D Nokia phone object here that has the Animator component.")]
    public Animator phoneAnimator;

    public static bool isGamePaused = false;

    private string animatorBoolName = "IsPhoneOpen";

    [SerializeField]
    UnityEvent m_OnPhoneOpen;

    [SerializeField]
    UnityEvent m_OnPhoneClose;

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            isGamePaused = !isGamePaused;
            UpdatePhoneState();
        }
    }

    private void UpdatePhoneState()
    {
        if (phoneAnimator != null)
        {
            phoneAnimator.SetBool(animatorBoolName, isGamePaused);
        }
        else
        {
            Debug.LogWarning("Phone Animator is missing! Please assign it in the Inspector.");
        }

        if (isGamePaused)
        {
            if (m_OnPhoneOpen != null)
            {
                m_OnPhoneOpen.Invoke();
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

        }
        else
        {
            if (m_OnPhoneClose != null)
            {
                m_OnPhoneClose.Invoke();
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    

    public void ClosePhoneFromButton()
    {
        if (isGamePaused)
        {
            isGamePaused = false;
            UpdatePhoneState();
        }
    }
}