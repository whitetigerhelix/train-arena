using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Unity.MLAgents.Policies;
using Unity.Barracuda;

/// <summary>
/// Automated ML-Agents model management system
/// Handles model detection, selection, and automatic assignment to agents
/// </summary>
public static class ModelManager
{
    public const string MODELS_FOLDER = "Assets/ML-Agents/Models";
    
    [System.Serializable]
    public class ModelInfo
    {
        public string fileName;
        public string fullPath;
        public string agentType; // "Cube" or "Ragdoll"
        public System.DateTime lastModified;
        public NNModel model;
        
        public string DisplayName => $"{agentType} - {fileName} ({lastModified:MM/dd HH:mm})";
    }
    
    /// <summary>
    /// Scan for all available ML-Agents models
    /// </summary>
    public static List<ModelInfo> GetAvailableModels()
    {
        var models = new List<ModelInfo>();
        
        if (!Directory.Exists(MODELS_FOLDER))
        {
            Directory.CreateDirectory(MODELS_FOLDER);
            TrainArenaDebugManager.Log($"Created models directory: {MODELS_FOLDER}", TrainArenaDebugManager.DebugLogLevel.Important);
            return models;
        }
        
        var onnxFiles = Directory.GetFiles(MODELS_FOLDER, "*.onnx", SearchOption.AllDirectories);
        
        foreach (var filePath in onnxFiles)
        {
            var fileName = Path.GetFileName(filePath);
            var assetPath = filePath.Replace('\\', '/');
            
            // Skip if not in Assets folder (Unity requirement)
            if (!assetPath.StartsWith("Assets/"))
                continue;
                
            var model = AssetDatabase.LoadAssetAtPath<NNModel>(assetPath);
            if (model == null) continue;
            
            var modelInfo = new ModelInfo
            {
                fileName = fileName,
                fullPath = assetPath,
                agentType = DetermineAgentType(fileName),
                lastModified = File.GetLastWriteTime(filePath),
                model = model
            };
            
            models.Add(modelInfo);
        }
        
        // Sort by last modified (newest first)
        models = models.OrderByDescending(m => m.lastModified).ToList();
        
        TrainArenaDebugManager.Log($"🔍 Found {models.Count} ML-Agents models in {MODELS_FOLDER}", TrainArenaDebugManager.DebugLogLevel.Important);
        
        return models;
    }
    
    /// <summary>
    /// Determine agent type from model filename
    /// </summary>
    static string DetermineAgentType(string fileName)
    {
        fileName = fileName.ToLower();
        
        if (fileName.Contains("cube") || fileName.Contains("cubeagent"))
            return "Cube";
        else if (fileName.Contains("ragdoll") || fileName.Contains("ragdollagent"))
            return "Ragdoll";
        else
            return "Unknown";
    }
    
    /// <summary>
    /// Get the newest model for a specific agent type
    /// </summary>
    public static ModelInfo GetNewestModel(string agentType)
    {
        var models = GetAvailableModels();
        return models.FirstOrDefault(m => m.agentType.Equals(agentType, System.StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// Automatically apply the newest model to all agents of a specific type in the scene
    /// </summary>
    public static void ApplyNewestModelToAgents(string agentType)
    {
        var newestModel = GetNewestModel(agentType);
        if (newestModel == null)
        {
            TrainArenaDebugManager.Log($"❌ No {agentType} models found in {MODELS_FOLDER}", TrainArenaDebugManager.DebugLogLevel.Important);
            return;
        }
        
        int agentsUpdated = 0;
        
        // Find all BehaviorParameters in scene
        var behaviorParams = Object.FindObjectsByType<BehaviorParameters>(FindObjectsSortMode.None);
        
        foreach (var bp in behaviorParams)
        {
            bool isMatchingAgent = false;
            
            // Check if this is the right agent type
            if (agentType.Equals("Cube", System.StringComparison.OrdinalIgnoreCase))
            {
                isMatchingAgent = bp.GetComponent<CubeAgent>() != null;
            }
            else if (agentType.Equals("Ragdoll", System.StringComparison.OrdinalIgnoreCase))
            {
                isMatchingAgent = bp.GetComponentInChildren<RagdollAgent>() != null;
            }
            
            if (isMatchingAgent)
            {
                bp.Model = newestModel.model;
                bp.BehaviorType = BehaviorType.InferenceOnly;
                agentsUpdated++;
                
                TrainArenaDebugManager.Log($"🤖 Applied model '{newestModel.fileName}' to {bp.gameObject.name}", TrainArenaDebugManager.DebugLogLevel.Important);
            }
        }
        
        if (agentsUpdated > 0)
        {
            TrainArenaDebugManager.Log($"✅ Updated {agentsUpdated} {agentType} agents with newest model: {newestModel.fileName}", TrainArenaDebugManager.DebugLogLevel.Important);
        }
        else
        {
            TrainArenaDebugManager.Log($"❌ No {agentType} agents found in scene to update", TrainArenaDebugManager.DebugLogLevel.Important);
        }
    }
    
    /// <summary>
    /// Apply specific model to agents by model info
    /// </summary>
    public static void ApplyModelToAgents(ModelInfo modelInfo)
    {
        ApplySpecificModelToAgents(modelInfo.agentType, modelInfo.model, modelInfo.fileName);
    }
    
    /// <summary>
    /// Apply specific model to agents of matching type
    /// </summary>
    static void ApplySpecificModelToAgents(string agentType, NNModel model, string modelName)
    {
        int agentsUpdated = 0;
        var behaviorParams = Object.FindObjectsByType<BehaviorParameters>(FindObjectsSortMode.None);
        
        foreach (var bp in behaviorParams)
        {
            bool isMatchingAgent = false;
            
            if (agentType.Equals("Cube", System.StringComparison.OrdinalIgnoreCase))
            {
                isMatchingAgent = bp.GetComponent<CubeAgent>() != null;
            }
            else if (agentType.Equals("Ragdoll", System.StringComparison.OrdinalIgnoreCase))
            {
                isMatchingAgent = bp.GetComponentInChildren<RagdollAgent>() != null;
            }
            
            if (isMatchingAgent)
            {
                bp.Model = model;
                bp.BehaviorType = BehaviorType.InferenceOnly;
                agentsUpdated++;
            }
        }
        
        TrainArenaDebugManager.Log($"✅ Applied '{modelName}' to {agentsUpdated} {agentType} agents", TrainArenaDebugManager.DebugLogLevel.Important);
    }
    
    /// <summary>
    /// Create standardized model name from training metadata
    /// </summary>
    public static string CreateStandardModelName(string agentType, string runId, System.DateTime timestamp, int steps = 0)
    {
        var timeString = timestamp.ToString("yyyyMMdd_HHmm");
        var stepsString = steps > 0 ? $"_{steps}k" : "";
        return $"{agentType}Agent_{runId}_{timeString}{stepsString}.onnx";
    }
    
    /// <summary>
    /// Menu items for quick model operations
    /// </summary>
    [MenuItem("Tools/ML Hack/Models/Apply Newest Cube Model")]
    public static void ApplyNewestCubeModel()
    {
        ApplyNewestModelToAgents("Cube");
    }
    
    [MenuItem("Tools/ML Hack/Models/Apply Newest Ragdoll Model")]
    public static void ApplyNewestRagdollModel()
    {
        ApplyNewestModelToAgents("Ragdoll");
    }
    
    [MenuItem("Tools/ML Hack/Models/Open Models Folder")]
    public static void OpenModelsFolder()
    {
        if (!Directory.Exists(MODELS_FOLDER))
        {
            Directory.CreateDirectory(MODELS_FOLDER);
        }
        
        EditorUtility.RevealInFinder(MODELS_FOLDER);
    }
    
    [MenuItem("Tools/ML Hack/Models/Show Model Selection Window")]
    public static void ShowModelSelectionWindow()
    {
        ModelSelectionWindow.ShowWindow();
    }
}

/// <summary>
/// Editor window for manual model selection and management
/// </summary>
public class ModelSelectionWindow : EditorWindow
{
    private List<ModelManager.ModelInfo> availableModels;
    private Vector2 scrollPosition;
    private int selectedAgentType = 0; // Changed from string to int
    private readonly string[] agentTypeOptions = { "All", "Cube", "Ragdoll" }; // Added options array
    private bool autoRefresh = true;
    private double lastRefreshTime;
    
    public static void ShowWindow()
    {
        var window = GetWindow<ModelSelectionWindow>("ML-Agents Models");
        window.Show();
    }
    
    void OnEnable()
    {
        RefreshModels();
        lastRefreshTime = EditorApplication.timeSinceStartup;
    }
    
    void OnGUI()
    {
        EditorGUILayout.LabelField("ML-Agents Model Manager", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Controls
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh Models"))
        {
            RefreshModels();
        }
        autoRefresh = EditorGUILayout.Toggle("Auto Refresh", autoRefresh);
        EditorGUILayout.EndHorizontal();
        
        // Filter by agent type
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Filter:", GUILayout.Width(50));
        selectedAgentType = EditorGUILayout.Popup(selectedAgentType, agentTypeOptions); // Fixed line
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Auto refresh every 5 seconds
        if (autoRefresh && EditorApplication.timeSinceStartup - lastRefreshTime > 5.0)
        {
            RefreshModels();
            lastRefreshTime = EditorApplication.timeSinceStartup;
        }
        
        // Models list
        if (availableModels == null || availableModels.Count == 0)
        {
            EditorGUILayout.HelpBox($"No models found in {ModelManager.MODELS_FOLDER}\\nPlace .onnx model files there from your training runs.", MessageType.Info);
            return;
        }
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        var selectedAgentTypeString = agentTypeOptions[selectedAgentType]; // Get string from index
        var filteredModels = availableModels.Where(m => 
            selectedAgentTypeString == "All" || m.agentType.Equals(selectedAgentTypeString, System.StringComparison.OrdinalIgnoreCase)).ToList();
        
        foreach (var model in filteredModels)
        {
            EditorGUILayout.BeginHorizontal("box");
            
            // Model info
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(model.fileName, EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Type: {model.agentType} | Modified: {model.lastModified:yyyy-MM-dd HH:mm}");
            EditorGUILayout.EndVertical();
            
            // Apply button
            if (GUILayout.Button($"Apply to {model.agentType} Agents", GUILayout.Width(150)))
            {
                ModelManager.ApplyModelToAgents(model);
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }
        
        EditorGUILayout.EndScrollView();
        
        // Quick actions
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
        
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
    }
    
    void RefreshModels()
    {
        availableModels = ModelManager.GetAvailableModels();
        Repaint();
    }
}