using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour {
    #region Private Fields
    
    GameInput input;
    
    #endregion
    
    #region Movement Input
    
    public Vector2 MoveInput => input.Player.Move.ReadValue<Vector2>();
    public Vector2 LookInput => input.Player.Look.ReadValue<Vector2>();
    public bool IsSprinting => input.Player.Sprint.IsPressed();
    public bool IsJumping => input.Player.Jump.WasPressedThisFrame();
    public bool IsJumpHeld => input.Player.Jump.IsPressed();
    
    #endregion
    
    #region Interaction Input
    
    public bool PrimaryActionPressed => input.Player.UsePrimary.WasPressedThisFrame();
    public bool SecondaryActionPressed => input.Player.UseSecondary.WasPressedThisFrame();
    
    public bool IsPrimaryActionHeld => input.Player.UsePrimary.IsPressed();
    public bool IsSecondaryActionHeld => input.Player.UseSecondary.IsPressed();
    
    public bool PickBlockPressed => input.Player.PickBlock.WasPressedThisFrame();
    
    #endregion
    
    #region Camera and Stance Input
    
    public bool TogglePerspectivePressed => input.Player.TogglePerspective.WasPressedThisFrame();
    public bool IsCrouching => input.Player.Crouch.IsPressed();
    public bool IsCrawling => input.Player.Crawl.IsPressed();
    
    #endregion
    
    #region Lifecycle Methods

    void Awake() {
        input = new GameInput();
    }

    void OnEnable() {
        input.Enable();
    }

    void OnDisable() {
        input.Disable();
    }
    
    #endregion
}
