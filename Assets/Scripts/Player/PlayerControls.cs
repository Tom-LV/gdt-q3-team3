using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerControls : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float airMovementMultiplyer = 0.067f;
    [SerializeField] private float airDrag = 0.003f;
    [SerializeField] private float sprintAddition = 4.0f;

    [Header("Jump")]
    [SerializeField] private float jumpStrength = 5.0f;
    [SerializeField] private float lookSensitivity = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.15f;

    [Header("Body Parts")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Transform cameraTransform;

    [Header("Camera Effects")]
    [SerializeField] private int baseFov = 80;
    [SerializeField] private float fovChangeSpeed = 3f;
    [SerializeField] private float tiltAmount = 15f;
    [SerializeField] private float tiltSpeed = 3f;
    private Rigidbody rb;
    private Camera cam;
    private struct PlayerStates
    {
        public float pitch;
        public bool isSprinting;
        public bool onGround;
        public bool touchingGround;
        public float jumpBufferCounter;
        public ContactPoint contact;  
    };
    PlayerStates state;
    private PlayerInputs controls;
    private struct InputStates
    {
        public Vector2 Move;
        public bool Jump;
        public Vector2 Look;
        public bool Sprint;
    };
    InputStates inputs;
    void Awake()
    {
        // Basic Initiating...
        rb = GetComponent<Rigidbody>();
        cam = cameraTransform.GetComponent<Camera>();
        controls = new PlayerInputs();
        inputs = new InputStates();
        state = new PlayerStates();

        // Initiate All The Input Event Triggers (stuff you neither need to understand nor touch)
        var actionMap = controls.asset.FindActionMap("Inputs");
        foreach(var action in actionMap.actions)
        {
            var field = typeof(InputStates).GetField(action.name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if(field == null)
            {
                continue;
            }
            action.performed += ctx => field.SetValueDirect(__makeref(inputs), (field.FieldType == typeof(bool)) ? true : ctx.ReadValue<Vector2>());
            action.canceled += ctx => field.SetValueDirect(__makeref(inputs), (field.FieldType == typeof(bool)) ? false : Vector2.zero);
        }

        // Invisible Locked Cursor!!
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // More Input Event Trigger Stuff...
    void OnEnable() => controls.Inputs.Enable();
    void OnDisable() => controls.Inputs.Disable();

    // Physics Updates
    void FixedUpdate()
    {
        Vector3 playerMovement = new Vector3(inputs.Move.x, 0f, inputs.Move.y);

        Vector3 camForward = cameraPivot.forward;
        camForward.y = 0;
        camForward.Normalize();
        Vector3 camRight = cameraPivot.right;
        camRight.y = 0;
        camRight.Normalize();
        Vector3 move = camForward * playerMovement.z + camRight * playerMovement.x;

        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 targetVelocity = Vector3.zero;
        if(state.touchingGround)
        {
            targetVelocity = new Vector3(move.x * moveSpeed, currentVelocity.y, move.z * moveSpeed);
            if(inputs.Sprint || state.isSprinting)
            {
                if(inputs.Move.y > 0 && Vector3.Dot(currentVelocity.normalized, camForward) >= 1 / Mathf.Sqrt(2)) // 45° Angle
                {
                    targetVelocity += camForward * (inputs.Sprint ? sprintAddition : sprintAddition / 2);
                    state.isSprinting = true;
                }
                else
                {
                    state.isSprinting = false;
                }
            }
            if(state.jumpBufferCounter > 0)
            {
                if(state.onGround)
                {
                    state.jumpBufferCounter = 0;
                    targetVelocity.y = jumpStrength;
                }
            }
        }
        else
        {
            float maxAirSpeed = moveSpeed;
            if(inputs.Sprint)
            {
                maxAirSpeed += sprintAddition;
            } 
            else if(state.isSprinting)
            {
                maxAirSpeed += sprintAddition / 2;
            }
            targetVelocity.x = (currentVelocity.x * (1 - airDrag)) + (move.x * moveSpeed * airMovementMultiplyer);
            targetVelocity.y = 0;
            targetVelocity.z = (currentVelocity.z * (1 - airDrag)) + (move.z * moveSpeed * airMovementMultiplyer);
            targetVelocity = Vector3.ClampMagnitude(targetVelocity, maxAirSpeed);
            targetVelocity.y = currentVelocity.y;
        }
        rb.linearVelocity = targetVelocity;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, Mathf.Clamp(targetVelocity.magnitude, 7, 10) * (float)baseFov / 10, fovChangeSpeed * Time.deltaTime);
    }

    // Frame Updates
    void Update()
    {
        if (inputs.Look != Vector2.zero)
        {
            cameraPivot.Rotate(Vector3.up * inputs.Look.x * lookSensitivity);

            state.pitch -= inputs.Look.y * lookSensitivity;
            state.pitch = Mathf.Clamp(state.pitch, -80f, 80f);
            cameraTransform.localRotation = Quaternion.Euler(state.pitch, 0f, 0f);

            Vector3 newCamUp;
            if(state.touchingGround && !state.onGround)
            {
                newCamUp = Vector3.RotateTowards(state.contact.normal, Vector3.up, Mathf.Deg2Rad * (90f - tiltAmount), 0f);
            }
            else
            {
                newCamUp = Vector3.up;
            }
            Quaternion targetRotation = Quaternion.LookRotation(cameraPivot.forward, newCamUp);
            cameraPivot.rotation = Quaternion.Slerp(cameraPivot.rotation, targetRotation, tiltSpeed * Time.deltaTime);
        }

        if(inputs.Jump)
        {
            inputs.Jump = false;
            state.jumpBufferCounter = jumpBufferTime;
        }
        else if(state.jumpBufferCounter > 0)
        {
            state.jumpBufferCounter -= Time.deltaTime;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.CompareTag("Ground")) {
            state.touchingGround = true;
            state.contact = collision.contacts[0];
            foreach(ContactPoint contact in collision.contacts)
            {
                if(contact.normal.y > 0.5f)
                {
                    state.onGround = true;
                    return;
                }
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.CompareTag("Ground"))
        {
            state.onGround = false;
            state.touchingGround = false;
        }
    }
}