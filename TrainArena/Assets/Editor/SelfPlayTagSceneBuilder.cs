using UnityEditor;
using UnityEngine;
using Unity.MLAgents.Policies;

public static class SelfPlayTagSceneBuilder
{
    [MenuItem("Tools/ML Hack/Build Self-Play Tag Scene")]
    public static void Build()
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);

        // Camera
        var cam = new GameObject("Main Camera");
        cam.AddComponent<Camera>();
        cam.tag = "MainCamera";
        cam.transform.position = new Vector3(18, 20, -18);
        cam.transform.LookAt(Vector3.zero);

        // Light
        var lightGO = new GameObject("Directional Light");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);

        // Ground
        var ground = PrimitiveBuilder.CreateGround(Vector3.zero, Vector3.one * 1.5f, new Color(0.788f, 0.788f, 0.788f));

        // Domain randomization + UI
        var dom = new GameObject("DomainRandomizer").AddComponent<DomainRandomizer>();
        var ui = new GameObject("DomainUI").AddComponent<DomainRandomizationUI>();
        ui.randomizer = dom;

        // Runner
        var runner = PrimitiveBuilder.CreateCubeAgent("Runner", new Vector3(-2, 0.5f, 0), Color.blue);
        var runnerAgent = runner.AddComponent<RunnerAgent>();
        var rBP = runner.AddComponent<BehaviorParameters>();
        rBP.BehaviorName = "RunnerAgent";
        rBP.BehaviorType = BehaviorType.Default;

        // Tagger (trainable)
        var tagger = PrimitiveBuilder.CreateCubeAgent("Tagger", new Vector3(2, 0.5f, 0), Color.red);
        var taggerAgent = tagger.AddComponent<TaggerAgentTrainable>();
        var tBP = tagger.AddComponent<BehaviorParameters>();
        tBP.BehaviorName = "TaggerAgent";
        tBP.BehaviorType = BehaviorType.Default;

        runnerAgent.tagger = tagger.transform;
        taggerAgent.runner = runner.transform;

        Debug.Log("Self-Play Tag scene created. Train both behaviors with selfplay_combo.yaml");
    }
}