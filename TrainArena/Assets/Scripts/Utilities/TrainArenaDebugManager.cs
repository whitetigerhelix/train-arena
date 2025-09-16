using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Global debug settings manager for TrainArena
/// Controls visualization and logging levels across all components
/// </summary>
public class TrainArenaDebugManager : MonoBehaviour
{
    [Header("Visualization Controls")]
    public static bool ShowRaycastVisualization = false;
    public static bool ShowAgentDebugInfo = false;
    public static bool ShowArenaDebugInfo = false;
    public static bool ShowObservations = false;
    public static bool ShowVelocityDisplay = false;
    public static bool ShowArenaBounds = false;
    public static bool ShowHelp = true; // Show by default
    
    [Header("Logging Controls")]
    public static DebugLogLevel LogLevel = DebugLogLevel.Warnings;
    
    public enum DebugLogLevel
    {
        None = 0,        // No debug logs
        Errors = 1,      // Errors only
        Warnings = 2,    // Errors + Warnings
        Important = 3,   // Errors + Warnings + Important info
        Verbose = 4      // Everything
    }
    
    // Singleton instance
    private static TrainArenaDebugManager instance;
    public static TrainArenaDebugManager Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("TrainArenaDebugManager");
                instance = go.AddComponent<TrainArenaDebugManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }
    
    void Update()
    {
        HandleInput();
    }
    
    void HandleInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // Toggle raycast visualization with 'R' key
        if (keyboard.rKey.wasPressedThisFrame)
        {
            ShowRaycastVisualization = !ShowRaycastVisualization;
            Debug.Log($"Raycast Visualization: {(ShowRaycastVisualization ? "ON" : "OFF")}");
        }
        
        // Toggle agent debug info with 'I' key
        if (keyboard.iKey.wasPressedThisFrame)
        {
            ShowAgentDebugInfo = !ShowAgentDebugInfo;
            Debug.Log($"Agent Debug Info: {(ShowAgentDebugInfo ? "ON" : "OFF")}");
        }
        
        // Toggle observations with 'O' key
        if (keyboard.oKey.wasPressedThisFrame)
        {
            ShowObservations = !ShowObservations;
            Debug.Log($"Observations Display: {(ShowObservations ? "ON" : "OFF")}");
        }
        
        // Toggle velocity display with 'V' key
        if (keyboard.vKey.wasPressedThisFrame)
        {
            ShowVelocityDisplay = !ShowVelocityDisplay;
            Debug.Log($"Velocity Display: {(ShowVelocityDisplay ? "ON" : "OFF")}");
        }
        
        // Toggle arena bounds with 'A' key
        if (keyboard.aKey.wasPressedThisFrame)
        {
            ShowArenaBounds = !ShowArenaBounds;
            Debug.Log($"Arena Bounds: {(ShowArenaBounds ? "ON" : "OFF")}");
        }
        
        // Cycle through log levels with 'L' key
        if (keyboard.lKey.wasPressedThisFrame)
        {
            LogLevel = (DebugLogLevel)(((int)LogLevel + 1) % 5);
            Debug.Log($"Log Level: {LogLevel}");
        }
        
        // Show help with 'H' key
        if (keyboard.hKey.wasPressedThisFrame)
        {
            ShowHelp = !ShowHelp;
            Debug.Log($"Debug Help: {(ShowHelp ? "ON" : "OFF")}");
        }
    }
    
    void LogHelp()
    {
        Debug.Log("=== TrainArena Debug Controls ===\n" +
                  "R - Toggle Raycast Visualization\n" +
                  "I - Toggle Agent Debug Info\n" +
                  "O - Toggle Observations Display\n" +
                  "V - Toggle Velocity Display\n" +
                  "A - Toggle Arena Bounds\n" +
                  "L - Cycle Log Level\n" +
                  "H - Toggle Help Display\n" +
                  "================================");
    }
    
    void OnGUI()
    {
        // Show current debug status in top-right corner
        string debugStatus = $"Debug: R:{(ShowRaycastVisualization ? "ON" : "OFF")} " +
                           $"I:{(ShowAgentDebugInfo ? "ON" : "OFF")} " +
                           $"O:{(ShowObservations ? "ON" : "OFF")} " +
                           $"V:{(ShowVelocityDisplay ? "ON" : "OFF")} " +
                           $"A:{(ShowArenaBounds ? "ON" : "OFF")}";
        
        GUI.color = Color.white;
        GUI.backgroundColor = new Color(0, 0, 0, 0.7f);
        
        float statusWidth = 280f;
        Rect statusRect = new Rect(Screen.width - statusWidth - 10, 10, statusWidth, 20);
        GUI.Label(statusRect, debugStatus);
        
        // Show help panel positioned under status bar
        if (ShowHelp)
        {
            float panelWidth = 280f;
            float panelHeight = 200f;
            float panelX = Screen.width - panelWidth - 10;
            float panelY = 35f; // Just under the status bar
            
            Rect helpRect = new Rect(panelX, panelY, panelWidth, panelHeight);
            
            GUI.backgroundColor = new Color(0, 0, 0, 0.8f);
            GUI.Box(helpRect, "");
            
            GUI.color = Color.white;
            GUILayout.BeginArea(helpRect);
            GUILayout.BeginVertical();
            
            GUILayout.Space(8);
            GUILayout.Label("=== Debug Controls ===", GUI.skin.box);
            
            GUILayout.Label("R - Toggle Raycast Visualization");
            GUILayout.Label("I - Toggle Agent Debug Info");
            GUILayout.Label("O - Toggle Observations Display");
            GUILayout.Label("V - Toggle Velocity Display");
            GUILayout.Label("A - Toggle Arena Bounds");
            GUILayout.Label("L - Cycle Log Level");
            GUILayout.Label("H - Toggle this help");
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        else
        {
            // Show toggle indicator when help is hidden
            GUI.color = Color.white;
            GUI.backgroundColor = new Color(0, 0, 0, 0.5f);
            string helpHint = "Press H for help";
            Rect hintRect = new Rect(Screen.width - 120, 35, 110, 18);
            GUI.Label(hintRect, helpHint);
        }
    }
    
    // Logging methods with level filtering
    public static void Log(string message, DebugLogLevel level = DebugLogLevel.Important)
    {
        if (LogLevel >= level)
        {
            Debug.Log($"[TrainArena] {message}");
        }
    }
    
    public static void LogWarning(string message)
    {
        if (LogLevel >= DebugLogLevel.Warnings)
        {
            Debug.LogWarning($"[TrainArena] {message}");
        }
    }
    
    public static void LogError(string message)
    {
        if (LogLevel >= DebugLogLevel.Errors)
        {
            Debug.LogError($"[TrainArena] {message}");
        }
    }
}