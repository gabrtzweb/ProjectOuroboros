using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class RingPlayerController : MonoBehaviour
{
    public WorldManager worldManager;
    public float moveSpeed = 20f;
    public float lookSensitivity = 0.5f;
    public float flySpeed = 40f;
    public float jumpForce = 10f;
    public float gravityStrength = 20f;
    public float doubleTapWindow = 0.3f;

    private float xRotation = 0f;
    private Transform cameraTransform;
    private GameInput gameInput;
    private Rigidbody rb;
    private bool isFlying = true;
    private float lastJumpTime = 0f;
    private bool isGrounded;

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
        rb.useGravity = false;
        rb.freezeRotation = true;

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

        isGrounded = Physics.Raycast(transform.position, -targetUp, 1.1f, ~0, QueryTriggerInteraction.Ignore);

        Vector2 moveInput = gameInput.Player.Move.ReadValue<Vector2>();
        Vector3 moveDirection = (transform.right * moveInput.x) + (transform.forward * moveInput.y);

        if (isFlying)
        {
            float verticalInput = 0f;

            if (gameInput.Player.Jump.IsPressed())
            {
                verticalInput += 1f;
            }

            if (gameInput.Player.Sprint.IsPressed())
            {
                verticalInput -= 1f;
            }

            moveDirection += transform.up * verticalInput;
            rb.velocity = moveDirection.normalized * flySpeed;
            return;
        }

        rb.AddForce(-targetUp * gravityStrength, ForceMode.Acceleration);

        Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
        Vector3 localTargetVelocity = new Vector3(moveInput.x * moveSpeed, localVelocity.y, moveInput.y * moveSpeed);
        rb.velocity = transform.TransformDirection(localTargetVelocity);

    }
}
