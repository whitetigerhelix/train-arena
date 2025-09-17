using UnityEngine;
using Unity.MLAgents;

/// <summary>
/// Displays and manages Unity time scale for ML-Agents training.
/// Shows current time scale and provides controls for manual adjustment.
/// 
/// ML-Agents automatically manages time scale:
/// - Training mode: High time scale (10-20x) for faster learning
/// - Inference mode: Normal time scale (1x) for real-time playback
/// </summary>
public class TimeScaleManager : MonoBehaviour
{
    [Header("Time Scale Information")]
    [SerializeField, Tooltip("Current Unity time scale (read-only)")]
    private float currentTimeScale = 1f;
    
    [SerializeField, Tooltip("Time scale when ML-Agents training is active")]
    private float trainingTimeScale = 20f;
    
    [SerializeField, Tooltip("Time scale when not training (normal gameplay)")]
    private float normalTimeScale = 1f;
    
    [Space]
    [Header("Auto-Management")]
    [Tooltip("Automatically manage time scale based on training status")]
    public bool autoManageTimeScale = true;
    
    // Precision constants
    private const float TIMESCALE_PRECISION = 0.01f;        // Precision threshold for time scale comparisons
    
    [Space]
    [Header("Manual Controls")]
    [Tooltip("Override automatic time scale management (for debugging)")]
    public bool manualTimeScaleControl = false;
    
    [Tooltip("Manual time scale value (only used if manual control enabled)")]
    [Range(0.1f, 100f)]
    public float manualTimeScale = 1f;
    
    [Space]
    [Header("Status (Read-Only)")]
    [SerializeField, Tooltip("Is ML-Agents Academy connected to trainer?")]
    private bool isTrainingActive = false;
    
    [SerializeField, Tooltip("Number of time scale changes this session")]
    private int timeScaleChanges = 0;
    
    private Academy academy;
    private float lastTimeScale;
    
    void Start()
    {
        academy = Academy.Instance;
        lastTimeScale = Time.timeScale;
        currentTimeScale = Time.timeScale;
        
        TrainArenaDebugManager.Log($"⏱️ TimeScaleManager initialized. Current time scale: {currentTimeScale}x", 
                                 TrainArenaDebugManager.DebugLogLevel.Important);
    }
    
    void Update()
    {
        // Update current time scale display
        currentTimeScale = Time.timeScale;
        
        // Check ML-Agents training status
        if (academy != null)
        {
            isTrainingActive = academy.IsCommunicatorOn;
        }
        
        // Apply manual control if enabled
        if (manualTimeScaleControl)
        {
            if (Mathf.Abs(Time.timeScale - manualTimeScale) > TIMESCALE_PRECISION)
            {
                Time.timeScale = manualTimeScale;
                TrainArenaDebugManager.Log($"⏱️ Manual time scale applied: {manualTimeScale}x", 
                                         TrainArenaDebugManager.DebugLogLevel.Verbose);
            }
        }
        // Apply auto-management if enabled and not in manual mode
        else if (autoManageTimeScale)
        {
            float targetTimeScale = isTrainingActive ? trainingTimeScale : normalTimeScale;
            
            if (Mathf.Abs(Time.timeScale - targetTimeScale) > TIMESCALE_PRECISION)
            {
                Time.timeScale = targetTimeScale;
                string mode = isTrainingActive ? "Training" : "Normal";
                TrainArenaDebugManager.Log($"⏱️ Auto-adjusted time scale to {targetTimeScale}x ({mode} mode)", 
                                         TrainArenaDebugManager.DebugLogLevel.Important);
            }
        }
        
        // Log time scale changes (helpful for debugging)
        if (Mathf.Abs(Time.timeScale - lastTimeScale) > TIMESCALE_PRECISION)
        {
            timeScaleChanges++;
            
            string source = manualTimeScaleControl ? "Manual" : 
                           isTrainingActive ? "ML-Agents Training" : "Unity Default";
                           
            TrainArenaDebugManager.Log($"⏱️ Time scale changed: {lastTimeScale:F1}x → {Time.timeScale:F1}x ({source})", 
                                     TrainArenaDebugManager.DebugLogLevel.Important);
                                     
            lastTimeScale = Time.timeScale;
        }
    }
    
    /// <summary>
    /// Set time scale to normal speed (1x)
    /// </summary>
    [ContextMenu("Set Normal Speed (1x)")]
    public void SetNormalSpeed()
    {
        Time.timeScale = normalTimeScale;
        manualTimeScaleControl = true;
        manualTimeScale = normalTimeScale;
        
        TrainArenaDebugManager.Log("⏱️ Time scale set to normal speed (1x)", 
                                 TrainArenaDebugManager.DebugLogLevel.Important);
    }
    
    /// <summary>
    /// Set time scale to training speed (20x)
    /// </summary>
    [ContextMenu("Set Training Speed (20x)")]
    public void SetTrainingSpeed()
    {
        Time.timeScale = trainingTimeScale;
        manualTimeScaleControl = true;
        manualTimeScale = trainingTimeScale;
        
        TrainArenaDebugManager.Log("⏱️ Time scale set to training speed (20x)", 
                                 TrainArenaDebugManager.DebugLogLevel.Important);
    }
    
    /// <summary>
    /// Let ML-Agents manage time scale automatically
    /// </summary>
    [ContextMenu("Enable Auto Time Scale")]
    public void EnableAutoTimeScale()
    {
        manualTimeScaleControl = false;
        
        TrainArenaDebugManager.Log("⏱️ Auto time scale enabled - ML-Agents will manage time scale", 
                                 TrainArenaDebugManager.DebugLogLevel.Important);
    }
    
    void OnGUI()
    {
        if (!Application.isPlaying) return;
        
        // Check if TimeScale UI should be shown (controlled by TrainArenaDebugManager)
        if (!TrainArenaDebugManager.ShowTimeScale) return;
        
        // Prominent UI in top-left corner
        const float PANEL_WIDTH = 280f;
        const float PANEL_HEIGHT = 120f;
        const float PANEL_MARGIN = 10f;
        
        // Background box with more prominent styling
        GUI.Box(new Rect(PANEL_MARGIN, PANEL_MARGIN, PANEL_WIDTH, PANEL_HEIGHT), "", 
               new GUIStyle(GUI.skin.box) { 
                   normal = { background = MakeTex(2, 2, new Color(0, 0, 0, 0.8f)) }
               });
        
        GUILayout.BeginArea(new Rect(PANEL_MARGIN + 5, PANEL_MARGIN + 5, PANEL_WIDTH - 10, PANEL_HEIGHT - 10));
        
        // Title with larger, bold text
        var titleStyle = new GUIStyle(GUI.skin.label) { 
            fontSize = 16, 
            fontStyle = FontStyle.Bold, 
            normal = { textColor = Color.white }
        };
        GUILayout.Label($"⏱️ TIME SCALE: {currentTimeScale:F1}x", titleStyle);
        
        // Training status with prominent color coding
        var statusStyle = new GUIStyle(GUI.skin.label) { 
            fontSize = 14, 
            fontStyle = FontStyle.Bold 
        };
        
        if (isTrainingActive)
        {
            statusStyle.normal.textColor = Color.green;
            GUILayout.Label("🚄 TRAINING MODE (FAST)", statusStyle);
        }
        else
        {
            statusStyle.normal.textColor = Color.yellow;
            GUILayout.Label("� TESTING MODE (NORMAL)", statusStyle);
        }
        
        // Control mode indicator
        var controlStyle = new GUIStyle(GUI.skin.label) { 
            fontSize = 12,
            normal = { textColor = manualTimeScaleControl ? Color.cyan : Color.white }
        };
        
        string controlText = manualTimeScaleControl ? "🔧 Manual Control" : "🤖 Auto Control";
        GUILayout.Label(controlText, controlStyle);
        
        // Quick action buttons
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("1x", GUILayout.Width(40)))
        {
            SetNormalSpeed();
        }
        if (GUILayout.Button("20x", GUILayout.Width(40)))
        {
            SetTrainingSpeed();
        }
        if (GUILayout.Button("Auto", GUILayout.Width(50)))
        {
            EnableAutoTimeScale();
        }
        GUILayout.EndHorizontal();
        
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