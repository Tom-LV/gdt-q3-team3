using UnityEngine;
using System.Collections;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance;
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerCamera;
    private const float shiftDuration = 0.3f;
    private Vector3 localStartPos;
    private Quaternion localStartRot;
    private Vector3 shiftTargetPos;
    private Vector3 shiftStartPos;
    private Quaternion shiftTargetRot;
    private Quaternion shiftStartRot;
    private float cutsceneDuration;
    private void Awake()
    {
        Instance = this;
    }
    public void DoStaticCutscene(Vector3 pos, Quaternion rot, float time)
    {
        localStartPos = playerCamera.localPosition;
        localStartRot = playerCamera.localRotation;
        shiftStartPos = playerCamera.position;
        shiftStartRot = playerCamera.rotation;
        shiftTargetPos = pos;
        shiftTargetRot = rot;
        cutsceneDuration = time;
        PhoneController.isGamePaused = true;
        StartCoroutine(Cutscene());
    }
    private IEnumerator Cutscene()
    {
        float t0 = 0f;

        while (t0 < shiftDuration)
        {
            t0 += Time.deltaTime;
            float lerpT = t0 / shiftDuration;
            playerCamera.position = Vector3.Lerp(shiftStartPos, shiftTargetPos, lerpT);
            playerCamera.rotation = Quaternion.Lerp(shiftStartRot, shiftTargetRot, lerpT);

            yield return null;
        }
        playerCamera.position = shiftTargetPos;
        playerCamera.rotation = shiftTargetRot;

        float t1 = 0f;
        while (t1 < cutsceneDuration)
        {
            t1 += Time.deltaTime;
            yield return null;
        }

        shiftStartPos = playerCamera.position;
        shiftStartRot = playerCamera.rotation;
        shiftTargetPos = localStartPos + player.position;
        shiftTargetRot = localStartRot * player.rotation;
        float t2 = 0f;
        while (t2 < shiftDuration)
        {
            t2 += Time.deltaTime;
            float lerpT = t2 / shiftDuration;
            playerCamera.position = Vector3.Lerp(shiftStartPos, shiftTargetPos, lerpT);
            playerCamera.rotation = Quaternion.Lerp(shiftStartRot, shiftTargetRot, lerpT);

            yield return null;
        }
        playerCamera.position = shiftTargetPos;
        playerCamera.rotation = shiftTargetRot;
        PhoneController.isGamePaused = false;
    }   
}
