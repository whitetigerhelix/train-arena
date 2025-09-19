using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents.Policies;

/// <summary>
/// Comprehensive ML-Agents training dashboard
/// Combines scene building, model management, and training workflow
/// </summary>
public class TrainingDashboard : EditorWindow
{
    private enum TabMode { SceneBuilder, ModelManager, Training, Settings }
    private TabMode currentTab = TabMode.SceneBuilder;
    
    private Vector2 scrollPosition;
    private List<ModelManager.ModelInfo> availableModels;
    private bool autoRefresh = true;
    private double lastRefreshTime;
    
    // Scene Builder settings
    private int cubeArenaCount = 16;
    private int ragdollArenaCount = 4;
    private bool includeCameraPrefab = true;
    
    [MenuItem("Tools/ML Hack/Training Dashboard")]
    public static void ShowWindow()
    {
        var window = GetWindow<TrainingDashboard>("ML Training Dashboard");
        window.minSize = new Vector2(500, 400);
        window.Show();
    }
    
    void OnEnable()
    {
        RefreshModels();
        lastRefreshTime = EditorApplication.timeSinceStartup;
    }
    
    void OnGUI()
    {
        DrawTabs();
        
        // Auto refresh models every 5 seconds
        if (autoRefresh && EditorApplication.timeSinceStartup - lastRefreshTime > 5.0)
        {
            RefreshModels();
            lastRefreshTime = EditorApplication.timeSinceStartup;
        }
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        switch (currentTab)
        {
            case TabMode.SceneBuilder:
                DrawSceneBuilderTab();
                break;
            case TabMode.ModelManager:
                DrawModelManagerTab();
                break;
            case TabMode.Training:
                DrawTrainingTab();
                break;
            case TabMode.Settings:
                DrawSettingsTab();
                break;
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    void DrawTabs()
    {
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Toggle(currentTab == TabMode.SceneBuilder, "Scene Builder", "Button"))
            currentTab = TabMode.SceneBuilder;
        if (GUILayout.Toggle(currentTab == TabMode.ModelManager, "Models", "Button"))
            currentTab = TabMode.ModelManager;
        if (GUILayout.Toggle(currentTab == TabMode.Training, "Training", "Button"))
            currentTab = TabMode.Training;
        if (GUILayout.Toggle(currentTab == TabMode.Settings, "Settings", "Button"))
            currentTab = TabMode.Settings;
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
    }
    
    void DrawSceneBuilderTab()
    {
        EditorGUILayout.LabelField("🏗️ Scene Builder", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Quick scene creation
        EditorGUILayout.LabelField("Quick Scene Creation", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("🎯 Build Cube Arena", GUILayout.Height(40)))
        {
            SceneBuilder.BuildCubeTrainingScene();//TODO: cubeArenaCount, includeCameraPrefab);
        }
        if (GUILayout.Button("🎭 Build Ragdoll Arena", GUILayout.Height(40)))
        {
            SceneBuilder.BuildRagdollTrainingScene();//TODO: ragdollArenaCount, includeCameraPrefab);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Settings
        EditorGUILayout.LabelField("Scene Settings", EditorStyles.boldLabel);
        cubeArenaCount = EditorGUILayout.IntSlider("Cube Arenas", cubeArenaCount, 1, 64);
        ragdollArenaCount = EditorGUILayout.IntSlider("Ragdoll Arenas", ragdollArenaCount, 1, 16);
        includeCameraPrefab = EditorGUILayout.Toggle("Include Camera Prefab", includeCameraPrefab);
        
        EditorGUILayout.Space();
        
        // Current scene info
        DrawCurrentSceneInfo();
    }
    
    void DrawModelManagerTab()
    {
        EditorGUILayout.LabelField("🤖 Model Manager", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Controls
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh Models"))
        {
            RefreshModels();
        }
        if (GUILayout.Button("Open Models Folder"))
        {
            ModelManager.OpenModelsFolder();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Quick actions
        EditorGUILayout.LabelField("Quick Apply", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Apply Newest Cube Model"))
        {
            ModelManager.ApplyNewestModelToAgents("Cube");
        }
        if (GUILayout.Button("Apply Newest Ragdoll Model"))
        {
            ModelManager.ApplyNewestModelToAgents("Ragdoll");
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Available models
        if (availableModels == null || availableModels.Count == 0)
        {
            EditorGUILayout.HelpBox("No models found. Place .onnx files in Assets/ML-Agents/Models/", MessageType.Info);
            return;
        }
        
        EditorGUILayout.LabelField($"Available Models ({availableModels.Count})", EditorStyles.boldLabel);
        
        foreach (var model in availableModels.Take(10)) // Show first 10
        {
            EditorGUILayout.BeginHorizontal("box");
            
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(model.fileName, EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"{model.agentType} | {model.lastModified:MM/dd HH:mm}");
            EditorGUILayout.EndVertical();
            
            if (GUILayout.Button($"Apply", GUILayout.Width(60)))
            {
                ModelManager.ApplyModelToAgents(model);
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        if (availableModels.Count > 10)
        {
            EditorGUILayout.LabelField($"... and {availableModels.Count - 10} more models");
        }
    }
    
    void DrawTrainingTab()
    {
        EditorGUILayout.LabelField("🏃 Training Workflow", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Prepare training runs
        EditorGUILayout.LabelField("Prepare Training Run", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("🎯 Prepare Cube Training", GUILayout.Height(35)))
        {
            TrainingWorkflow.PrepareCubeTrainingRun();
        }
        if (GUILayout.Button("🎭 Prepare Ragdoll Training", GUILayout.Height(35)))
        {
            TrainingWorkflow.PrepareRagdollTrainingRun();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Folder shortcuts
        EditorGUILayout.LabelField("Quick Access", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Training Runs Folder"))
        {
            TrainingWorkflow.OpenTrainingRunsFolder();
        }
        if (GUILayout.Button("Models Folder"))
        {
            ModelManager.OpenModelsFolder();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Training instructions
        EditorGUILayout.LabelField("Training Instructions", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "1. Build your training scene using Scene Builder tab\\n" +
            "2. Prepare a training run to create config files\\n" +
            "3. Run training: mlagents-learn [config.yaml] --run-id=[your-run-id]\\n" +
            "4. After training, copy .onnx files to Models folder\\n" +
            "5. Use Model Manager to apply trained models to agents", 
            MessageType.Info);
        
        // Training command examples
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Example Commands", EditorStyles.boldLabel);
        
        var cubeCommand = "mlagents-learn Assets/ML-Agents/TrainingRuns/CubeAgent_*/config/CubeAgent_config.yaml --run-id=CubeAgent_MyRun";
        var ragdollCommand = "mlagents-learn Assets/ML-Agents/TrainingRuns/RagdollAgent_*/config/RagdollAgent_config.yaml --run-id=RagdollAgent_MyRun";
        
        EditorGUILayout.SelectableLabel(cubeCommand, EditorStyles.textField, GUILayout.Height(20));
        EditorGUILayout.SelectableLabel(ragdollCommand, EditorStyles.textField, GUILayout.Height(20));
    }
    
    void DrawSettingsTab()
    {
        EditorGUILayout.LabelField("⚙️ Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Model Manager Settings
        EditorGUILayout.LabelField("Model Manager", EditorStyles.boldLabel);
        autoRefresh = EditorGUILayout.Toggle("Auto Refresh Models", autoRefresh);
        
        EditorGUILayout.Space();
        
        // Debug Settings
        EditorGUILayout.LabelField("Debug Settings", EditorStyles.boldLabel);
        if (GUILayout.Button("Clear Console"))
        {
            var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
            var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            clearMethod?.Invoke(null, null);
        }
        
        EditorGUILayout.Space();
        
        // System Info
        EditorGUILayout.LabelField("System Info", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Unity Version: {Application.unityVersion}");
        EditorGUILayout.LabelField($"Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        
        var agentCount = Object.FindObjectsByType<BehaviorParameters>(FindObjectsSortMode.None).Length;
        EditorGUILayout.LabelField($"ML-Agents in Scene: {agentCount}");
    }
    
    void DrawCurrentSceneInfo()
    {
        EditorGUILayout.LabelField("Current Scene Info", EditorStyles.boldLabel);
        
        var cubeAgents = Object.FindObjectsByType<CubeAgent>(FindObjectsSortMode.None);
        var ragdollAgents = Object.FindObjectsByType<RagdollAgent>(FindObjectsSortMode.None);
        var behaviorParams = Object.FindObjectsByType<BehaviorParameters>(FindObjectsSortMode.None);
        
        EditorGUILayout.LabelField($"Cube Agents: {cubeAgents.Length}");
        EditorGUILayout.LabelField($"Ragdoll Agents: {ragdollAgents.Length}");
        EditorGUILayout.LabelField($"Total ML-Agents: {behaviorParams.Length}");
        
        // Model status
        var modelsAssigned = behaviorParams.Count(bp => bp.Model != null);
        var modelsForInference = behaviorParams.Count(bp => bp.BehaviorType == BehaviorType.InferenceOnly);
        
        EditorGUILayout.LabelField($"Models Assigned: {modelsAssigned}/{behaviorParams.Length}");
        EditorGUILayout.LabelField($"Inference Mode: {modelsForInference}/{behaviorParams.Length}");
    }
    
    void RefreshModels()
    {
        availableModels = ModelManager.GetAvailableModels();
        Repaint();
    }
}