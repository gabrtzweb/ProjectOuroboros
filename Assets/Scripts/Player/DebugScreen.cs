using UnityEngine;
using TMPro;

public class DebugScreen : MonoBehaviour {
    [Header("References")]
    public Transform playerTransform;
    public TextMeshProUGUI debugText;

    float minFps = float.MaxValue;
    float maxFps = 0f;
    float fpsTimer = 0f;
    int frameCount = 0;
    float currentFps = 0f;

    void Update() {
        if (playerTransform == null || debugText == null) return;

        fpsTimer += Time.unscaledDeltaTime;
        frameCount++;
        
        if (fpsTimer >= 0.25f) {
            currentFps = Mathf.RoundToInt(frameCount / fpsTimer);
            
            // Só começa a gravar os recordes depois de 3 segundos para evitar o lag inicial do Play
            if (Time.realtimeSinceStartup > 3f) {
                // Ignora o 0 para evitar bugs de travamento da thread
                if (currentFps < minFps && currentFps > 0) minFps = currentFps;
                if (currentFps > maxFps) maxFps = currentFps;
            }
            
            frameCount = 0;
            fpsTimer = 0f;
        }

        int x = Mathf.FloorToInt(playerTransform.position.x);
        int y = Mathf.FloorToInt(playerTransform.position.y);
        int z = Mathf.FloorToInt(playerTransform.position.z);

        string direction = GetFacingDirection(playerTransform.eulerAngles.y);

        debugText.text = $"FPS: {currentFps} (Min: {(minFps == float.MaxValue ? 0 : minFps)}, Max: {maxFps})\nCoordinates: x {x}, y {y}, z {z}\nDirection: {direction}";
    }

    string GetFacingDirection(float yaw) {
        yaw = yaw % 360f;
        if (yaw < 0) yaw += 360f;

        if (yaw >= 315f || yaw < 45f) return "North";
        if (yaw >= 45f && yaw < 135f) return "East";
        if (yaw >= 135f && yaw < 225f) return "South";
        if (yaw >= 225f && yaw < 315f) return "West";
        
        return "Unknown";
    }
}
