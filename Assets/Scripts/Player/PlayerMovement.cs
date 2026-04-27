using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(PlayerInputHandler))]
public class PlayerMovement : MonoBehaviour {
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float jumpForce = 5f;
    public float gravity = -15f;

    CharacterController controller;
    PlayerInputHandler inputHandler;
    Vector3 velocity;

    void Awake() {
        controller = GetComponent<CharacterController>();
        inputHandler = GetComponent<PlayerInputHandler>();
    }

    void Update() {
        ApplyMovement();
        ApplyGravityAndJump();
    }

    void ApplyMovement() {
        float currentSpeed = inputHandler.IsSprinting ? runSpeed : walkSpeed;
        Vector2 moveInput = inputHandler.MoveInput;
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        
        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    void ApplyGravityAndJump() {
        bool isGrounded = controller.isGrounded;
        
        if (isGrounded && velocity.y < 0) {
            velocity.y = -2f; 
        }

        if (inputHandler.IsJumping && isGrounded) {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
