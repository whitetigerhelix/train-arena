using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Linq;

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

        // Prefabs (create basic ones procedurally - be sure to disable after spawning the environment)
        init.cubeAgentPrefab = CreateCubeAgentPrefab(init);
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
        var cam = new GameObject("Main Camera");
        cam.tag = "MainCamera";
        cam.AddComponent<Camera>();
        cam.transform.position = new Vector3(0, 3, -6);
        cam.transform.LookAt(Vector3.zero);

        var lightGO = new GameObject("Directional Light");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);
        
        // Enhanced lighting settings for better quality
        light.shadows = LightShadows.Soft;
        light.shadowStrength = 0.6f;
        light.shadowResolution = UnityEngine.Rendering.LightShadowResolution.Medium; // 1024x1024

        // Ground
        var ground = PrimitiveBuilder.CreateGround("Ground");
        ground.transform.localScale = Vector3.one;

        Debug.Log("Ragdoll test scene created. Build your ragdoll and add RagdollAgent + PDJointController components.");
    }

    static GameObject CreateCubeAgentPrefab(EnvInitializer init)
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
            // Start with HeuristicOnly - AutoBehaviorSwitcher will handle runtime switching
            behaviorParams.BehaviorType = Unity.MLAgents.Policies.BehaviorType.HeuristicOnly;
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
        
        // Add automatic behavior switching for seamless training/testing
        var autoSwitcher = agent.AddComponent<AutoBehaviorSwitcher>();
        autoSwitcher.enableAutoSwitching = true;
        autoSwitcher.showDebugMessages = true;
        
        TrainArenaDebugManager.Log("Added AutoBehaviorSwitcher for seamless training/testing mode switching", 
                                 TrainArenaDebugManager.DebugLogLevel.Important);

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
    

}