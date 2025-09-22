using System.Linq;
using TrainArena.Core;
using TrainArena.Configuration;
using Unity.MLAgents;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using static Codice.Client.Common.WebApi.WebApiEndpoints;
using static PrimitiveBuilder;

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
/// - TrainArena/Scenes/Build Cube Training Scene: 4x4 cube agent training environment
/// - TrainArena/Scenes/Build Cube Test Scene: 2x2 cube agent testing environment  
/// - TrainArena/Scenes/Build Ragdoll Training Scene: 2x2 ragdoll agent training environment
/// - TrainArena/Scenes/Build Ragdoll Test Scene: 1x1 ragdoll agent testing environment
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

    static void BuildSceneInternal(SceneType sceneType, AgentType agentType)
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);

        // Camera with controller for navigation - optimized for 16:9 and isometric view
        var cam = new GameObject("Main Camera");
        var camera = cam.AddComponent<Camera>();
        cam.tag = "MainCamera";
        
        // Use centralized configuration for camera positioning
        if (agentType == AgentType.Cube)
        {
            cam.transform.position = SceneConfiguration.Camera.CubeAgent.Position;
            cam.transform.LookAt(SceneConfiguration.Camera.CubeAgent.LookAtTarget);
            camera.fieldOfView = SceneConfiguration.Camera.DefaultFieldOfView;
        }
        else
        {   
            cam.transform.position = SceneConfiguration.Camera.RagdollAgent.Position;
            cam.transform.LookAt(SceneConfiguration.Camera.RagdollAgent.LookAtTarget);
            camera.fieldOfView = SceneConfiguration.Camera.RagdollFieldOfView;
        }
        
        // Use configuration for camera settings
        camera.aspect = SceneConfiguration.Camera.AspectRatio;
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

        // Light - Enhanced setup with soft shadows using centralized configuration
        var lightGO = new GameObject("Directional Light");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = SceneConfiguration.Lighting.DirectionalLightColor;
        light.intensity = SceneConfiguration.Lighting.DirectionalLightIntensity;
        lightGO.transform.rotation = Quaternion.Euler(SceneConfiguration.Lighting.DirectionalLightRotation);
        
        // Enhanced lighting settings for better quality
        light.shadows = LightShadows.Soft;
        light.shadowStrength = 0.6f;
        light.shadowResolution = UnityEngine.Rendering.LightShadowResolution.Medium;

        // Manager
        var agentTypeStr = agentType == AgentType.Cube ? "Cube" : "Ragdoll";
        var sceneTypeStr = sceneType == SceneType.Training ? "Training" : "Test";
        var envManagerName = $"{agentTypeStr}{sceneTypeStr}EnvManager";
        var manager = new GameObject(envManagerName);
        var init = manager.AddComponent<EnvInitializer>();

        // Configure based on scene type using centralized configuration
        if (sceneType == SceneType.Testing)
        {
            // Test scene configuration with more obstacles
            if (agentType == AgentType.Cube)
            {
                init.EnvCountX = SceneConfiguration.Layout.GridDimensions.CubeTestingX;
                init.EnvCountZ = SceneConfiguration.Layout.GridDimensions.CubeTestingZ;
            }
            else
            {
                init.EnvCountX = SceneConfiguration.Layout.GridDimensions.RagdollTestingX;
                init.EnvCountZ = SceneConfiguration.Layout.GridDimensions.RagdollTestingZ;
                init.ArenaHelper.AgentSpawnHeight = 1.5f; // Ragdolls are taller than cubes
            }
            init.ObstaclesPerArena = 6;
        }
        else // Training scene
        {
            // Training scene configuration
            if (agentType == AgentType.Cube)
            {
                init.EnvCountX = SceneConfiguration.Layout.GridDimensions.CubeTrainingX;
                init.EnvCountZ = SceneConfiguration.Layout.GridDimensions.CubeTrainingZ;
            }
            else
            {
                init.EnvCountX = SceneConfiguration.Layout.GridDimensions.RagdollTrainingX;
                init.EnvCountZ = SceneConfiguration.Layout.GridDimensions.RagdollTrainingZ;
                init.ArenaHelper.AgentSpawnHeight = 1.5f; // Ragdolls are taller than cubes
            }
        }

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

    [MenuItem("TrainArena/Scenes/Build Cube Training Scene")]
    public static void BuildCubeTrainingScene()
    {
        BuildSceneInternal(SceneType.Training, AgentType.Cube);
    }
    
    [MenuItem("TrainArena/Scenes/Build Cube Test Scene")]
    public static void BuildCubeTestScene()
    {
        BuildSceneInternal(SceneType.Testing, AgentType.Cube);
    }

    [MenuItem("TrainArena/Scenes/Build Ragdoll Training Scene")]
    public static void BuildRagdollTrainingScene()
    {
        BuildSceneInternal(SceneType.Training, AgentType.Ragdoll);
    }
    
    [MenuItem("TrainArena/Scenes/Build Ragdoll Test Scene")]
    public static void BuildRagdollTestScene()
    {
        BuildSceneInternal(SceneType.Testing, AgentType.Ragdoll);
    }

    #endregion Scene Builder Menu Functions

    #region Create Agent Prefabs

    static GameObject CreateAgentPrefab(EnvInitializer init, AgentType agentType, SceneType sceneType)
    {
        // Use centralized configuration for agent names
        GameObject agentObject = agentType == AgentType.Cube 
            ? PrimitiveBuilder.CreateCubeAgent(SceneConfiguration.AgentPrefabs.CubeAgentPrefabName) 
            : PrimitiveBuilder.CreateRagdoll(SceneConfiguration.AgentPrefabs.RagdollAgentPrefabName);

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
            
            // Set behavior name to match training configuration
            if (agentType == AgentType.Cube)
            {
                behaviorParams.BehaviorName = AgentConfiguration.CubeAgent.BehaviorName;  // "CubeAgent"
            }
            else
            {
                behaviorParams.BehaviorName = AgentConfiguration.RagdollAgent.BehaviorName;  // "RagdollAgent"
            }
            
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