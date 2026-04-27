using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour {
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float jumpForce = 5f;
    public float gravity = -15f;

    CharacterController controller;
    GameInput input;
    Vector3 velocity;
    bool isGrounded;

    void Awake() {
        controller = GetComponent<CharacterController>();
        input = new GameInput();
    }

    void OnEnable() {
        input.Enable();
    }

    void OnDisable() {
        input.Disable();
    }

    void Start() {
    }

    // Read movement input, then apply horizontal movement and jump physics.
    void Update() {
        isGrounded = controller.isGrounded;
        
        if (isGrounded && velocity.y < 0) {
            velocity.y = -2f; 
        }

        Vector2 moveInput = input.Player.Move.ReadValue<Vector2>();
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        bool isSprinting = input.Player.Sprint.IsPressed();
        float currentSpeed = isSprinting ? runSpeed : walkSpeed;

        controller.Move(move * currentSpeed * Time.deltaTime);

        if (input.Player.Jump.WasPressedThisFrame() && isGrounded) {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
