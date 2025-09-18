using System.Linq;
using Unity.MLAgents;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using static Codice.Client.Common.WebApi.WebApiEndpoints;

/// <summary>
/// Unity Editor tools for generating complete ML-Agents training scenes
/// 
/// Features:
/// - Automated scene creation for cube and ragdoll agents
/// - ML-Agents integration with behavior switching and time scale management
/// - Camera setup with navigation controls and optimal viewing angles
/// - Debug system integration with comprehensive visualization tools
/// - Separate training/testing scene configurations
/// 
/// Menu Items:
/// - Tools/ML Hack/Build Cube Training Scene: 4x4 cube agent training environment
/// - Tools/ML Hack/Build Cube Test Scene: 2x2 cube agent testing environment  
/// - Tools/ML Hack/Build Ragdoll Training Scene: 2x2 ragdoll agent training environment
/// - Tools/ML Hack/Build Ragdoll Test Scene: 1x1 ragdoll agent testing environment
/// </summary>
public static class SceneBuilder
{
    /// <summary>
    /// Detects if ML-Agents training is active by checking for Academy communication port
    /// This allows automatic switching between training and testing modes
    /// </summary>
    private static bool IsMLAgentsTrainingActive()
    {
        // Check if Academy is connected to external trainer
        var academy = Unity.MLAgents.Academy.Instance;
        if (academy != null)
        {
            // In training mode, Academy.IsCommunicatorOn returns true when connected to mlagents-learn
            return academy.IsCommunicatorOn;
        }
        return false;
    }

    [MenuItem("Tools/ML Hack/Build Cube Training Scene")]
    public static void BuildCubeScene()
    {
        BuildCubeSceneInternal(SceneType.Training);
    }
    
    [MenuItem("Tools/ML Hack/Build Cube Test Scene")]
    public static void BuildCubeTestScene()
    {
        BuildCubeSceneInternal(SceneType.Testing);
    }
    
    public enum SceneType { Training, Testing }
    
    static void BuildCubeSceneInternal(SceneType sceneType)
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);

        // Camera with controller for navigation - optimized for 16:9 and isometric view
        var cam = new GameObject("Main Camera");
        var camera = cam.AddComponent<Camera>();
        cam.tag = "MainCamera";
        
        // Position camera with slight isometric angle for better arena framing
        // Center of grid is at (30, 0, 30) - halfway between (0,0) and (60,60)
        cam.transform.position = new Vector3(30, 50, -15); // Higher and closer for better framing
        cam.transform.LookAt(new Vector3(30, 0, 30)); // Look at center of 4x4 arena grid
        
        // Optimize for 16:9 aspect ratio
        camera.aspect = 16f / 9f;
        camera.fieldOfView = 60f; // Slightly wider FOV to frame all arenas tightly
        camera.clearFlags = CameraClearFlags.Skybox;
        
        // Add camera controller for WASD navigation
        cam.AddComponent<EditorCameraController>();
        
        // Add camera controls UI (positioned to not conflict with TimeScaleManager)
        cam.AddComponent<CameraControlsUI>();
        
        // Add debug manager to scene
        var debugManager = new GameObject("TrainArenaDebugManager");
        debugManager.AddComponent<TrainArenaDebugManager>();
        
        // Add time scale manager for training speed monitoring
        var timeManager = new GameObject("TimeScaleManager");
        timeManager.AddComponent<TimeScaleManager>();

        // Light - Enhanced setup with soft shadows
        var lightGO = new GameObject("Directional Light");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);
        
        // Enhanced lighting settings for better quality
        light.shadows = LightShadows.Soft;
        light.shadowStrength = 0.6f;
        light.shadowResolution = UnityEngine.Rendering.LightShadowResolution.Medium; // 1024x1024

        // Manager
        var manager = new GameObject("EnvManager");
        var init = manager.AddComponent<EnvInitializer>();

        // Configure based on scene type
        if (sceneType == SceneType.Testing)
        {
            // Test scene configuration: 2x2 grid with more obstacles
            init.EnvCountX = 2;
            init.EnvCountZ = 2;
            init.ObstaclesPerArena = 6;
        }
        // Training scene uses default settings (configured in EnvInitializer)

        // Prefabs (create basic ones procedurally - be sure to disable after spawning the environment)
        init.cubeAgentPrefab = CreateCubeAgentPrefab(init, sceneType);
        init.goalPrefab = CreateGoalPrefab(init);
        init.obstaclePrefab = CreateObstaclePrefab(init);

        TrainArenaDebugManager.Log("🎯 Cube training scene created with auto behavior switching and time scale monitoring!", TrainArenaDebugManager.DebugLogLevel.Important);
        TrainArenaDebugManager.Log("✅ AutoBehaviorSwitcher: Automatically switches between training and testing modes", TrainArenaDebugManager.DebugLogLevel.Important);
        TrainArenaDebugManager.Log("⏱️ TimeScaleManager: Monitors training speed (20x during training, 1x during testing)", TrainArenaDebugManager.DebugLogLevel.Important);
        TrainArenaDebugManager.Log("🎮 Usage: Press Play to simulate (press 'H' for debug controls), or start training via mlagents-learn", TrainArenaDebugManager.DebugLogLevel.Important);
    }



    [MenuItem("Tools/ML Hack/Build Ragdoll Training Scene")]
    public static void BuildRagdollTrainingScene()
    {
        BuildRagdollSceneInternal(SceneType.Training);
    }
    
    [MenuItem("Tools/ML Hack/Build Ragdoll Test Scene")]
    public static void BuildRagdollTestScene()
    {
        BuildRagdollSceneInternal(SceneType.Testing);
    }
    
    static void BuildRagdollSceneInternal(SceneType sceneType)
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);

        // Camera with controller for navigation - optimized for ragdoll training view
        var cam = new GameObject("Main Camera");
        var camera = cam.AddComponent<Camera>();
        cam.tag = "MainCamera";
        
        // Position camera for 2x2 ragdoll grid (4 arenas) 
        // Center of grid is at (4, 0, 4) - halfway between (0,0) and (8,8) 
        cam.transform.position = new Vector3(4, 12, -8); // Higher and further back for ragdolls
        cam.transform.LookAt(new Vector3(4, 0, 4)); // Look at center of 2x2 arena grid
        
        // Optimize camera settings for ragdoll observation
        camera.aspect = 16f / 9f;
        camera.fieldOfView = 50f; // Slightly narrower FOV for better ragdoll detail
        camera.clearFlags = CameraClearFlags.Skybox;
        
        // Add camera controller for WASD navigation
        cam.AddComponent<EditorCameraController>();
        cam.AddComponent<CameraControlsUI>();
        
        // Add debug manager and time scale manager (same as cube scene)
        var debugManager = new GameObject("TrainArenaDebugManager");
        debugManager.AddComponent<TrainArenaDebugManager>();
        
        var timeManager = new GameObject("TimeScaleManager");
        timeManager.AddComponent<TimeScaleManager>();
        
        // Add Domain Randomization UI for ragdoll physics testing
        var domainUI = new GameObject("DomainRandomizationUI");
        domainUI.AddComponent<DomainRandomizationUI>();

        // Enhanced lighting setup (same as cube scene)
        var lightGO = new GameObject("Directional Light");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);
        light.shadows = LightShadows.Soft;
        light.shadowStrength = 0.6f;
        light.shadowResolution = UnityEngine.Rendering.LightShadowResolution.Medium;

        // Manager using same pattern as cube scene
        var manager = new GameObject("RagdollEnvManager");
        var init = manager.AddComponent<EnvInitializer>();

        // Configure based on scene type
        if (sceneType == SceneType.Testing)
        {
            // Test scene configuration: 2x2 grid with more obstacles
            init.EnvCountX = 2;
            init.EnvCountZ = 2;
            init.ObstaclesPerArena = 6;
            init.ArenaHelper.ArenaSize = 15f; // Larger arenas for ragdoll movement for testing
            init.ArenaHelper.AgentSpawnHeight = 0.75f; // Ragdolls are taller than cubes
        }
        // Training scene uses default settings (configured in EnvInitializer)
		else
		{
            // Configure for ragdoll using existing EnvInitializer structure
            //init.ArenaHelper.ArenaSize = 12f; // Smaller arena for single ragdoll training
			init.ArenaHelper.ArenaSize = 15f; //TODO: Make the same as testing - any reason they should be different sizes?
        }
        init.ArenaHelper.AgentSpawnHeight = 0.75f; // Ragdolls are taller than cubes

        // Create ragdoll and supporting prefabs
        init.cubeAgentPrefab = CreateRagdollAgentPrefab(init, sceneType);
        
        // Create goal and obstacle prefabs for EnvInitializer consistency
        var goalPrefab = PrimitiveBuilder.CreateGoal(1f, "Goal");
        var obstaclePrefab = PrimitiveBuilder.CreateObstacle(1.5f, 0.8f, 0.8f, "Obstacle");
        
        init.goalPrefab = goalPrefab;
        init.obstaclePrefab = obstaclePrefab;

        // Ensure ML-Agents Academy is initialized (singleton pattern)
        if (Academy.Instance == null)
        {
            // Academy.Instance automatically creates the Academy if it doesn't exist
            var academy = Academy.Instance;
            TrainArenaDebugManager.Log("✅ Academy initialized for ragdoll training scene", TrainArenaDebugManager.DebugLogLevel.Important);
        }

        TrainArenaDebugManager.Log("🎭 Ragdoll scene created with auto behavior switching and time scale monitoring!", TrainArenaDebugManager.DebugLogLevel.Important);
        TrainArenaDebugManager.Log("✅ AutoBehaviorSwitcher: Automatically switches between training and testing modes", TrainArenaDebugManager.DebugLogLevel.Important);
        TrainArenaDebugManager.Log("⏱️ TimeScaleManager: Monitors training speed (20x during training, 1x during testing)", TrainArenaDebugManager.DebugLogLevel.Important);
        TrainArenaDebugManager.Log("🎮 Usage: Press Play to simulate (press 'H' for debug controls), or start training via mlagents-learn", TrainArenaDebugManager.DebugLogLevel.Important);
    }

    static GameObject CreateCubeAgentPrefab(EnvInitializer init, SceneType sceneType = SceneType.Training)
    {
        // Use PrimitiveBuilder for consistent agent creation
        var agent = PrimitiveBuilder.CreateAgent("CubeAgent");

        var cubeAgent = agent.AddComponent<CubeAgent>();
        // Set obstacle mask to everything for tag-based detection
        cubeAgent.obstacleMask = -1;
        
        // ML-Agents Agent class automatically adds BehaviorParameters - configure it
        var behaviorParams = agent.GetComponent<Unity.MLAgents.Policies.BehaviorParameters>();
        if (behaviorParams != null)
        {
            behaviorParams.BehaviorName = "CubeAgent";
            // Configure behavior type based on scene type
            if (sceneType == SceneType.Testing)
            {
                // Test scene: InferenceOnly for AI model testing
                behaviorParams.BehaviorType = Unity.MLAgents.Policies.BehaviorType.InferenceOnly;
            }
            else
            {
                // Training scene: Start with HeuristicOnly - AutoBehaviorSwitcher will handle runtime switching
                behaviorParams.BehaviorType = Unity.MLAgents.Policies.BehaviorType.HeuristicOnly;
            }
            behaviorParams.TeamId = 0;
            behaviorParams.UseChildSensors = true;
            
            // Configure action and observation space
            if (behaviorParams.BrainParameters.ActionSpec.NumContinuousActions == 0)
            {
                // Set up action space: 2 continuous actions (moveX, moveZ)
                behaviorParams.BrainParameters.ActionSpec = 
                    Unity.MLAgents.Actuators.ActionSpec.MakeContinuous(2);
            }
            
            // Configure observation space dynamically based on agent configuration
            var cubeAgentComponent = agent.GetComponent<CubeAgent>();
            int totalObservations = cubeAgentComponent.GetTotalObservationCount();
            
            if (behaviorParams.BrainParameters.VectorObservationSize != totalObservations)
            {
                behaviorParams.BrainParameters.VectorObservationSize = totalObservations;
            }
            
            string behaviorMode = behaviorParams.BehaviorType == Unity.MLAgents.Policies.BehaviorType.Default ? "ML Training" : "Editor Testing";
            TrainArenaDebugManager.Log($"Configured agent: {behaviorParams.BrainParameters.ActionSpec.NumContinuousActions} actions, {behaviorParams.BrainParameters.VectorObservationSize} observations " +
                                     $"({CubeAgent.VELOCITY_OBSERVATIONS} velocity + {CubeAgent.GOAL_OBSERVATIONS} goal + {cubeAgentComponent.raycastDirections} raycasts), Mode: {behaviorMode}", 
                                     TrainArenaDebugManager.DebugLogLevel.Important);
        }
        
        // Add blinking animation for visual polish
        agent.AddComponent<EyeBlinker>();
        
        // Add debug info component for development
        agent.AddComponent<AgentDebugInfo>();
        
        // Add automatic behavior switching only for training scenes
        if (sceneType == SceneType.Training)
        {
            var autoSwitcher = agent.AddComponent<AutoBehaviorSwitcher>();
            autoSwitcher.enableAutoSwitching = true;
            autoSwitcher.showDebugMessages = true;
            
            TrainArenaDebugManager.Log("Added AutoBehaviorSwitcher for seamless training/testing mode switching", 
                                     TrainArenaDebugManager.DebugLogLevel.Important);
        }
        else
        {
            TrainArenaDebugManager.Log("Test scene: No AutoBehaviorSwitcher (InferenceOnly mode for AI model testing)", 
                                     TrainArenaDebugManager.DebugLogLevel.Important);
        }

        return agent;
    }

    static GameObject CreateGoalPrefab(EnvInitializer init)
    {
        // Use PrimitiveBuilder for consistent goal creation
        return PrimitiveBuilder.CreateGoal(init.ArenaHelper.GoalHeight);
    }

    static GameObject CreateObstaclePrefab(EnvInitializer init)
    {
        // Use PrimitiveBuilder for consistent obstacle creation
        PrimitiveBuilder.EnsureTagExists("Obstacle");
        return PrimitiveBuilder.CreateObstacle(init.ArenaHelper.ObstacleHeight);
    }

    static GameObject CreateRagdollAgentPrefab(EnvInitializer init, SceneType sceneType = SceneType.Training)
    {
        // Use PrimitiveBuilder for consistent ragdoll creation
        var ragdoll = PrimitiveBuilder.CreateRagdoll("RagdollAgent");

        // The ragdoll already has RagdollAgent and BehaviorParameters from PrimitiveBuilder
        // Just need to configure behavior switching like cube agents
        var ragdollAgent = ragdoll.GetComponentInChildren<RagdollAgent>();
        var behaviorParams = ragdoll.GetComponentInChildren<Unity.MLAgents.Policies.BehaviorParameters>();
        
        if (behaviorParams != null)
        {
            // Start with HeuristicOnly - AutoBehaviorSwitcher will handle runtime switching
            behaviorParams.BehaviorType = Unity.MLAgents.Policies.BehaviorType.HeuristicOnly;
            behaviorParams.TeamId = 0;
            behaviorParams.UseChildSensors = true;
            
            // Ensure observation space is correct (16 observations for ragdoll)
            if (behaviorParams.BrainParameters.VectorObservationSize != 16)
            {
                behaviorParams.BrainParameters.VectorObservationSize = 16;
                TrainArenaDebugManager.Log("Fixed VectorObservationSize to 16 for RagdollAgent", TrainArenaDebugManager.DebugLogLevel.Important);
            }
            
            // CRITICAL: Ensure ActionSpec is correctly configured for 6 continuous actions
            if (behaviorParams.BrainParameters.ActionSpec.NumContinuousActions != 6)
            {
                behaviorParams.BrainParameters.ActionSpec = Unity.MLAgents.Actuators.ActionSpec.MakeContinuous(6);
                TrainArenaDebugManager.Log($"FIXED ActionSpec: Set to 6 continuous actions (was {behaviorParams.BrainParameters.ActionSpec.NumContinuousActions})", TrainArenaDebugManager.DebugLogLevel.Important);
            }
            
            TrainArenaDebugManager.Log($"Configured ragdoll: {behaviorParams.BrainParameters.ActionSpec.NumContinuousActions} actions, " +
                                     $"{behaviorParams.BrainParameters.VectorObservationSize} observations " +
                                     $"(6 joints: hips, knees, ankles), Mode: Editor Testing → ML Training (auto-switch)", 
                                     TrainArenaDebugManager.DebugLogLevel.Important);
        }
        
        // Add debug UI components (same as cube agents)
        var pelvis = ragdoll.GetComponentInChildren<RagdollAgent>().gameObject;
        pelvis.AddComponent<AgentDebugInfo>();
        // Note: No EyeBlinker for ragdolls - they don't have eyes
        
        // Add domain randomization for physics testing (random mass, friction, etc.)
        var domainRandomizer = pelvis.AddComponent<DomainRandomizer>(); //TODO: Or add to root ragdoll object?
        domainRandomizer.randomizeMass = true;
        domainRandomizer.randomizeFriction = true;
        domainRandomizer.randomizeGravity = false; // Keep gravity stable for ragdolls

        // Add automatic behavior switching only for training scenes
        if (sceneType == SceneType.Training)
        {
            var autoSwitcher = pelvis.AddComponent<AutoBehaviorSwitcher>();
            autoSwitcher.enableAutoSwitching = true;
            autoSwitcher.showDebugMessages = true;

            TrainArenaDebugManager.Log("Added AutoBehaviorSwitcher for seamless training/testing mode switching",
                                     TrainArenaDebugManager.DebugLogLevel.Important);
        }
        else
        {
            TrainArenaDebugManager.Log("Test scene: No AutoBehaviorSwitcher (InferenceOnly mode for AI model testing)",
                                     TrainArenaDebugManager.DebugLogLevel.Important);
        }

        TrainArenaDebugManager.Log("Added debug components: AgentDebugInfo, DomainRandomizer", 
                                 TrainArenaDebugManager.DebugLogLevel.Important);

        return ragdoll;
    }
    

}