using UnityEditor;
using UnityEngine;
using System.Linq;

public static class SceneBuilder
{
    [MenuItem("Tools/ML Hack/Build Cube Training Scene")]
    public static void BuildCubeScene()
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);

        // Camera
        var cam = new GameObject("Main Camera");
        var camera = cam.AddComponent<Camera>();
        cam.tag = "MainCamera";
        cam.transform.position = new Vector3(20, 12, -10);
        cam.transform.rotation = Quaternion.Euler(40, 0, 0);
        camera.clearFlags = CameraClearFlags.Skybox;

        // Light
        var lightGO = new GameObject("Directional Light");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);

        // Manager
        var manager = new GameObject("EnvManager");
        var init = manager.AddComponent<EnvInitializer>();

        // Prefabs (create basic ones procedurally)
        init.cubeAgentPrefab = CreateCubeAgentPrefab();
        init.goalPrefab = CreateGoalPrefab();
        init.obstaclePrefab = CreateObstaclePrefab();

        Debug.Log("Cube training scene created. Press Play to simulate, or start training via mlagents-learn.");
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
        
        var col = agent.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0, 0.5f, 0);
        col.height = 1f;
        col.radius = 0.4f;

        var rb = agent.AddComponent<Rigidbody>();
        rb.mass = 1f;

        var cubeAgent = agent.AddComponent<CubeAgent>();
        // Set obstacle mask to everything for tag-based detection
        cubeAgent.obstacleMask = -1;
        
        // Add ML-Agents Behavior Parameters for proper agent behavior
        var behaviorParams = agent.AddComponent<Unity.MLAgents.Policies.BehaviorParameters>();
        behaviorParams.BehaviorName = "CubeAgent";
        behaviorParams.BehaviorType = Unity.MLAgents.Policies.BehaviorType.HeuristicOnly; // Use heuristic for sanity check
        behaviorParams.TeamId = 0;
        behaviorParams.UseChildSensors = true;

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
}