using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(InputHandler))]
public class PlayerMovement : MonoBehaviour {
    #region Constants
    
    const float VOID_KILL_Y = -80f;
    const float RESPAWN_HEIGHT_OFFSET = 16f;
    const float STANCE_TRANSITION_SPEED = 10f;
    const float GROUNDED_GRAVITY_RESET = -2f;
    const float EDGE_CHECK_DISTANCE = 0.5f;
    const float EDGE_CHECK_Y_OFFSET = 0.1f;
    
    #endregion

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
    
    CharacterController charController;
    InputHandler inputHandler;
    WorldManager worldManager;
    Vector3 velocity;

    bool isFlying = false;
    bool wasJumping = false;
    float lastJumpPressTime = 0f;
    
    #endregion
    
    #region Lifecycle Methods

    void Awake() {
        charController = GetComponent<CharacterController>();
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
        if (transform.position.y < VOID_KILL_Y) {
            charController.enabled = false;
            
            float maxHeight = (worldManager.config.chunkBounds * VoxelData.ChunkHeight) + RESPAWN_HEIGHT_OFFSET;
            transform.position = new Vector3(transform.position.x, maxHeight, transform.position.z);
            velocity = Vector3.zero;
            
            charController.enabled = true;
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

        charController.height = Mathf.Lerp(charController.height, targetHeight, Time.deltaTime * STANCE_TRANSITION_SPEED);
        charController.center = new Vector3(0, charController.height / 2f, 0);

        Vector3 camPos = cameraTarget.localPosition;
        camPos.y = Mathf.Lerp(camPos.y, camTargetY, Time.deltaTime * STANCE_TRANSITION_SPEED);
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
        charController.Move(finalVelocity * Time.deltaTime);

        if (charController.isGrounded && !jumpHeld) {
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
        bool isGrounded = charController.isGrounded;

        HandleEdgeDetection(ref finalVelocity, isGrounded);
        HandleJumping(jumpInput, isGrounded);
        HandleGravity();
        
        finalVelocity.y = velocity.y;
        charController.Move(finalVelocity * Time.deltaTime);
    }
    
    float GetCurrentSpeed() {
        if (inputHandler.IsCrawling) return crawlSpeed;
        if (inputHandler.IsCrouching) return crouchSpeed;
        if (inputHandler.IsSprinting) return runSpeed;
        return walkSpeed;
    }
    
    void HandleEdgeDetection(ref Vector3 finalVelocity, bool isGrounded) {
        if (inputHandler.IsCrouching && isGrounded) {
            Vector3 futurePosX = transform.position + new Vector3(finalVelocity.x * Time.deltaTime, EDGE_CHECK_Y_OFFSET, 0);
            if (!Physics.Raycast(futurePosX, Vector3.down, EDGE_CHECK_DISTANCE)) {
                finalVelocity.x = 0;
            }

            Vector3 futurePosZ = transform.position + new Vector3(0, EDGE_CHECK_Y_OFFSET, finalVelocity.z * Time.deltaTime);
            if (!Physics.Raycast(futurePosZ, Vector3.down, EDGE_CHECK_DISTANCE)) {
                finalVelocity.z = 0;
            }
        }
    }
    
    void HandleJumping(bool jumpInput, bool isGrounded) {
        if (isGrounded && velocity.y < 0) {
            velocity.y = GROUNDED_GRAVITY_RESET; 
        }

        bool canJump = !inputHandler.IsCrawling && !inputHandler.IsCrouching;
        if (jumpInput && isGrounded && canJump && !wasJumping) {
            velocity.y = Mathf.Sqrt(jumpHeight * GROUNDED_GRAVITY_RESET * gravity);
        }
    }
    
    void HandleGravity() {
        velocity.y += gravity * Time.deltaTime;
    }
    
    #endregion
}
