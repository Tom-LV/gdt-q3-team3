using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerControls : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float airSpeed = 1f;
    [SerializeField] private float gravity = -15f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float jumpBufferTime = 0.2f;

    [Header("Crouch Settings")]
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchTransitionSpeed = 10f;

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
    private InputAction crouchAction;

    // State Variables
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isSprinting;
    private bool isCrouching;

    private float jumpBuffer;
    private float cameraPitch = 0f;
    private float playerYaw = 0f;
    private Vector3 velocity;

    // Stored default sizes for crouching math
    private float standingHeight;
    private Vector3 standingCenter;
    private float standingCameraY;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        tr = GetComponent<Transform>();
        playerYaw = transform.eulerAngles.y;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Save original sizes so we can scale back to them when standing
        standingHeight = cc.height;
        standingCenter = cc.center;
        standingCameraY = cameraPivotTransform.localPosition.y;
    }

    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");
        sprintAction = InputSystem.actions.FindAction("Sprint");
        jumpAction = InputSystem.actions.FindAction("Jump");
        crouchAction = InputSystem.actions.FindAction("Crouch");

        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 0.1f);
    }

    void Update()
    {


        ReadInputs();
        HandleLooking();
        HandleCrouching();
        HandleMovement();
    }

    private void ReadInputs()
    {
        if (PhoneController.isGamePaused)
        {
            moveInput = new Vector2(0f, 0f);
            lookInput = new Vector2(0f, 0f);
            jumpBuffer = 0f;
            isCrouching = false;
            return;
        }
        moveInput = moveAction.ReadValue<Vector2>();
        lookInput = lookAction.ReadValue<Vector2>();
        jumpBuffer = jumpAction.WasPressedThisFrame() ? jumpBufferTime : jumpBuffer - Time.deltaTime;

        // Crouch Input Logic
        if (crouchAction.IsPressed() && cc.isGrounded)
        {
            isCrouching = true;
        }
        else if (isCrouching)
        {
            isCrouching = false;
            //// If we let go of the crouch button, shoot a sphere up to see if a ceiling blocks us
            //if (!Physics.SphereCast(tr.position + Vector3.up * crouchHeight, cc.radius + 0.05f, Vector3.up, out _, standingHeight - crouchHeight))
            //{
                
            //}
        }
    }

    public void SetSensitivity(float newSensitivity)
    {
        mouseSensitivity = newSensitivity;
    }

    private void HandleLooking()
    {
        if (PhoneController.isGamePaused) return;
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);

        playerYaw += mouseX;
        transform.rotation = Quaternion.Euler(0f, playerYaw, 0f);
    }

    private void HandleCrouching()
    {
        // Determine what height we *want* to be at
        float targetHeight = isCrouching ? crouchHeight : standingHeight;

        // Smoothly lerp the CharacterController's height
        cc.height = Mathf.Lerp(cc.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);

        float diff = standingHeight - cc.height;


        Vector3 ccCenter = standingCenter;
        ccCenter.y -= diff / 2;
        cc.center = ccCenter;

        Vector3 camPos = cameraPivotTransform.localPosition;
        camPos.y = standingCameraY - diff;
        cameraPivotTransform.localPosition = camPos;
    }

    private void HandleMovement()
    {
        bool isGrounded = cc.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        Vector3 moveDirection = (transform.right * moveInput.x) + (transform.forward * moveInput.y);
        moveDirection.Normalize();

        float currentSpeed;

        // Speed override logic: Crouching beats Sprinting
        if (isCrouching)
        {
            currentSpeed = crouchSpeed;
            isSprinting = false;
        }
        else if (sprintAction.IsPressed() && isSprinting)
        {
            currentSpeed = sprintSpeed;
        }
        else if (isSprinting)
        {
            currentSpeed = (sprintSpeed + walkSpeed) / 2;
        }
        else
        {
            currentSpeed = walkSpeed;
        }

        Vector3 xzMovement = new Vector3(velocity.x, 0, velocity.z);
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

        // Prevent jumping while crouched
        if (jumpAction.IsPressed() && jumpBuffer > 0 && isGrounded && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        Vector3 oldPosition = tr.position;
        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
        xzMovement = (oldPosition - tr.position) / Time.deltaTime;
        xzMovement.y = 0;

        isSprinting = sprintAction.IsPressed() ? true : isSprinting;
        if (Vector3.Dot(xzMovement.normalized, cameraPivotTransform.forward) > -0.7 || xzMovement.magnitude < walkSpeed)
        {
            isSprinting = false;
        }
    }

    public Vector3 GetCameraForward()
    {
        return cameraTransform.forward;
    }

    public bool IsSprinting()
    {
        return isSprinting;
    }

    public bool IsCrouching()
    {
        return isCrouching;
    }
}