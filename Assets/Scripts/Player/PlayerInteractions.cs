using UnityEngine;

[RequireComponent(typeof(InputHandler))]
public class PlayerInteractions : MonoBehaviour {
    [Header("Interaction Settings")]
    public float reach = 5f;
    public BlockType selectedBlock = BlockType.OakPlanks; 

    [Header("Highlight Box")]
    public GameObject highlightBox;

    InputHandler inputHandler;
    Transform playerCamera;
    WorldManager worldManager;

    void Awake() {
        inputHandler = GetComponent<InputHandler>();
    }

    void Start() {
        playerCamera = Camera.main.transform;
        worldManager = Object.FindAnyObjectByType<WorldManager>();
    }

    void Update() {
        UpdateHighlight();

        if (inputHandler.PrimaryActionPressed) Interact(false);
        if (inputHandler.SecondaryActionPressed) Interact(true);
        if (inputHandler.PickBlockPressed) PickBlock();
    }

    void UpdateHighlight() {
        if (highlightBox == null) return;

        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit, reach)) {
            Vector3 targetPos = hit.point - (hit.normal * 0.01f);
            
            Vector3Int blockPos = new Vector3Int(
                Mathf.FloorToInt(targetPos.x),
                Mathf.FloorToInt(targetPos.y),
                Mathf.FloorToInt(targetPos.z)
            );

            BlockType blockTargeted = worldManager.GetBlockFromGlobal(blockPos);
            
            if (blockTargeted != BlockType.Air && blockTargeted != BlockType.Water) {
                highlightBox.SetActive(true);
                highlightBox.transform.position = blockPos - new Vector3(0.005f, 0.005f, 0.005f);
            } else {
                highlightBox.SetActive(false);
            }
        } else {
            highlightBox.SetActive(false);
        }
    }

    void Interact(bool isPlacing) {
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit, reach)) {
            Vector3 targetPos = isPlacing ? hit.point + (hit.normal * 0.01f) : hit.point - (hit.normal * 0.01f);
            
            Vector3Int blockPos = new Vector3Int(
                Mathf.FloorToInt(targetPos.x),
                Mathf.FloorToInt(targetPos.y),
                Mathf.FloorToInt(targetPos.z)
            );

            BlockType typeToSet = isPlacing ? selectedBlock : BlockType.Air;
            worldManager.SetBlock(blockPos, typeToSet);
        }
    }

    void PickBlock() {
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit, reach)) {
            Vector3 targetPos = hit.point - (hit.normal * 0.01f);
            
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
}
