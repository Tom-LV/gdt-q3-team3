using TMPro;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI UiDisplayText;

    public void UpdateText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            UiDisplayText.text = "";
            UiDisplayText.gameObject.SetActive(false);
        }
        else
        {
            UiDisplayText.text = text;
            UiDisplayText.gameObject.SetActive(true);
            Debug.Log("Set Text To: " + text);
        }
    }
}