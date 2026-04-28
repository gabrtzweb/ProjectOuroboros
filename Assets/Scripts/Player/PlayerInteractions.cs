using UnityEngine;

[RequireComponent(typeof(InputHandler), typeof(CharacterController))]
public class PlayerInteractions : MonoBehaviour {
    #region Constants
    
    const float RAYCAST_HIT_OFFSET = 0.01f;
    const float HIGHLIGHT_BOX_OFFSET = 0.005f;
    const float BOUNDS_EXPANSION = -0.05f;
    
    #endregion

    #region Serialized Fields
    
    [Header("Interaction Settings")]
    public float reach = 6f;
    public BlockType selectedBlock = BlockType.OakPlanks; 
    public float interactionCooldown = 0.15f;

    [Header("Highlight Box")]
    public GameObject highlightBox;
    
    #endregion
    
    #region Private Fields
    
    InputHandler inputHandler;
    CharacterController playerController;
    Transform playerCamera;
    WorldManager worldManager;

    float lastInteractTime;
    
    #endregion
    
    #region Lifecycle Methods

    void Awake() {
        inputHandler = GetComponent<InputHandler>();
        playerController = GetComponent<CharacterController>();
    }

    void Start() {
        playerCamera = Camera.main.transform;
        worldManager = Object.FindAnyObjectByType<WorldManager>();
    }

    void Update() {
        UpdateHighlight();
        HandleBlockInteractions();
        HandlePickBlock();
    }
    
    #endregion
    
    #region Highlight and Targeting

    void UpdateHighlight() {
        if (highlightBox == null) return;

        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit, reach)) {
            Vector3 targetPos = hit.point - (hit.normal * RAYCAST_HIT_OFFSET);
            
            Vector3Int blockPos = new Vector3Int(
                Mathf.FloorToInt(targetPos.x),
                Mathf.FloorToInt(targetPos.y),
                Mathf.FloorToInt(targetPos.z)
            );

            BlockType blockTargeted = worldManager.GetBlockFromGlobal(blockPos);
            
            if (blockTargeted != BlockType.Air && blockTargeted != BlockType.Water) {
                highlightBox.SetActive(true);
                highlightBox.transform.position = blockPos - new Vector3(HIGHLIGHT_BOX_OFFSET, HIGHLIGHT_BOX_OFFSET, HIGHLIGHT_BOX_OFFSET);
            } else {
                highlightBox.SetActive(false);
            }
        } else {
            highlightBox.SetActive(false);
        }
    }
    
    #endregion
    
    #region Block Interactions
    
    void HandleBlockInteractions() {
        bool isBreaking = inputHandler.IsPrimaryActionHeld; 
        bool isPlacing = inputHandler.IsSecondaryActionHeld;

        if (isBreaking && Time.time - lastInteractTime >= interactionCooldown) {
            Interact(false);
            lastInteractTime = Time.time;
        }
        
        if (isPlacing && Time.time - lastInteractTime >= interactionCooldown) {
            Interact(true);
            lastInteractTime = Time.time;
        }
    }
    
    void Interact(bool isPlacing) {
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit, reach)) {
            Vector3 targetPos = isPlacing ? hit.point + (hit.normal * RAYCAST_HIT_OFFSET) : hit.point - (hit.normal * RAYCAST_HIT_OFFSET);
            
            Vector3Int blockPos = new Vector3Int(
                Mathf.FloorToInt(targetPos.x),
                Mathf.FloorToInt(targetPos.y),
                Mathf.FloorToInt(targetPos.z)
            );

            if (isPlacing) {
                Bounds blockBounds = new Bounds(blockPos + new Vector3(0.5f, 0.5f, 0.5f), Vector3.one);
                blockBounds.Expand(BOUNDS_EXPANSION); 
                
                if (playerController.bounds.Intersects(blockBounds)) {
                    return;
                }
            } else {
                if (worldManager.GetBlockFromGlobal(blockPos) == BlockType.Bedrock) {
                    return;
                }
            }

            BlockType typeToSet = isPlacing ? selectedBlock : BlockType.Air;
            worldManager.SetBlock(blockPos, typeToSet);
        }
    }
    
    #endregion
    
    #region Pick Block
    
    void HandlePickBlock() {
        if (inputHandler.PickBlockPressed) PickBlock();
    }
    
    void PickBlock() {
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit, reach)) {
            Vector3 targetPos = hit.point - (hit.normal * RAYCAST_HIT_OFFSET);
            
            Vector3Int blockPos = new Vector3Int(
                Mathf.FloorToInt(targetPos.x),
                Mathf.FloorToInt(targetPos.y),
                Mathf.FloorToInt(targetPos.z)
            );

            BlockType blockTargeted = worldManager.GetBlockFromGlobal(blockPos);
            
            if (blockTargeted != BlockType.Air && blockTargeted != BlockType.Water) {
                selectedBlock = blockTargeted;
            }
        }
    }
    
    #endregion
}
