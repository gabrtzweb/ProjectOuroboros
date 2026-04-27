using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractions : MonoBehaviour {
    GameInput input;

    void Awake() {
        input = new GameInput();
    }

    void OnEnable() {
        input.Enable();
    }

    void OnDisable() {
        input.Disable();
    }

    void Update() {
        if (input.Player.UsePrimary.WasPressedThisFrame()) {
            Debug.Log("Primary Action activated (Ready to break blocks)");
        }

        if (input.Player.UseSecondary.WasPressedThisFrame()) {
            Debug.Log("Secondary Action activated (Ready to place blocks)");
        }
    }
}
