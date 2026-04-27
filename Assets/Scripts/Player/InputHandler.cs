using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour {
    GameInput input;

    public Vector2 MoveInput => input.Player.Move.ReadValue<Vector2>();
    public Vector2 LookInput => input.Player.Look.ReadValue<Vector2>();
    public bool IsSprinting => input.Player.Sprint.IsPressed();
    public bool IsJumping => input.Player.Jump.WasPressedThisFrame();
    public bool PrimaryActionPressed => input.Player.UsePrimary.WasPressedThisFrame();
    public bool SecondaryActionPressed => input.Player.UseSecondary.WasPressedThisFrame();
    public bool TogglePerspectivePressed => input.Player.TogglePerspective.WasPressedThisFrame();
    public bool IsCrouching => input.Player.Crouch.IsPressed();
    public bool IsCrawling => input.Player.Crawl.IsPressed();

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
