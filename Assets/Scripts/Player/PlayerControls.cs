using UnityEngine;
using UnityEngine.InputSystem;

// 1. We replace Rigidbody and CapsuleCollider with CharacterController (I approve! It's so much nicer)
[RequireComponent(typeof(CharacterController))]
public class PlayerControls : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float airSpeed = 1f;
    [SerializeField] private float pushSpeed = 1f;
    [SerializeField] private float pushExitTime = 0.5f;
    [SerializeField] private float gravity = -15f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float jumpBufferTime = 0.2f;


    [Header("Look Settings")]
    [SerializeField] private Transform cameraPivotTransform;

    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 15f;
    [SerializeField] private float maxLookAngle = 85f;

    // References
    private CharacterController cc;
    private Transform tr;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction sprintAction;
    private InputAction jumpAction;

    // State Variables
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isSprinting;

    private float jumpBuffer;
    private float cameraPitch = 0f;
    private float playerYaw = 0f;
    private Vector3 velocity;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        tr = GetComponent<Transform>();
        playerYaw = transform.eulerAngles.y;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");
        sprintAction = InputSystem.actions.FindAction("Sprint");
        jumpAction = InputSystem.actions.FindAction("Jump");
    }

    void Update()
    {
        if(isShifting) HandleShiftLerp();
        if (PhoneController.isGamePaused) return;

        ReadInputs();
        HandleMovement();
        HandleLooking();
    }

    private void ReadInputs()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        lookInput = lookAction.ReadValue<Vector2>();
        jumpBuffer = jumpAction.WasPressedThisFrame() ? jumpBufferTime : jumpBuffer - Time.deltaTime;
    }

    private void HandleLooking()
    {
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);

        playerYaw += mouseX;
        transform.rotation = Quaternion.Euler(0f, playerYaw, 0f);
    }

    private void HandleMovement()
    {
        bool isGrounded = cc.isGrounded;

        if (isGrounded && velocity.y < 0) velocity.y = -2f; // step logic

        Vector3 moveDirection = (transform.right * moveInput.x) + (transform.forward * moveInput.y); // movement direction
        moveDirection.Normalize();

        float currentSpeed; // movement speed
        if (IsPushing()) currentSpeed = pushSpeed;
        else if (sprintAction.IsPressed() && isSprinting) currentSpeed = sprintSpeed;
        else if (isSprinting) currentSpeed = (sprintSpeed + walkSpeed) / 2;
        else currentSpeed = walkSpeed;

        Vector3 xzMovement = new Vector3(velocity.x, 0, velocity.z); // movement vector
        if (isGrounded)
        {
            xzMovement = moveDirection * currentSpeed;
            velocity.x = xzMovement.x;
            velocity.z = xzMovement.z;
        }
        else
        {
            Vector3 diff = (moveDirection * currentSpeed) - xzMovement;
            xzMovement = Vector3.ClampMagnitude(xzMovement + diff.normalized * airSpeed * 0.05f, currentSpeed);
            velocity.x = xzMovement.x;
            velocity.z = xzMovement.z;
        }

        if (jumpAction.IsPressed() && jumpBuffer > 0 && isGrounded && !IsPushing()) // jump logic
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        
        Vector3 oldPosition = tr.position; // apply movement
        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
        Vector3 xzTrueVelocity = (tr.position - oldPosition) / Time.deltaTime; // find actual velocity
        xzTrueVelocity.y = 0;

        if(IsPushing())
        {
            ClampToPath();
            pushObject.PushToPlayerPos(tr.position);
            
            xzTrueVelocity = (tr.position - oldPosition) / Time.deltaTime; // find actual, actual velocity
            xzTrueVelocity.y = 0;
            Quaternion angleChange = pushObject.GetAngleChange(oldPosition, oldPosition + xzTrueVelocity);
            cameraPitch += angleChange.eulerAngles.x;
            playerYaw += angleChange.eulerAngles.y;
            if (xzTrueVelocity.sqrMagnitude < 0.3f && xzMovement.sqrMagnitude > 0.01f) pushExitTimer -= Time.deltaTime;
            else pushExitTimer = pushExitTime;
            if (pushExitTimer <= 0) ClearPushState();
            return;
        }

        isSprinting = sprintAction.IsPressed() ? true : isSprinting; // sprint logic for next update
        if (Vector3.Dot(xzTrueVelocity.normalized, cameraPivotTransform.forward) < 0.7 || xzTrueVelocity.magnitude < walkSpeed)
            isSprinting = false;
    }

    //---------------------
    // special interactions
    private bool isShifting;
    readonly float shiftDuration = 1f;
    private Vector3 shiftTargetPos;
    private Vector3 shiftStartPos;
    private Quaternion shiftTargetOrientation;
    private Quaternion shiftStartRot;
    private float shiftTimer;
    private PushableItem pushObject;
    private float pushExitTimer;
    
    public Vector3 GetCameraForward()
    {
        return cameraTransform.forward;
    }

    public bool IsSprinting()
    {
        return isSprinting;
    }

    public void ShiftToPos(Vector3 pos, Quaternion orientation)
    {
        shiftStartPos = tr.position;
        shiftStartRot = tr.rotation;
        shiftTimer = 0f;
        PhoneController.isGamePaused = true;
        shiftTargetPos = pos;
        shiftTargetOrientation = orientation;
        isShifting = true;
    }

    public void HandleShiftLerp()
    {
        if(Vector3.Distance(tr.position, shiftTargetPos) < 0.01f && Quaternion.Angle(tr.rotation, shiftTargetOrientation) < 1f)
        {
            isShifting = false;
            PhoneController.isGamePaused = false;
            playerYaw = transform.rotation.eulerAngles.y;
            return;
        }
        shiftTimer += Time.deltaTime;
        float t = shiftTimer / shiftDuration;

        t = Mathf.Clamp01(t);

        float easedT = Mathf.SmoothStep(0f, 1f, t);

        tr.position = Vector3.Lerp(shiftStartPos, shiftTargetPos, easedT);
        tr.rotation = Quaternion.Slerp(shiftStartRot, shiftTargetOrientation, easedT);
    }

    public void SetPushObject(PushableItem pushable)
    {
        ShiftToPos(pushable.FindStartPointOnPath(), pushable.FindStartOrientation());
        pushObject = pushable;
    }

    public void ClearPushState()
    {
        pushObject = null;
        pushExitTimer = 0;
    }

    public bool IsPushing()
    {
        return pushObject != null;
    }

    public void ClampToPath()
    {
        Vector3 clampedPos = pushObject.FindNearestPointOnPath(tr.position);
        Vector3 delta = clampedPos - tr.position;
        delta.y = 0;
        cc.Move(delta);
    }
}