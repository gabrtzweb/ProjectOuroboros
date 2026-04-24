using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class RingPlayerController : MonoBehaviour
{
    public WorldManager worldManager;
    public float moveSpeed = 4f;
    public float runSpeedMultiplier = 1.5f;
    public float crouchSpeedMultiplier = 0.5f;
    public float flyRunSpeedMultiplier = 1.5f;
    public float lookSensitivity = 0.5f;
    public float flySpeed = 40f;
    public float jumpForce = 7f;
    public float gravityStrength = 20f;
    public float doubleTapWindow = 0.3f;

    private float xRotation = 0f;
    private Transform cameraTransform;
    private GameInput gameInput;
    private Rigidbody rb;
    private BoxCollider playerCollider;
    private bool isFlying = false;
    private float lastJumpTime = 0f;
    private bool isGrounded;
    private float defaultHeight = 2f;
    private float crouchHeight = 1f;
    private float defaultCamY = 1.6f;
    private float crouchCamY = 0.8f;

    private void Awake()
    {
        gameInput = new GameInput();
    }

    private void OnEnable()
    {
        gameInput.Enable();
    }

    private void OnDisable()
    {
        gameInput.Disable();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<BoxCollider>();

        playerCollider.size = new Vector3(0.8f, 2f, 0.8f);
        playerCollider.center = new Vector3(0f, 1f, 0f);

        rb.useGravity = false;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        if (worldManager != null)
        {
            float surfaceY = Mathf.PerlinNoise(worldManager.XOffset * worldManager.NoiseScale, worldManager.ZOffset * worldManager.NoiseScale) * worldManager.HeightMultiplier;
            transform.position = new Vector3(0f, surfaceY + 2f, 0f);
        }
        else
        {
            transform.position = new Vector3(0f, 2f, 0f);
        }

        Camera childCamera = GetComponentInChildren<Camera>();
        if (childCamera != null)
        {
            cameraTransform = childCamera.transform;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Vector2 lookInput = gameInput.Player.Look.ReadValue<Vector2>();

        xRotation -= lookInput.y * lookSensitivity;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        transform.Rotate(0f, lookInput.x * lookSensitivity, 0f, Space.Self);

        bool isCrouching = gameInput.Player.Crouch.IsPressed();
        if (isCrouching && !isFlying)
        {
            playerCollider.size = new Vector3(0.8f, crouchHeight, 0.8f);
            playerCollider.center = new Vector3(0f, crouchHeight / 2f, 0f);
            if (cameraTransform != null)
            {
                cameraTransform.localPosition = new Vector3(0f, crouchCamY, 0f);
            }
        }
        else
        {
            playerCollider.size = new Vector3(0.8f, defaultHeight, 0.8f);
            playerCollider.center = new Vector3(0f, defaultHeight / 2f, 0f);
            if (cameraTransform != null)
            {
                cameraTransform.localPosition = new Vector3(0f, defaultCamY, 0f);
            }
        }

        isGrounded = Physics.Raycast(transform.position, -transform.up, 1.1f);

        if (gameInput.Player.Jump.WasPressedThisFrame())
        {
            if (Time.time - lastJumpTime <= doubleTapWindow)
            {
                isFlying = !isFlying;
            }

            lastJumpTime = Time.time;

            if (!isFlying && isGrounded)
            {
                rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            }
        }
    }

    private void FixedUpdate()
    {
        if (worldManager == null || rb == null)
        {
            return;
        }

        Vector3 ringCenter = new Vector3(transform.position.x, worldManager.RingRadius, 0f);
        Vector3 targetUp = (ringCenter - transform.position).normalized;

        rb.MoveRotation(Quaternion.FromToRotation(transform.up, targetUp) * transform.rotation);

        Vector2 moveInput = gameInput.Player.Move.ReadValue<Vector2>();
        bool isRunning = gameInput.Player.Sprint.IsPressed();
        bool isCrouching = gameInput.Player.Crouch.IsPressed();
        Vector3 moveDirection = (transform.right * moveInput.x) + (transform.forward * moveInput.y);

        if (isFlying)
        {
            float verticalInput = 0f;

            if (gameInput.Player.Jump.IsPressed())
            {
                verticalInput += 1f;
            }

            if (isCrouching)
            {
                verticalInput -= 1f;
            }

            moveDirection += transform.up * verticalInput;
            float currentFlySpeed = flySpeed;
            if (isRunning)
            {
                currentFlySpeed *= flyRunSpeedMultiplier;
            }

            rb.linearVelocity = moveDirection.normalized * currentFlySpeed;
            return;
        }

        rb.AddForce(-targetUp * gravityStrength, ForceMode.Acceleration);

        Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);
        float currentSpeed = moveSpeed;
        if (isRunning)
        {
            currentSpeed *= runSpeedMultiplier;
        }
        else if (isCrouching)
        {
            currentSpeed *= crouchSpeedMultiplier;
        }

        if (!isGrounded)
        {
            currentSpeed *= 0.6f;
        }

        Vector3 targetLocalVel = new Vector3(moveInput.x * currentSpeed, localVel.y, moveInput.y * currentSpeed);
        rb.linearVelocity = transform.TransformDirection(targetLocalVel);

    }
}
