using UnityEngine;

[RequireComponent(typeof(PlayerMovement), typeof(PlayerInteractions), typeof(CameraController))]
[RequireComponent(typeof(InputHandler), typeof(CharacterController))]
public class PlayerController : MonoBehaviour {
    public PlayerMovement Movement { get; private set; }
    public PlayerInteractions Interactions { get; private set; }
    public CameraController CameraCtrl { get; private set; }
    public InputHandler InputData { get; private set; }
    public CharacterController CharController { get; private set; }

    void Awake() {
        Movement = GetComponent<PlayerMovement>();
        Interactions = GetComponent<PlayerInteractions>();
        CameraCtrl = GetComponent<CameraController>();
        InputData = GetComponent<InputHandler>();
        CharController = GetComponent<CharacterController>();
    }
}
