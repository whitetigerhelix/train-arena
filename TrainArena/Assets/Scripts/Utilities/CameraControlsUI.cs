using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Displays camera control help information in a clean, unobtrusive way.
/// Positioned to complement the TimeScaleManager UI.
/// Compatible with both old Input Manager and new Input System.
/// </summary>
public class CameraControlsUI : MonoBehaviour
{
    [Header("UI Settings")]
    [Tooltip("Show camera controls help UI")]
    public bool showControlsUI = true;
    
    [Tooltip("Position UI in top-right corner below existing help")]
    public bool topRightPosition = true;
    
    [Tooltip("Auto-hide after this many seconds (0 = never hide)")]
    [Range(0, 60)]
    public float autoHideAfter = 0f;  // Disabled by default - too annoying
    
    [Space]
    [Header("Controls")]
    [Tooltip("Key to toggle the help display")]
    public KeyCode toggleKey = KeyCode.H;
    
    private float startTime;
    private bool manuallyHidden = false;
    
    void Start()
    {
        startTime = Time.time;
    }
    
    void Update()
    {
        // Toggle visibility with key (compatible with both Input systems)
        bool keyPressed = false;
        
        #if ENABLE_INPUT_SYSTEM
            keyPressed = Keyboard.current != null && Keyboard.current[Key.H].wasPressedThisFrame;
        #else
            keyPressed = Input.GetKeyDown(toggleKey);
        #endif
        
        if (keyPressed)
        {
            if (showControlsUI && !IsAutoHidden())
            {
                manuallyHidden = !manuallyHidden;
            }
            else if (IsAutoHidden() || manuallyHidden)
            {
                // Show UI again and reset timer
                manuallyHidden = false;
                startTime = Time.time;
            }
        }
    }
    
    private bool IsAutoHidden()
    {
        return autoHideAfter > 0 && (Time.time - startTime) > autoHideAfter;
    }
    
    private bool ShouldShowUI()
    {
        return showControlsUI && !manuallyHidden && !IsAutoHidden();
    }
    
    void OnGUI()
    {
        if (!Application.isPlaying || !ShouldShowUI()) return;
        
        // Calculate position (top-right corner, below existing help UI)
        const float PANEL_WIDTH = 220f;
        const float PANEL_HEIGHT = 140f;
        const float RIGHT_MARGIN = 40f;
        const float BELOW_HELP_OFFSET = 300f;  // Position below existing help UI
        
        float xPos = topRightPosition ? Screen.width - PANEL_WIDTH - RIGHT_MARGIN : 300; // Offset from TimeScaleManager
        float yPos = topRightPosition ? BELOW_HELP_OFFSET : 10f;  // Position below existing help UI
        
        // Semi-transparent background
        GUI.Box(new Rect(xPos, yPos, PANEL_WIDTH, PANEL_HEIGHT), "", 
               new GUIStyle(GUI.skin.box) { 
                   normal = { background = MakeTex(2, 2, new Color(0, 0, 0, 0.6f)) }
               });
        
        GUILayout.BeginArea(new Rect(xPos + 5, yPos + 5, PANEL_WIDTH - 10, PANEL_HEIGHT - 10));
        
        // Title
        var titleStyle = new GUIStyle(GUI.skin.label) { 
            fontSize = 14, 
            fontStyle = FontStyle.Bold, 
            normal = { textColor = Color.white }
        };
        GUILayout.Label("🎮 CAMERA CONTROLS", titleStyle);
        
        // Controls list
        var controlStyle = new GUIStyle(GUI.skin.label) { 
            fontSize = 11,
            normal = { textColor = Color.lightGray }
        };
        
        GUILayout.Label("WASD - Move", controlStyle);
        GUILayout.Label("Q/E - Up/Down", controlStyle);
        GUILayout.Label("Shift - Fast Move", controlStyle);
        GUILayout.Label("Right Click + Mouse - Look", controlStyle);
        GUILayout.Label("Mouse Wheel - Zoom", controlStyle);
        
        // Toggle hint
        var hintStyle = new GUIStyle(GUI.skin.label) { 
            fontSize = 10,
            fontStyle = FontStyle.Italic,
            normal = { textColor = Color.gray }
        };
        
        GUILayout.Space(5);
        GUILayout.Label($"Press '{toggleKey}' to toggle this help", hintStyle);
        
        // Auto-hide timer
        if (autoHideAfter > 0)
        {
            float timeLeft = autoHideAfter - (Time.time - startTime);
            if (timeLeft > 0)
            {
                GUILayout.Label($"Auto-hide in {timeLeft:F0}s", hintStyle);
            }
        }
        
        GUILayout.EndArea();
    }
    
    // Helper method to create colored textures for UI backgrounds
    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;
        
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}