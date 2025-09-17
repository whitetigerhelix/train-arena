# 1-Week Plan & Copilot Grounding (Unity + ML-Agents)

Use this document to drive GitHub Copilot / tasks. Keep commits small and runnable.

## 🎯 Current Status (Sept 16, 2025)

**Phase:** Day 1 - TRAINING COMPLETE! ✅ Ready for Model Testing 🧠

**What's Working:**

- ✅ Unity 6.2 with ML-Agents package v4.0.0
- ✅ Python 3.10.11 + ML-Agents 1.1.0 environment setup
- ✅ CubeAgent with 14 observations (velocity + goal + 8 raycasts) + 2 continuous actions
- ✅ 16-arena training environment with programmatic scene generation
- ✅ Comprehensive debug system with keyboard controls (R/I/O/V/A/H)
- ✅ AutoBehaviorSwitcher for automatic Default/HeuristicOnly mode switching
- ✅ Ultra-short episode management (100 steps, 10s limit) preventing Unity hanging
- ✅ **SUCCESSFUL TRAINING COMPLETION**: 500K steps with multiple model checkpoints

**Training Results (cube_run_20250916_155432):**

```
📄 CubeAgent.onnx (final model)
📄 CubeAgent-349999.onnx
📄 CubeAgent-399989.onnx
📄 CubeAgent-449968.onnx
📄 CubeAgent-499958.onnx
📄 CubeAgent-500009.onnx (latest checkpoint)
```

**Next Steps:**

1. **Create single-agent testing scene** for easy model evaluation
2. **Understand model checkpoints** and select best one for testing
3. **Setup inference workflow** with proper model loading
4. **Performance evaluation** and iteration planning

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

### Day 2 – Game-ify Cube

- [ ] Add curriculum (goal distance ↑, obstacle count ↑)
- [ ] Add moving hazards + raycast sensor debug gizmos
- [ ] Headless/fast training setup (timeScale, vsync off)
- Deliverable: trained model beating heuristic baseline

### Day 3 – Ragdoll Prototype

- [ ] Build ragdoll with joints + PD controllers
- [ ] `RagdollAgent` obs/actions/rewards skeleton
- [ ] Stand and slow forward locomotion
- Deliverable: ragdoll stands & shuffles forward

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
