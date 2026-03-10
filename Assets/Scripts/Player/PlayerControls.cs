using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerControls : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed; // default: 5f
    [SerializeField] private float airMovementMultiplier; // default: 0.067f
    [SerializeField] private float airDrag; // default: 0.003f
    [SerializeField] private float sprintAddition; // default: 4f

    [Header("Jump")]
    [SerializeField] private float jumpStrength; // default: 5f
    [SerializeField] private float jumpBufferTime; // default: 0.15f

    [Header("Body Parts")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Transform cameraTransform;

    [Header("Interaction")]
    [SerializeField] private float reach; // default: 5f
    [SerializeField] private Vector3 leftHoldPosition; // default: (-0.5, 0.25, 0.5)
    [SerializeField] private Vector3 rightHoldPosition; // default: (0.5, 0.25, 0.5)
    [SerializeField] private float throwForce; // default: 2f

    [Header("Camera Effects")]
    [SerializeField] private float lookSensitivity; // default: 0.1f
    [SerializeField] private int baseFov; // default: 80f
    [SerializeField] private float fovChangeSpeed; // default: 3f
    [SerializeField] private float tiltAmount; // default: 15f
    [SerializeField] private float tiltSpeed; // default: 3f
    private Rigidbody rb;
    private Collider col;
    private Camera cam;
    private struct PlayerStates
    {
        public float pitch;
        public bool isSprinting;
        public bool onGround;
        public bool touchingGround;
        public float jumpBufferCounter;
        public ContactPoint contact;
        public Collider lookObject;
        public Item leftHeldItem;
        public Item rightHeldItem;
    };
    PlayerStates state;
    private PlayerInputs controls;
    private struct InputStates
    {
        public Vector2 Move;
        public bool Jump;
        public Vector2 Look;
        public bool Sprint;
        public bool Interact;
        public bool LeftHold;
        public bool RightHold;
        public bool Throw;
    };
    InputStates inputs;
    void Awake()
    {
        // Basic Initiating...
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
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
        // Movement Physics
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

        if(state.touchingGround) // Grounded Movement
        {
            targetVelocity = new Vector3(move.x * moveSpeed, currentVelocity.y, move.z * moveSpeed);
            if(inputs.Sprint || state.isSprinting) // Sprint Logic
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
            if(state.jumpBufferCounter > 0) // Jump Logic
            {
                if(state.onGround)
                {
                    state.jumpBufferCounter = 0;
                    targetVelocity.y = jumpStrength;
                }
            }
        }
        else // Air Movement
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
            targetVelocity.x = (currentVelocity.x * (1 - airDrag)) + (move.x * moveSpeed * airMovementMultiplier);
            targetVelocity.y = 0;
            targetVelocity.z = (currentVelocity.z * (1 - airDrag)) + (move.z * moveSpeed * airMovementMultiplier);
            targetVelocity = Vector3.ClampMagnitude(targetVelocity, maxAirSpeed);
            targetVelocity.y = currentVelocity.y;
        }
        rb.linearVelocity = targetVelocity;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, Mathf.Clamp(targetVelocity.magnitude, 7, 10) * (float)baseFov / 10, fovChangeSpeed * Time.deltaTime);
        
        state.rightHeldItem?.SetPosition(cameraPivot.position + (cameraPivot.rotation * rightHoldPosition), cameraTransform.forward);
        state.leftHeldItem?.SetPosition(cameraPivot.position + (cameraPivot.rotation * leftHoldPosition), cameraTransform.forward);    
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

        if(inputs.Throw)
        {
            if(state.rightHeldItem != null)
            {
                state.rightHeldItem.Throw(throwForce * cameraTransform.forward + Vector3.ClampMagnitude(rb.linearVelocity, 11) / 11);
                state.rightHeldItem = null;
            }
            else if(state.leftHeldItem != null)
            {
                state.leftHeldItem.Throw(throwForce * cameraTransform.forward);
                state.leftHeldItem = null;
            }
        }
        if(Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, reach))
        {
            state.lookObject = hit.collider;
            Item itemPointer = state.lookObject.GetComponent<Item>(); // if it is an item, this references that cs instance
            if(itemPointer != null && itemPointer != state.rightHeldItem && itemPointer != state.leftHeldItem)
            {
                if(inputs.RightHold && state.rightHeldItem == null)
                {
                    state.rightHeldItem = itemPointer;
                    itemPointer.PickUp(col);
                }
                if(inputs.LeftHold && state.leftHeldItem == null)
                {
                    state.leftHeldItem = itemPointer;
                    itemPointer.PickUp(col);
                }
            }
            Interactable interactablePointer = state.lookObject.GetComponent<Interactable>(); // same as itemPointer
            if(interactablePointer != null)
            {
                string uiDisplayText = interactablePointer.interactName; // I just want this as a reference for later when we make the ui
            }
            if(inputs.Interact)
            {

            }
        }
        else
        {
            state.lookObject = null;
        }
        if(!inputs.RightHold && state.rightHeldItem)
        {
            state.rightHeldItem.Drop();
            state.rightHeldItem = null;
        }
        if(!inputs.LeftHold && state.leftHeldItem)
        {
            state.leftHeldItem.Drop();
            state.leftHeldItem = null;
        }

        state.rightHeldItem?.SetPosition(cameraPivot.position + (cameraPivot.rotation * rightHoldPosition), cameraTransform.forward);
        state.leftHeldItem?.SetPosition(cameraPivot.position + (cameraPivot.rotation * leftHoldPosition), cameraTransform.forward);
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