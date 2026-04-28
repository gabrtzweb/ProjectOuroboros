using UnityEngine;

public class UnderwaterCamera : MonoBehaviour {
    WorldManager worldManager;
    
    Color normalFogColor;
    float normalFogDensity;
    bool normalFogState;

    void Start() {
        worldManager = Object.FindAnyObjectByType<WorldManager>();
        
        normalFogColor = RenderSettings.fogColor;
        normalFogDensity = RenderSettings.fogDensity;
        normalFogState = RenderSettings.fog;
    }

    void Update() {
        if (worldManager == null) return;

        Vector3Int camPos = new Vector3Int(
            Mathf.FloorToInt(transform.position.x),
            Mathf.FloorToInt(transform.position.y),
            Mathf.FloorToInt(transform.position.z)
        );

        BlockType currentBlock = worldManager.GetBlockFromGlobal(camPos);

        if (currentBlock == BlockType.Water) {
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.1f, 0.3f, 0.6f, 1f);
            RenderSettings.fogDensity = 0.02f;
        } else {
            RenderSettings.fog = normalFogState;
            RenderSettings.fogColor = normalFogColor;
            RenderSettings.fogDensity = normalFogDensity;
        }
    }
}
