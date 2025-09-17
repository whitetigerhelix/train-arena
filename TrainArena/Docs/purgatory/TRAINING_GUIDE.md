# ML-Agents Training Guide

Complete step-by-step guide for training PPO agents in Unity with ML-Agents.

## 🚀 Quick Start (TL;DR)

```powershell
# 1. Set up isolated Python environment
.\Scripts\setup_venv.ps1

# 2. Verify everything works
.\Scripts\check_environment.ps1

# 3. Start training
.\Scripts\train_cube.ps1

# 4. In Unity: Open scene, press PLAY
# 5. Monitor at http://localhost:6006
```

---

## 📋 Detailed Setup Instructions

### Step 1: Python Environment Setup

**Option A: Automated Setup (Recommended)**

```powershell
.\Scripts\setup_venv.ps1
```

**Option B: Manual Setup**

```powershell
# Create virtual environment
python -m venv venv\mlagents-env

# Activate it
.\venv\mlagents-env\Scripts\Activate.ps1

# Install ML-Agents
pip install mlagents
```

### Step 2: Environment Verification

```powershell
.\Scripts\check_environment.ps1
```

**Expected Output:**

```
✅ Virtual environment active: D:\train-arena\TrainArena\venv\mlagents-env
✅ Python found: Python 3.11.x
✅ Pip found: pip 24.x.x
✅ mlagents package found: 1.0.0
✅ mlagents-learn command works correctly
✅ TensorBoard found: 2.20.0
```

### Step 3: Unity Scene Preparation

1. **Open Unity Hub** and load the TrainArena project
2. **Build the training scene**:
   - Go to `Tools → ML Hack → Build Cube Training Scene`
   - Scene will be created with 16 training arenas
3. **Verify agents**: You should see blue cubes with yellow goal spheres

### Step 4: Start Training

```powershell
.\Scripts\train_cube.ps1
```

**The script will:**

- ✅ Check virtual environment
- ✅ Validate config files
- ✅ Launch TensorBoard automatically
- ✅ Start ML-Agents training
- ✅ Wait for Unity connection

### Step 5: Connect Unity

1. **Press PLAY** in Unity when you see: `"Listening on port 5004. Start training by pressing the Play button in the Unity Editor."`
2. **Verify connection**: Console should show agents connecting
3. **Watch agents**: They should start moving (initially random, then learning)

### Step 6: Monitor Progress

**TensorBoard (Automatic)**

- Opens at: `http://localhost:6006`
- Key metrics: `Environment/Cumulative Reward`, `Environment/Episode Length`

**Unity Console**

- Shows episode rewards, steps, learning progress
- Agents start random, gradually improve

---

## 📊 Understanding Training Progress

### Key TensorBoard Metrics

| Metric                          | What It Means             | Good Trend       |
| ------------------------------- | ------------------------- | ---------------- |
| `Environment/Cumulative Reward` | Total reward per episode  | ↗️ Increasing    |
| `Environment/Episode Length`    | Steps before episode ends | Depends on task  |
| `Policy/Learning Rate`          | How fast agent learns     | Steady decline   |
| `Policy/Entropy`                | Action randomness         | Gradual decrease |

### Training Phases

1. **Random Phase (0-50k steps)**: Agents move randomly, low rewards
2. **Learning Phase (50k-500k steps)**: Rewards increase, behavior improves
3. **Convergence (500k+ steps)**: Performance stabilizes, consistent behavior

### Expected Timeline

- **Initial Learning**: 10-20 minutes
- **Good Performance**: 1-2 hours
- **Convergence**: 3-6 hours (depending on hardware)

---

## 🔧 Troubleshooting

### Common Issues & Solutions

**"mlagents-learn command failed"**

```powershell
# Check if virtual environment is active
$env:VIRTUAL_ENV  # Should show path to venv

# If not active, activate it
.\activate_mlagents.ps1

# Reinstall if needed
pip uninstall mlagents
pip install mlagents
```

**"No package 'mlagents' found"**

```powershell
# Ensure you're in the right environment
.\venv\mlagents-env\Scripts\Activate.ps1
pip install mlagents
```

**Unity won't connect to trainer**

1. Ensure ML-Agents trainer is running (shows "Listening on port 5004")
2. Press PLAY in Unity (not just open the scene)
3. Check Unity Console for connection messages
4. Verify BehaviorParameters is set to "Default" (should be automatic)

**Training is slow**

- Close other applications
- Use "Fast" quality settings in Unity
- Increase `Time.timeScale` in Unity (2-4x)
- More training arenas = faster learning

**Agents not learning**

- Check TensorBoard for increasing rewards
- Verify proper reward function in `CubeAgent.cs`
- Ensure episodes reset properly (agents reach goals or hit obstacles)

---

## 📁 File Reference

### Key Training Files

- `Assets/ML-Agents/Configs/cube_ppo.yaml` - PPO hyperparameters
- `Assets/Scripts/CubeAgent.cs` - Agent behavior and rewards
- `Scripts/train_cube.ps1` - Training launcher
- `Scripts/setup_venv.ps1` - Environment setup

### Generated Files

- `Results/cube_run_YYYYMMDD_HHMMSS/` - Training checkpoints and logs
- `venv/mlagents-env/` - Python virtual environment
- `activate_mlagents.ps1` - Quick environment activation

### Debug Controls (In Unity Play Mode)

- `R` - Toggle raycast visualization
- `I` - Toggle agent info display
- `O` - Toggle observation values
- `V` - Toggle velocity vectors
- `A` - Toggle arena bounds
- `H` - Show/hide help panel

---

## ⚙️ Advanced Configuration

### Hyperparameter Tuning

Edit `Assets/ML-Agents/Configs/cube_ppo.yaml`:

```yaml
behaviors:
  CubeAgent:
    trainer_type: ppo
    hyperparameters:
      batch_size: 1024 # Increase for stability
      learning_rate: 3.0e-4 # Decrease if unstable
      epsilon: 0.2 # PPO clip parameter
      lambd: 0.95 # GAE parameter
      num_epoch: 3 # Training epochs per batch
      gamma: 0.99 # Discount factor
    max_steps: 5.0e6 # Total training steps
    time_horizon: 128 # Episode buffer size
```

### Custom Training Runs

```powershell
# Custom run ID
.\Scripts\train_cube.ps1 -RunId "my_experiment_01"

# Resume previous training
.\Scripts\train_cube.ps1 -RunId "cube_run_20250916_143022" -Resume

# Skip TensorBoard launch
.\Scripts\train_cube.ps1 -SkipTensorBoard
```

### Multi-Environment Training

- Current setup: 16 arenas (4x4 grid)
- To modify: Edit `EnvInitializer.cs` gridSize parameter
- More environments = faster training but higher memory usage

---

## 🎯 Success Criteria

### Training Complete When:

- ✅ Cumulative reward consistently above 0.8
- ✅ Agents reliably reach goals within 30 seconds
- ✅ Success rate > 80% over 100 episodes
- ✅ Behavior is consistent across different arena layouts

### Ready for Next Steps:

- Export trained model to ONNX
- Switch to inference-only mode
- Test against heuristic baseline
- Move to ragdoll training (Day 3-4 of plan)

---

## 📞 Support & Next Steps

If you encounter issues not covered here:

1. Check Unity Console for detailed error messages
2. Verify Python virtual environment is activated
3. Ensure all file paths are correct
4. Check TensorBoard for training progress indicators

**Ready for the next phase?** Once cube training converges, we'll move to ragdoll locomotion and more complex behaviors!
