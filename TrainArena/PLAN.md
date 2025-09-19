# 1-Week Plan & Copilot Grounding (Unity + ML-Agents)

Use this document to drive GitHub Copilot / tasks. Keep commits small and runnable.

## 🎯 Current Status (Sept 18, 2025)

**Phase:** Day 3 - RAGDOLL DEVELOPMENT & COMPREHENSIVE CLEANUP 🧹 Code Quality & Documentation Update

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

**RagdollAgent (COMPLETE):**

- ✅ **Hierarchical skeleton structure**: Proper pelvis→thigh→shin→foot chains (fixed flat hierarchy)
- ✅ **Joint-based locomotion system**: 6 PDJointControllers with coordinated movement
- ✅ **Comprehensive observations**: Uprightness + velocity + joint states (16 total observations)
- ✅ **PD Controller tuning**: Natural gains (kp=80f, kd=8f) for fluid ragdoll physics
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

**Code Quality & Documentation (NEW - Sept 18):**

- ✅ **Comprehensive code cleanup**: All agent files, debug systems, and builders updated
- ✅ **Accurate documentation**: Comments now match current implementation
- ✅ **Consistent logging**: Proper log levels and meaningful debug messages throughout
- ✅ **Updated planning docs**: PLAN.md and RAGDOLL_SPRINT_PLAN.md reflect current progress
- ✅ **Component integration**: All systems properly reference each other with clear APIs

**Current Capabilities:**

- **Production-Ready Cube Training**: Complete pipeline from training to inference
- **Ragdoll Development Platform**: Full infrastructure ready for locomotion training
- **Comprehensive Debug Suite**: Real-time visualization and monitoring for all agent types
- **Scalable Architecture**: Clean, documented codebase ready for extension
- **Professional Documentation**: Up-to-date plans, comments, and setup guides

**Next Phase: Ragdoll Training & Demo Creation! 🤖**

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

### Day 4 – Ragdoll Training & Demo Creation (CURRENT)

- [ ] **Start ragdoll training** - first proper training runs with fixed system
- [ ] **Record demo videos** of cube and ragdoll agents 🎥
- [ ] **Training analysis** - compare ragdoll vs cube learning curves
- [ ] Side-by-side demonstrations (trained vs heuristic behavior)
- Deliverable: Working ragdoll training + demonstration videos

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

### B. Ragdoll

- [ ] Build joint hierarchy; add `PDJointController` per joint
- [ ] In `RagdollAgent`, expose list of joints; obs = angles, angVels, pelvis vel/orient
- [ ] Actions = target joint angles ([-1,1] mapped to joint limits)
- [ ] Rewards = forward speed, uprightness (dot(up, bodyUp)), energy, lateral drift
- [ ] EndEpisode on fall (pelvis height low or tilt > threshold)

### C. Training/Infra

- [x] Provide PPO YAMLs (cube & ragdoll) - `cube_ppo.yaml` ready
- [ ] Add shell/PowerShell scripts to launch training ← **TODO: Next**
- [ ] Add TensorBoard instructions
- [ ] Add `Results/` to .gitignore

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
