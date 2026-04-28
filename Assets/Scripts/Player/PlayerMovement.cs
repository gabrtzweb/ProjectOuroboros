using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(InputHandler))]
public class PlayerMovement : MonoBehaviour {
    [Header("Movement Speeds")]
    public float walkSpeed = 4.8f;
    public float runSpeed = 8.4f;
    public float crouchSpeed = 1.4f;
    public float crawlSpeed = 0.8f;

    [Header("Physics")]
    public float jumpHeight = 1.32f; 
    public float gravity = -16f;

    [Header("Stance & Camera")]
    public Transform cameraTarget;
    public float standingHeight = 1.8f;
    public float crouchingHeight = 1.4f;
    public float crawlingHeight = 0.8f;

    CharacterController controller;
    InputHandler inputHandler;
    Vector3 velocity;

    void Awake() {
        controller = GetComponent<CharacterController>();
        inputHandler = GetComponent<InputHandler>();
    }

    void Update() {
        ApplyStance();
        MovePlayer();
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

        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * 10f);
        
        // CORREÇÃO 1: Mantém o pivô perfeitamente alinhado com o chão
        controller.center = new Vector3(0, controller.height / 2f, 0);

        Vector3 camPos = cameraTarget.localPosition;
        camPos.y = Mathf.Lerp(camPos.y, camTargetY, Time.deltaTime * 10f);
        cameraTarget.localPosition = camPos;
    }

    void MovePlayer() {
        float currentSpeed = walkSpeed;

        if (inputHandler.IsCrawling) {
            currentSpeed = crawlSpeed;
        } else if (inputHandler.IsCrouching) {
            currentSpeed = crouchSpeed;
        } else if (inputHandler.IsSprinting) {
            currentSpeed = runSpeed;
        }

        Vector2 moveInput = inputHandler.MoveInput;
        Vector3 horizontalMove = transform.right * moveInput.x + transform.forward * moveInput.y;
        Vector3 finalVelocity = horizontalMove * currentSpeed;

        bool isGrounded = controller.isGrounded;
        
        if (isGrounded && velocity.y < 0) {
            velocity.y = -2f; 
        }

        bool canJump = !inputHandler.IsCrawling && !inputHandler.IsCrouching;

        if (inputHandler.IsJumping && isGrounded && canJump) {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        
        // CORREÇÃO 2: Junta a velocidade horizontal e vertical em um único Move
        finalVelocity.y = velocity.y;
        controller.Move(finalVelocity * Time.deltaTime);
    }
}
