using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using TrainArena.Core;

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
    public static bool ShowMLAgentsStatus = true; // Show ML-Agents behavior info by default
    public static bool ShowTimeScale = true; // Show TimeScale UI by default
    public static bool ShowHelp = true; // Show by default
    public static bool ShowDomainRandomization = false; // Show Domain Randomization UI

    [Header("Logging Controls")]
    public static DebugLogLevel LogLevel = DebugLogLevel.Verbose;//TODO: Important;  // Show important info by default
    
    [Header("Auto-Adjust Log Level")]
    public static bool autoAdjustLogLevel = false;  // Don't auto-spam with verbose logs
    
    [Header("Timing Constants")]
    private const float LOG_LEVEL_CHECK_INTERVAL = 10f;     // Check log level every 10 seconds  
    private const float STATUS_REPORT_INTERVAL = 60f;       // Report training status every 60 seconds (less spam)
    
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
    
    // UI state variables
    private Vector2 scrollPosition = Vector2.zero;
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
    
    static float lastGCTime = 0f;
    const float GC_INTERVAL = 120f; // Force GC every 2 minutes as safety net
    
    void Update()
    {
        HandleInput();
        
        // Auto-adjust log level for training mode
        if (autoAdjustLogLevel && Time.fixedTime % LOG_LEVEL_CHECK_INTERVAL < Time.fixedDeltaTime)
        {
            CheckAndAdjustLogLevel();
        }
        
        // Log training status periodically (less frequent, more useful)
        if (Time.fixedTime % STATUS_REPORT_INTERVAL < Time.fixedDeltaTime)
        {
            LogTrainingStatus();
        }
        
        // Memory management - force GC periodically to prevent OOM
        if (Time.time - lastGCTime > GC_INTERVAL)
        {
            long memoryBefore = System.GC.GetTotalMemory(false);
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            long memoryAfter = System.GC.GetTotalMemory(false);
            
            Log($"🧹 Periodic GC: Released {(memoryBefore - memoryAfter) / 1024 / 1024:F1} MB (mem: {memoryAfter / 1024 / 1024:F1} MB)", 
                DebugLogLevel.Important);
            lastGCTime = Time.time;
        }
    }
    
    private void LogTrainingStatus()
    {
        var academy = Unity.MLAgents.Academy.Instance;
        if (academy != null)
        {
            var agents = FindObjectsByType<Unity.MLAgents.Agent>(FindObjectsSortMode.None);
            var behaviorSwitchers = FindObjectsByType<AutoBehaviorSwitcher>(FindObjectsSortMode.None);

            Log($"🎯 Training Status Report:", DebugLogLevel.Important);
            Log($"   📊 Academy Connected: {academy.IsCommunicatorOn}", DebugLogLevel.Important);
            Log($"   🎮 Agents in Scene: {agents.Length}", DebugLogLevel.Important);
            Log($"   ⏱️ Time Scale: {Time.timeScale:F1}x", DebugLogLevel.Important);
            Log($"   🔄 Auto Switchers: {behaviorSwitchers.Length}", DebugLogLevel.Important);
            Log($"   📈 Academy Steps: {academy.TotalStepCount}", DebugLogLevel.Important);

            // Count agents by behavior type
            int defaultBehavior = 0, heuristicBehavior = 0, inferenceBehavior = 0;
            foreach (var agent in agents)
            {
                var behaviorParams = agent.GetComponent<Unity.MLAgents.Policies.BehaviorParameters>();
                if (behaviorParams != null)
                {
                    switch (behaviorParams.BehaviorType)
                    {
                        case Unity.MLAgents.Policies.BehaviorType.Default:
                            defaultBehavior++;
                            break;
                        case Unity.MLAgents.Policies.BehaviorType.HeuristicOnly:
                            heuristicBehavior++;
                            break;
                        case Unity.MLAgents.Policies.BehaviorType.InferenceOnly:
                            inferenceBehavior++;
                            break;
                    }
                }
            }

            Log($"   🤖 Behavior Types - Default: {defaultBehavior}, Heuristic: {heuristicBehavior}, Inference: {inferenceBehavior}", DebugLogLevel.Important);
            
            // Check if any agents are moving
            int movingAgents = 0;
            foreach (var agent in agents)
            {
                var rb = agent.GetComponent<Rigidbody>();
                if (rb != null && rb.linearVelocity.magnitude > 0.1f)
                {
                    movingAgents++;
                }
            }
            Log($"   🏃 Moving Agents: {movingAgents}/{agents.Length}", DebugLogLevel.Important);
        }
    }
    
    private void CheckAndAdjustLogLevel()
    {
        var academy = Unity.MLAgents.Academy.Instance;
        bool isTraining = academy != null && academy.IsCommunicatorOn;
        
        if (isTraining)
        {
            // During training, use Verbose logging to see everything
            if (LogLevel < DebugLogLevel.Verbose)
            {
                LogLevel = DebugLogLevel.Verbose;
                Log("🔊 Auto-enabled Verbose logging for training session", DebugLogLevel.Important);
            }
        }
        else
        {
            // When not training, use Important level to reduce noise
            if (LogLevel > DebugLogLevel.Important)
            {
                LogLevel = DebugLogLevel.Important;
                Log("🔇 Auto-reduced logging to Important level (not training)", DebugLogLevel.Important);
            }
        }
    }
    
    void HandleInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // Toggle raycast visualization with 'R' key
        if (keyboard.rKey.wasPressedThisFrame)
        {
            ShowRaycastVisualization = !ShowRaycastVisualization;
            Log($"Raycast Visualization: {(ShowRaycastVisualization ? "ON" : "OFF")}");
        }
        
        // Toggle agent debug info with 'I' key
        if (keyboard.iKey.wasPressedThisFrame)
        {
            ShowAgentDebugInfo = !ShowAgentDebugInfo;
            Log($"Agent Debug Info: {(ShowAgentDebugInfo ? "ON" : "OFF")}");
        }
        
        // Toggle observations with 'O' key
        if (keyboard.oKey.wasPressedThisFrame)
        {
            ShowObservations = !ShowObservations;
            Log($"Observations Display: {(ShowObservations ? "ON" : "OFF")}");
        }
        
        // Toggle velocity display with 'V' key
        if (keyboard.vKey.wasPressedThisFrame)
        {
            ShowVelocityDisplay = !ShowVelocityDisplay;
            Log($"Velocity Display: {(ShowVelocityDisplay ? "ON" : "OFF")}");
        }
        
        // Toggle arena bounds with 'B' key (A is camera control)
        if (keyboard.bKey.wasPressedThisFrame)
        {
            ShowArenaBounds = !ShowArenaBounds;
            Log($"Arena Bounds: {(ShowArenaBounds ? "ON" : "OFF")}");
        }
        
        // Toggle ML-Agents status with 'M' key
        if (keyboard.mKey.wasPressedThisFrame)
        {
            ShowMLAgentsStatus = !ShowMLAgentsStatus;
            Log($"ML-Agents Status: {(ShowMLAgentsStatus ? "ON" : "OFF")}");
        }
        
        // Cycle through log levels with 'L' key
        if (keyboard.lKey.wasPressedThisFrame)
        {
            LogLevel = (DebugLogLevel)(((int)LogLevel + 1) % 5);
            Log($"Log Level: {LogLevel}");
        }
        
        // Show help with 'H' key
        if (keyboard.hKey.wasPressedThisFrame)
        {
            ShowHelp = !ShowHelp;
            Log($"Debug Help: {(ShowHelp ? "ON" : "OFF")}");
        }
        
        // Toggle TimeScale UI with 'T' key
        if (keyboard.tKey.wasPressedThisFrame)
        {
            ShowTimeScale = !ShowTimeScale;
            Log($"TimeScale UI: {(ShowTimeScale ? "ON" : "OFF")}");
        }
        
        // Toggle Domain Randomization UI with 'N' key (D is camera control)
        if (keyboard.nKey.wasPressedThisFrame)
        {
            ShowDomainRandomization = !ShowDomainRandomization;
            Log($"Domain Randomization UI: {(ShowDomainRandomization ? "ON" : "OFF")}");
        }
        
        // Toggle all agents activity with 'Z' key (for demos) - Q is camera control
        if (keyboard.zKey.wasPressedThisFrame)
        {
            ToggleAllAgentsActivity();
        }
    }
    
    void LogHelp()
    {
        Log("=== TrainArena Debug Controls ===\n" +
            "R - Toggle Raycast Visualization\n" +
            "I - Toggle Agent Debug Info\n" +
            "O - Toggle Observations Display\n" +
            "V - Toggle Velocity Display\n" +
            "B - Toggle Arena Bounds\n" +
            "M - Toggle ML-Agents Status Panel\n" +
            "T - Toggle TimeScale UI\n" +
            "N - Toggle Domain Randomization UI\n" +
            "Z - Toggle All Agents Activity\n" +
            "L - Cycle Log Level\n" +
            "H - Toggle Help Display\n" +
            "================================");
    }
    
    void OnGUI()
    {
        // Check training status for UI state management
        var academy = Unity.MLAgents.Academy.Instance;
        bool isTraining = academy != null && academy.IsCommunicatorOn;
        
        // Training Status Banner (prominent top-center display)
        DrawTrainingStatusBanner(isTraining);
        
        // Show current debug status in top-right corner with better formatting
        GUI.color = Color.white;
        GUI.backgroundColor = new Color(0, 0, 0, 0.8f);
        
        float statusWidth = 400f;
        float statusHeight = 25f;
        Rect statusRect = new Rect(Screen.width - statusWidth - 10, 10, statusWidth, statusHeight);
        GUI.Box(statusRect, "");
        
        // Create cleaner debug status with emoji indicators
        string debugStatus = $"🔧 Debug: " +
                           $"{(ShowRaycastVisualization ? "🟢" : "⚫")}R " +
                           $"{(ShowAgentDebugInfo ? "🟢" : "⚫")}I " +
                           $"{(ShowObservations ? "🟢" : "⚫")}O " +
                           $"{(ShowVelocityDisplay ? "🟢" : "⚫")}V " +
                           $"{(ShowArenaBounds ? "🟢" : "⚫")}B " +
                           $"{(ShowMLAgentsStatus ? "🟢" : "⚫")}M " +
                           $"{(ShowTimeScale ? "🟢" : "⚫")}T " +
                           $"{(ShowDomainRandomization ? "🟢" : "⚫")}N " +
                           $"🎮Z " +
                           $"📊{LogLevel}";
        
        GUILayout.BeginArea(statusRect);
        GUILayout.Space(4);
        GUILayout.Label(debugStatus);
        GUILayout.EndArea();
        
        // Show help panel positioned under status bar
        if (ShowHelp)
        {
            float panelWidth = 400f; // Match status bar width
            float panelHeight = 280f; // Increased for better spacing
            float panelX = Screen.width - panelWidth - 10;
            float panelY = 40f; // Just under the status bar
            
            Rect helpRect = new Rect(panelX, panelY, panelWidth, panelHeight);
            
            GUI.backgroundColor = new Color(0, 0, 0, 0.85f);
            GUI.Box(helpRect, "");
            
            GUI.color = Color.white;
            GUILayout.BeginArea(helpRect);
            GUILayout.BeginVertical();
            
            GUILayout.Space(8);
            GUI.color = Color.cyan;
            GUILayout.Label("🔧 === DEBUG CONTROLS ===", GUI.skin.box);
            GUI.color = Color.white;
            
            GUILayout.Space(6);
            GUILayout.Label($"{(ShowRaycastVisualization ? "🟢" : "⚫")} [R] - Raycast Visualization");
            GUILayout.Label($"{(ShowAgentDebugInfo ? "🟢" : "⚫")} [I] - Agent Debug Info");
            GUILayout.Label($"{(ShowObservations ? "🟢" : "⚫")} [O] - Observations Display");
            GUILayout.Label($"{(ShowVelocityDisplay ? "🟢" : "⚫")} [V] - Velocity Display");
            GUILayout.Label($"{(ShowArenaBounds ? "🟢" : "⚫")} [B] - Arena Bounds");
            GUILayout.Label($"{(ShowMLAgentsStatus ? "🟢" : "⚫")} [M] - ML-Agents Status Panel");
            GUILayout.Label($"{(ShowTimeScale ? "🟢" : "⚫")} [T] - TimeScale UI");
            GUILayout.Label($"{(ShowDomainRandomization ? "🟢" : "⚫")} [N] - Domain Randomization UI");
            
            GUILayout.Space(4);
            GUI.color = Color.gray;
            GUILayout.Label("─────────────────────────────");
            GUI.color = Color.yellow;
            GUILayout.Label("🎮 [Z] - Toggle All Agents Active/Inactive");
            GUI.color = Color.white;
            GUILayout.Label($"📊 [L] - Log Level: {LogLevel}");
            GUILayout.Label("❓ [H] - Toggle This Help");
            
            if (isTraining)
            {
                GUILayout.Space(6);
                GUI.color = Color.yellow;
                GUILayout.Label("⚠️ TRAINING MODE ACTIVE");
                GUI.color = Color.gray;
                GUILayout.Label("(Some controls disabled during training)");
                GUI.color = Color.white;
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        // Show ML-Agents Status Panel (always visible when enabled)
        if (ShowMLAgentsStatus)
        {
            DrawMLAgentsStatusPanel(isTraining);
        }
        
        if (!ShowHelp)
        {
            // Show toggle indicator when help is hidden
            GUI.color = Color.white;
            GUI.backgroundColor = new Color(0, 0, 0, 0.5f);
            string helpHint = "Press H for help";
            const float hintWidth = 110f;
            const float hintHeight = 18f;
            const float rightMargin = 10f;
            const float topOffset = 35f;
            Rect hintRect = new Rect(Screen.width - hintWidth - rightMargin, topOffset, hintWidth, hintHeight);
            GUI.Label(hintRect, helpHint);
        }
    }
    
    void DrawTrainingStatusBanner(bool isTraining)
    {
        // Smaller, less intrusive banner at top-center of screen
        float bannerWidth = 220f; // Reduced from 300f
        float bannerHeight = 25f; // Reduced from 35f
        float bannerX = (Screen.width - bannerWidth) / 2f;
        float bannerY = 10f;
        
        Rect bannerRect = new Rect(bannerX, bannerY, bannerWidth, bannerHeight);
        
        if (isTraining)
        {
            GUI.backgroundColor = new Color(1f, 0.8f, 0f, 0.9f); // Bright yellow/orange for training
            GUI.color = Color.black;
        }
        else
        {
            GUI.backgroundColor = new Color(0f, 0.6f, 0.2f, 0.9f); // Green for inference/demo
            GUI.color = Color.white;
        }
        
        GUI.Box(bannerRect, "");
        
        GUILayout.BeginArea(bannerRect);
        GUILayout.BeginVertical();
        GUILayout.Space(4); // Reduced spacing
        
        // Smaller, more concise status text
        string status = isTraining ? "🎯 TRAINING" : "🧠 DEMO MODE";
        var bannerStyle = new GUIStyle(GUI.skin.label) { 
            fontSize = 12, // Smaller font
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = isTraining ? Color.black : Color.white }
        };
        GUILayout.Label(status, bannerStyle);
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
        
        GUI.color = Color.white;
        GUI.backgroundColor = Color.clear;
    }
    
    void DrawMLAgentsStatusPanel(bool isTraining)
    {
        // Find all TrainArena agents in the scene using unified interface
        ITrainArenaAgent[] allAgents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<ITrainArenaAgent>()
            .ToArray();
        
        // Always show panel even if no agents found (for debugging)
        bool hasAgents = allAgents.Length > 0;
        
        int totalAgents = allAgents.Length;
        
        // Count agents by type for display
        int cubeCount = allAgents.Count(a => a.AgentTypeIcon == "🧊");
        int ragdollCount = allAgents.Count(a => a.AgentTypeIcon == "🎭");
        
        // Panel positioning - top-left corner
        float panelWidth = 650f; // Wider for better button layout
        float maxPanelHeight = Screen.height * 0.8f; // Max 80% of screen height for more room
        float posX = 10f;
        float posY = 140f;
        
        // Better height calculation that shows more agents
        float headerHeight = 120f; // Title + training warning + global controls + activity controls + spacing
        float agentHeight = 28f; // Slightly more height per agent for better readability
        float agentLabelHeight = 25f;
        
        // Calculate how many agents we can fit vs want to show
        float availableScrollHeight = maxPanelHeight - headerHeight - 40f; // Space for scroll area
        int maxVisibleAgents = Mathf.FloorToInt(availableScrollHeight / agentHeight);
        
        // Always use maximum available space but ensure scroll works when needed
        float contentHeight = headerHeight + agentLabelHeight + (Mathf.Min(totalAgents, maxVisibleAgents) * agentHeight) + 40f;
        float panelHeight = Mathf.Min(contentHeight, maxPanelHeight);
        
        Rect panelRect = new Rect(posX, posY, panelWidth, panelHeight);
        
        // Draw panel background
        GUI.backgroundColor = new Color(0, 0, 0, 0.85f);
        GUI.Box(panelRect, "");
        
        GUI.color = Color.white;
        GUILayout.BeginArea(panelRect);
        GUILayout.BeginVertical();
        
        GUILayout.Space(4);
        
        // Header with global controls
        GUI.color = Color.cyan;
        GUILayout.Label("=== ML-AGENTS CONTROL ===", GUI.skin.box);
        GUI.color = Color.white;
        
        // Training status indicator
        if (isTraining)
        {
            GUI.color = Color.yellow;
            GUILayout.Label("⚠️ TRAINING ACTIVE - Controls Disabled");
            GUI.color = Color.white;
        }
        
        // Global behavior type switching (disabled during training)
        GUI.enabled = !isTraining;
        GUILayout.BeginHorizontal();
        GUILayout.Label($"All Agents ({totalAgents} - {cubeCount}🧊 {ragdollCount}🎭):", GUILayout.Width(180));
        
        GUI.backgroundColor = new Color(0f, 0.8f, 1f, isTraining ? 0.3f : 0.8f); // Dimmed when training
        if (GUILayout.Button("🎯 Training", GUILayout.Width(100)))
        {
            SetAllAgentsBehaviorType(Unity.MLAgents.Policies.BehaviorType.Default);
        }
        
        if (!isTraining)
        {
            GUI.backgroundColor = new Color(0f, 0.8f, 1f, isTraining ? 0.3f : 0.8f); // Dimmed when training
            if (GUILayout.Button("🤷 Random", GUILayout.Width(100)))
            {
                SetAllAgentsBehaviorType(Unity.MLAgents.Policies.BehaviorType.HeuristicOnly);
            }
            
            GUI.backgroundColor = new Color(0f, 0.8f, 1f, isTraining ? 0.3f : 0.8f); // Dimmed when training
            if (GUILayout.Button("🧠 AI Model", GUILayout.Width(100)))
            {
                SetAllAgentsBehaviorType(Unity.MLAgents.Policies.BehaviorType.InferenceOnly);
            }
        }
        
        GUI.backgroundColor = new Color(0, 0, 0, 0.85f); // Reset to panel background
        GUILayout.EndHorizontal();
        
        // Global activity controls - separate row
        GUILayout.BeginHorizontal();
        GUILayout.Label("Activity:", GUILayout.Width(110));
        
        GUI.backgroundColor = new Color(0f, 0.8f, 0.2f, 0.8f); // Green for active
        if (GUILayout.Button("🟢 All Active", GUILayout.Width(100)))
        {
            SetAllAgentsActivity(AgentActivity.Active);
        }
        
        GUI.backgroundColor = new Color(0.6f, 0.6f, 0.6f, 0.8f); // Gray for inactive
        if (GUILayout.Button("⚫ All Inactive", GUILayout.Width(100)))
        {
            SetAllAgentsActivity(AgentActivity.Inactive);
        }
        
        GUI.backgroundColor = new Color(1f, 1f, 0f, 0.8f); // Yellow for toggle
        if (GUILayout.Button("🎮 Toggle (Z)", GUILayout.Width(100)))
        {
            ToggleAllAgentsActivity();
        }
        
        GUI.backgroundColor = new Color(0, 0, 0, 0.85f); // Reset to panel background
        GUILayout.EndHorizontal();
        GUI.enabled = true; // Re-enable GUI
        
        GUILayout.Space(6);
        if (!hasAgents)
        {
            GUILayout.Space(6);
            GUI.color = Color.yellow;
            GUILayout.Label("⚠️ No agents found in scene", GUI.skin.box);
            GUI.color = Color.gray;
            GUILayout.Label("• Load a scene with CubeAgent or RagdollAgent");
            GUILayout.Label("• Check that agents have proper ML-Agents components");
            GUI.color = Color.white;
        }
        else
        {
            GUI.color = Color.gray;
            GUILayout.Label($"Agent Status ({totalAgents} total - {cubeCount} cubes, {ragdollCount} ragdolls):", GUI.skin.label);
            GUI.color = Color.white;
        }
        
        // Only show scroll view if we have agents
        if (hasAgents)
        {
            // Scroll view for agent controls with proper sizing
            float scrollHeight = panelHeight - headerHeight - 50f; // Account for spacing and label
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true, 
                                                     GUILayout.Width(panelWidth - 20), 
                                                     GUILayout.Height(scrollHeight),
                                                     GUILayout.ExpandHeight(false));
            
            // Display ALL agents with unified layout - polymorphic approach
            foreach (var agent in allAgents)
            {
                DrawAgentStatus(agent, isTraining);
            }
            
            GUILayout.EndScrollView();
        }
        GUILayout.Space(16);
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    void DrawAgentStatus(ITrainArenaAgent agent, bool isTraining)
    {
        if (agent == null) return;
        
        // Get behavior parameters using unified interface
        var behaviorParams = agent.BehaviorParameters;
        string behaviorType = "Unknown";
        string modelName = "NO MODEL";
        Color statusColor = Color.white;
        string behaviorEmoji = "❓";
        
        if (behaviorParams != null)
        {
            behaviorType = behaviorParams.BehaviorType.ToString();
            modelName = behaviorParams.Model?.name ?? "NO MODEL";
            
            // Color code and emoji by behavior type
            switch (behaviorParams.BehaviorType)
            {
                case Unity.MLAgents.Policies.BehaviorType.Default:
                    statusColor = Color.orange;
                    behaviorEmoji = "🎯";
                    behaviorType = "Training";
                    break;
                case Unity.MLAgents.Policies.BehaviorType.HeuristicOnly:
                    statusColor = Color.cyan;
                    behaviorEmoji = "🤷";
                    behaviorType = "Random";
                    break;
                case Unity.MLAgents.Policies.BehaviorType.InferenceOnly:
                    statusColor = Color.yellow;
                    behaviorEmoji = "🧠";
                    behaviorType = "AI Model: "; // Inference
                    break;
            }
        }
        
        // Get activity status using unified interface
        string activityEmoji = agent.AgentActivity == AgentActivity.Active ? "🟢" : "⚫";
        string activityStatus = agent.AgentActivity == AgentActivity.Active ? "Active" : "Inactive";
        
        // Ultra-compact single-line agent display with inline model info
        GUILayout.BeginHorizontal();
        
        // Agent name with status info inline for compactness
        GUI.color = statusColor;
        string agentDisplay = $"{activityEmoji} {agent.AgentTypeIcon} {behaviorEmoji} {agent.DisplayName}";
        
        // Add model info directly inline for AI agents
        if (behaviorParams != null && behaviorParams.BehaviorType == Unity.MLAgents.Policies.BehaviorType.InferenceOnly && !string.IsNullOrEmpty(modelName) && modelName != "NO MODEL")
        {
            string shortModel = modelName.Length > 30 ? modelName.Substring(0, 27) + "..." : modelName;
            agentDisplay += $" ({behaviorType} {shortModel})";
        }
        else
        {
            agentDisplay += $" ({behaviorType})";
        }
        
        float agentDescriptionWidth = 450f; // Not including buttons to the right
        GUILayout.Label(agentDisplay, GUILayout.Width(agentDescriptionWidth));
        GUI.color = Color.white;
        
        // Spacer to push buttons to the right
        GUILayout.FlexibleSpace();
        
        // Activity toggle button - unified for all agent types
        if (agent.AgentActivity == AgentActivity.Active)
        {
            GUI.backgroundColor = new Color(0f, 0.8f, 0.2f, 0.8f); // Green for active
            if (GUILayout.Button("🟢", GUILayout.Width(25)))
            {
                agent.AgentActivity = AgentActivity.Inactive;
                Log($"Set {agent.DisplayName} to ⚫ Inactive");
            }
        }
        else
        {
            GUI.backgroundColor = new Color(0.6f, 0.6f, 0.6f, 0.8f); // Gray for inactive
            if (GUILayout.Button("⚫", GUILayout.Width(25)))
            {
                agent.AgentActivity = AgentActivity.Active;
                Log($"Set {agent.DisplayName} to 🟢 Active");
            }
        }
        
        // Individual behavior type buttons (more compact) - unified for all agent types
        GUI.backgroundColor = new Color(0f, 0.8f, 1f, 0.6f);
        if (GUILayout.Button("🎯", GUILayout.Width(25)))
        {
            SetAgentBehaviorType(agent, Unity.MLAgents.Policies.BehaviorType.Default);
        }
        
        if (GUILayout.Button("🤷", GUILayout.Width(25)))
        {
            SetAgentBehaviorType(agent, Unity.MLAgents.Policies.BehaviorType.HeuristicOnly);
        }
        
        if (GUILayout.Button("🧠", GUILayout.Width(25)))
        {
            SetAgentBehaviorType(agent, Unity.MLAgents.Policies.BehaviorType.InferenceOnly);
        }
        
        GUI.backgroundColor = new Color(0, 0, 0, 0.85f);
        GUILayout.EndHorizontal();
        
        GUILayout.Space(3); // Better spacing between agents for readability and scroll accuracy
    }
    
    // DrawRagdollAgentStatus method removed - now using unified DrawAgentStatus
    
    void SetAllAgentsBehaviorType(Unity.MLAgents.Policies.BehaviorType behaviorType)
    {
        // Check if trying to set InferenceOnly without any models
        if (behaviorType == Unity.MLAgents.Policies.BehaviorType.InferenceOnly)
        {
            ITrainArenaAgent[] testAgents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .OfType<ITrainArenaAgent>()
                .ToArray();
            
            bool hasAnyModel = testAgents.Any(agent => 
                agent.BehaviorParameters != null && agent.BehaviorParameters.Model != null);
            
            if (!hasAnyModel)
            {
                Log("⚠️ Cannot set agents to Inference mode - no ML models found. Please load models first.", DebugLogLevel.Important);
                return;
            }
        }
        
        ITrainArenaAgent[] allAgents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<ITrainArenaAgent>()
            .ToArray();
        
        int totalSet = 0;
        int cubeCount = 0, ragdollCount = 0;
        
        foreach (var agent in allAgents)
        {
            var behaviorParams = agent.BehaviorParameters;
            if (behaviorParams != null)
            {
                behaviorParams.BehaviorType = behaviorType;
                totalSet++;
                
                // Count by type for logging
                if (agent.AgentTypeIcon == "🧊") cubeCount++;
                else if (agent.AgentTypeIcon == "🎭") ragdollCount++;
            }
        }
        
        string behaviorName = behaviorType switch
        {
            Unity.MLAgents.Policies.BehaviorType.Default => "Training",
            Unity.MLAgents.Policies.BehaviorType.HeuristicOnly => "Heuristic",
            Unity.MLAgents.Policies.BehaviorType.InferenceOnly => "AI Model",
            _ => behaviorType.ToString()
        };
        
        Log($"Set all {totalSet} agents ({cubeCount} cubes, {ragdollCount} ragdolls) to {behaviorName} behavior type");
    }
    
    void SetAgentBehaviorType(ITrainArenaAgent agent, Unity.MLAgents.Policies.BehaviorType behaviorType)
    {
        var behaviorParams = agent.BehaviorParameters;
        if (behaviorParams != null)
        {
            behaviorParams.BehaviorType = behaviorType;
            
            string behaviorName = behaviorType switch
            {
                Unity.MLAgents.Policies.BehaviorType.Default => "Training",
                Unity.MLAgents.Policies.BehaviorType.HeuristicOnly => "Heuristic",
                Unity.MLAgents.Policies.BehaviorType.InferenceOnly => "AI Model",
                _ => behaviorType.ToString()
            };
            
            Log($"Set {agent.DisplayName} to {behaviorName} behavior type");
        }
    }
    
    // SetRagdollAgentBehaviorType method removed - now using unified SetAgentBehaviorType
    
    void ToggleAllAgentsActivity()
    {
        ITrainArenaAgent[] allAgents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<ITrainArenaAgent>()
            .ToArray();
        
        if (allAgents.Length == 0)
        {
            Log("No agents found to toggle activity");
            return;
        }
        
        // Determine current state - check if any agent is active
        bool anyActive = allAgents.Length > 0 && allAgents[0].AgentActivity == AgentActivity.Active;
        bool newActiveState = !anyActive;
        int toggledCount = 0;
        int cubeCount = 0, ragdollCount = 0;
        AgentActivity newActivity = newActiveState ? AgentActivity.Active : AgentActivity.Inactive;
        
        foreach (var agent in allAgents)
        {
            agent.AgentActivity = newActivity;
            toggledCount++;
            
            // Count by type for logging
            if (agent.AgentTypeIcon == "🧊") cubeCount++;
            else if (agent.AgentTypeIcon == "🎭") ragdollCount++;
        }
        
        string activityName = newActiveState ? "🟢 ACTIVE" : "⚫ INACTIVE";
        Log($"Toggled {toggledCount} agents ({cubeCount} cubes, {ragdollCount} ragdolls) to {activityName} (press Z to toggle)", DebugLogLevel.Important);
    }
    
    void SetAllAgentsActivity(AgentActivity activity)
    {
        ITrainArenaAgent[] allAgents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<ITrainArenaAgent>()
            .ToArray();
        
        int cubeCount = 0, ragdollCount = 0;
        
        foreach (var agent in allAgents)
        {
            agent.AgentActivity = activity;
            
            // Count by type for logging
            if (agent.AgentTypeIcon == "🧊") cubeCount++;
            else if (agent.AgentTypeIcon == "🎭") ragdollCount++;
        }
        
        string activityName = activity == AgentActivity.Active ? "🟢 ACTIVE" : "⚫ INACTIVE";
        Log($"Set all {allAgents.Length} agents ({cubeCount} cubes, {ragdollCount} ragdolls) to {activityName}");
    }
    
    void SetAgentActivity(ITrainArenaAgent agent, AgentActivity activity)
    {
        agent.AgentActivity = activity;
        
        string activityName = activity == AgentActivity.Active ? "🟢 Active" : "⚫ Inactive";
        Log($"Set {((MonoBehaviour)agent).name} to {activityName}");
    }
    
    // Logging methods with level filtering
    public static void Log(string message, DebugLogLevel level = DebugLogLevel.Important, Object context = null)
    {
        if (LogLevel >= level)
        {
            Debug.Log($"[TrainArena] {message}", context);
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

    // Visual debug rendering with Gizmos
    void OnDrawGizmos()
    {
        if (ShowArenaBounds)
        {
            DrawArenaBounds();
        }
        
        if (ShowVelocityDisplay)
        {
            DrawVelocityVectors();
        }
        
        if (ShowRaycastVisualization)
        {
            DrawRaycastVisualization();
        }
        
        if (ShowObservations)
        {
            DrawObservationVisuals();
        }
    }
    
    void DrawArenaBounds()
    {
        // Find Ground objects as descendants of EnvManager for efficiency
        List<GameObject> arenas = new List<GameObject>();
        
        // Look for EnvManager first
        var envInitializer = FindFirstObjectByType<EnvInitializer>();
        GameObject envManager = envInitializer?.gameObject;
        if (envManager != null)
        {
            // Search for Ground objects under EnvManager
            Transform[] children = envManager.GetComponentsInChildren<Transform>();
            foreach (var child in children)
            {
                if (child.name.Contains("Ground"))
                {
                    var collider = child.GetComponent<Collider>();
                    if (collider != null)
                    {
                        arenas.Add(child.gameObject);
                    }
                }
            }
        }
        
        // Fallback: if no EnvManager or Ground objects found, use previous method
        if (arenas.Count == 0)
        {
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var obj in allObjects)
            {
                string objName = obj.name.ToLower();
                if (objName.Contains("arena") || objName.Contains("ground") || 
                    objName.Contains("platform") || objName.Contains("floor"))
                {
                    var collider = obj.GetComponent<Collider>();
                    if (collider != null && collider.bounds.size.magnitude > 5f)
                    {
                        arenas.Add(obj);
                    }
                }
            }
        }
        
        // Draw bounds for all found arenas
        foreach (var arena in arenas)
        {
            var bounds = arena.GetComponent<Collider>();
            if (bounds != null)
            {
                // Draw arena bounds
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(bounds.bounds.center, bounds.bounds.size);
                
                // Draw grid on the ground for reference
                Gizmos.color = new Color(0, 1, 1, 0.3f); // Transparent cyan
                Vector3 center = bounds.bounds.center;
                Vector3 size = bounds.bounds.size;
                
                // Draw grid lines (fewer lines for multiple arenas to avoid clutter)
                int gridLines = 5;
                for (int i = -gridLines; i <= gridLines; i++)
                {
                    float x = center.x + (i * size.x / (gridLines * 2));
                    Gizmos.DrawLine(new Vector3(x, center.y, center.z - size.z/2), 
                                   new Vector3(x, center.y, center.z + size.z/2));
                    
                    float z = center.z + (i * size.z / (gridLines * 2));
                    Gizmos.DrawLine(new Vector3(center.x - size.x/2, center.y, z), 
                                   new Vector3(center.x + size.x/2, center.y, z));
                }
            }
        }
        
        // If still no arenas found, draw default
        if (arenas.Count == 0)
        {
            // Default arena bounds (20x20x20) with grid
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(20, 20, 20));
            
            // Default grid
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            for (int i = -5; i <= 5; i++)
            {
                Gizmos.DrawLine(new Vector3(i * 2f, 0, -10), new Vector3(i * 2f, 0, 10));
                Gizmos.DrawLine(new Vector3(-10, 0, i * 2f), new Vector3(10, 0, i * 2f));
            }
        }
    }
    
    void DrawVelocityVectors()
    {
        // Show velocity vectors for all agents using polymorphic approach
        ITrainArenaAgent[] allAgents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<ITrainArenaAgent>()
            .ToArray();
        
        foreach (var agent in allAgents)
        {
            if (agent.MainRigidbody != null && agent.MainRigidbody.linearVelocity.magnitude > 0.1f)
            {
                // Color by agent type
                Gizmos.color = agent.AgentTypeIcon == "🧊" ? Color.yellow : Color.magenta;
                
                Vector3 start = agent.MainTransform.position;
                Vector3 end = start + agent.MainRigidbody.linearVelocity;
                Gizmos.DrawLine(start, end);
                Gizmos.DrawWireSphere(end, 0.1f);
                
                // Draw speed indicator
                float speed = agent.MainRigidbody.linearVelocity.magnitude;
                Gizmos.color = Color.white;
                Gizmos.DrawWireCube(start + Vector3.up * 0.5f, Vector3.one * (0.1f + speed * 0.1f));
            }
        }
    }
    
    void DrawRaycastVisualization()
    {
        // Show raycasts for all agents with raycast sensors using polymorphic approach
        ITrainArenaAgent[] allAgents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<ITrainArenaAgent>()
            .ToArray();
        
        foreach (var agent in allAgents)
        {
            // Find raycast sensors on the agent or its children
            var agentMono = (MonoBehaviour)agent;
            var sensors = agentMono.GetComponentsInChildren<Unity.MLAgents.Sensors.RayPerceptionSensorComponent3D>();
            foreach (var sensor in sensors)
            {
                DrawRaycastSensor(sensor);
            }
        }
    }
    
    void DrawRaycastSensor(Unity.MLAgents.Sensors.RayPerceptionSensorComponent3D sensor)
    {
        if (sensor == null) return;

        // Use the sensor's own method to get ray perception input with proper angles
        var rayInput = sensor.GetRayPerceptionInput();
        var transform = sensor.transform;

        // Draw each ray using the angles from the ray input
        for (int i = 0; i < rayInput.Angles.Count; i++)
        {
            float angle = rayInput.Angles[i];
            Vector3 direction = Quaternion.AngleAxis(angle, transform.up) * transform.forward;
            Vector3 start = transform.position + Vector3.up * sensor.GetStartVerticalOffset();
            Vector3 end = start + direction * rayInput.RayLength;

            // Use different colors based on what the ray hits
            RaycastHit hit;
            if (Physics.Raycast(start, direction, out hit, rayInput.RayLength, rayInput.LayerMask))
            {
                // Check if the hit object has a tag that's in the detectable tags list
                bool isDetectable = rayInput.DetectableTags.Contains(hit.collider.tag);
                Gizmos.color = isDetectable ? Color.green : Color.red;
                Gizmos.DrawLine(start, hit.point);
                Gizmos.DrawWireSphere(hit.point, 0.1f);
            }
            else
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(start, end);
            }
        }
    }
    
    void DrawObservationVisuals()
    {
        // Visual indicators for agent observations
        CubeAgent[] cubeAgents = FindObjectsByType<CubeAgent>(FindObjectsSortMode.None);
        RagdollAgent[] ragdollAgents = FindObjectsByType<RagdollAgent>(FindObjectsSortMode.None);
        
        // Show cube agent observation sphere
        foreach (var agent in cubeAgents)
        {
            Gizmos.color = new Color(0, 1, 1, 0.2f); // Transparent cyan
            Gizmos.DrawWireSphere(agent.transform.position, 2f); // Observation range
        }
        
        // Show ragdoll agent observation sphere (around pelvis)
        foreach (var agent in ragdollAgents)
        {
            if (agent.pelvis != null)
            {
                Gizmos.color = new Color(1, 0, 1, 0.2f); // Transparent magenta
                Gizmos.DrawWireSphere(agent.pelvis.position, 2f); // Observation range
            }
        }
    }

    private static float[] GetRayAngles(int raysPerDirection, float maxRayDegrees)
    {
        // This is a simplified version of the ML-Agents internal method.
        // It generates evenly spaced angles from -maxRayDegrees to +maxRayDegrees.
        int numRays = raysPerDirection * 2 + 1;
        float[] angles = new float[numRays];
        if (numRays == 1)
        {
            angles[0] = 0f;
            return angles;
        }
        float delta = (maxRayDegrees * 2f) / (numRays - 1);
        for (int i = 0; i < numRays; i++)
        {
            angles[i] = -maxRayDegrees + delta * i;
        }
        return angles;
    }
}