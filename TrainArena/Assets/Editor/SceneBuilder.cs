using UnityEditor;
using UnityEngine;

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
        cam.transform.position = new Vector3(10, 12, -10);
        cam.transform.LookAt(Vector3.zero);
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
        ground.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Standard"));

        Debug.Log("Ragdoll test scene created. Build your ragdoll and add RagdollAgent + PDJointController components.");
    }

    static GameObject CreateCubeAgentPrefab()
    {
        var agent = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.DestroyImmediate(agent.GetComponent<Collider>());
        agent.name = "CubeAgent";
        var col = agent.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0, 0.5f, 0);
        col.height = 1f;
        col.radius = 0.4f;

        var rb = agent.AddComponent<Rigidbody>();
        rb.mass = 1f;

        agent.AddComponent<CubeAgent>();

        return agent;
    }

    static GameObject CreateGoalPrefab()
    {
        var goal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        goal.transform.localScale = Vector3.one * 0.6f;
        var mr = goal.GetComponent<Renderer>();
        mr.sharedMaterial = new Material(Shader.Find("Standard"));
        mr.sharedMaterial.color = Color.yellow;
        goal.name = "Goal";
        return goal;
    }

    static GameObject CreateObstaclePrefab()
    {
        var obs = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obs.transform.localScale = new Vector3(0.8f, 1.0f, 0.8f);
        var mr = obs.GetComponent<Renderer>();
        mr.sharedMaterial = new Material(Shader.Find("Standard"));
        mr.sharedMaterial.color = Color.red;
        obs.name = "Obstacle";
        obs.tag = "Obstacle";
        return obs;
    }
}