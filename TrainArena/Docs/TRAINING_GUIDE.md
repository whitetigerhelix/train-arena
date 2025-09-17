# 🎓 ML-Agents Training Guide

Complete guide for training PPO agents in Unity with ML-Agents, including setup, configuration, optimization, and troubleshooting.

## 📋 Table of Contents

- [Environment Setup](#-environment-setup)
- [Training Workflow](#-training-workflow)
- [Configuration & Parameters](#-configuration--parameters)
- [Performance Optimization](#-performance-optimization)
- [Model Management](#-model-management)
- [Training Monitoring](#-training-monitoring)

---

## 🛠️ Environment Setup

### Automated Setup (Recommended)

The project includes automated scripts that handle all compatibility issues:

```powershell
# 1. Create Python 3.10.11 environment with ML-Agents 1.1.0
.\Scripts\setup_python310.ps1

# 2. Activate environment with compatibility variables
.\Scripts\activate_mlagents_py310.ps1

# 3. Verify everything works
.\Scripts\check_environment.ps1
```

**What This Does:**

- ✅ Installs Python 3.10.11 (Unity official requirement)
- ✅ Creates isolated virtual environment
- ✅ Installs ML-Agents 1.1.0 (latest compatible version)
- ✅ Sets PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION=python
- ✅ Handles all dependency compatibility issues

### Manual Setup (Advanced)

If you prefer manual control:

```powershell
# Create virtual environment
python -m venv venv\mlagents-py310

# Activate environment
.\venv\mlagents-py310\Scripts\Activate.ps1

# Set compatibility variable
$env:PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION = "python"

# Install specific versions for compatibility
pip install protobuf==3.20.3
pip install mlagents==1.1.0
```

---

## 🚀 Training Workflow

### Basic Training (Cube Agent)

```powershell
# Automated training with TensorBoard
.\Scripts\train_cube.ps1

# Manual training
mlagents-learn Assets/ML-Agents/Configs/cube_ppo.yaml --run-id=my_cube_run --train
```

### Training Process

1. **Start Training Command** - Initializes ML-Agents trainer
2. **Unity Scene Setup** - Build scene: `Tools → ML Hack → Build Cube Training Scene`
3. **Enter Play Mode** - Unity connects to Python trainer
4. **Monitor Progress** - Watch TensorBoard at http://localhost:6006
5. **Training Complete** - Exported .onnx model ready for inference

### Expected Timeline

- **10-20 minutes**: Visible learning behavior
- **30-60 minutes**: Good performance for simple navigation
- **Full 500K steps**: Optimized behavior and robustness

---

## ⚙️ Configuration & Parameters

### Cube PPO Configuration

Located in `Assets/ML-Agents/Configs/cube_ppo.yaml`:

```yaml
behaviors:
  CubeAgent:
    trainer_type: ppo
    hyperparameters:
      batch_size: 1024
      buffer_size: 10240
      learning_rate: 3.0e-4
      beta: 5.0e-3
      epsilon: 0.2
      lambd: 0.95
      num_epoch: 3
      learning_rate_schedule: linear
      normalize: true # KEY: Input normalization for faster learning

    network_settings:
      normalize: true
      hidden_units: 128
      num_layers: 2

    reward_signals:
      extrinsic:
        gamma: 0.99
        strength: 1.0

    max_steps: 500000
    time_horizon: 500 # Optimized for performance
    summary_freq: 10000
```

### Key Parameter Explanations

**Input Normalization (`normalize: true`)**

- Normalizes observations for faster, more stable learning
- Essential for mixed observation types (velocities + distances)
- Results in smoother training curves

**Time Horizon (500 steps)**

- Balanced between learning opportunity and performance
- Allows visible navigation behavior
- Prevents Unity timeout issues

**Batch Size (1024) & Buffer Size (10240)**

- Optimized for stable gradient updates
- Good balance of sample efficiency and computational cost

---

## ⚡ Performance Optimization

### Arena Configuration

**Training Setup:**

- **Grid Size**: 3x3 arenas (9 parallel agents)
- **Obstacles**: 4 per arena (optimized load)
- **Episode Length**: 500 steps (30 seconds)

**Performance Benefits:**

- ~44% fewer objects to simulate vs original 4x4
- Smoother Unity performance
- Faster training iterations

### Unity Optimization Settings

**Physics Settings:**

```csharp
// CubeAgent optimized settings
moveAccel = 50f;           // Enhanced responsiveness
Rigidbody.drag = 0.5f;     // Balanced momentum
Rigidbody.angularDrag = 5f; // Reduced spinning
```

**Debug Settings:**

- Disable raycast visualization during training
- Use `--no-graphics` for headless training
- Increase `--time-scale` for faster training

### Common Performance Issues & Solutions

**Unity choppy/timeout (337s):**

```powershell
# Reduce episode length in config
time_horizon: 500  # Down from 1000+

# Use smaller arena grid
# Modify SceneBuilder.cs: TrainingArenaGridSize = 3
```

**Slow training convergence:**

```yaml
# Enable input normalization
normalize: true

# Adjust learning rate
learning_rate: 3.0e-4 # Start here, increase if slow
```

---

## 📦 Model Management

### Training Outputs

Training generates multiple model checkpoints in `Results/<run-id>/`:

```
Results/cube_run_20250916_155432/
├── CubeAgent.onnx              # ⭐ Final model (use this)
├── CubeAgent-349999.onnx       # Checkpoint at 350K steps
├── CubeAgent-399989.onnx       # Checkpoint at 400K steps
├── CubeAgent-449968.onnx       # Checkpoint at 450K steps
├── CubeAgent-499958.onnx       # Checkpoint at 500K steps
├── configuration.yaml          # Training configuration used
├── run_logs/                   # TensorBoard logs
└── timers.json                # Performance metrics
```

### Testing Different Models

1. **Load Model in Unity:**

   - Select CubeAgent in scene
   - Drag .onnx file to **Behavior Parameters > Model**
   - Set **Behavior Type** to `Inference Only`

2. **Compare Performance:**

   - Test different checkpoint models
   - Earlier checkpoints show learning progression
   - Final model usually performs best

3. **Model Switching Shortcuts:**
   - Press `M` key to cycle through behavior modes
   - Use AutoBehaviorSwitcher for automatic Default/HeuristicOnly switching

---

## 📊 Training Monitoring

### TensorBoard Metrics

Access at http://localhost:6006 during training:

**Key Metrics to Watch:**

- **Environment/Cumulative Reward**: Overall agent performance
- **Policy/Loss**: Training stability (should decrease)
- **Policy/Entropy**: Exploration vs exploitation balance
- **Environment/Episode Length**: How long episodes last

**Good Training Signs:**

- Cumulative reward increases over time
- Policy loss stabilizes (not chaotic)
- Episode length varies appropriately
- No NaN or infinite values

### Debug Information

**In-Unity Debug Features:**

- Press `R`: Toggle raycast visualization
- Press `I`: Show ML-Agents status info
- Press `V`: Toggle velocity visualization
- Press `A`: Toggle arena bounds

**Console Logging:**

```
🤖 CubeAgent_Arena_1: SUCCESS | Inference | Vel=1.23 | Goal=0.45 | Actions=127 | Step=156/500
```

**Log Interpretation:**

- **Actions > 0**: Receiving ML-Agents commands ✅
- **Actions = 0**: No trainer connection ❌
- **Vel > 0.1**: Forces being applied ✅
- **SUCCESS/STUCK**: Episode outcome

### Troubleshooting Training Issues

**No learning progress:**

- Check TensorBoard for flat reward curves
- Verify input normalization is enabled
- Consider adjusting learning rate or reward structure

**Training unstable/chaotic:**

- Reduce learning rate
- Check for NaN values in observations
- Verify episode termination conditions

**Unity won't connect:**

- Ensure BehaviorType is "Default" (not Inference Only)
- Check ML-Agents package is properly installed
- Verify Python environment is active

---

## 🔗 Related Documentation

- **Quick setup**: [Quick Start Guide](QUICK_START.md)
- **Problems?**: [Debug & Troubleshooting](DEBUG_AND_TROUBLESHOOTING.md)
- **Advanced features**: [Advanced Features Guide](ADVANCED_FEATURES.md)
- **Recording demos**: [Recording & Demo Guide](RECORDING_AND_DEMO.md)
