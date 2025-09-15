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
        
        // Cycle through log levels with 'L' key
        if (keyboard.lKey.wasPressedThisFrame)
        {
            LogLevel = (DebugLogLevel)(((int)LogLevel + 1) % 5);
            Debug.Log($"Log Level: {LogLevel}");
        }
        
        // Show help with 'H' key
        if (keyboard.hKey.wasPressedThisFrame)
        {
            ShowHelp();
        }
    }
    
    void ShowHelp()
    {
        Debug.Log("=== TrainArena Debug Controls ===\n" +
                  "R - Toggle Raycast Visualization\n" +
                  "I - Toggle Agent Debug Info\n" + 
                  "L - Cycle Log Level\n" +
                  "H - Show this help\n" +
                  "================================");
    }
    
    void OnGUI()
    {
        // Show current debug status in top-right corner
        string debugStatus = $"Debug: R:{(ShowRaycastVisualization ? "ON" : "OFF")} " +
                           $"I:{(ShowAgentDebugInfo ? "ON" : "OFF")} " +
                           $"Log:{LogLevel}";
        
        GUI.color = Color.white;
        GUI.backgroundColor = new Color(0, 0, 0, 0.5f);
        GUI.Label(new Rect(Screen.width - 200, 10, 190, 20), debugStatus);
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