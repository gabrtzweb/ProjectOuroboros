using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(InputHandler))]
public class PlayerMovement : MonoBehaviour {
    #region Serialized Movement Settings
    
    [Header("Movement Speeds")]
    public float walkSpeed = 4.8f;
    public float runSpeed = 6.8f;
    public float crouchSpeed = 1.4f;
    public float crawlSpeed = 0.8f;
    
    #endregion
    
    #region Serialized Flight Settings
    
    [Header("Flight Settings")]
    public float flySpeed = 10f;
    public float fastFlySpeed = 22f;
    public float verticalFlySpeed = 8f;
    public float doubleTapWindow = 0.3f;
    
    #endregion
    
    #region Serialized Physics Settings
    
    [Header("Physics")]
    public float jumpHeight = 1.32f; 
    public float gravity = -16f;
    
    #endregion
    
    #region Serialized Stance Settings
    
    [Header("Stance & Camera")]
    public Transform cameraTarget;
    public float standingHeight = 1.8f;
    public float crouchingHeight = 1.4f;
    public float crawlingHeight = 0.8f;
    
    #endregion
    
    #region Private Fields
    
    CharacterController controller;
    InputHandler inputHandler;
    WorldManager worldManager;
    Vector3 velocity;

    bool isFlying = false;
    bool wasJumping = false;
    float lastJumpPressTime = 0f;
    
    #endregion
    
    #region Lifecycle Methods

    void Awake() {
        controller = GetComponent<CharacterController>();
        inputHandler = GetComponent<InputHandler>();
    }

    void Start() {
        worldManager = Object.FindAnyObjectByType<WorldManager>();
    }

    void Update() {
        CheckVoidFall();
        ApplyStance();
        MovePlayer();
    }
    
    #endregion
    
    #region Void Fall Detection

    void CheckVoidFall() {
        if (transform.position.y < -80f) {
            controller.enabled = false;
            
            float maxHeight = (worldManager.chunkBounds * VoxelData.ChunkHeight) + 16f;
            transform.position = new Vector3(transform.position.x, maxHeight, transform.position.z);
            velocity = Vector3.zero;
            
            controller.enabled = true;
        }
    }
    
    #endregion
    
    #region Stance System

    void ApplyStance() {
        if (cameraTarget == null) return;

        float targetHeight = standingHeight;
        float camTargetY = 1.5f;

        if (inputHandler.IsCrawling) {
            targetHeight = crawlingHeight;
            camTargetY = 0.6f;
        } else if (inputHandler.IsCrouching && !isFlying) {
            targetHeight = crouchingHeight;
            camTargetY = 1.2f;
        }

        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * 10f);
        controller.center = new Vector3(0, controller.height / 2f, 0);

        Vector3 camPos = cameraTarget.localPosition;
        camPos.y = Mathf.Lerp(camPos.y, camTargetY, Time.deltaTime * 10f);
        cameraTarget.localPosition = camPos;
    }
    
    #endregion
    
    #region Movement and Flight

    void MovePlayer() {
        bool jumpInput = inputHandler.IsJumping;
        bool jumpHeld = inputHandler.IsJumpHeld;

        HandleDoubleJumpForFlight(jumpInput);
        
        if (isFlying) {
            HandleFlightMovement(jumpHeld);
        } else {
            HandleGroundMovement(jumpInput, jumpHeld);
        }

        wasJumping = jumpInput;
    }
    
    #endregion
    
    #region Flight Mechanics
    
    void HandleDoubleJumpForFlight(bool jumpInput) {
        if (jumpInput && !wasJumping) {
            if (Time.time - lastJumpPressTime <= doubleTapWindow) {
                isFlying = !isFlying;
            }
            lastJumpPressTime = Time.time;
        }
    }
    
    void HandleFlightMovement(bool jumpHeld) {
        Vector2 moveInput = inputHandler.MoveInput;
        Vector3 horizontalMove = transform.right * moveInput.x + transform.forward * moveInput.y;
        
        float currentSpeed = inputHandler.IsSprinting ? fastFlySpeed : flySpeed;
        Vector3 finalVelocity = horizontalMove * currentSpeed;

        if (jumpHeld) {
            velocity.y = verticalFlySpeed;
        } else if (inputHandler.IsCrouching) {
            velocity.y = -verticalFlySpeed;
        } else {
            velocity.y = 0f; 
        }

        finalVelocity.y = velocity.y;
        controller.Move(finalVelocity * Time.deltaTime);

        if (controller.isGrounded && !jumpHeld) {
            isFlying = false;
        }
    }
    
    #endregion
    
    #region Ground Movement
    
    void HandleGroundMovement(bool jumpInput, bool jumpHeld) {
        Vector2 moveInput = inputHandler.MoveInput;
        Vector3 horizontalMove = transform.right * moveInput.x + transform.forward * moveInput.y;

        float currentSpeed = GetCurrentSpeed();
        Vector3 finalVelocity = horizontalMove * currentSpeed;
        bool isGrounded = controller.isGrounded;

        HandleEdgeDetection(ref finalVelocity, isGrounded);
        HandleJumping(jumpInput, isGrounded);
        HandleGravity();
        
        finalVelocity.y = velocity.y;
        controller.Move(finalVelocity * Time.deltaTime);
    }
    
    float GetCurrentSpeed() {
        if (inputHandler.IsCrawling) return crawlSpeed;
        if (inputHandler.IsCrouching) return crouchSpeed;
        if (inputHandler.IsSprinting) return runSpeed;
        return walkSpeed;
    }
    
    void HandleEdgeDetection(ref Vector3 finalVelocity, bool isGrounded) {
        if (inputHandler.IsCrouching && isGrounded) {
            float checkDistance = 0.5f;
            
            Vector3 futurePosX = transform.position + new Vector3(finalVelocity.x * Time.deltaTime, 0.1f, 0);
            if (!Physics.Raycast(futurePosX, Vector3.down, checkDistance)) {
                finalVelocity.x = 0;
            }

            Vector3 futurePosZ = transform.position + new Vector3(0, 0.1f, finalVelocity.z * Time.deltaTime);
            if (!Physics.Raycast(futurePosZ, Vector3.down, checkDistance)) {
                finalVelocity.z = 0;
            }
        }
    }
    
    void HandleJumping(bool jumpInput, bool isGrounded) {
        if (isGrounded && velocity.y < 0) {
            velocity.y = -2f; 
        }

        bool canJump = !inputHandler.IsCrawling && !inputHandler.IsCrouching;
        if (jumpInput && isGrounded && canJump && !wasJumping) {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }
    
    void HandleGravity() {
        velocity.y += gravity * Time.deltaTime;
    }
    
    #endregion
}
