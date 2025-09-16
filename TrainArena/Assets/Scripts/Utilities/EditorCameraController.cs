using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple camera controller for editor scene navigation - Unity 6.2 New Input System
/// WASD = Move, Right Click + Mouse = Look, Mouse Wheel = Zoom
/// </summary>
public class EditorCameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float fastMoveSpeed = 20f;
    public float mouseSensitivity = 0.5f;
    public float zoomSpeed = 30f;

    [Header("Zoom Limits")]
    public float minZoom = 2f;
    public float maxZoom = 100f;
    
    // Precision constants
    private const float SCROLL_PRECISION = 0.01f;           // Minimum scroll delta to process

    private Vector3 lastMousePosition;
    private bool isRotating = false;

    void Update()
    {
        // Only work in Editor or when not training
        if (!Application.isEditor && Application.isPlaying)
            return;

        HandleMovement();
        HandleMouseLook();
        HandleZoom();
    }

    void HandleMovement()
    {
        // Use new Input System
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        float currentSpeed = keyboard.leftShiftKey.isPressed ? fastMoveSpeed : moveSpeed;
        Vector3 moveDirection = Vector3.zero;

        // WASD movement
        if (keyboard.wKey.isPressed)
            moveDirection += transform.forward;
        if (keyboard.sKey.isPressed)
            moveDirection -= transform.forward;
        if (keyboard.aKey.isPressed)
            moveDirection -= transform.right;
        if (keyboard.dKey.isPressed)
            moveDirection += transform.right;
        
        // Up/Down movement
        if (keyboard.qKey.isPressed)
            moveDirection += Vector3.up;
        if (keyboard.eKey.isPressed)
            moveDirection -= Vector3.up;

        // Apply movement
        transform.position += moveDirection.normalized * currentSpeed * Time.unscaledDeltaTime;
    }

    void HandleMouseLook()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        // Right click to rotate camera
        if (mouse.rightButton.wasPressedThisFrame)
        {
            isRotating = true;
            lastMousePosition = mouse.position.ReadValue();
            Cursor.lockState = CursorLockMode.Locked;
        }
        else if (mouse.rightButton.wasReleasedThisFrame)
        {
            isRotating = false;
            Cursor.lockState = CursorLockMode.None;
        }

        if (isRotating)
        {
            Vector2 mouseDelta = mouse.delta.ReadValue();
            
            // Rotate around Y-axis (horizontal mouse movement)
            transform.Rotate(Vector3.up, mouseDelta.x * mouseSensitivity, Space.World);
            
            // Rotate around X-axis (vertical mouse movement)
            transform.Rotate(Vector3.right, -mouseDelta.y * mouseSensitivity, Space.Self);
        }
    }

    void HandleZoom()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        float scroll = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > SCROLL_PRECISION)
        {
            // Normalize scroll value and apply zoom
            scroll = scroll * 0.01f; // Scale down the scroll input
            Vector3 zoomDirection = transform.forward * scroll * zoomSpeed;
            
            // Check distance limits
            float currentDistance = Vector3.Distance(transform.position, Vector3.zero);
            float newDistance = currentDistance - (scroll * zoomSpeed);
            
            if (newDistance >= minZoom && newDistance <= maxZoom)
            {
                transform.position += zoomDirection;
            }
        }
    }

    void OnGUI()
    {
        if (!Application.isEditor && Application.isPlaying)
            return;
            
        // Only show camera controls when debug help is enabled
        if (!TrainArenaDebugManager.ShowHelp)
            return;
            
        // Show controls hint
        GUI.color = Color.white;
        GUI.backgroundColor = new Color(0, 0, 0, 0.5f);
        
        string controls = "Camera Controls:\n" +
                         "WASD - Move\n" +
                         "Q/E - Up/Down\n" +
                         "Shift - Fast Move\n" +
                         "Right Click + Mouse - Look\n" +
                         "Mouse Wheel - Zoom";
        
        const float HELP_WIDTH = 200f;
        const float HELP_HEIGHT = 120f;
        const float HELP_MARGIN = 10f;
        GUI.Label(new Rect(HELP_MARGIN, HELP_MARGIN, HELP_WIDTH, HELP_HEIGHT), controls);
    }
}