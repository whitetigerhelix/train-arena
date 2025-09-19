# 🔧 API Reference

Technical reference for TrainArena's code components, systems, and integration points.

## 📋 Table of Contents

- [Agent Components](#-agent-components)
- [Debug System APIs](#-debug-system-apis)
- [Scene Builder System](#-scene-builder-system)
- [Model Management](#-model-management)
- [Training Configuration](#-training-configuration)
- [Utility Systems](#-utility-systems)

---

## 🤖 Agent Components

### CubeAgent

Primary ML-Agents reinforcement learning agent for navigation tasks.

```csharp
public class CubeAgent : Agent
{
    [Header("Movement Settings")]
    [SerializeField] private float moveAccel = 50f;    // Force multiplier for movement
    [SerializeField] private float maxSpeed = 10f;     // Maximum agent velocity

    [Header("Observation Settings")]
    [SerializeField] private float rayDistance = 10f;  // Raycast sensing range
    [SerializeField] private int numRays = 8;          // Number of raycast directions

    [Header("Reward Settings")]
    [SerializeField] private float goalReward = 1f;              // Success reward
    [SerializeField] private float obstacleReward = -0.1f;       // Collision penalty
    [SerializeField] private float energyPenalty = 0.001f;       // Action cost
    [SerializeField] private float timePenalty = -0.001f;        // Time pressure
}
```

#### Key Methods

**Observation Collection:**

```csharp
public override void CollectObservations(VectorSensor sensor)
{
    // Local velocity (3 values)
    Vector3 localVel = transform.InverseTransformDirection(rb.velocity);
    sensor.AddObservation(localVel);

    // Vector to goal in local frame (3 values)
    Vector3 localGoal = transform.InverseTransformPoint(goalTransform.position);
    sensor.AddObservation(localGoal);

    // 8-direction raycast distances (8 values)
    for (int i = 0; i < numRays; i++)
    {
        float angle = i * 2f * Mathf.PI / numRays;
        Vector3 rayDir = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));
        float distance = PerformRaycast(rayDir, rayDistance);
        sensor.AddObservation(distance / rayDistance); // Normalized [0,1]
    }
}
```

**Action Processing:**

```csharp
public override void OnActionReceived(ActionBuffers actions)
{
    // Convert continuous actions to movement forces
    float moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
    float moveZ = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

    // Apply forces in local coordinate system
    Vector3 force = transform.TransformDirection(new Vector3(moveX, 0, moveZ));
    rb.AddForce(force * moveAccel, ForceMode.Force);

    // Apply energy penalty based on action magnitude
    float energyCost = (Mathf.Abs(moveX) + Mathf.Abs(moveZ)) * energyPenalty;
    AddReward(-energyCost);
}
```

#### Observation Space

| Index | Type      | Range   | Description                   |
| ----- | --------- | ------- | ----------------------------- |
| 0-2   | Vector3   | [-∞, ∞] | Local velocity (x, y, z)      |
| 3-5   | Vector3   | [-∞, ∞] | Local goal position (x, y, z) |
| 6-13  | Float × 8 | [0, 1]  | Normalized raycast distances  |

**Total**: 14 continuous observations

#### Action Space

| Index | Type       | Range   | Description            |
| ----- | ---------- | ------- | ---------------------- |
| 0     | Continuous | [-1, 1] | Forward/backward force |
| 1     | Continuous | [-1, 1] | Left/right force       |

**Total**: 2 continuous actions

---

## 🔍 Debug System APIs

### TrainArenaDebugManager

Centralized debug visualization and control system.

```csharp
public class TrainArenaDebugManager : MonoBehaviour
{
    [Header("Debug Settings")]
    public static bool ShowRaycastVisualization = false;
    public static bool ShowDebugInfo = false;
    public static bool ShowVelocityVectors = false;
    public static bool ShowArenaBounds = false;

    [Header("Logging")]
    public static LogLevel CurrentLogLevel = LogLevel.Info;

    // Global debug toggles
    public static void ToggleRaycastVisualization() { ShowRaycastVisualization = !ShowRaycastVisualization; }
    public static void ToggleDebugInfo() { ShowDebugInfo = !ShowDebugInfo; }
    public static void ToggleVelocityVectors() { ShowVelocityVectors = !ShowVelocityVectors; }
    public static void ToggleArenaBounds() { ShowArenaBounds = !ShowArenaBounds; }
}
```

#### Keyboard Input Handler

```csharp
public class DebugInputHandler : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) TrainArenaDebugManager.ToggleRaycastVisualization();
        if (Input.GetKeyDown(KeyCode.I)) TrainArenaDebugManager.ToggleDebugInfo();
        if (Input.GetKeyDown(KeyCode.V)) TrainArenaDebugManager.ToggleVelocityVectors();
        if (Input.GetKeyDown(KeyCode.A)) TrainArenaDebugManager.ToggleArenaBounds();
        if (Input.GetKeyDown(KeyCode.M)) ToggleBehaviorMode();
        if (Input.GetKeyDown(KeyCode.L)) CycleLogLevel();
    }
}
```

### Debug Visualization Components

**RaycastVisualizer:**

```csharp
public class RaycastVisualizer : MonoBehaviour
{
    [SerializeField] private LineRenderer[] rayLines;
    [SerializeField] private Color clearColor = Color.green;
    [SerializeField] private Color hitColor = Color.red;

    public void UpdateRayVisualization(Vector3[] rayDirections, float[] rayDistances, float maxDistance)
    {
        for (int i = 0; i < rayLines.Length; i++)
        {
            Vector3 startPos = transform.position + Vector3.up * 0.1f;
            Vector3 endPos = startPos + rayDirections[i] * rayDistances[i];

            rayLines[i].SetPosition(0, startPos);
            rayLines[i].SetPosition(1, endPos);
            rayLines[i].color = (rayDistances[i] < maxDistance) ? hitColor : clearColor;
            rayLines[i].enabled = TrainArenaDebugManager.ShowRaycastVisualization;
        }
    }
}
```

---

## 🏗️ Scene Builder System

### SceneBuilder

Programmatic scene generation system for training environments.

```csharp
public static class SceneBuilder
{
    // Configuration constants
    private const int TrainingArenaGridSize = 3;      // 3x3 grid = 9 arenas
    private const float ArenaSpacing = 20f;           // Units between arena centers
    private const int ObstaclesPerArena = 4;          // Obstacles per training arena
    private const float GroundSize = 200f;            // Ground plane dimensions

    // Menu integration
    [MenuItem("TrainArena/Scenes/Build Cube Training Scene")]
    public static void BuildCubeTrainingScene()
    {
        // Create new scene
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Build environment
        CreateGround();
        CreateArenaGrid();
        SetupLighting();
        SetupCamera();

        // Initialize debug systems
        SetupDebugManager();
    }
}
```

#### Arena Generation

```csharp
private static void CreateArenaGrid()
{
    GameObject arenaParent = new GameObject("Training Arenas");

    for (int x = 0; x < TrainingArenaGridSize; x++)
    {
        for (int z = 0; z < TrainingArenaGridSize; z++)
        {
            Vector3 arenaCenter = new Vector3(
                (x - TrainingArenaGridSize/2f) * ArenaSpacing,
                0,
                (z - TrainingArenaGridSize/2f) * ArenaSpacing
            );

            CreateArena(arenaCenter, x * TrainingArenaGridSize + z, arenaParent.transform);
        }
    }
}

private static void CreateArena(Vector3 center, int arenaIndex, Transform parent)
{
    GameObject arena = new GameObject($"Arena_{arenaIndex}");
    arena.transform.parent = parent;
    arena.transform.position = center;

    // Create agent
    GameObject agent = CreateCubeAgent(center, arenaIndex);

    // Create goal
    GameObject goal = CreateGoal(center + Random.insideUnitSphere * 8f);

    // Create obstacles
    for (int i = 0; i < ObstaclesPerArena; i++)
    {
        CreateObstacle(center + Random.insideUnitSphere * 10f);
    }
}
```

#### Prefab Management

```csharp
public static class PrefabManager
{
    private static GameObject cubeAgentPrefab;
    private static GameObject goalPrefab;
    private static GameObject obstaclePrefab;

    public static GameObject GetCubeAgentPrefab()
    {
        if (cubeAgentPrefab == null)
        {
            cubeAgentPrefab = LoadPrefab("Assets/Prefabs/CubeAgent.prefab");
        }
        return cubeAgentPrefab;
    }

    private static GameObject LoadPrefab(string path)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError($"Could not load prefab at path: {path}");
        }
        return prefab;
    }
}
```

---

## 🔄 Model Management

### AutoBehaviorSwitcher

Automatic switching between training and inference modes.

```csharp
public class AutoBehaviorSwitcher : MonoBehaviour
{
    [Header("Behavior Control")]
    [SerializeField] private BehaviorParameters behaviorParams;
    [SerializeField] private float timeoutDuration = 10f;        // Seconds to wait for trainer
    [SerializeField] private bool enableAutoSwitch = true;       // Enable automatic switching

    private float connectionTimer = 0f;
    private bool isWaitingForTrainer = true;

    private void Update()
    {
        if (!enableAutoSwitch) return;

        if (isWaitingForTrainer)
        {
            connectionTimer += Time.deltaTime;

            // Check if trainer connected
            if (Academy.Instance.IsCommunicatorOn)
            {
                SwitchToTraining();
            }
            // Timeout - switch to heuristic control
            else if (connectionTimer >= timeoutDuration)
            {
                SwitchToHeuristic();
            }
        }
    }

    private void SwitchToTraining()
    {
        behaviorParams.BehaviorType = BehaviorType.Default;
        isWaitingForTrainer = false;
        Debug.Log("Switched to Training mode - Trainer connected");
    }

    private void SwitchToHeuristic()
    {
        behaviorParams.BehaviorType = BehaviorType.HeuristicOnly;
        isWaitingForTrainer = false;
        Debug.Log("Switched to Heuristic mode - No trainer found");
    }
}
```

### ModelSwitcher

Runtime model loading and switching system.

```csharp
public class ModelSwitcher : MonoBehaviour
{
    [Header("Model Management")]
    [SerializeField] private BehaviorParameters behaviorParams;
    [SerializeField] private NNModel[] availableModels;
    [SerializeField] private int currentModelIndex = 0;

    [Header("UI Integration")]
    [SerializeField] private Text modelDisplayText;
    [SerializeField] private Button nextModelButton;
    [SerializeField] private Button prevModelButton;

    public void SetModel(NNModel newModel)
    {
        if (newModel == null)
        {
            Debug.LogWarning("Attempted to set null model");
            return;
        }

        behaviorParams.Model = newModel;
        behaviorParams.BehaviorType = BehaviorType.InferenceOnly;

        UpdateUI();
        Debug.Log($"Loaded model: {newModel.name}");
    }

    public void SwitchToNextModel()
    {
        if (availableModels.Length == 0) return;

        currentModelIndex = (currentModelIndex + 1) % availableModels.Length;
        SetModel(availableModels[currentModelIndex]);
    }

    public void SwitchToPreviousModel()
    {
        if (availableModels.Length == 0) return;

        currentModelIndex = (currentModelIndex - 1 + availableModels.Length) % availableModels.Length;
        SetModel(availableModels[currentModelIndex]);
    }
}
```

---

## ⚙️ Training Configuration

### Configuration File Structure

**cube_ppo.yaml** structure and parameter explanations:

```yaml
behaviors:
  CubeAgent:
    trainer_type: ppo

    hyperparameters:
      batch_size: 1024 # Samples per gradient update
      buffer_size: 10240 # Experience buffer size
      learning_rate: 3.0e-4 # Adam optimizer learning rate
      learning_rate_schedule: linear # Decay schedule
      beta: 5.0e-3 # Entropy regularization
      epsilon: 0.2 # PPO clipping parameter
      lambd: 0.95 # GAE lambda parameter
      num_epoch: 3 # Gradient updates per batch
      normalize: true # Input normalization (CRITICAL)

    network_settings:
      normalize: true # Network-level normalization
      hidden_units: 128 # Hidden layer size
      num_layers: 2 # Number of hidden layers
      vis_encode_type: simple # Visual encoding (if used)

    reward_signals:
      extrinsic:
        gamma: 0.99 # Discount factor
        strength: 1.0 # Reward signal weight

    keep_checkpoints: 5 # Number of model checkpoints to save
    max_steps: 500000 # Total training steps
    time_horizon: 500 # Episode length in steps
    summary_freq: 10000 # TensorBoard logging frequency
```

### Parameter Tuning Guide

**Learning Rate (`learning_rate`):**

- Start: `3.0e-4` (default)
- Too slow learning: Increase to `1.0e-3`
- Unstable training: Decrease to `1.0e-4`

**Batch Size (`batch_size`):**

- Larger = More stable gradients, slower updates
- Smaller = Faster updates, more noise
- Sweet spot: 512-2048 for most scenarios

**Time Horizon (`time_horizon`):**

- Longer = More learning per episode, higher computational cost
- Shorter = Faster training, may not learn complex behaviors
- Balance: 200-1000 steps depending on task complexity

**Normalization (`normalize: true`):**

- ALWAYS enable for mixed observation types
- Critical for velocity + distance observations
- Significantly improves learning speed and stability

---

## 🛠️ Utility Systems

### EnvInitializer

Environment management and multi-agent coordination.

```csharp
public class EnvInitializer : MonoBehaviour
{
    [Header("Environment Settings")]
    [SerializeField] private int maxEnvironmentSteps = 500;
    [SerializeField] private bool resetOnMaxSteps = true;
    [SerializeField] private float episodeTimeout = 30f;

    private CubeAgent[] allAgents;
    private int currentStep = 0;

    private void Start()
    {
        // Find all agents in scene
        allAgents = FindObjectsOfType<CubeAgent>();

        // Initialize episode
        ResetEnvironment();
    }

    public void ResetEnvironment()
    {
        currentStep = 0;

        foreach (CubeAgent agent in allAgents)
        {
            agent.EndEpisode(); // Triggers agent reset
        }

        // Randomize goal positions
        RandomizeGoalPositions();

        Debug.Log($"Environment reset - {allAgents.Length} agents initialized");
    }

    private void FixedUpdate()
    {
        currentStep++;

        if (resetOnMaxSteps && currentStep >= maxEnvironmentSteps)
        {
            ResetEnvironment();
        }
    }
}
```

### SimpleRecorder

Frame capture system for development recording.

```csharp
public class SimpleRecorder : MonoBehaviour
{
    [Header("Recording Settings")]
    public int fps = 30;
    public string outputDir = "Recordings";
    public KeyCode recordKey = KeyCode.R;
    public int resolutionScale = 1;

    private bool isRecording = false;
    private int frameCount = 0;
    private Camera recordingCamera;

    private void Start()
    {
        recordingCamera = GetComponent<Camera>();

        // Ensure output directory exists
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(recordKey))
        {
            ToggleRecording();
        }
    }

    private void ToggleRecording()
    {
        isRecording = !isRecording;

        if (isRecording)
        {
            StartRecording();
        }
        else
        {
            StopRecording();
        }
    }

    private IEnumerator CaptureFrames()
    {
        while (isRecording)
        {
            yield return new WaitForEndOfFrame();

            CaptureFrame();
            frameCount++;

            yield return new WaitForSeconds(1f / fps);
        }
    }

    private void CaptureFrame()
    {
        RenderTexture renderTexture = new RenderTexture(
            Screen.width * resolutionScale,
            Screen.height * resolutionScale,
            24
        );

        recordingCamera.targetTexture = renderTexture;
        recordingCamera.Render();

        RenderTexture.active = renderTexture;
        Texture2D screenshot = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        screenshot.Apply();

        // Save frame
        byte[] data = screenshot.EncodeToPNG();
        string filename = Path.Combine(outputDir, $"frame_{frameCount:D4}.png");
        File.WriteAllBytes(filename, data);

        // Cleanup
        recordingCamera.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(renderTexture);
        DestroyImmediate(screenshot);
    }
}
```

---

## 🔗 Integration Examples

### Custom Agent Creation

```csharp
// Inherit from CubeAgent for custom behaviors
public class MyCustomAgent : CubeAgent
{
    [Header("Custom Settings")]
    [SerializeField] private float customParameter = 1f;

    public override void CollectObservations(VectorSensor sensor)
    {
        // Call base implementation
        base.CollectObservations(sensor);

        // Add custom observations
        sensor.AddObservation(customParameter);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Custom action processing
        base.OnActionReceived(actions);

        // Add custom rewards
        AddCustomRewards();
    }

    private void AddCustomRewards()
    {
        // Custom reward logic
        AddReward(customParameter * Time.fixedDeltaTime);
    }
}
```

### Debug System Extension

```csharp
// Add custom debug visualization
public class CustomDebugVisualizer : MonoBehaviour
{
    [SerializeField] private LineRenderer debugLine;

    private void Update()
    {
        if (TrainArenaDebugManager.ShowDebugInfo)
        {
            UpdateCustomVisualization();
        }

        debugLine.enabled = TrainArenaDebugManager.ShowDebugInfo;
    }

    private void UpdateCustomVisualization()
    {
        // Custom debug drawing logic
        Vector3 start = transform.position;
        Vector3 end = start + transform.forward * 5f;

        debugLine.SetPosition(0, start);
        debugLine.SetPosition(1, end);
    }
}
```

---

## 🔗 Related Documentation

- **Getting started**: [Quick Start Guide](QUICK_START.md)
- **Training setup**: [Training Guide](TRAINING_GUIDE.md)
- **Debug usage**: [Debug & Troubleshooting](DEBUG_AND_TROUBLESHOOTING.md)
- **Advanced systems**: [Advanced Features Guide](ADVANCED_FEATURES.md)
- **Recording integration**: [Recording & Demo Guide](RECORDING_AND_DEMO.md)
