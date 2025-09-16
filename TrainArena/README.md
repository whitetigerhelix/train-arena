# TrainArena (Cube -> Ragdoll -> Mini-Game)

This project is a **minimal Unity 6** starter to learn PPO-based Reinforcement Learning with
**Unity ML-Agents** and ship a slick demo by week's end. It includes:

- A **CubeAgent** scene builder (programmatic) for the _reach-the-goal & avoid obstacles_ task.
- PPO **trainer configs** for cube and ragdoll.
- A **PD joint controller** and agent skeleton for ragdoll locomotion.
- **Automated Python setup** with verified compatibility.
- Tools and docs to keep you moving fast.

> You'll create an empty Unity project, then drop this `Assets/` and `Tools/` in.
> Scenes are created **programmatically** so you don't need binary `.unity` files.

---

## 🚀 **Quick Start (Verified Setup)**

### **Prerequisites**

- **Unity 6.2+** with **com.unity.ml-agents v4.0.0** and **com.unity.barracuda** package
- **Python and ML dependencies** (automatically installed by setup script - see [Scripts README](./Scripts/README.md))
  - **Python 3.10.11**
  - **ML-Agents 1.1.0** (latest compatible version)
  - **PyTorch 2.1.2** with CUDA support
  - **TensorBoard 2.20.0** for training monitoring

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

## 4) Ragdoll Locomotion

The starter includes minimal components for ragdoll control:

- `PDJointController.cs` — PD controller that converts target angles to joint torques.
- `RagdollAgent.cs` — skeleton agent collecting joint angles/velocities and issuing target angles.
- `ragdoll_ppo.yaml` — a reasonable starting PPO config (will need tuning).

**Workflow**

1. Construct a ragdoll using **ArticulationBody** (preferred) or **ConfigurableJoint**.
2. Add `PDJointController` to each actuated joint.
3. Add `RagdollAgent` to the pelvis/root with references to joints.
4. Use the provided menu **Tools ▸ ML Hack ▸ Build Ragdoll Test Scene** for a minimal flat world.
5. Train with:
   ```powershell
   # Automated (when available)
   .\Scripts\train_ragdoll.ps1
   
   # Or manual
   mlagents-learn Assets/ML-Agents/Configs/ragdoll_ppo.yaml --run-id=ragdoll_run_01 --train
   ```

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