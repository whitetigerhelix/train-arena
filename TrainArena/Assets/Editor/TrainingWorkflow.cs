using UnityEngine;
using UnityEditor;
using System.IO;
using System;

/// <summary>
/// Training workflow utilities for ML-Agents model management
/// Provides standardized naming, folder organization, and training run metadata
/// </summary>
public static class TrainingWorkflow
{
    public const string TRAINING_RUNS_FOLDER = "Assets/ML-Agents/TrainingRuns";
    public const string MODELS_FOLDER = "Assets/ML-Agents/Models";
    
    /// <summary>
    /// Generate a standardized training run ID with timestamp
    /// </summary>
    public static string GenerateRunId(string agentType)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        return $"{agentType}_{timestamp}";
    }
    
    /// <summary>
    /// Create training run folder structure
    /// </summary>
    public static string PrepareTrainingRun(string agentType, out string configPath, out string resultsPath)
    {
        var runId = GenerateRunId(agentType);
        var runFolder = Path.Combine(TRAINING_RUNS_FOLDER, runId).Replace('\\', '/');
        
        // Create folder structure
        Directory.CreateDirectory(runFolder);
        Directory.CreateDirectory(Path.Combine(runFolder, "config"));
        Directory.CreateDirectory(Path.Combine(runFolder, "results"));
        Directory.CreateDirectory(Path.Combine(runFolder, "models"));
        
        configPath = Path.Combine(runFolder, "config").Replace('\\', '/');
        resultsPath = Path.Combine(runFolder, "results").Replace('\\', '/');
        
        // Create training metadata file
        var metadata = new TrainingRunMetadata
        {
            runId = runId,
            agentType = agentType,
            startTime = DateTime.Now,
            unityVersion = Application.unityVersion,
            sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        };
        
        var metadataPath = Path.Combine(runFolder, "training_metadata.json");
        File.WriteAllText(metadataPath, JsonUtility.ToJson(metadata, true));
        
        TrainArenaDebugManager.Log($"📁 Created training run folder: {runFolder}", TrainArenaDebugManager.DebugLogLevel.Important);
        TrainArenaDebugManager.Log($"🏃 Run ID: {runId}", TrainArenaDebugManager.DebugLogLevel.Important);
        
        return runId;
    }
    
    /// <summary>
    /// Create standardized ML-Agents config file for a training run
    /// </summary>
    public static void CreateTrainingConfig(string configPath, string behaviorName, TrainingConfigSettings settings)
    {
        var configContent = GenerateMLAgentsConfig(behaviorName, settings);
        var configFilePath = Path.Combine(configPath, $"{behaviorName}_config.yaml");
        
        File.WriteAllText(configFilePath, configContent);
        
        TrainArenaDebugManager.Log($"⚙️ Created training config: {configFilePath}", TrainArenaDebugManager.DebugLogLevel.Important);
    }
    
    /// <summary>
    /// Generate ML-Agents YAML config content
    /// </summary>
    static string GenerateMLAgentsConfig(string behaviorName, TrainingConfigSettings settings)
    {
        return $@"behaviors:
  {behaviorName}:
    trainer_type: ppo
    hyperparameters:
      batch_size: {settings.batchSize}
      buffer_size: {settings.bufferSize}
      learning_rate: {settings.learningRate}
      beta: {settings.beta}
      epsilon: {settings.epsilon}
      lambd: {settings.lambda}
      num_epoch: {settings.numEpochs}
      learning_rate_schedule: linear
    network_settings:
      normalize: {settings.normalize.ToString().ToLower()}
      hidden_units: {settings.hiddenUnits}
      num_layers: {settings.numLayers}
      vis_encode_type: simple
    reward_signals:
      extrinsic:
        gamma: {settings.gamma}
        strength: 1.0
    max_steps: {settings.maxSteps}
    time_horizon: {settings.timeHorizon}
    summary_freq: {settings.summaryFreq}
    threaded: true
";
    }
    
    /// <summary>
    /// Copy models from training results to main models folder with proper naming
    /// </summary>
    public static void ProcessTrainingResults(string runId, string resultsPath)
    {
        if (!Directory.Exists(resultsPath))
        {
            TrainArenaDebugManager.Log($"❌ Results folder not found: {resultsPath}", TrainArenaDebugManager.DebugLogLevel.Important);
            return;
        }
        
        // Ensure models folder exists
        Directory.CreateDirectory(MODELS_FOLDER);
        
        // Find .onnx files in results
        var onnxFiles = Directory.GetFiles(resultsPath, "*.onnx", SearchOption.AllDirectories);
        
        foreach (var onnxFile in onnxFiles)
        {
            var fileName = Path.GetFileName(onnxFile);
            var agentType = runId.Split('_')[0]; // Extract agent type from run ID
            
            // Create standardized name
            var standardName = ModelManager.CreateStandardModelName(agentType, runId, DateTime.Now);
            var targetPath = Path.Combine(MODELS_FOLDER, standardName);
            
            // Copy to models folder
            File.Copy(onnxFile, targetPath, overwrite: true);
            
            TrainArenaDebugManager.Log($"📦 Copied model: {fileName} → {standardName}", TrainArenaDebugManager.DebugLogLevel.Important);
        }
        
        // Refresh Unity assets
        AssetDatabase.Refresh();
        
        TrainArenaDebugManager.Log($"✅ Processing complete for training run: {runId}", TrainArenaDebugManager.DebugLogLevel.Important);
    }
    
    /// <summary>
    /// Menu items for training workflow
    /// </summary>
    [MenuItem("Tools/ML Hack/Training/Prepare Cube Training Run")]
    public static void PrepareCubeTrainingRun()
    {
        var runId = PrepareTrainingRun("CubeAgent", out var configPath, out var resultsPath);
        CreateTrainingConfig(configPath, "CubeAgent", TrainingConfigSettings.DefaultCube());
        
        EditorUtility.RevealInFinder(TRAINING_RUNS_FOLDER);
    }
    
    [MenuItem("Tools/ML Hack/Training/Prepare Ragdoll Training Run")]
    public static void PrepareRagdollTrainingRun()
    {
        var runId = PrepareTrainingRun("RagdollAgent", out var configPath, out var resultsPath);
        CreateTrainingConfig(configPath, "RagdollAgent", TrainingConfigSettings.DefaultRagdoll());
        
        EditorUtility.RevealInFinder(TRAINING_RUNS_FOLDER);
    }
    
    [MenuItem("Tools/ML Hack/Training/Open Training Runs Folder")]
    public static void OpenTrainingRunsFolder()
    {
        Directory.CreateDirectory(TRAINING_RUNS_FOLDER);
        EditorUtility.RevealInFinder(TRAINING_RUNS_FOLDER);
    }
}

/// <summary>
/// Training run metadata for tracking
/// </summary>
[System.Serializable]
public class TrainingRunMetadata
{
    public string runId;
    public string agentType;
    public DateTime startTime;
    public string unityVersion;
    public string sceneName;
    public string notes;
}

/// <summary>
/// ML-Agents training configuration settings
/// </summary>
[System.Serializable]
public class TrainingConfigSettings
{
    public int batchSize = 1024;
    public int bufferSize = 10240;
    public float learningRate = 3e-4f;
    public float beta = 5e-3f;
    public float epsilon = 0.2f;
    public float lambda = 0.95f;
    public int numEpochs = 3;
    public bool normalize = false;
    public int hiddenUnits = 128;
    public int numLayers = 2;
    public float gamma = 0.99f;
    public int maxSteps = 500000;
    public int timeHorizon = 64;
    public int summaryFreq = 10000;
    
    public static TrainingConfigSettings DefaultCube()
    {
        return new TrainingConfigSettings
        {
            batchSize = 1024,
            bufferSize = 10240,
            learningRate = 3e-4f,
            maxSteps = 300000,
            timeHorizon = 64
        };
    }
    
    public static TrainingConfigSettings DefaultRagdoll()
    {
        return new TrainingConfigSettings
        {
            batchSize = 2048,
            bufferSize = 20480,
            learningRate = 3e-4f,
            maxSteps = 1000000,
            timeHorizon = 1000,
            hiddenUnits = 256,
            numLayers = 3
        };
    }
}