using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(InputHandler))]
public class PlayerMovement : MonoBehaviour {
    [Header("Movement Speeds")]
    public float walkSpeed = 4.3f;
    public float runSpeed = 5.6f;
    public float crouchSpeed = 1.5f;
    public float crawlSpeed = 1.0f;

    [Header("Physics")]
    public float jumpHeight = 1.25f; 
    public float gravity = -30f;

    [Header("Stance & Camera")]
    public Transform cameraTarget;
    public float standingHeight = 1.8f;
    public float crouchingHeight = 1.5f;
    public float crawlingHeight = 0.9f;

    CharacterController controller;
    InputHandler inputHandler;
    Vector3 velocity;

    void Awake() {
        controller = GetComponent<CharacterController>();
        inputHandler = GetComponent<InputHandler>();
    }

    void Update() {
        ApplyStance();
        ApplyMovement();
        ApplyGravityAndJump();
    }

    void ApplyStance() {
        if (cameraTarget == null) return;

        float targetHeight = standingHeight;
        float camTargetY = 1.5f;

        if (inputHandler.IsCrawling) {
            targetHeight = crawlingHeight;
            camTargetY = 0.6f;
        } else if (inputHandler.IsCrouching) {
            targetHeight = crouchingHeight;
            camTargetY = 1.2f;
        }

        // Smoothly transition controller height
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * 10f);
        
        // Keep the player's feet grounded by adjusting the local center
        controller.center = new Vector3(0, (controller.height / 2f) - 0.9f, 0);

        // Smoothly transition camera position
        Vector3 camPos = cameraTarget.localPosition;
        camPos.y = Mathf.Lerp(camPos.y, camTargetY, Time.deltaTime * 10f);
        cameraTarget.localPosition = camPos;
    }

    void ApplyMovement() {
        float currentSpeed = walkSpeed;

        if (inputHandler.IsCrawling) {
            currentSpeed = crawlSpeed;
        } else if (inputHandler.IsCrouching) {
            currentSpeed = crouchSpeed;
        } else if (inputHandler.IsSprinting) {
            currentSpeed = runSpeed;
        }

        Vector2 moveInput = inputHandler.MoveInput;
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        
        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    void ApplyGravityAndJump() {
        bool isGrounded = controller.isGrounded;
        
        if (isGrounded && velocity.y < 0) {
            velocity.y = -2f; 
        }

        // A player cannot jump while crawling or crouching
        bool canJump = !inputHandler.IsCrawling && !inputHandler.IsCrouching;

        if (inputHandler.IsJumping && isGrounded && canJump) {
            // New physics formula for exact jump height
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
