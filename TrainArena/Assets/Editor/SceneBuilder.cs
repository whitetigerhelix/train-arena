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

        // Camera with controller for navigation
        var cam = new GameObject("Main Camera");
        var camera = cam.AddComponent<Camera>();
        cam.tag = "MainCamera";
        // Position camera to view 4x4 grid: arenas at (0,0), (20,0), (40,0), (60,0), etc.
        // Center of grid is at (30, 0, 30) - halfway between (0,0) and (60,60)
        cam.transform.position = new Vector3(30, 40, -20); 
        cam.transform.LookAt(new Vector3(30, 0, 30)); // Look at center of 4x4 arena grid
        camera.clearFlags = CameraClearFlags.Skybox;
        
        // Add camera controller for WASD navigation
        cam.AddComponent<EditorCameraController>();
        
        // Add debug manager to scene
        var debugManager = new GameObject("TrainArenaDebugManager");
        debugManager.AddComponent<TrainArenaDebugManager>();

        // Light
        var lightGO = new GameObject("Directional Light");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);

        // Manager
        var manager = new GameObject("EnvManager");
        var init = manager.AddComponent<EnvInitializer>();

        // Prefabs (create basic ones procedurally - be sure to disable after spawning the environment)
        init.cubeAgentPrefab = CreateCubeAgentPrefab();
        init.goalPrefab = CreateGoalPrefab();
        init.obstaclePrefab = CreateObstaclePrefab();

        TrainArenaDebugManager.Log("Cube training scene created. Press Play to simulate (press 'H' for debug controls), or start training via mlagents-learn.", TrainArenaDebugManager.DebugLogLevel.Important);
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

        // Ground
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.transform.localScale = Vector3.one;
        ground.name = "Ground";
        
        // Unity 6.2 compatible ground material
        Material groundMat = CreateCompatibleMaterial();
        groundMat.color = Color.white;
        groundMat.name = "GroundMaterial";
        ground.GetComponent<Renderer>().material = groundMat;

        Debug.Log("Ragdoll test scene created. Build your ragdoll and add RagdollAgent + PDJointController components.");
    }

    static GameObject CreateCubeAgentPrefab()
    {
        var agent = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.DestroyImmediate(agent.GetComponent<Collider>());
        agent.name = "CubeAgent";
        
        // Set up material for the agent
        var mr = agent.GetComponent<Renderer>();
        Material mat = CreateCompatibleMaterial();
        mat.color = Color.blue;
        mat.name = "AgentMaterial";
        mr.material = mat;
        
        // Use BoxCollider to match cube shape perfectly
        var col = agent.AddComponent<BoxCollider>();
        col.center = Vector3.zero; // Centered on the cube
        col.size = Vector3.one; // 1x1x1 cube

        var rb = agent.AddComponent<Rigidbody>();
        rb.mass = 1f;

        var cubeAgent = agent.AddComponent<CubeAgent>();
        // Set obstacle mask to everything for tag-based detection
        cubeAgent.obstacleMask = -1;
        
        // ML-Agents Agent class automatically adds BehaviorParameters - configure it
        var behaviorParams = agent.GetComponent<Unity.MLAgents.Policies.BehaviorParameters>();
        if (behaviorParams != null)
        {
            behaviorParams.BehaviorName = "CubeAgent";
            behaviorParams.BehaviorType = IsMLAgentsTrainingActive() ? 
                Unity.MLAgents.Policies.BehaviorType.Default : 
                Unity.MLAgents.Policies.BehaviorType.HeuristicOnly;
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
        
        // Add simple face to show agent orientation
        AddAgentFace(agent);
        
        // Add blinking animation for visual polish
        agent.AddComponent<EyeBlinker>();
        
        // Add debug info component for development
        agent.AddComponent<AgentDebugInfo>();

        return agent;
    }

    static GameObject CreateGoalPrefab()
    {
        var goal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        goal.transform.localScale = Vector3.one * 0.6f;
        var mr = goal.GetComponent<Renderer>();
        
        // Unity 6.2 compatible material creation
        Material mat = CreateCompatibleMaterial();
        mat.color = Color.yellow;
        mat.name = "GoalMaterial";
        mr.material = mat;
        
        goal.name = "Goal";
        return goal;
    }

    static GameObject CreateObstaclePrefab()
    {
        var obs = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obs.transform.localScale = new Vector3(0.8f, 1.0f, 0.8f);
        var mr = obs.GetComponent<Renderer>();
        
        // Unity 6.2 compatible material creation
        Material mat = CreateCompatibleMaterial();
        mat.color = Color.red;
        mat.name = "ObstacleMaterial";
        mr.material = mat; // Use instance material, not shared
        
        obs.name = "Obstacle";
        
        // Ensure Obstacle tag exists
        EnsureTagExists("Obstacle");
        obs.tag = "Obstacle";
        
        return obs;
    }
    
    /// <summary>
    /// Creates a material compatible with Unity 6.2, handling URP/Built-in pipeline differences
    /// </summary>
    static Material CreateCompatibleMaterial()
    {
        // Try URP Lit shader first (Unity 6.2 default), fall back to Standard for Built-in
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }
        if (shader == null)
        {
            // Final fallback to built-in unlit
            shader = Shader.Find("Unlit/Color");
        }
        
        return new Material(shader);
    }
    
    /// <summary>
    /// Ensures a tag exists in Unity's TagManager, Unity 6.2 compatible
    /// </summary>
    static void EnsureTagExists(string tagName)
    {
        // First check if tag already exists
        if (UnityEditorInternal.InternalEditorUtility.tags.Contains(tagName))
            return;
            
        // Add the tag using Unity's built-in method
        UnityEditorInternal.InternalEditorUtility.AddTag(tagName);
    }
    
    /// <summary>
    /// Adds simple "eyes" to show agent forward direction
    /// </summary>
    static void AddAgentFace(GameObject agent)
    {
        // Create left eye - positioned ON the cube surface, not inside
        var leftEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leftEye.name = "LeftEye";
        leftEye.transform.parent = agent.transform;
        leftEye.transform.localPosition = new Vector3(-0.2f, 0.2f, 0.51f); // Outside cube surface (0.5 + 0.01 margin)
        leftEye.transform.localScale = Vector3.one * 0.15f; // Slightly larger for visibility
        
        // Create right eye  
        var rightEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rightEye.name = "RightEye";
        rightEye.transform.parent = agent.transform;
        rightEye.transform.localPosition = new Vector3(0.2f, 0.2f, 0.51f); // Outside cube surface
        rightEye.transform.localScale = Vector3.one * 0.15f;
        
        // Make eyes white/black for visibility
        var eyeMaterial = CreateCompatibleMaterial();
        eyeMaterial.color = Color.white;
        eyeMaterial.name = "EyeMaterial";
        
        leftEye.GetComponent<Renderer>().material = eyeMaterial;
        rightEye.GetComponent<Renderer>().material = eyeMaterial;
        
        // Remove colliders from eyes so they don't interfere with physics
        Object.DestroyImmediate(leftEye.GetComponent<Collider>());
        Object.DestroyImmediate(rightEye.GetComponent<Collider>());
    }
}