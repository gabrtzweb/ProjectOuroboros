using UnityEngine;
using UnityEngine.InputSystem;

public class RingPlayerController : MonoBehaviour
{
    public WorldManager worldManager;
    public float moveSpeed = 20f;
    public float lookSensitivity = 0.5f;

    private float xRotation = 0f;
    private Transform cameraTransform;
    private GameInput gameInput;

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

        Vector2 moveInput = gameInput.Player.Move.ReadValue<Vector2>();
        Vector3 moveDirection = (transform.right * moveInput.x) + (transform.forward * moveInput.y);
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);

        if (worldManager != null)
        {
            Vector3 ringCenter = new Vector3(transform.position.x, worldManager.RingRadius, 0f);
            Vector3 targetUp = (ringCenter - transform.position).normalized;
            transform.rotation = Quaternion.FromToRotation(transform.up, targetUp) * transform.rotation;
        }
    }
}
