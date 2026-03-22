using UnityEngine;
using UnityEngine.InputSystem;

// 1. We replace Rigidbody and CapsuleCollider with CharacterController
[RequireComponent(typeof(CharacterController))]
public class PlayerControls : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float gravity = -15f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 1.5f;

    [Header("Look Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 15f;
    [SerializeField] private float maxLookAngle = 85f;

    // References
    private CharacterController cc;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction sprintAction;
    private InputAction jumpAction;

    // State Variables
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isSprinting;
    private float cameraPitch = 0f;
    private float playerYaw = 0f;
    private Vector3 velocity;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
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
        if (PhoneController.isGamePaused) return;

        ReadInputs();
        HandleLooking();
        HandleMovement();
    }

    private void ReadInputs()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        lookInput = lookAction.ReadValue<Vector2>();
        isSprinting = sprintAction.IsPressed();
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

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        Vector3 moveDirection = (transform.right * moveInput.x) + (transform.forward * moveInput.y);
        moveDirection.Normalize();

        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        cc.Move(moveDirection * currentSpeed * Time.deltaTime);

        if (jumpAction.WasPressedThisFrame() && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
    }

    public Vector3 GetCameraForward()
    {
        return cameraTransform.forward;
    }
}