# TrainArena (Cube -> Ragdoll -> Mini-Game)

This project is a **minimal Unity 6** starter to learn PPO-based Reinforcement Learning with
**Unity ML-Agents** and ship a slick demo by week’s end. It includes:
- A **CubeAgent** scene builder (programmatic) for the *reach-the-goal & avoid obstacles* task.
- PPO **trainer configs** for cube and ragdoll.
- A **PD joint controller** and agent skeleton for ragdoll locomotion.
- Tools and docs to keep you moving fast.

> You’ll create an empty Unity project, then drop this `Assets/` and `Tools/` in.
> Scenes are created **programmatically** so you don’t need binary `.unity` files.

---

## 0) Prereqs

- **Unity 6.2+**
- Package Manager: add
  - **ML Agents** (com.unity.ml-agents)
  - **Barracuda** (com.unity.barracuda)
- Python 3.10+
  ```bash
  pip install mlagents tensorboard
  ```

Optional (for ragdoll exploration):
```bash
pip install numpy
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
- Energy penalty (-||action||² * coeff)
- Obstacle collision penalty (-)
- Success bonus (+1) and EndEpisode

---

## 3) Train PPO (Cube)

In a terminal from the project root (where `Assets/` lives):

```bash
mlagents-learn Assets/ML-Agents/Configs/cube_ppo.yaml --run-id=cube_run_01 --train
```

- In Unity, enter **Play** mode in the training scene (or start a headless player build).
- Watch TensorBoard:
```bash
tensorboard --logdir=results
```

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
   ```bash
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
Tools/
  train_cube.sh
  train_cube.ps1
  train_ragdoll.sh
  train_ragdoll.ps1
README.md
PLAN.md
```

---

## 6) Next steps / Stretch
- Multi-agent “tag” (runner vs chaser)
- Domain randomization toggles in UI
- In-game reward breakdown bars for live debugging
- ONNX model switcher (Random / Heuristic / Trained)
- Record training timelapse for your sizzle reel

Good luck and have fun! 🚀