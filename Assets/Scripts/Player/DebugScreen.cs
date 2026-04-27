using UnityEngine;
using TMPro;

public class DebugScreen : MonoBehaviour {
    [Header("References")]
    public Transform playerTransform;
    public TextMeshProUGUI debugText;

    void Update() {
        if (playerTransform == null || debugText == null) return;

        int x = Mathf.FloorToInt(playerTransform.position.x);
        int y = Mathf.FloorToInt(playerTransform.position.y);
        int z = Mathf.FloorToInt(playerTransform.position.z);

        string direction = GetFacingDirection(playerTransform.eulerAngles.y);

        debugText.text = $"Coordinates: x {x}, y {y}, z {z}\nDirection: {direction}";
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
