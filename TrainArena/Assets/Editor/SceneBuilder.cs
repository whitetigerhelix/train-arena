using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Linq;
using Unity.MLAgents;

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
    
    enum SceneType { Training, Testing }
    
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

    [MenuItem("Tools/ML Hack/Build Ragdoll Test Scene")]
    public static void BuildRagdollScene()
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);

        // Camera with controller for navigation - same as cube scene
        var cam = new GameObject("Main Camera");
        var camera = cam.AddComponent<Camera>();
        cam.tag = "MainCamera";
        
        // Position camera for single ragdoll testing
        cam.transform.position = new Vector3(0, 4, -8);
        cam.transform.LookAt(Vector3.zero);
        
        // Optimize for ragdoll testing
        camera.aspect = 16f / 9f;
        camera.fieldOfView = 60f;
        camera.clearFlags = CameraClearFlags.Skybox;
        
        // Add camera controller for WASD navigation (same as cube scene)
        cam.AddComponent<EditorCameraController>();
        cam.AddComponent<CameraControlsUI>();
        
        // Add debug manager to scene (same as cube scene)
        var debugManager = new GameObject("TrainArenaDebugManager");
        debugManager.AddComponent<TrainArenaDebugManager>();
        
        // Add time scale manager for consistent behavior (same as cube scene)
        var timeManager = new GameObject("TimeScaleManager");
        timeManager.AddComponent<TimeScaleManager>();
        
        // Add Domain Randomization UI for ragdoll physics testing
        var domainUI = new GameObject("DomainRandomizationUI");
        domainUI.AddComponent<DomainRandomizationUI>();

        // Light - Enhanced setup with soft shadows (same as cube scene)
        var lightGO = new GameObject("Directional Light");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);
        light.shadows = LightShadows.Soft;
        light.shadowStrength = 0.6f;
        light.shadowResolution = UnityEngine.Rendering.LightShadowResolution.Medium;

        // Manager - USE SAME PATTERN AS CUBE SCENE
        var manager = new GameObject("RagdollEnvManager");
        var init = manager.AddComponent<EnvInitializer>();

        // Configure for single ragdoll testing
        init.ArenaHelper.ArenaSize = 15f; // Smaller arena for single ragdoll testing
        init.ArenaHelper.AgentSpawnHeight = 0.75f; // Ragdolls are taller than cubes

        // Prefabs - create ragdoll and supporting prefabs
        init.cubeAgentPrefab = CreateRagdollAgentPrefab(init);
        
        // Create goal and obstacle prefabs for EnvInitializer consistency
        var goalPrefab = PrimitiveBuilder.CreateGoal(1f, "Goal");
        var obstaclePrefab = PrimitiveBuilder.CreateObstacle(1.5f, 0.8f, 0.8f, "Obstacle");
        
        init.goalPrefab = goalPrefab;
        init.obstaclePrefab = obstaclePrefab;

        // Ensure ML-Agents Academy is initialized (same as cube scene)
        if (Academy.Instance == null)
        {
            var academy = Academy.Instance;
            TrainArenaDebugManager.Log("✅ Academy initialized for ragdoll test scene", TrainArenaDebugManager.DebugLogLevel.Important);
        }

        TrainArenaDebugManager.Log("🎭 Ragdoll test scene created with EnvInitializer infrastructure!", TrainArenaDebugManager.DebugLogLevel.Important);
        TrainArenaDebugManager.Log("✅ AutoBehaviorSwitcher: Automatically switches between training and testing modes", TrainArenaDebugManager.DebugLogLevel.Important);
        TrainArenaDebugManager.Log("⏱️ TimeScaleManager: Monitors training speed (20x during training, 1x during testing)", TrainArenaDebugManager.DebugLogLevel.Important);
        TrainArenaDebugManager.Log("🎮 Usage: Press Play to test physics, 'H' for heuristic mode, or start training", TrainArenaDebugManager.DebugLogLevel.Important);
    }

    [MenuItem("Tools/ML Hack/Build Ragdoll Training Scene")]
    public static void BuildRagdollTrainingScene()
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

        // Configure for ragdoll training using existing EnvInitializer structure
        // EnvInitializer defaults to Training preset (2x2 grid), just configure arena size
        init.ArenaHelper.ArenaSize = 12f; // Larger arenas for ragdoll movement
        init.ArenaHelper.AgentSpawnHeight = 0.75f; // Ragdolls are taller than cubes

        // Create ragdoll and supporting prefabs
        init.cubeAgentPrefab = CreateRagdollAgentPrefab(init);
        
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

        TrainArenaDebugManager.Log("🎭 Ragdoll training scene created with full infrastructure!", TrainArenaDebugManager.DebugLogLevel.Important);
        TrainArenaDebugManager.Log("✅ AutoBehaviorSwitcher: Automatically switches between training and testing modes", TrainArenaDebugManager.DebugLogLevel.Important);
        TrainArenaDebugManager.Log("⏱️ TimeScaleManager: Monitors training speed (20x during training, 1x during testing)", TrainArenaDebugManager.DebugLogLevel.Important);
        TrainArenaDebugManager.Log("🚀 Usage: Start training with: mlagents-learn Assets/ML-Agents/Configs/ragdoll_ppo.yaml --run-id=ragdoll_sprint --train", TrainArenaDebugManager.DebugLogLevel.Important);
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

    static GameObject CreateRagdollAgentPrefab(EnvInitializer init)
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
            
            TrainArenaDebugManager.Log($"Configured ragdoll: {behaviorParams.BrainParameters.ActionSpec.NumContinuousActions} actions, " +
                                     $"{behaviorParams.BrainParameters.VectorObservationSize} observations " +
                                     $"(6 joints: hips, knees, ankles), Mode: Editor Testing → ML Training (auto-switch)", 
                                     TrainArenaDebugManager.DebugLogLevel.Important);
        }
        
        // Configure existing AutoBehaviorSwitcher (already added by PrimitiveBuilder)
        var autoSwitcher = ragdoll.GetComponentInChildren<AutoBehaviorSwitcher>();
        if (autoSwitcher != null)
        {
            autoSwitcher.enableAutoSwitching = true;
            autoSwitcher.showDebugMessages = true;
        }
        
        // Add debug UI components (same as cube agents)
        var pelvis = ragdoll.GetComponentInChildren<RagdollAgent>().gameObject;
        pelvis.AddComponent<AgentDebugInfo>();
        // Note: No EyeBlinker for ragdolls - they don't have eyes
        
        // Add domain randomization for physics testing (random mass, friction, etc.)
        var domainRandomizer = ragdoll.AddComponent<DomainRandomizer>();
        domainRandomizer.randomizeMass = true;
        domainRandomizer.randomizeFriction = true;
        domainRandomizer.randomizeGravity = false; // Keep gravity stable for ragdolls
        
        TrainArenaDebugManager.Log("Added debug components: AgentDebugInfo, EyeBlinker, DomainRandomizer", 
                                 TrainArenaDebugManager.DebugLogLevel.Important);
        TrainArenaDebugManager.Log("Added AutoBehaviorSwitcher for seamless ragdoll training/testing mode switching", 
                                 TrainArenaDebugManager.DebugLogLevel.Important);

        return ragdoll;
    }
    

}