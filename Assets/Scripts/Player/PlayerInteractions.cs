using UnityEngine;

[RequireComponent(typeof(InputHandler))]
public class PlayerInteractions : MonoBehaviour {
    [Header("Interaction Settings")]
    public float reach = 5f;
    public float stepSize = 0.05f; // Tamanho do passo do raio (precisão)
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

    // O nosso novo Raycast Matemático que ignora os colisores da Unity
    bool VoxelRaycast(out Vector3Int hitBlock, out Vector3Int previousBlock) {
        hitBlock = Vector3Int.zero;
        previousBlock = Vector3Int.zero;
        
        Vector3 currentPos = playerCamera.position;
        Vector3 forward = playerCamera.forward;
        Vector3Int lastPosInt = new Vector3Int(Mathf.FloorToInt(currentPos.x), Mathf.FloorToInt(currentPos.y), Mathf.FloorToInt(currentPos.z));

        // Avança o raio gradualmente
        for (float t = 0; t < reach; t += stepSize) {
            currentPos = playerCamera.position + (forward * t);
            Vector3Int currentPosInt = new Vector3Int(Mathf.FloorToInt(currentPos.x), Mathf.FloorToInt(currentPos.y), Mathf.FloorToInt(currentPos.z));

            // Só checa o dicionário se o raio entrou em um bloco novo
            if (currentPosInt != lastPosInt) {
                BlockType block = worldManager.GetBlockFromGlobal(currentPosInt);
                
                // Se não for ar e nem água, encontramos nosso alvo!
                if (block != BlockType.Air && block != BlockType.Water) {
                    hitBlock = currentPosInt;
                    previousBlock = lastPosInt; // O bloco de ar logo antes do impacto
                    return true;
                }
                lastPosInt = currentPosInt;
            }
        }
        return false;
    }

    void UpdateHighlight() {
        if (highlightBox == null) return;

        if (VoxelRaycast(out Vector3Int hitBlock, out _)) {
            highlightBox.SetActive(true);
            highlightBox.transform.position = hitBlock - new Vector3(0.005f, 0.005f, 0.005f);
        } else {
            highlightBox.SetActive(false);
        }
    }

    void Interact(bool isPlacing) {
        if (VoxelRaycast(out Vector3Int hitBlock, out Vector3Int previousBlock)) {
            if (isPlacing) {
                // Coloca o bloco no espaço de ar logo antes do impacto
                worldManager.SetBlock(previousBlock, selectedBlock);
            } else {
                // Destrói o bloco atingido
                worldManager.SetBlock(hitBlock, BlockType.Air);
            }
        }
    }

    void PickBlock() {
        if (VoxelRaycast(out Vector3Int hitBlock, out _)) {
            BlockType blockTargeted = worldManager.GetBlockFromGlobal(hitBlock);
            if (blockTargeted != BlockType.Air && blockTargeted != BlockType.Water) {
                selectedBlock = blockTargeted;
            }
        }
    }
}
