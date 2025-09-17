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
    public static bool ShowMLAgentsStatus = true; // Show ML-Agents behavior info by default
    public static bool ShowTimeScale = true; // Show TimeScale UI by default
    public static bool ShowHelp = true; // Show by default
    
    [Header("Logging Controls")]
    public static DebugLogLevel LogLevel = DebugLogLevel.Important;  // Show important info by default
    
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
            "A - Toggle Arena Bounds\n" +
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
        // Prominent banner at top-center of screen
        float bannerWidth = 300f;
        float bannerHeight = 35f;
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
        GUILayout.Space(8);
        
        string status = isTraining ? "🎯 TRAINING MODE ACTIVE" : "🧠 INFERENCE/DEMO MODE";
        GUILayout.Label(status, GUI.skin.box);
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
        
        GUI.color = Color.white;
        GUI.backgroundColor = Color.clear;
    }
    
    void DrawMLAgentsStatusPanel(bool isTraining)
    {
        // Find all CubeAgent instances in the scene
        CubeAgent[] agents = FindObjectsByType<CubeAgent>(FindObjectsSortMode.None);
        
        if (agents.Length == 0)
        {
            return; // No agents to display
        }
        
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
        float contentHeight = headerHeight + agentLabelHeight + (Mathf.Min(agents.Length, maxVisibleAgents) * agentHeight) + 40f;
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
        GUILayout.Label($"All Agents ({agents.Length}):", GUILayout.Width(110));
        
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
        GUI.color = Color.gray;
        GUILayout.Label($"Agent Status ({agents.Length} total):", GUI.skin.label);
        GUI.color = Color.white;
        
        // Scroll view for agent controls with proper sizing
        float scrollHeight = panelHeight - headerHeight - 50f; // Account for spacing and label
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true, 
                                                 GUILayout.Width(panelWidth - 20), 
                                                 GUILayout.Height(scrollHeight),
                                                 GUILayout.ExpandHeight(false));
        
        // Display ALL agents with compact layout - no artificial limit
        for (int i = 0; i < agents.Length; i++)
        {
            DrawAgentStatus(agents[i], isTraining);
        }
        
        GUILayout.EndScrollView();
        GUILayout.Space(16);
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    void DrawAgentStatus(CubeAgent agent, bool isTraining)
    {
        if (agent == null) return;
        
        // Get behavior parameters
        var behaviorParams = agent.GetComponent<Unity.MLAgents.Policies.BehaviorParameters>();
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
        
        // Get activity status
        string activityEmoji = agent.agentActivity == AgentActivity.Active ? "🟢" : "⚫";
        string activityStatus = agent.agentActivity == AgentActivity.Active ? "Active" : "Inactive";
        
        // Ultra-compact single-line agent display with inline model info
        GUILayout.BeginHorizontal();
        
        // Agent name with status info inline for compactness
        GUI.color = statusColor;
        string agentDisplay = $"{activityEmoji} {behaviorEmoji} {agent.name}";
        
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
        
        float agentDescriptionWidth = 425f; // Not including buttons to the right
        GUILayout.Label(agentDisplay, GUILayout.Width(agentDescriptionWidth));
        GUI.color = Color.white;
        
        // Spacer to push buttons to the right
        GUILayout.FlexibleSpace();
        
        // Activity toggle button
        if (agent.agentActivity == AgentActivity.Active)
        {
            GUI.backgroundColor = new Color(0f, 0.8f, 0.2f, 0.8f); // Green for active
            if (GUILayout.Button("🟢", GUILayout.Width(25)))
            {
                SetAgentActivity(agent, AgentActivity.Inactive);
            }
        }
        else
        {
            GUI.backgroundColor = new Color(0.6f, 0.6f, 0.6f, 0.8f); // Gray for inactive
            if (GUILayout.Button("⚫", GUILayout.Width(25)))
            {
                SetAgentActivity(agent, AgentActivity.Active);
            }
        }
        
        // Individual behavior type buttons (more compact)
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
    
    void SetAllAgentsBehaviorType(Unity.MLAgents.Policies.BehaviorType behaviorType)
    {
        var cubeAgents = FindObjectsByType<CubeAgent>(FindObjectsSortMode.None);
        foreach (var agent in cubeAgents)
        {
            var behaviorParams = agent.GetComponent<Unity.MLAgents.Policies.BehaviorParameters>();
            if (behaviorParams != null)
            {
                behaviorParams.BehaviorType = behaviorType;
            }
        }
        
        string behaviorName = behaviorType switch
        {
            Unity.MLAgents.Policies.BehaviorType.Default => "Training",
            Unity.MLAgents.Policies.BehaviorType.HeuristicOnly => "Manual",
            Unity.MLAgents.Policies.BehaviorType.InferenceOnly => "AI Model",
            _ => behaviorType.ToString()
        };
        
        Log($"Set all {cubeAgents.Length} agents to {behaviorName} behavior type");
    }
    
    void SetAgentBehaviorType(CubeAgent agent, Unity.MLAgents.Policies.BehaviorType behaviorType)
    {
        var behaviorParams = agent.GetComponent<Unity.MLAgents.Policies.BehaviorParameters>();
        if (behaviorParams != null)
        {
            behaviorParams.BehaviorType = behaviorType;
            
            string behaviorName = behaviorType switch
            {
                Unity.MLAgents.Policies.BehaviorType.Default => "Training",
                Unity.MLAgents.Policies.BehaviorType.HeuristicOnly => "Manual",
                Unity.MLAgents.Policies.BehaviorType.InferenceOnly => "AI Model",
                _ => behaviorType.ToString()
            };
            
            Log($"Set {agent.name} to {behaviorName} behavior type");
        }
    }
    
    void ToggleAllAgentsActivity()
    {
        var cubeAgents = FindObjectsByType<CubeAgent>(FindObjectsSortMode.None);
        if (cubeAgents.Length == 0)
        {
            Log("No agents found to toggle activity");
            return;
        }
        
        // Determine current state by checking first agent
        AgentActivity newActivity = cubeAgents[0].agentActivity == AgentActivity.Active ? 
                                   AgentActivity.Inactive : AgentActivity.Active;
        
        int toggledCount = 0;
        foreach (var agent in cubeAgents)
        {
            agent.agentActivity = newActivity;
            toggledCount++;
        }
        
        string activityName = newActivity == AgentActivity.Active ? "🟢 ACTIVE" : "⚫ INACTIVE";
        Log($"Toggled {toggledCount} agents to {activityName} (press Z to toggle)", DebugLogLevel.Important);
    }
    
    void SetAllAgentsActivity(AgentActivity activity)
    {
        var cubeAgents = FindObjectsByType<CubeAgent>(FindObjectsSortMode.None);
        foreach (var agent in cubeAgents)
        {
            agent.agentActivity = activity;
        }
        
        string activityName = activity == AgentActivity.Active ? "🟢 ACTIVE" : "⚫ INACTIVE";
        Log($"Set all {cubeAgents.Length} agents to {activityName}");
    }
    
    void SetAgentActivity(CubeAgent agent, AgentActivity activity)
    {
        agent.agentActivity = activity;
        
        string activityName = activity == AgentActivity.Active ? "🟢 Active" : "⚫ Inactive";
        Log($"Set {agent.name} to {activityName}");
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