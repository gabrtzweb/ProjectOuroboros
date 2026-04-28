using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(InputHandler))]
public class CameraController : MonoBehaviour {
    #region Serialized Fields
    
    [Header("Cameras")]
    public CinemachineCamera firstPersonCamera;
    public CinemachineCamera thirdPersonCamera;
    public Transform cameraTarget; 
    
    [Header("Settings")]
    public float mouseSensitivity = 0.4f;
    public float topClamp = -80f;
    public float bottomClamp = 80f;
    
    #endregion
    
    #region Private Fields
    
    InputHandler inputHandler;
    float cameraPitch = 0f;
    float playerYaw = 0f;
    bool isFirstPerson = true;
    
    #endregion

    #region Lifecycle Methods
    
    void Awake() {
        inputHandler = GetComponent<InputHandler>();
    }

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        SetPerspective(true);
    }

    void Update() {
        HandleRotation();
        
        if (inputHandler.TogglePerspectivePressed) {
            SetPerspective(!isFirstPerson);
        }
    }
    
    #endregion
    
    #region Rotation and Perspective

    void HandleRotation() {
        Vector2 lookInput = inputHandler.LookInput * mouseSensitivity;

        playerYaw += lookInput.x;
        cameraPitch -= lookInput.y;
        
        cameraPitch = Mathf.Clamp(cameraPitch, topClamp, bottomClamp);

        cameraTarget.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        transform.rotation = Quaternion.Euler(0f, playerYaw, 0f);
    }

    void SetPerspective(bool firstPerson) {
        isFirstPerson = firstPerson;
        firstPersonCamera.Priority = isFirstPerson ? 10 : 0;
        thirdPersonCamera.Priority = isFirstPerson ? 0 : 10;
    }
    
    #endregion
}
