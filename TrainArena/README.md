# TrainArena - Unity ML-Agents Development Platform

**Production-ready ML-Agents training environment** with working cube navigation and advanced ragdoll locomotion systems. Built on Unity 6.2 with comprehensive development tools and proven training pipelines.

**Current Status (Sept 18, 2025):** ✅ Cube agents working perfectly, ✅ Ragdoll system complete and ready for training

## What's Included

- **🧊 CubeAgent System**: Complete navigation training with 500K+ validated steps
- **🤖 RagdollAgent System**: Hierarchical joint-based locomotion with natural physics
- **🎬 Scene Generation**: Programmatic multi-arena creation for scalable training
- **🔧 Debug Infrastructure**: Real-time visualization and ML-Agents monitoring
- **📊 Training Configs**: Optimized PPO configurations for both agent types

> You'll create an empty Unity project, then drop this `Assets/` and `Tools/` in.
> Scenes are created **programmatically** so you don't need binary `.unity` files.

---

## 🚀 **Quick Start (Verified Setup)**

### **Prerequisites (Validated Configuration)**

- **Unity 6.2+** with **com.unity.ml-agents v4.0.0** and **com.unity.barracuda** package
- **Python Environment** (automatically configured by setup scripts)
  - **Python 3.10.11** (verified compatible)
  - **ML-Agents 1.1.0** (latest stable version)
  - **PyTorch 2.1.2** with CUDA support for GPU acceleration
  - **TensorBoard 2.20.0** for real-time training monitoring

**✅ Verified Training Results:** CubeAgent trained to 500K steps with perfect inference performance

### **One-Command Setup**

```powershell
# Automated setup - installs Python 3.10.11 + ML-Agents 1.1.0
.\Scripts\setup_python310.ps1

# Activate environment
.\Scripts\activate_mlagents_py310.ps1

# Verify everything works
.\Scripts\check_environment.ps1

# Start training
.\Scripts\train_cube.ps1
```

---

## 1) Import this starter

1. Create a **new 3D (URP ok)** Unity project.
2. Quit the editor.
3. Copy the contents of this repo into your project folder (merge `Assets/` and `Tools/`).
4. Reopen the project. Unity compiles scripts (ML-Agents/Barracuda must be present).

---

## 2) Build the **Cube** training scene (one click)

- Menu: **Tools ▸ ML Hack ▸ Build Cube Training Scene**
- This creates a new scene in memory with:
  - A ground plane
  - An `EnvInitializer` that spawns N parallel **CubeAgent** arenas
  - A camera and lighting
- Press **Play** to sanity-check.
- For training, use the Python trainer (below).

### Observations

- Local velocity (3)
- Vector to goal in agent local frame (3)
- 8 planar raycasts distances for obstacle sensing (8)

### Actions

- 2 continuous actions → force along local XZ

### Rewards (per step)

- Progress toward goal (+)
- Small time penalty (-)
- Energy penalty (-||action||² \* coeff)
- Obstacle collision penalty (-)
- Success bonus (+1) and EndEpisode

---

## 3) Train PPO (Cube)

**Automated Training** (Recommended):

```powershell
# Start training with automatic TensorBoard
.\Scripts\train_cube.ps1

# Then open Unity and press PLAY
```

> 💡 **Optimized Configuration**: The included `cube_ppo.yaml` uses input normalization and balanced hyperparameters for fast, stable learning. Expect visible progress within 10-20 minutes!

**Manual Training**:

```powershell
# Activate environment first
.\Scripts\activate_mlagents_py310.ps1

# Start training
mlagents-learn Assets/ML-Agents/Configs/cube_ppo.yaml --run-id=cube_run_01 --train

# In another terminal, start TensorBoard
tensorboard --logdir=results
```

- In Unity, enter **Play** mode in the training scene (or start a headless player build).
- Watch TensorBoard at: http://localhost:6006

When training finishes, an `.onnx` policy is exported under `results/<run-id>/`.

### Inference in Unity

- Select the CubeAgent prefab in play or your scene.
- In **Behavior Parameters**, set **Model** to the exported `.onnx` file and **Behavior Type** to `Inference Only`.
- Press Play. You now have a trained cube agent.

---

## 4) RagdollAgent Training (Advanced Locomotion)

**🤖 Production-Ready Ragdoll System:**

TrainArena includes a complete hierarchical ragdoll with natural physics:

- **`PDJointController.cs`** — Tuned PD controller (kp=80f, kd=8f) for realistic joint movement
- **`RagdollAgent.cs`** — Complete ML-Agents integration with 16 observations, 6 continuous actions
- **`PrimitiveBuilder.CreateRagdoll()`** — Programmatic ragdoll generation with proper skeleton hierarchy
- **Validated Configuration** — ActionSpec, observations, and reward system fully tested

**Key Features:**

- **Hierarchical Skeleton**: Pelvis → Thigh → Shin → Foot chains (not flat structure)
- **16 Observations**: Uprightness (1) + pelvis velocity (3) + joint states (6×2)
- **6 Continuous Actions**: Left/right hip, knee, ankle joint targets
- **Balanced Rewards**: Uprightness + forward movement + energy efficiency + stability

**Training Setup:**

1. **Create Scene**: Menu → **Tools → ML Hack → Build Ragdoll Training Scene**
2. **Start Training**:

   ```powershell
   # Automated training (recommended)
   .\Scripts\train_ragdoll.ps1

   # Manual training
   mlagents-learn Assets/ML-Agents/Configs/ragdoll_ppo.yaml --run-id=ragdoll_run_01 --train
   ```

**Expected Learning Progression:**

- **0-50K steps**: Balance and uprightness learning
- **50K-200K steps**: Weight shifting and joint coordination
- **200K+ steps**: Forward locomotion and stepping patterns

> Tip: Start with a **stand-up** or **slow-forward** curriculum before asking for fast walking.

---

## 5) Project Structure

```
Assets/
  Editor/SceneBuilder.cs          # Menu items to auto-create scenes
  ML-Agents/Configs/
    cube_ppo.yaml
    ragdoll_ppo.yaml
  Scripts/
    CubeAgent.cs
    Ragdoll/PDJointController.cs
    Ragdoll/RagdollAgent.cs
    Utilities/EnvInitializer.cs

Scripts/                          # Python environment management
  setup_python310.ps1            # Primary setup script
  activate_mlagents_py310.ps1     # Environment activation
  check_environment.ps1           # Verification script
  train_cube.ps1                 # Training launcher
  purgatory/                     # Archived scripts

RESET_GUIDE.md                   # Clean project reset instructions
README.md                        # This file
PLAN.md                         # Development plan
```

---

## 🔧 **Environment Management**

### **Clean Reset**

If you need to reset your Python environment:

```powershell
# Surgical cleanup (recommended)
.\Scripts\surgical_cleanup.ps1

# Then fresh setup
.\Scripts\setup_python310.ps1
```

See `RESET_GUIDE.md` for detailed reset options.

### **Troubleshooting**

**Environment issues?**

```powershell
.\Scripts\check_environment.ps1  # Diagnose problems
```

**Training connection issues?**

- Ensure Unity ML-Agents package is installed
- Check that BehaviorType is set to "Default" in Unity
- Verify the scene has CubeAgent objects

**Performance issues?**

- Use `--no-graphics` flag for headless training
- Increase `--time-scale` for faster training
- Monitor GPU usage during training

---

## 📚 **Documentation**

**Quick Navigation:**

- 🚀 **[Quick Start Guide](Docs/QUICK_START.md)** - 5-minute setup and first training
- 📖 **[Complete Training Guide](Docs/TRAINING_GUIDE.md)** - In-depth training workflow and optimization
- 🤖 **[ML-Agents System Guide](Docs/ML_AGENTS_GUIDE.md)** - Model management, automation, and training dashboard
- 🔧 **[Debug & Troubleshooting](Docs/DEBUG_AND_TROUBLESHOOTING.md)** - Debug system usage and problem solving
- 🌟 **[Advanced Features](Docs/ADVANCED_FEATURES.md)** - Self-play, domain randomization, TensorBoard integration
- 🎬 **[Recording & Demo Guide](Docs/RECORDING_AND_DEMO.md)** - Create shareable videos and demos
- 📋 **[API Reference](Docs/API_REFERENCE.md)** - Technical code reference and integration

**User Journey:**

1. **New to project?** Start with [Quick Start Guide](Docs/QUICK_START.md)
2. **Training issues?** Check [Debug & Troubleshooting](Docs/DEBUG_AND_TROUBLESHOOTING.md)
3. **Want to learn more?** Read the [Complete Training Guide](Docs/TRAINING_GUIDE.md)
4. **Ready for advanced features?** Explore [Advanced Features](Docs/ADVANCED_FEATURES.md)
5. **Creating demos?** Use [Recording & Demo Guide](Docs/RECORDING_AND_DEMO.md)
6. **Building custom features?** Reference [API Documentation](Docs/API_REFERENCE.md)

---

## 6) Next steps / Stretch

- Multi-agent "tag" (runner vs chaser)
- Domain randomization toggles in UI
- In-game reward breakdown bars for live debugging
- ONNX model switcher (Random / Heuristic / Trained)
- Record training timelapse for your sizzle reel

Good luck and have fun! 🚀

---

## 📊 **Verified Compatibility**

This project uses the latest compatible versions as of September 2025:

- **Unity**: 6.2+ with ML-Agents Package v4.0.0
- **Python**: 3.10.11 (Unity official requirement)
- **ML-Agents**: 1.1.0 (latest available)
- **PyTorch**: 2.1.2+cu121 (CUDA support)
- **TensorBoard**: 2.20.0 (latest)

All compatibility issues with newer Python versions have been resolved.

---

## ⚙️ **Optimized Training Configuration**

The included `cube_ppo.yaml` configuration has been optimized for fast, stable learning:

### **Key Optimizations**

- ✅ **Input normalization enabled** - Faster convergence and smoother learning curves
- ✅ **Balanced hyperparameters** - Stable learning without overfitting
- ✅ **Appropriate network size** - 128 hidden units, 2 layers for cube navigation
- ✅ **Conservative training steps** - 500K steps for quick initial results

### **Expected Performance**

- **Learning visible** within 50K-100K steps (10-20 minutes)
- **Smooth training curves** with normalization enabled
- **Stable convergence** to good performance
- **Ready for inference** after full 500K step training

### **Configuration Details**

For complete parameter explanations and tuning guidance, see:
📚 **[Assets/ML-Agents/Configs/README.md](./Assets/ML-Agents/Configs/README.md)**
