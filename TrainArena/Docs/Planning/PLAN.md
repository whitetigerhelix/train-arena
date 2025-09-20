# 1-Week Plan & Copilot Grounding (Unity + ML-Agents)

Use this document to drive GitHub Copilot / tasks. Keep commits small and runnable.

## 🎯 Current Status (Sept 18, 2025)

**Phase:** Day 3+ - RAGDOLL PHYSICS & TRAINING PIPELINE COMPLETION ✅ Complete System Ready for Production Training

**What's Working:**

**Core ML-Agents Infrastructure:**

- ✅ Unity 6.2 with ML-Agents package v4.0.0
- ✅ Python 3.10.11 + ML-Agents 1.1.0 environment setup
- ✅ **SUCCESSFUL TRAINING & INFERENCE**: Cube agents with perfect goal navigation
- ✅ AutoBehaviorSwitcher for seamless training/testing mode switching
- ✅ Enhanced debug system with comprehensive visualization (R/I/O/V/B/M/T/N/H/Z controls)

**CubeAgent (COMPLETE & WORKING):**

- ✅ 14 observations (velocity + goal + 8 raycasts) + 2 continuous actions
- ✅ 4x4 training environment (16 arenas) with optimized physics (moveAccel=50f)
- ✅ Ultra-short episode management (500 steps, 30s limit)
- ✅ **500K step training completion** with multiple model checkpoints
- ✅ **Perfect inference**: Trained cubes navigate directly to goals

**RagdollAgent (COMPLETE & PHYSICS OPTIMIZED):**

- ✅ **Hierarchical skeleton structure**: Proper pelvis→thigh→shin→foot chains (fixed flat hierarchy)
- ✅ **Joint-based locomotion system**: 6 PDJointControllers with coordinated movement
- ✅ **Comprehensive observations**: Uprightness + velocity + joint states (16 total observations)
- ✅ **Enhanced PD Controller tuning**: Boosted gains (kp=200f, kd=20f) for strong heuristic response
- ✅ **Natural joint physics**: Hip 90° limits, ankle -30°/60° range, softened spring forces (60f vs 120f)
- ✅ **Mass-scaled torque calculation**: Proper joint axis detection and physics-based control
- ✅ **Centralized configuration system**: RagdollHeuristicConfig eliminates all hardcoded values
- ✅ **ActionSpec validation**: 6 continuous actions properly configured
- ✅ **Scene generation**: Complete ragdoll training environments with SceneBuilder integration
- ✅ **Simple stable ragdoll system**: Replaced complex procedural generation with stable block-based approach
- ✅ **ML-Agents integration**: All 6 ConfigurableJoints properly connected to PDJointControllers
- ✅ **Visual polish**: Blinking eyes system integrated from CubeAgent for character appeal

**Training Results (cube_run_20250916_155432):**

```
📄 CubeAgent.onnx (final model) ⭐ WORKING PERFECTLY
📄 CubeAgent-349999.onnx
📄 CubeAgent-399989.onnx
📄 CubeAgent-449968.onnx
📄 CubeAgent-499958.onnx
📄 CubeAgent-500009.onnx (latest checkpoint)
```

**Code Quality & Documentation (COMPLETE - Sept 18+):**

- ✅ **Comprehensive code cleanup**: All agent files, debug systems, and builders updated
- ✅ **Accurate documentation**: Comments now match current implementation
- ✅ **Consistent logging**: Proper log levels and meaningful debug messages throughout
- ✅ **Centralized configuration**: AgentConfiguration.cs with RagdollHeuristicConfig eliminates magic numbers
- ✅ **Enhanced training scripts**: train_ragdoll.ps1 with auto-environment activation
- ✅ **Professional training tools**: TrainingDashboard, ModelManager, HotReloadWindow integrated
- ✅ **Complete documentation suite**: Training guides, configuration docs, workflow documentation
- ✅ **Updated planning docs**: PLAN.md and RAGDOLL_SPRINT_PLAN.md reflect current progress
- ✅ **Component integration**: All systems properly reference each other with clear APIs

**Current Capabilities:**

- **Production-Ready Cube Training**: Complete pipeline from training to inference
- **Optimized Ragdoll Physics**: Natural joint limits, enhanced PD controllers, mass-scaled torque
- **Centralized Configuration**: No hardcoded values, easy parameter tuning via RagdollHeuristicConfig
- **Professional Training Workflow**: TrainingDashboard, ModelManager, HotReload integration
- **Comprehensive Debug Suite**: Real-time visualization and monitoring for all agent types
- **Enhanced Training Scripts**: Auto-activation, proper environment management
- **Scalable Architecture**: Clean, documented codebase ready for extension
- **Professional Documentation**: Complete guides, workflow docs, and setup instructions

**Next Phase: First Ragdoll Training Session! 🤖**

---

## Goals

1. **Learn PPO in practice** with Unity ML-Agents
2. Ship a **cool demo**: Cube → Ragdoll → Mini-Game (tag/obstacles)
3. Export **ONNX** and run **inference-only** in-engine

---

## Day-by-Day Schedule

### Day 1 – Boot & Baseline (Cube) ✅ COMPLETE

- [x] Create Unity project, add ML-Agents + Barracuda
- [x] Add `CubeAgent`, `EnvInitializer`, and scene builder menu
- [x] Run a few env instances in play mode (16 arenas implemented)
- [x] **Train PPO with `cube_ppo.yaml`** ✅ **COMPLETE - 500K steps!**
- [x] **Setup Python 3.10 + ML-Agents 1.1.0 environment**
- [x] **Resolve Unity hanging with ultra-short episodes**
- [x] **Generate multiple model checkpoints for testing**
- Deliverable: ✅ Trained models ready for inference testing

### Day 2 – Inference Success & Documentation ✅ COMPLETE

- [x] **Perfect inference testing** - cubes navigate directly to goals! 🎯
- [x] **Enhanced ML-Agents status GUI** - real-time behavior monitoring (M key)
- [x] **Physics optimization** - 5x stronger forces, optimized Rigidbody settings
- [x] **Debug system enhancement** - comprehensive collision and velocity tracking
- [x] **Recording system review** - Unity Recorder + SimpleRecorder ready for demos
- [x] **Documentation updates** - recording guides and quick reference available
- Deliverable: ✅ **Working trained models + recording capability ready!** 🎬

### Day 3 – Ragdoll Development & Code Quality ✅ COMPLETE

- [x] **Major ragdoll system overhaul** - fixed hierarchy, joint control, ActionSpec
- [x] **Simple stable ragdoll integration** - replaced complex procedural with block-based approach
- [x] **ML-Agents readiness** - all 6 joints configured with PDJointControllers and proper limits
- [x] **Visual polish** - integrated blinking eye system for character appeal
- [x] **Clean system integration** - updated SceneBuilder, menu items, all using stable ragdoll
- [x] **Comprehensive code cleanup** - updated all comments, logging, documentation
- [x] **Agent file audit** - RagdollAgent, CubeAgent, PDJointController fully documented
- [x] **Debug system review** - TrainArenaDebugManager cleaned up and optimized
- [x] **Scene builder integration** - ragdoll creation fully integrated with training pipeline
- [x] **Planning document updates** - PLAN.md and RAGDOLL_SPRINT_PLAN.md reflect current state
- Deliverable: ✅ **Production-ready ragdoll infrastructure + comprehensive documentation!** 🧹

### Day 4 – First Ragdoll Training Session (READY TO LAUNCH) 🚀

- [ ] **Launch first ragdoll training** - optimized physics + centralized config system
- [ ] **Monitor training with professional tools** - TrainingDashboard, live metrics
- [ ] **Test model hot-reload** - ModelHotReloadWindow for rapid iteration
- [ ] **Validate physics improvements** - compare old vs new joint behavior
- [ ] **Record demo videos** of cube and ragdoll agents 🎥
- Deliverable: First successful ragdoll training session + performance analysis

### Day 5 – Ragdoll Locomotion Training

- [ ] **Multi-hour training sessions** with balanced reward system
- [ ] **Hyperparameter tuning** - learning rate, episode length, reward weights
- [ ] **Curriculum development** - standing → weight shifting → stepping → walking
- [ ] **Model checkpoints** - save models at key learning milestones
- Deliverable: Ragdoll agents showing consistent locomotion progress

### Day 4 – Walk

- [ ] Tune PD gains; reward shaping (uprightness, speed, energy)
- [ ] Domain randomization (mass/friction)
- [ ] Add perturbation pushes to test robustness
- Deliverable: stable forward walk for 10+ s

### Day 5 – Objective Layer

- [ ] Simple tag or capture-the-flag (single agent vs. moving target)
- [ ] Add UI toggles for hazards, target speed
- Deliverable: fun arena scene

### Day 6 – Inference & Polish

- [ ] Export ONNX, set Behavior to Inference Only
- [ ] Add model switcher (Random/Heuristic/Trained)
- [ ] Add in-game reward bars
- Deliverable: 120 FPS playable demo

### Day 7 – Package & Share

- [ ] Record sizzle reel + GIFs
- [ ] One-pager of PPO settings & lessons learned
- [ ] Clean repo, write final README

---

## Technical Tasks (Copilot-friendly)

### A. Cube Agent

- [x] Implement `CollectObservations`: local vel, vector-to-goal, 8 raycasts (14 total observations)
- [x] Implement `OnActionReceived`: apply local force, time & energy penalties
- [x] Implement goal success / obstacle penalties
- [x] Add `Heuristic` for WASD (+ random actions for testing)
- [x] Add `EnvInitializer` to spawn N arenas laid out in a grid (4x4 = 16 arenas)

### B. Ragdoll ✅ COMPLETE

- [x] Build joint hierarchy; add `PDJointController` per joint
- [x] In `RagdollAgent`, expose list of joints; obs = angles, angVels, pelvis vel/orient
- [x] Actions = target joint angles ([-1,1] mapped to joint limits)
- [x] Rewards = forward speed, uprightness (dot(up, bodyUp)), energy, lateral drift
- [x] EndEpisode on fall (pelvis height low or tilt > threshold)
- [x] **Enhanced PD control**: Stronger gains (kp=200f, kd=20f) with mass-scaled torque
- [x] **Natural joint limits**: Hip 90°, ankle -30°/60°, softened springs for realism
- [x] **Centralized configuration**: RagdollHeuristicConfig eliminates hardcoded values

### C. Training/Infra ✅ ENHANCED

- [x] Provide PPO YAMLs (cube & ragdoll) - `cube_ppo.yaml` ready
- [x] **Enhanced PowerShell training scripts** - `train_ragdoll.ps1` with auto-activation
- [x] **Professional training tools integration** - TrainingDashboard, ModelManager, HotReload
- [x] **Comprehensive documentation** - Training guides, workflow docs, configuration references
- [x] Add TensorBoard instructions
- [x] Add `Results/` to .gitignore

### D. Demo Polish

- [ ] Simple UI Canvas: speed slider, hazard toggle, model switcher dropdown
- [ ] Domain randomization toggle
- [ ] Reward bar graph during play
- [ ] Side-by-side heuristic vs trained agents

---

## PPO Defaults (starting points)

- hidden_units: 128
- num_layers: 2
- gamma: 0.99
- gae lambda: 0.95
- clip: 0.2
- entropy: 0.003–0.01
- learning_rate: 3e-4
- batch_size: 1024–4096
- time_horizon: 128–256
- num_epoch: 3–5

---

## Definition of Done (per stage)

- **Cube**: reaches random goals w/ obstacles in under 30s sim time
- **Ragdoll**: walks 10 m without falling; generalizes to minor friction/mass changes
- **Mini-Game**: runner avoids tagger for ≥ 10 s reliably

---

## Risks & Mitigations

- Slow learning → increase env instances, entropy, simplify reward, add curriculum
- Ragdoll instability → PD gains, clip actions, lower max torques, bigger feet, start with stand task
- Demo performance → inference-only, disable expensive graphics, fixed timestep tuning

---

## 🎯 IMMEDIATE NEXT STEPS (Ready to Execute)

### 1. Launch First Ragdoll Training Session

- **Command**: `.\train_ragdoll.ps1`
- **What to Expect**: Auto-activation of ML-Agents environment + optimized training launch
- **Monitoring**: Use TrainingDashboard for real-time metrics and progress visualization
- **Duration**: Start with 1-2 hour session to validate physics improvements

### 2. Validate Physics & Configuration Improvements

- **Compare Heuristic Behavior**: Old vs new PD controller gains (80f→200f kp, 8f→20f kd)
- **Test Joint Limits**: Hip 90° movement, ankle -30°/60° range, natural motion
- **Verify Configuration**: All parameters loaded from RagdollHeuristicConfig (no hardcoded values)

### 3. Professional Training Workflow Testing

- **ModelManager**: Test model loading/switching during training
- **HotReloadWindow**: Validate real-time model updates without stopping training
- **TrainingDashboard**: Monitor reward curves, episode lengths, training stability

### 4. Performance Analysis & Iteration

- **Training Metrics**: Compare ragdoll learning curve vs cube baseline
- **Physics Validation**: Confirm enhanced joint control produces better training signals
- **Configuration Tuning**: Adjust RagdollHeuristicConfig parameters based on training results

### 5. Demo Creation Preparation

- **Record Training Progress**: Capture heuristic vs early training behavior
- **Document Improvements**: Before/after physics comparison videos
- **Prepare Inference Testing**: Set up trained model evaluation workflow

**Status**: All infrastructure complete ✅ Ready for production training! 🚀

---
