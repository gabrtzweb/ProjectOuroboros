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
            Debug.Log("Ação Primária ativada (Pronta para quebrar blocos)");
        }

        if (input.Player.UseSecondary.WasPressedThisFrame()) {
            Debug.Log("Ação Secundária ativada (Pronta para colocar blocos)");
        }
    }
}
