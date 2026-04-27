using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour {
    GameInput input;

    public Vector2 MoveInput => input.Player.Move.ReadValue<Vector2>();
    public bool IsSprinting => input.Player.Sprint.IsPressed();
    public bool IsJumping => input.Player.Jump.WasPressedThisFrame();
    public bool PrimaryActionPressed => input.Player.UsePrimary.WasPressedThisFrame();
    public bool SecondaryActionPressed => input.Player.UseSecondary.WasPressedThisFrame();

    void Awake() {
        input = new GameInput();
    }

    void OnEnable() {
        input.Enable();
    }

    void OnDisable() {
        input.Disable();
    }
}