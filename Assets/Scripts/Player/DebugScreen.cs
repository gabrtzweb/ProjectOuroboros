using UnityEngine;
using TMPro;

public class DebugScreen : MonoBehaviour {
    #region Serialized Fields
    
    [Header("References")]
    public Transform playerTransform;
    public TextMeshProUGUI debugText;
    
    #endregion
    
    #region Private Fields
    
    float minFps = float.MaxValue;
    float maxFps = 0f;
    float fpsTimer = 0f;
    int frameCount = 0;
    float currentFps = 0f;
    
    #endregion

    #region Display Update

    void Update() {
        if (playerTransform == null || debugText == null) return;

        UpdateFpsMetrics();
        UpdateDisplayText();
    }
    
    #endregion
    
    #region FPS Tracking
    
    void UpdateFpsMetrics() {
        fpsTimer += Time.unscaledDeltaTime;
        frameCount++;
        
        if (fpsTimer >= 0.25f) {
            currentFps = Mathf.RoundToInt(frameCount / fpsTimer);
            
            if (Time.realtimeSinceStartup > 3f) {
                if (currentFps < minFps && currentFps > 0) minFps = currentFps;
                if (currentFps > maxFps) maxFps = currentFps;
            }
            
            frameCount = 0;
            fpsTimer = 0f;
        }
    }
    
    #endregion
    
    #region Display Content
    
    void UpdateDisplayText() {
        int x = Mathf.FloorToInt(playerTransform.position.x);
        int y = Mathf.FloorToInt(playerTransform.position.y);
        int z = Mathf.FloorToInt(playerTransform.position.z);

        string direction = GetFacingDirection(playerTransform.eulerAngles.y);

        debugText.text = $"FPS: {currentFps} (Min: {(minFps == float.MaxValue ? 0 : minFps)}, Max: {maxFps})\nCoordinates: x {x}, y {y}, z {z}\nDirection: {direction}";
    }
    
    #endregion
    
    #region Direction Detection

    string GetFacingDirection(float yaw) {
        yaw = yaw % 360f;
        if (yaw < 0) yaw += 360f;

        if (yaw >= 315f || yaw < 45f) return "North";
        if (yaw >= 45f && yaw < 135f) return "East";
        if (yaw >= 135f && yaw < 225f) return "South";
        if (yaw >= 225f && yaw < 315f) return "West";
        
        return "Unknown";
    }
    
    #endregion
}
