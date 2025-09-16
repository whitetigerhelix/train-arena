using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Policies;

/// <summary>
/// Automatically switches BehaviorParameters type based on ML-Agents training status.
/// 
/// When ML-Agents trainer is connected (Academy.IsCommunicatorOn == true):
/// - Switches to BehaviorType.Default (enables ML training/inference)
/// 
/// When ML-Agents trainer is NOT connected (Academy.IsCommunicatorOn == false):
/// - Switches to BehaviorType.HeuristicOnly (uses manual controls for testing)
/// 
/// This allows seamless switching between training mode and testing mode without
/// manual Inspector changes.
/// </summary>
public class AutoBehaviorSwitcher : MonoBehaviour
{
    [Header("Auto Behavior Switching")]
    [Tooltip("Enable automatic switching between Default and HeuristicOnly based on ML-Agents connection")]
    public bool enableAutoSwitching = true;
    
    [Tooltip("Show debug messages when switching behavior types")]
    public bool showDebugMessages = true;
    
    // Timing and threshold constants
    private const float CONNECTION_CHECK_INTERVAL = 5f;     // Check connection every 5 seconds
    private const float FAST_TIMESCALE_THRESHOLD = 1.5f;    // Consider >1.5x as "fast" time scale
    
    [Space]
    [Header("Status (Runtime - Read Only)")]
    [SerializeField, Tooltip("Current Academy communication status")]
    private bool academyConnected = false;
    
    [SerializeField, Tooltip("Current behavior type")]
    private BehaviorType currentBehaviorType = BehaviorType.HeuristicOnly;
    
    [SerializeField, Tooltip("Number of times behavior has been switched this session")]
    private int switchCount = 0;
    
    private BehaviorParameters behaviorParams;
    private Academy academy;
    private bool wasConnectedLastFrame = false;
    
    void Start()
    {
        // Get components
        behaviorParams = GetComponent<BehaviorParameters>();
        if (behaviorParams == null)
        {
            TrainArenaDebugManager.LogError($"AutoBehaviorSwitcher on {gameObject.name}: No BehaviorParameters component found!");
            enabled = false;
            return;
        }
        
        // Get Academy instance
        academy = Academy.Instance;
        if (academy == null)
        {
            TrainArenaDebugManager.LogError($"AutoBehaviorSwitcher on {gameObject.name}: Academy not found!");
            enabled = false;
            return;
        }
        
        // Log initial setup
        currentBehaviorType = behaviorParams.BehaviorType;
        TrainArenaDebugManager.Log($"🔄 AutoBehaviorSwitcher initialized on {gameObject.name}. Initial behavior: {currentBehaviorType}", 
                                   TrainArenaDebugManager.DebugLogLevel.Important);
        
        // Perform initial check
        CheckAndUpdateBehaviorType();
        
        // Log initial state for debugging
        TrainArenaDebugManager.Log($"🔍 Initial connection check: Academy.IsCommunicatorOn = {academy.IsCommunicatorOn}, TimeScale = {Time.timeScale:F1}x", 
                                   TrainArenaDebugManager.DebugLogLevel.Important);
    }
    
    void Update()
    {
        if (!enableAutoSwitching) return;
        
        // Check Academy connection status every frame
        CheckAndUpdateBehaviorType();
        
        // Additional debugging - log connection status periodically
        if (Time.fixedTime % CONNECTION_CHECK_INTERVAL < Time.fixedDeltaTime)
        {
            LogConnectionDebugInfo();
        }
    }
    
    private void CheckAndUpdateBehaviorType()
    {
        if (academy == null || behaviorParams == null) return;
        
        // Use multiple methods to detect trainer connection
        bool isConnected = DetectTrainerConnection();
        academyConnected = isConnected;
        
        // Determine desired behavior type
        BehaviorType desiredType = isConnected ? BehaviorType.Default : BehaviorType.HeuristicOnly;
        
        // Update if changed
        if (behaviorParams.BehaviorType != desiredType)
        {
            BehaviorType previousType = behaviorParams.BehaviorType;
            behaviorParams.BehaviorType = desiredType;
            currentBehaviorType = desiredType;
            switchCount++;
            
            if (showDebugMessages)
            {
                string reason = isConnected ? "ML-Agents trainer connected" : "ML-Agents trainer disconnected";
                TrainArenaDebugManager.Log($"🔄 {gameObject.name}: Switched behavior {previousType} → {desiredType} ({reason})", 
                                         TrainArenaDebugManager.DebugLogLevel.Important);
            }
        }
        
        // Log connection status changes
        if (isConnected != wasConnectedLastFrame)
        {
            if (isConnected)
            {
                TrainArenaDebugManager.Log($"🔗 {gameObject.name}: ML-Agents trainer connected - ready for training!", 
                                         TrainArenaDebugManager.DebugLogLevel.Important);
            }
            else
            {
                TrainArenaDebugManager.Log($"🔌 {gameObject.name}: ML-Agents trainer disconnected - using heuristic controls", 
                                         TrainArenaDebugManager.DebugLogLevel.Important);
            }
            
            wasConnectedLastFrame = isConnected;
        }
    }
    
    /// <summary>
    /// More robust trainer connection detection using multiple methods
    /// </summary>
    private bool DetectTrainerConnection()
    {
        if (academy == null) return false;
        
        // Method 1: Standard Academy communicator check
        bool communicatorOn = academy.IsCommunicatorOn;
        
        // Method 2: Check if Academy is initialized and communicating
        bool academyInitialized = Academy.IsInitialized;
        
        // Method 3: Check for non-zero total steps (indicates training activity)
        bool hasSteps = academy.TotalStepCount > 0;
        
        // Method 4: Check if time scale is elevated (training usually runs faster)
        bool fastTimeScale = Time.timeScale > FAST_TIMESCALE_THRESHOLD;
        
        // Log detailed debug info when connection status might be changing
        if (showDebugMessages && (communicatorOn != wasConnectedLastFrame))
        {
            TrainArenaDebugManager.Log($"🔍 Connection Debug - Communicator: {communicatorOn}, " +
                                     $"Initialized: {academyInitialized}, " +
                                     $"Steps: {academy.TotalStepCount}, " +
                                     $"TimeScale: {Time.timeScale:F1}x", 
                                     TrainArenaDebugManager.DebugLogLevel.Verbose);
        }
        
        // Primary detection: Academy communicator is the most reliable
        // Secondary fallback: Fast time scale often indicates training mode
        bool isConnected = communicatorOn || (fastTimeScale && academyInitialized);
        
        // Additional debug logging for troubleshooting
        if (showDebugMessages && Time.frameCount % 300 == 0) // Every 5 seconds at 60fps
        {
            TrainArenaDebugManager.Log($"🔍 {gameObject.name}: Connection check - " +
                                     $"Communicator: {communicatorOn}, " +
                                     $"TimeScale: {Time.timeScale:F1}x, " +
                                     $"Steps: {academy.TotalStepCount}, " +
                                     $"Connected: {isConnected}", 
                                     TrainArenaDebugManager.DebugLogLevel.Verbose);
        }
        
        return isConnected;
    }
    
    /// <summary>
    /// Manually force a specific behavior type (disables auto-switching temporarily)
    /// </summary>
    public void SetBehaviorType(BehaviorType type)
    {
        if (behaviorParams == null) return;
        
        behaviorParams.BehaviorType = type;
        currentBehaviorType = type;
        
        TrainArenaDebugManager.Log($"🔧 {gameObject.name}: Manually set behavior to {type}", 
                                 TrainArenaDebugManager.DebugLogLevel.Important);
    }
    
    /// <summary>
    /// Re-enable automatic behavior switching
    /// </summary>
    public void EnableAutoSwitching()
    {
        enableAutoSwitching = true;
        CheckAndUpdateBehaviorType();
        
        TrainArenaDebugManager.Log($"✅ {gameObject.name}: Auto behavior switching enabled", 
                                 TrainArenaDebugManager.DebugLogLevel.Important);
    }
    
    /// <summary>
    /// Disable automatic behavior switching (keeps current type)
    /// </summary>
    public void DisableAutoSwitching()
    {
        enableAutoSwitching = false;
        
        TrainArenaDebugManager.Log($"❌ {gameObject.name}: Auto behavior switching disabled (locked to {currentBehaviorType})", 
                                 TrainArenaDebugManager.DebugLogLevel.Important);
    }
    
    private void LogConnectionDebugInfo()
    {
        if (academy != null)
        {
            TrainArenaDebugManager.Log($"🔍 {gameObject.name}: Academy.IsCommunicatorOn = {academy.IsCommunicatorOn}, " +
                                     $"BehaviorType = {currentBehaviorType}, " +
                                     $"AutoSwitching = {enableAutoSwitching}", 
                                     TrainArenaDebugManager.DebugLogLevel.Verbose);
        }
        else
        {
            TrainArenaDebugManager.LogWarning($"🔍 {gameObject.name}: Academy is null!");
        }
    }
    
    /// <summary>
    /// Force an immediate check and switch (useful for debugging)
    /// </summary>
    [ContextMenu("Force Check Connection")]
    public void ForceCheckConnection()
    {
        TrainArenaDebugManager.Log($"🔧 {gameObject.name}: Force checking ML-Agents connection...", 
                                 TrainArenaDebugManager.DebugLogLevel.Important);
        
        LogConnectionDebugInfo();
        CheckAndUpdateBehaviorType();
    }
    
    /// <summary>
    /// Manually switch to Default behavior for testing
    /// </summary>
    [ContextMenu("Test: Switch to Default")]
    public void TestSwitchToDefault()
    {
        SetBehaviorType(BehaviorType.Default);
        TrainArenaDebugManager.Log($"🧪 {gameObject.name}: Test switched to Default behavior", 
                                 TrainArenaDebugManager.DebugLogLevel.Important);
    }
    
    /// <summary>
    /// Manually switch to HeuristicOnly behavior for testing
    /// </summary>
    [ContextMenu("Test: Switch to HeuristicOnly")]
    public void TestSwitchToHeuristic()
    {
        SetBehaviorType(BehaviorType.HeuristicOnly);
        TrainArenaDebugManager.Log($"🧪 {gameObject.name}: Test switched to HeuristicOnly behavior", 
                                 TrainArenaDebugManager.DebugLogLevel.Important);
    }
    
    // Unity Inspector info
    void OnValidate()
    {
        if (behaviorParams == null)
            behaviorParams = GetComponent<BehaviorParameters>();
    }
}