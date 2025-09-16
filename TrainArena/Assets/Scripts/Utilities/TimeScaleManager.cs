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
    [Header("Manual Controls")]
    [Tooltip("Override ML-Agents time scale management (for debugging)")]
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
            if (Mathf.Abs(Time.timeScale - manualTimeScale) > 0.01f)
            {
                Time.timeScale = manualTimeScale;
                TrainArenaDebugManager.Log($"⏱️ Manual time scale applied: {manualTimeScale}x", 
                                         TrainArenaDebugManager.DebugLogLevel.Verbose);
            }
        }
        
        // Log time scale changes (helpful for debugging)
        if (Mathf.Abs(Time.timeScale - lastTimeScale) > 0.01f)
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
        
        // Simple on-screen display
        GUILayout.BeginArea(new Rect(10, 100, 300, 120));
        GUILayout.BeginVertical("Box");
        
        GUILayout.Label($"⏱️ Time Scale: {currentTimeScale:F1}x", 
                       new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold });
        
        if (isTrainingActive)
        {
            GUILayout.Label("🚄 Training Mode (Fast)", 
                           new GUIStyle(GUI.skin.label) { normal = { textColor = Color.green } });
        }
        else
        {
            GUILayout.Label("🏃 Normal Mode", 
                           new GUIStyle(GUI.skin.label) { normal = { textColor = Color.yellow } });
        }
        
        if (manualTimeScaleControl)
        {
            GUILayout.Label("🔧 Manual Control", 
                           new GUIStyle(GUI.skin.label) { normal = { textColor = Color.cyan } });
        }
        else
        {
            GUILayout.Label("🤖 Auto Control", 
                           new GUIStyle(GUI.skin.label) { normal = { textColor = Color.white } });
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}