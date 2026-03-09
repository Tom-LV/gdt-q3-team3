using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerControls : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float airMovementMultiplyer = 0.1f;
    public float airDrag = 0.05f;
    public float sprintAddition = 4.0f;
    public float jumpStrength = 5.0f;
    public float wallJumpStrength = 3.0f;
    public float lookSensitivity = 0.1f;
    public float jumpBufferTime = 0.15f;
    public Transform cameraPivot;
    public Transform cameraTransform;

    private Rigidbody rb;
    private float pitch = 0f;
    private bool isSprinting;
    private bool onGround;
    private bool touchingGround;
    private bool hasWallJumped;
    private float jumpBufferCounter = 0f;
    private Vector3 averageNormal;
    private PlayerInputs controls;
    private struct Input
    {
        public Vector2 Move;
        public bool Jump;
        public Vector2 Look;
        public bool Sprint;
    };
    Input inputs;
    void Awake()
    {
        // Basic Initiating...
        rb = GetComponent<Rigidbody>();
        controls = new PlayerInputs();
        inputs = new Input();

        // Initiate All The Input Event Triggers (stuff you neither need to understand nor touch)
        var actionMap = controls.asset.FindActionMap("Inputs");
        foreach(var action in actionMap.actions)
        {
            var field = typeof(Input).GetField(action.name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
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
        if(touchingGround)
        {
            targetVelocity = new Vector3(move.x * moveSpeed, currentVelocity.y, move.z * moveSpeed);
            if(inputs.Sprint || isSprinting)
            {
                if(inputs.Move.y > 0 && Vector3.Dot(currentVelocity, camForward) > 0)
                {
                    targetVelocity += camForward * ((inputs.Sprint) ? sprintAddition : sprintAddition / 2);
                    isSprinting = true;
                }
                else
                {
                    isSprinting = false;
                }
            }
            if(jumpBufferCounter > 0)
            {
                if(onGround)
                {
                    jumpBufferCounter = 0;
                    targetVelocity.y = jumpStrength;
                    hasWallJumped = false;
                }
                else if(!hasWallJumped)
                {
                    jumpBufferCounter = 0;
                    targetVelocity += (averageNormal + move + (Vector3.up / 2)) * wallJumpStrength;
                    hasWallJumped = true;
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
            else if(isSprinting)
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
    }

    // Frame Updates
    void Update()
    {
        if (inputs.Look != Vector2.zero)
        {
            cameraPivot.Rotate(Vector3.up * inputs.Look.x * lookSensitivity);

            pitch -= inputs.Look.y * lookSensitivity;
            pitch = Mathf.Clamp(pitch, -80f, 80f);
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        if(inputs.Jump)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else if(jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.CompareTag("Ground")) {
            touchingGround = true;
            foreach(ContactPoint contact in collision.contacts)
            {
                if(contact.normal.y > 0.5f)
                {
                    hasWallJumped = true; // this may look weird, but this makes walljumps only possible after a normal jump, which is nice
                    onGround = true;
                    return;
                }
            }
            if(!onGround)
            {
                foreach(ContactPoint contact in collision.contacts)
                { 
                    averageNormal += contact.normal;
                }
                averageNormal /= (collision.contactCount + 1);
                averageNormal.Normalize();
            }
            else
            {
                averageNormal = Vector3.zero;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.CompareTag("Ground"))
        {
            onGround = false;
            touchingGround = false;
        }
    }
}