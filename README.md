# TrainArena — Project Summary

TrainArena is a compact Unity + ML-Agents playground for “learn-by-doing” reinforcement learning. It ships a clean Core (Cube→Goal PPO) and optional Add-ons (Tag mini-game, Self-Play, Ragdoll locomotion, Domain Randomization, Reward HUD, Recording, TensorBoard overlay, Model Hot-Reload) so you can get a result fast, then scale up to showy demos and reusable NPC controllers.

## Goals

- Learn PPO end-to-end inside a real game engine.
- Build fun, explainable demos you can share with a team.
- Create reusable patterns for physics-based NPC behavior (obs/actions/rewards, curricula, inference).

## Tech

- Unity 6.2+, ML-Agents (PPO), Barracuda (ONNX inference)
- Python trainer (mlagents-learn) + TensorBoard
- Optional quality-of-life: Model Switcher, Reward HUD, Domain Randomization, Recording, TensorBoard UI overlay, Hot-Reload

## Included Scenarios

- Core: Cube reaches a goal while avoiding obstacles (fast PPO win).
- Add-ons: Tag (runner vs tagger), Self-Play tag (both learn), Ragdoll stand/walk.

# QuickStart

## 0) Two-minute setup

1. Unity 6.2+ → Package Manager: install ML-Agents and Barracuda.

2. Unzip TrainArena-Starter into your Unity project (merge Assets/ and Tools/).

3. Unzip TrainArena-Extras on top (merge again).

4. Open Unity → Tools → ML Hack → Build Cube Training Scene → Play (sanity check).

5. From your project root in a terminal:

```
pip install mlagents tensorboard

mlagents-learn Assets/ML-Agents/Configs/cube\_ppo.yaml --run-id=cube --train

tensorboard --logdir results
```

6. When training exports a .onnx, set the agent’s Behavior Type to Inference Only and assign the model.

- Using the “Complete” consolidated pack instead? Paths are under Assets/TrainArena/Core and Assets/TrainArena/Addons. With Starter+Extras, use the flat Assets/... paths shown below.

## 1) Feature map (what it is, why it helps, how to use)

### Core (from Starter)

#### Cube → Goal PPO

- What: A minimal agent that moves to a target and avoids obstacles.
- Why: Fastest way to see PPO learning end-to-end.
- Use: Tools → ML Hack → Build Cube Training Scene → press Play. Train with:
  - `mlagents-learn Assets/ML-Agents/Configs/cube_ppo.yaml --run-id=cube --train`
- Where: Assets/Scripts/CubeAgent.cs, Assets/Scripts/Utilities/EnvInitializer.cs, Assets/ML-Agents/Configs/cube_ppo.yaml.

#### Editor Scene Builder

- What: Menu items that autogenerate scenes—no .unity files needed.
- Why: Zero scene boilerplate; easy to spawn many parallel arenas.
- Use: Unity Tools → ML Hack → …
- Where: Assets/Editor/SceneBuilder.cs.

#### PPO configs + training scripts

- What: Tuned YAML and shell/PowerShell runners.
- Why: One command to train; sensible PPO defaults.
- Use: Tools/train_cube.sh or the mlagents-learn … command above.
- Where: Assets/ML-Agents/Configs/\*.yaml, Tools/.

### Add-ons (from Extras and earlier add-ons)

#### Tag Mini-Game (Runner vs. Tagger)

- What: Runner learns to avoid a chaser (tagger). Tagger can be heuristic or trainable.
- Why: More “gamey” objective and a great demo.
- Use (heuristic tagger): Tools → ML Hack → Build Tag Arena Scene → Train Runner:
  - `mlagents-learn Assets/ML-Agents/Configs/runner_ppo.yaml --run-id=runner --train`
- Where: Assets/Scripts/Tag/RunnerAgent.cs, HeuristicTagger.cs, TagArenaBuilder.cs, Assets/ML-Agents/Configs/runner_ppo.yaml.

#### Self-play Tag (both learn)

- What: Runner and Tagger are PPO agents co-trained in one run.
- Why: Showcases multi-agent interaction; richer behaviors.
- Use: Tools → ML Hack → Build Self-Play Tag Scene →
  - `mlagents-learn Assets/ML-Agents/Configs/selfplay_combo.yaml --run-id=tag_selfplay --train`
- Where: Assets/Scripts/Tag/TaggerAgentTrainable.cs, Assets/Editor/SelfPlayTagSceneBuilder.cs, Assets/ML-Agents/Configs/selfplay_combo.yaml.

#### Ragdoll Locomotion (PD + PPO)

- What: Skeleton joints with PD controllers; agent learns to stand/walk.
- Why: Embodied control—exactly the “NPC physics brain” you want.
- Use: Build a ragdoll with ConfigurableJoints, add PDJointController per joint, wire into RagdollAgent. Train:
  - `mlagents-learn Assets/ML-Agents/Configs/ragdoll_ppo.yaml --run-id=ragdoll --train`
- Where: Assets/Scripts/Ragdoll/PDJointController.cs, RagdollAgent.cs, Assets/ML-Agents/Configs/ragdoll_ppo.yaml.

#### Curriculum (difficulty slider)

- What: Runtime difficulty scaler (arena size, obstacles, tagger speed).
- Why: Start easy → get wins → scale up; faster, stabler learning.
- Use: Scenes with the add-on UI include a Difficulty slider.
- Where: Assets/Scripts/Utilities/CurriculumController.cs.

#### Model Switcher (Random / Heuristic / Inference)

- What: Toggle behavior live for A/B demos.
- Why: Instantly compare learned vs baseline behaviors.
- Use: Assign your exported .onnx to ModelSwitcher.trainedModel, pick mode in the in-scene dropdown.
- Where: Assets/Scripts/UI/ModelSwitcher.cs.

#### Reward HUD (live bars)

- What: On-screen bars for survival/energy/etc.
- Why: Makes reward shaping visible during play.
- Use: Already wired in Runner; call RewardHUD.SetReward("Name", value).
- Where: Assets/Scripts/UI/RewardHUD.cs.

#### Domain Randomization UI

- What: Toggle and re-roll mass, friction, lighting, gravity.
- Why: Improves robustness/generalization; cool demo lever.
- Use: In scenes with the panel, click Apply Randomization (ideally at episode start).
- Where: Assets/Scripts/DomainRandomization/DomainRandomizer.cs, DomainRandomizationUI.cs.

#### Recording Utility (MP4/GIF)

- What: Press R in Play mode to save PNG frames; scripts convert to MP4/GIF via ffmpeg.
- Why: Quick sizzle reels for your hackathon demo.
- Use: Add SimpleRecorder to your Camera → press R → then:
  - Windows: `Tools/make_gif.ps1 -InputDir Recordings -OutMp4 out.mp4 -OutGif out.gif -Fps 30`
  - macOS/Linux: `Tools/make_gif.sh Recordings out.mp4 out.gif 30`
- Where: Assets/Scripts/Recording/SimpleRecorder.cs, Tools/make_gif.\*.

#### TensorBoard Dashboard (UI overlay)

- What: In-Unity mini charts for key metrics (Cumulative Reward, Loss, Entropy).
- Why: Keep an eye on learning without tab-switching.
- Use: Start TensorBoard: `tensorboard --logdir results --port 6006` → Tools → ML Hack → Build TensorBoard Dashboard → set Run/Tags → Refresh or Auto.
- Where: Assets/Editor/TensorBoardDashboardBuilder.cs, Assets/Scripts/Dashboard/\*.

#### Model Hot-Reload (Editor)

- What: Pull newest .onnx from results/ into Assets/…/latest.onnx and auto-assign to all ModelSwitchers.
- Why: Swap in the latest policy mid-iteration without fiddling.
- Use: Tools → ML Hack → Model Hot-Reload → Import Newest .onnx → Assign To All ModelSwitchers.
- Where: Assets/Editor/ModelHotReloadWindow.cs.

## 2) How it all fits your workflow

- Start tiny (Cube PPO) to grok observations/actions/rewards → you’ll learn PPO knobs with quick feedback.
- Game-ify with Tag Mini-Game to get a flashy, relatable demo and practice curriculum \& reward shaping.
- Scale up to Self-Play for emergent tactics and to Ragdoll for embodied control you can reuse for NPCs.
- Polish with Domain Randomization (robustness), Reward HUD (explainability), and Recording (shareability).
- Iterate fast using the TensorBoard overlay + Model Hot-Reload to avoid context switching.

## 3) Common training commands

```
# Cube (Core)
mlagents-learn Assets/ML-Agents/Configs/cube\_ppo.yaml --run-id=cube --train

# Tag Runner (heuristic tagger)
mlagents-learn Assets/ML-Agents/Configs/runner\_ppo.yaml --run-id=runner --train

# Self-play (Runner + Tagger)
mlagents-learn Assets/ML-Agents/Configs/selfplay\_combo.yaml --run-id=tag\_selfplay --train

# Ragdoll locomotion
mlagents-learn Assets/ML-Agents/Configs/ragdoll\_ppo.yaml --run-id=ragdoll --train

# Visualize learning
tensorboard --logdir results
```

## 4) Quick tuning \& troubleshooting

- It’s learning too slowly → add more parallel arenas; increase entropy (beta), simplify rewards; start with a small arena \& no obstacles (curriculum).
- It’s unstable → reduce action magnitudes; add energy penalties; clamp torques; lower time scale a bit; verify observations (no NaNs / huge ranges).
- Ragdoll keeps falling → raise PD gains gradually; larger feet; reward uprightness more; add “stand first” curriculum.
- Policy looks good in training but bad in demo → use Domain Randomization during training; normalize observations; check that inference scene matches training setup (friction, masses, timescale).

## 5) Where files live (Starter + Extras)

### Core scripts/configs:

- Assets/Scripts/\*.cs, Assets/Scripts/Utilities/\*.cs, Assets/Editor/SceneBuilder.cs, Assets/ML-Agents/Configs/\*.yaml, Tools/

### Add-ons:

- Assets/Scripts/Tag/\*, Assets/Scripts/Ragdoll/\*, Assets/Scripts/UI/\*,
- Assets/Scripts/DomainRandomization/\*, Assets/Scripts/Recording/\*,
- Assets/Scripts/Dashboard/\*, Assets/Editor/\* (additional builders \& windows), Tools/make_gif.\*

## 6) Suggested 1-week arc (condensed)

- Day 1: Cube PPO up, TensorBoard running → first successful runs.
- Day 2: Tag Mini-Game + curriculum + reward HUD.
- Day 3–4: Ragdoll stand → walk; PD tuning; domain randomization.
- Day 5: Self-play Tag variant; compare to heuristic tagger.
- Day 6: Inference polish, Model Switcher, recordings.
- Day 7: Sizzle reel + tidy docs.
