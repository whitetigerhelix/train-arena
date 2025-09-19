using UnityEditor;
using UnityEngine;

public static class TagSceneBuilder
{
    [MenuItem("Tools/ML Hack/Build Tag Arena Scene")]
    public static void BuildTagScene()
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);

        // Camera
        var cam = new GameObject("Main Camera");
        cam.AddComponent<Camera>();
        cam.tag = "MainCamera";
        cam.transform.position = new Vector3(20, 22, -20);
        cam.transform.LookAt(Vector3.zero);

        // Light
        var lightGO = new GameObject("Directional Light");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);

        // Prefabs
        var runnerPrefab = CreateRunnerPrefab();
        var taggerPrefab = CreateTaggerPrefab();

        // Arena builder
        var arena = new GameObject("TagArena");
        var builder = arena.AddComponent<TagArenaBuilder>();
        builder.runnerPrefab = runnerPrefab;
        builder.taggerPrefab = taggerPrefab;

        Debug.Log("Tag arena scene created. Train Runner agent via runner_ppo.yaml; Tagger is heuristic.");
    }

    static GameObject CreateRunnerPrefab()
    {
        var go = PrimitiveBuilder.CreateCubeAgent("Runner", Vector3.zero, Color.blue);

        var agent = go.AddComponent<RunnerAgent>();
        var bp = go.AddComponent<Unity.MLAgents.Policies.BehaviorParameters>();
        bp.BehaviorName = "RunnerAgent";
        bp.BrainParameters.VectorObservationSize = 3 + 3 + 3 + 8; // coarse hint; will be auto-handled by sensors
        bp.BrainParameters.NumStackedVectorObservations = 1;
        bp.BehaviorType = Unity.MLAgents.Policies.BehaviorType.Default;

        go.AddComponent<ModelSwitcher>(); // lets UI toggle Random/Heuristic/Inference

        // Reward HUD anchor (bars can be created in a separate canvas; here we just keep reference empty)
        return go;
    }

    static GameObject CreateTaggerPrefab()
    {
        var go = PrimitiveBuilder.CreateCubeAgent("Tagger", Vector3.zero, Color.red);
        go.AddComponent<HeuristicTagger>();
        return go;
    }
}