using System.Linq;
using TrainArena.Core;
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
    public enum SceneType { Training, Testing }
    public enum AgentType { Cube, Ragdoll }

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

    static void BuildSceneInternal(SceneType sceneType, AgentType agentType)
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);

        // Camera with controller for navigation - optimized for 16:9 and isometric view
        var cam = new GameObject("Main Camera");
        var camera = cam.AddComponent<Camera>();
        cam.tag = "MainCamera";
        
        //TODO: Don't hardcode camera and lights (and any other magic numbers we're using), make them configurable or use constants/properties
        //TODO: Cleanup camera tuning, align both types of agents, etc
        if (agentType == AgentType.Cube)
        {
            // Position camera with slight isometric angle for better arena framing
            // Center of grid is at (30, 0, 30) - halfway between (0,0) and (60,60)
            cam.transform.position = new Vector3(30, 50, -15); // Higher and closer for better framing
            cam.transform.LookAt(new Vector3(30, 0, 30)); // Look at center of 4x4 arena grid
        }
        else
        {   
            // Position camera for 2x2 ragdoll grid (4 arenas) 
            // Center of grid is at (4, 0, 4) - halfway between (0,0) and (8,8) 
            cam.transform.position = new Vector3(4, 12, -8); // Higher and further back for ragdolls
            cam.transform.LookAt(new Vector3(4, 0, 4)); // Look at center of 2x2 arena grid
        }
        
        // Optimize for 16:9 aspect ratio
        camera.aspect = 16f / 9f;
        camera.fieldOfView = 60f; // Slightly wider FOV to frame all arenas tightly
        //TODO: camera.fieldOfView = 50f; // Slightly narrower FOV for better ragdoll detail
        camera.clearFlags = CameraClearFlags.Skybox;
        
        // Add camera controller for WASD navigation
        cam.AddComponent<EditorCameraController>();
        cam.AddComponent<CameraControlsUI>();
        
        // Add debug manager to scene
        var debugManager = new GameObject("TrainArenaDebugManager");
        debugManager.AddComponent<TrainArenaDebugManager>();
        
        // Add time scale manager for training speed monitoring
        var timeManager = new GameObject("TimeScaleManager");
        timeManager.AddComponent<TimeScaleManager>();

        if (agentType == AgentType.Ragdoll)
        {
            // Add Domain Randomization UI for ragdoll physics testing
            var domainUI = new GameObject("DomainRandomizationUI");
            domainUI.AddComponent<DomainRandomizationUI>();
        }

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
        var agentTypeStr = agentType == AgentType.Cube ? "Cube" : "Ragdoll";
        var sceneTypeStr = sceneType == SceneType.Training ? "Training" : "Test";
        var envManagerName = $"{agentTypeStr}{sceneTypeStr}EnvManager";
        var manager = new GameObject(envManagerName);
        var init = manager.AddComponent<EnvInitializer>();

        // Configure based on scene type
        if (sceneType == SceneType.Testing)
        {
            // Test scene configuration: 2x2 grid with more obstacles
            init.EnvCountX = 2;
            init.EnvCountZ = 2;
            init.ObstaclesPerArena = 6;

            if (agentType == AgentType.Ragdoll)
            {
                init.ArenaHelper.AgentSpawnHeight = 1.5f; // Ragdolls are a lot taller than cubes
            }
        }
        // Training scene uses default settings (configured in EnvInitializer)

        // Prefabs (create basic ones procedurally - be sure to disable after spawning the environment)
        if (agentType == AgentType.Cube)
        {
            init.agentPrefab = CreateCubeAgentPrefab(init, sceneType);
        }
        else
        {
            init.agentPrefab = CreateRagdollAgentPrefab(init, sceneType);
        }
        init.goalPrefab = PrimitiveBuilder.CreateGoal(init.ArenaHelper.GoalHeight);
        init.obstaclePrefab = PrimitiveBuilder.CreateObstacle(init.ArenaHelper.ObstacleHeight);

        // Ensure ML-Agents Academy is initialized (singleton pattern)
        if (Academy.Instance == null)
        {
            // Academy.Instance automatically creates the Academy if it doesn't exist
            var academy = Academy.Instance;
            TrainArenaDebugManager.Log("✅ Academy initialized for scene", TrainArenaDebugManager.DebugLogLevel.Important);
        }

        // Apply scene enhancements (post-processing, skybox, lighting, camera settings)
        SceneEnhancer.EnhanceScene(camera, isRagdollScene: agentType == AgentType.Ragdoll);
        SceneEnhancer.ApplyCameraPrefabSettings(camera);
        
        // Auto-apply newest cube model if available
        ModelManager.ApplyNewestModelToAgents(agentTypeStr);

        TrainArenaDebugManager.Log($"🎯 {agentTypeStr} {sceneTypeStr} created with comprehensive enhancements!", TrainArenaDebugManager.DebugLogLevel.Important);
        TrainArenaDebugManager.Log("🎮 Usage: Press Play to simulate (press 'H' for debug controls), or start training via mlagents-learn", TrainArenaDebugManager.DebugLogLevel.Important);
    }

    #region Scene Builder Menu Functions

    [MenuItem("Tools/ML Hack/SceneBuilder/Build Cube Training Scene")]
    public static void BuildCubeTrainingScene()
    {
        BuildSceneInternal(SceneType.Training, AgentType.Cube);
    }
    
    [MenuItem("Tools/ML Hack/SceneBuilder/Build Cube Test Scene")]
    public static void BuildCubeTestScene()
    {
        BuildSceneInternal(SceneType.Testing, AgentType.Cube);
    }

    [MenuItem("Tools/ML Hack/SceneBuilder/Build Ragdoll Training Scene")]
    public static void BuildRagdollTrainingScene()
    {
        BuildSceneInternal(SceneType.Training, AgentType.Ragdoll);
    }
    
    [MenuItem("Tools/ML Hack/SceneBuilder/Build Ragdoll Test Scene")]
    public static void BuildRagdollTestScene()
    {
        BuildSceneInternal(SceneType.Testing, AgentType.Ragdoll);
    }

    #endregion Scene Builder Menu Functions

    #region Create Agent Prefabs

    static GameObject CreateAgentPrefab(EnvInitializer init, AgentType agentType, SceneType sceneType)
    {
        //TODO: Use the configured names instead of hardcoding
        GameObject agentObject = agentType == AgentType.Cube 
            ? PrimitiveBuilder.CreateCubeAgent("CubeAgent") 
            : PrimitiveBuilder.CreateRagdoll("RagdollAgent");

        BaseTrainArenaAgent agent = agentType == AgentType.Cube 
            ? agentObject.GetComponentInChildren<CubeAgent>() 
            : agentObject.GetComponentInChildren<RagdollAgent>();

        // Set obstacle mask to everything for tag-based detection
        agent.obstacleMask = -1;

        // ML-Agents Agent class automatically adds BehaviorParameters - configure it
        var behaviorParams = agent.GetComponentInChildren<Unity.MLAgents.Policies.BehaviorParameters>();
        if (behaviorParams != null)
        {
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
            
            // Configure observation space dynamically based on agent configuration
            int totalObservations = agent.GetTotalObservationCount();
            if (behaviorParams.BrainParameters.VectorObservationSize != totalObservations)
            {
                behaviorParams.BrainParameters.VectorObservationSize = totalObservations;
            }
            
            string behaviorMode = behaviorParams.BehaviorType == Unity.MLAgents.Policies.BehaviorType.Default ? "ML Training" : "Editor Testing";
            
            // Log configuration with agent-specific details
            if (agentType == AgentType.Cube)
            {
                var cubeAgent = agent as CubeAgent;
                TrainArenaDebugManager.Log($"Configured CubeAgent: {behaviorParams.BrainParameters.ActionSpec.NumContinuousActions} actions, {behaviorParams.BrainParameters.VectorObservationSize} observations " +
                                         $"({CubeAgent.VELOCITY_OBSERVATIONS} velocity + {CubeAgent.GOAL_OBSERVATIONS} goal + {cubeAgent.raycastDirections} raycasts), Mode: {behaviorMode}", 
                                         TrainArenaDebugManager.DebugLogLevel.Important);
            }
            else
            {
                var ragdollAgent = agent as RagdollAgent;
                TrainArenaDebugManager.Log($"Configured RagdollAgent: {behaviorParams.BrainParameters.ActionSpec.NumContinuousActions} actions, {behaviorParams.BrainParameters.VectorObservationSize} observations " +
                                         $"({RagdollAgent.UPRIGHTNESS_OBSERVATIONS} uprightness + {RagdollAgent.VELOCITY_OBSERVATIONS} velocity + {ragdollAgent.joints.Count * RagdollAgent.JOINT_STATE_OBSERVATIONS_PER_JOINT} joint states), Mode: {behaviorMode}", 
                                         TrainArenaDebugManager.DebugLogLevel.Important);
            }
        }

        //NOTE: agent.gameObject != agentObject (ragdoll agent is on pelvis, not root)
        
        // Add DecisionRequester for automatic ML-Agents decision scheduling (consistent with RagdollAgent)
        var decisionRequester = agent.gameObject.AddComponent<Unity.MLAgents.DecisionRequester>();
        decisionRequester.DecisionPeriod = 5;  // Request decisions every 5 fixed updates (matches typical setup)
        decisionRequester.TakeActionsBetweenDecisions = true;  // Allow actions between decisions
        
        TrainArenaDebugManager.Log($"Added DecisionRequester with period {decisionRequester.DecisionPeriod} (automatic ML-Agents decisions)", 
                                 TrainArenaDebugManager.DebugLogLevel.Important);
                
        // Add automatic behavior switching only for training scenes
        if (sceneType == SceneType.Training)
        {
            var autoSwitcher = agent.gameObject.AddComponent<AutoBehaviorSwitcher>();
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

        return agentObject;
    }

    static GameObject CreateCubeAgentPrefab(EnvInitializer init, SceneType sceneType = SceneType.Training)
    {
        return CreateAgentPrefab(init, AgentType.Cube, sceneType);
    }

    static GameObject CreateRagdollAgentPrefab(EnvInitializer init, SceneType sceneType = SceneType.Training)
    {
        return CreateAgentPrefab(init, AgentType.Ragdoll, sceneType);
    }

    #endregion Create Agent Prefabs
}