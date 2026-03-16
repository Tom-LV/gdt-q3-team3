using UnityEngine;

public class BlockController : MonoBehaviour
{
    [SerializeField]
    private Animator blockAnimator;
    [SerializeField]
    private PlateScript linkedPlate;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetState(linkedPlate.isActive);
        linkedPlate.m_OnToggle.AddListener(SetState);
    }

    void SetState(bool isDown)
    {
        blockAnimator.SetBool("IsDown", isDown);
    }
}
