using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Policies;

/// <summary>
/// Automatically switches BehaviorParameters type based on ML-Agents training status and model availability.
/// 
/// When ML-Agents trainer is connected (Academy.IsCommunicatorOn == true):
/// - Always switches to BehaviorType.Default (enables ML training)
/// 
/// When ML-Agents trainer is NOT connected (Academy.IsCommunicatorOn == false):
/// - If manual override is set: Uses override behavior type
/// - If trained model is available: Switches to BehaviorType.InferenceOnly (model-driven behavior)
/// - If no model available: Switches to BehaviorType.HeuristicOnly (manual controls)
/// 
/// Manual Override Behavior:
/// - When connected: Manual changes are temporary and revert when training resumes
/// - When disconnected: Manual changes persist until cleared or auto-switching is re-enabled
/// 
/// This allows seamless switching between training, inference, and testing modes without
/// manual Inspector changes, while supporting user preferences when not training.
/// </summary>
public class AutoBehaviorSwitcher : MonoBehaviour
{
    [Header("Auto Behavior Switching")]
    [Tooltip("Enable automatic switching between Default, InferenceOnly, and HeuristicOnly based on ML-Agents connection and model availability")]
    public bool enableAutoSwitching = true;
    
    [Tooltip("Show debug messages when switching behavior types")]
    public bool showDebugMessages = true;
    
    [Tooltip("Allow manual override when not connected to trainer")]
    public bool allowManualOverride = true;
    
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
    
    [SerializeField, Tooltip("Whether a trained model is available for inference")]
    private bool hasTrainedModel = false;
    
    [SerializeField, Tooltip("User manually set behavior type (overrides auto-switching when not connected)")]
    private BehaviorType? manualOverride = null;
    
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

        // If not connected, just use whatever is set
        if (!academyConnected)// && (behaviorParams.BehaviorType == BehaviorType.HeuristicOnly || behaviorParams.BehaviorType == BehaviorType.InferenceOnly))
        {
            //TrainArenaDebugManager.Log($"ℹ️ {gameObject.name}: Not connected to trainer, current behavior remains {behaviorParams.BehaviorType}", 
            //                         TrainArenaDebugManager.DebugLogLevel.Important);
            return;
        }

        // Determine desired behavior type
        BehaviorType desiredType = DetermineDesiredBehaviorType(isConnected);

        // Update if changed
        if (behaviorParams.BehaviorType != desiredType)
        {
            BehaviorType previousType = behaviorParams.BehaviorType;
            behaviorParams.BehaviorType = desiredType;
            currentBehaviorType = desiredType;
            switchCount++;
            
            if (showDebugMessages)
            {
                string reason = GetSwitchReason(isConnected, desiredType);
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
    /// Determine the desired behavior type based on connection status, model availability, and user preferences
    /// </summary>
    private BehaviorType DetermineDesiredBehaviorType(bool isConnected)
    {
        // If connected to trainer, always use Default (training mode)
        if (isConnected)
        {
            return BehaviorType.Default;
        }
        
        // If not connected, check for manual override first
        if (allowManualOverride && manualOverride.HasValue)
        {
            return manualOverride.Value;
        }
        
        // Auto-determine based on model availability
        hasTrainedModel = HasTrainedModel();
        
        if (hasTrainedModel)
        {
            // Model available: prefer inference mode
            return BehaviorType.InferenceOnly;
        }
        else
        {
            // No model: use heuristic controls
            return BehaviorType.HeuristicOnly;
        }
    }
    
    /// <summary>
    /// Check if a trained model is available for inference
    /// </summary>
    private bool HasTrainedModel()
    {
        if (behaviorParams == null) return false;
        
        // Check if a model is assigned to the BehaviorParameters
        var model = behaviorParams.Model;
        return model != null;
    }
    
    /// <summary>
    /// Get human-readable reason for behavior switch
    /// </summary>
    private string GetSwitchReason(bool isConnected, BehaviorType targetType)
    {
        if (isConnected)
        {
            return "ML-Agents trainer connected";
        }
        
        if (manualOverride.HasValue)
        {
            return $"Manual override to {manualOverride.Value}";
        }
        
        switch (targetType)
        {
            case BehaviorType.InferenceOnly:
                return "Model available, using inference";
            case BehaviorType.HeuristicOnly:
                return "No model available, using heuristic";
            default:
                return "Auto-determined";
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
    /// Manually force a specific behavior type
    /// Sets manual override when not connected to trainer
    /// </summary>
    public void SetBehaviorType(BehaviorType type)
    {
        if (behaviorParams == null) return;
        
        bool isConnected = DetectTrainerConnection();
        
        if (isConnected)
        {
            // When connected, temporarily set but don't store override
            behaviorParams.BehaviorType = type;
            TrainArenaDebugManager.Log($"🔧 {gameObject.name}: Temporarily set behavior to {type} (trainer connected)", 
                                     TrainArenaDebugManager.DebugLogLevel.Important);
        }
        else
        {
            // When not connected, store as manual override
            manualOverride = type;
            behaviorParams.BehaviorType = type;
            TrainArenaDebugManager.Log($"🔧 {gameObject.name}: Manual override to {type} (trainer not connected)", 
                                     TrainArenaDebugManager.DebugLogLevel.Important);
        }
        
        currentBehaviorType = type;
    }
    
    /// <summary>
    /// Re-enable automatic behavior switching (clears manual override)
    /// </summary>
    public void EnableAutoSwitching()
    {
        enableAutoSwitching = true;
        manualOverride = null; // Clear manual override
        CheckAndUpdateBehaviorType();
        
        TrainArenaDebugManager.Log($"✅ {gameObject.name}: Auto behavior switching enabled (manual override cleared)", 
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
            string overrideInfo = manualOverride.HasValue ? $", ManualOverride = {manualOverride.Value}" : ", No Override";
            TrainArenaDebugManager.Log($"🔍 {gameObject.name}: Academy.IsCommunicatorOn = {academy.IsCommunicatorOn}, " +
                                     $"BehaviorType = {currentBehaviorType}, " +
                                     $"HasModel = {hasTrainedModel}, " +
                                     $"AutoSwitching = {enableAutoSwitching}" + overrideInfo, 
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
    
    /// <summary>
    /// Manually switch to InferenceOnly behavior for testing
    /// </summary>
    [ContextMenu("Test: Switch to InferenceOnly")]
    public void TestSwitchToInference()
    {
        if (!HasTrainedModel())
        {
            TrainArenaDebugManager.LogWarning($"⚠️ {gameObject.name}: No trained model available for inference mode!");
        }
        
        SetBehaviorType(BehaviorType.InferenceOnly);
        TrainArenaDebugManager.Log($"🧪 {gameObject.name}: Test switched to InferenceOnly behavior", 
                                 TrainArenaDebugManager.DebugLogLevel.Important);
    }
    
    /// <summary>
    /// Clear manual override and return to automatic switching
    /// </summary>
    [ContextMenu("Clear Manual Override")]
    public void ClearManualOverride()
    {
        manualOverride = null;
        CheckAndUpdateBehaviorType();
        TrainArenaDebugManager.Log($"🔄 {gameObject.name}: Manual override cleared, returning to auto-switching", 
                                 TrainArenaDebugManager.DebugLogLevel.Important);
    }
    
    // Unity Inspector info
    void OnValidate()
    {
        if (behaviorParams == null)
            behaviorParams = GetComponent<BehaviorParameters>();
    }
}