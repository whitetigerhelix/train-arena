# 🚀 Advanced Features Guide

Explore TrainArena's advanced capabilities including self-play, domain randomization, TensorBoard integration, and model hot-reload systems.

## 📋 Table of Contents

- [Self-Play Tag System](#-self-play-tag-system)
- [Domain Randomization](#-domain-randomization)
- [TensorBoard Dashboard](#-tensorboard-dashboard)
- [Model Hot-Reload System](#-model-hot-reload-system)
- [Multi-Agent Training](#-multi-agent-training)
- [Recording Integration](#-recording-integration)

---

## 🏃‍♂️ Self-Play Tag System

Train competitive agents where both Runner and Tagger learn simultaneously through self-play.

### Setup Self-Play Scene

```csharp
// Unity Menu
Tools → ML Hack → Build Self-Play Tag Scene
```

**Scene Components:**

- **Runner Agent**: Tries to avoid being tagged
- **Tagger Agent**: Tries to catch the runner
- **Dynamic Environment**: Obstacles and boundaries
- **Reward Structure**: Competitive zero-sum rewards

### Training Both Agents

```powershell
# Train runner and tagger simultaneously
mlagents-learn Assets/ML-Agents/Configs/selfplay_combo.yaml --run-id=tag_selfplay_01 --train

# Then in Unity: Enter Play mode in Self-Play Tag scene
```

### Self-Play Configuration

Located in `Assets/ML-Agents/Configs/selfplay_combo.yaml`:

```yaml
behaviors:
  RunnerAgent:
    trainer_type: ppo
    hyperparameters:
      batch_size: 512
      buffer_size: 5120
      learning_rate: 3.0e-4

    self_play:
      save_steps: 50000
      team_change: 200000
      swap_steps: 25000
      window: 10
      play_against_latest_model_ratio: 0.5
      initial_elo: 1200.0

  TaggerAgent:
    trainer_type: ppo
    # Similar configuration with self-play settings
```

### Self-Play Benefits

- **Adaptive Opponents**: Agents improve against progressively stronger opponents
- **Emergent Strategies**: Complex behaviors emerge from competitive pressure
- **Balanced Learning**: Both agents evolve together
- **Curriculum Learning**: Automatic difficulty scaling

---

## 🎲 Domain Randomization

Improve agent robustness by training in varied environments with randomized parameters.

### Randomization Panel

**Runtime Controls:**

- Appears automatically during Play mode in domain randomization scenes
- Real-time parameter adjustment
- Apply changes without restarting training

**Randomizable Parameters:**

- **Physics**: Mass, friction, gravity strength
- **Lighting**: Intensity, color, direction
- **Geometry**: Obstacle size, position, count
- **Materials**: Surface properties, visual appearance

### Implementation

```csharp
// Domain randomization in scene
public class DomainRandomizer : MonoBehaviour
{
    [Header("Physics Randomization")]
    public Vector2 massRange = new Vector2(0.8f, 1.2f);
    public Vector2 frictionRange = new Vector2(0.3f, 0.8f);

    [Header("Environmental")]
    public Vector2 gravityRange = new Vector2(-12f, -8f);
    public Vector2 lightIntensityRange = new Vector2(0.8f, 1.5f);

    // Apply randomization each episode
    public void RandomizeEnvironment()
    {
        // Randomize physics parameters
        // Randomize lighting conditions
        // Randomize obstacle placement
    }
}
```

### Training with Domain Randomization

```powershell
# Standard training with randomization active
mlagents-learn Assets/ML-Agents/Configs/cube_domain_random.yaml --run-id=robust_cube --train
```

**Benefits:**

- **Sim-to-Real Transfer**: Better real-world performance
- **Robustness**: Handles parameter variations
- **Generalization**: Works in unseen conditions
- **Reduced Overfitting**: Less dependence on specific conditions

---

## 📊 TensorBoard Dashboard

Integrated TensorBoard monitoring with in-editor dashboard for real-time training visualization.

### TensorBoard Dashboard Setup

```csharp
// Unity Menu
Tools → ML Hack → Build TensorBoard Dashboard

// This creates an in-editor panel for TensorBoard integration
```

### Dashboard Configuration

**Setup Process:**

1. **Start TensorBoard Server:**

   ```powershell
   tensorboard --logdir results --port 6006 --samples_per_plugin scalars=9999999
   ```

2. **Configure Dashboard:**

   - Set `Run` to your run-id (e.g., `cube_run_01`)
   - Edit `Tags` for desired metrics:
     - `Environment/Cumulative Reward`
     - `Policy/Loss`
     - `Policy/Entropy`
     - `Environment/Episode Length`

3. **Enable Monitoring:**
   - Press **Refresh** for manual updates
   - Enable **Auto** for continuous monitoring

### API Integration

**Under the hood**, the dashboard calls TensorBoard HTTP endpoints:

```javascript
// Tag discovery
GET /data/plugin/scalars/tags?run=<RUN_ID>

// Scalar data retrieval
GET /data/plugin/scalars/scalars?tag=<TAG>&run=<RUN_ID>&format=csv
```

### Advanced TensorBoard Usage

**High-Resolution Logging:**

```powershell
# Prevent subsampling for detailed analysis
tensorboard --logdir results --samples_per_plugin scalars=9999999
```

**Custom Metrics:**

```csharp
// Log custom metrics from Unity
public void LogCustomMetric(string metricName, float value)
{
    // Integration with ML-Agents metrics system
    Academy.Instance.StatsRecorder.Add(metricName, value);
}
```

---

## 🔄 Model Hot-Reload System

Rapidly iterate between training and testing with automatic model loading and assignment.

### Hot-Reload Panel

```csharp
// Unity Menu
Tools → ML Hack → Model Hot-Reload
```

### Workflow

**Rapid Iteration Process:**

1. **Train Model:**

   ```powershell
   mlagents-learn config.yaml --run-id=experiment_01 --train
   # Exports .onnx to results/experiment_01/
   ```

2. **Import Latest Model:**

   - Click **Import Newest .onnx** in Hot-Reload panel
   - Copies most recent exported policy to `Assets/Models/TrainArena/latest.onnx`
   - Automatically imports as Unity `NNModel`

3. **Apply to Scene:**

   - Click **Assign To All ModelSwitchers**
   - Updates all `ModelSwitcher` components in open scenes
   - Calls `Apply()` to activate new model

4. **Test Immediately:**
   - Press Play → demo updates instantly
   - No manual model assignment needed
   - Seamless iteration cycle

### ModelSwitcher Component

```csharp
public class ModelSwitcher : MonoBehaviour
{
    [SerializeField] private BehaviorParameters behaviorParams;
    [SerializeField] private NNModel[] availableModels;
    [SerializeField] private int currentModelIndex = 0;

    // Hot-reload integration
    public void SetModel(NNModel newModel)
    {
        behaviorParams.Model = newModel;
        behaviorParams.BehaviorType = BehaviorType.InferenceOnly;
    }

    // Runtime model switching
    [ContextMenu("Next Model")]
    public void SwitchToNextModel()
    {
        currentModelIndex = (currentModelIndex + 1) % availableModels.Length;
        SetModel(availableModels[currentModelIndex]);
    }
}
```

---

## 👥 Multi-Agent Training

Scale up training with multiple agent types and cooperative/competitive scenarios.

### Multi-Agent Configurations

**Cooperative Training:**

```yaml
# Shared reward structure
behaviors:
  TeamAgent:
    trainer_type: ppo
    reward_signals:
      extrinsic:
        gamma: 0.99
        strength: 1.0
    # Agents share same policy network
```

**Competitive Training:**

```yaml
# Different policies for different roles
behaviors:
  AttackerAgent:
    trainer_type: ppo
    # Attacker-specific rewards and observations

  DefenderAgent:
    trainer_type: ppo
    # Defender-specific rewards and observations
```

### Group Training Benefits

- **Scalable Scenarios**: Train complex multi-agent interactions
- **Role Specialization**: Different agent types learn specialized behaviors
- **Emergent Coordination**: Agents learn to work together or compete
- **Real-World Relevance**: Mirrors many real-world multi-agent scenarios

---

## 🎬 Recording Integration

Advanced features integrate seamlessly with recording systems for demo creation.

### Automated Recording Workflows

**Training + Recording Pipeline:**

```powershell
# 1. Train model
.\Scripts\train_cube.ps1

# 2. Hot-reload latest model
# Use: Tools → ML Hack → Model Hot-Reload

# 3. Start recording
# Use: Tools → ML Hack → Start Recording

# 4. Demonstrate trained behavior
# Press Play, let agents perform

# 5. Export demo
# Use: Tools → make_gif.ps1 for MP4/GIF conversion
```

### Advanced Recording Features

**Multi-Model Comparison:**

- Record different model checkpoints side-by-side
- Show learning progression from early to final models
- Demonstrate improvement over training

**Self-Play Recordings:**

- Capture competitive agent interactions
- Show emergent strategies and counter-strategies
- Document self-play evolution

**Domain Randomization Demos:**

- Record agents performing in varied environments
- Demonstrate robustness across different conditions
- Show adaptation to randomized parameters

---

## 🛠️ Configuration Tips

### Performance Optimization for Advanced Features

**Self-Play Training:**

- Use smaller batch sizes for faster iteration
- Increase save frequency for model checkpointing
- Monitor both agents' learning curves

**Domain Randomization:**

- Start with narrow randomization ranges
- Gradually increase variation as training progresses
- Balance realism with training stability

**TensorBoard Monitoring:**

- Use custom scalar logging for domain-specific metrics
- Monitor resource usage during advanced training
- Set appropriate logging frequencies

### Troubleshooting Advanced Features

**Self-Play Issues:**

- Verify both agent configurations are correct
- Check that self-play parameters are properly set
- Monitor for learning stagnation (too competitive)

**Domain Randomization Problems:**

- Ensure randomization ranges are reasonable
- Check that randomized parameters don't break physics
- Verify randomization applies correctly each episode

**Hot-Reload Issues:**

- Check file permissions on Results directory
- Verify .onnx export is completing successfully
- Ensure ModelSwitcher components are properly configured

---

## 🔗 Related Documentation

- **Getting started**: [Quick Start Guide](QUICK_START.md)
- **Basic training**: [Training Guide](TRAINING_GUIDE.md)
- **Troubleshooting**: [Debug & Troubleshooting](DEBUG_AND_TROUBLESHOOTING.md)
- **Recording demos**: [Recording & Demo Guide](RECORDING_AND_DEMO.md)
- **API details**: [API Reference](API_REFERENCE.md)
