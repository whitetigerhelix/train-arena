# Ragdoll Locomotion - Hackathon Sprint Plan

## 🎯 Mission: Ragdoll Walking in 3-4 Days

**Timeline:** September 17-20, 2025  
**Goal:** From standing balance to basic locomotion  
**Success Metric:** Ragdoll walks 5+ meters forward consistently

---

## 📅 Daily Sprint Breakdown

### Day 1 (Sept 17) - Foundation & Standing

**Target:** Get ragdoll standing upright for 10+ seconds

- [ ] **Morning (2-3 hours)**

  - [ ] Create basic ragdoll prefab (Unity Ragdoll Wizard)
  - [ ] Build RagdollAgent class extending Agent
  - [ ] Basic observation space (12-16 observations)
  - [ ] Simple action space (6-8 joint targets)

- [ ] **Afternoon (2-3 hours)**
  - [ ] Implement reward function (uprightness focus)
  - [ ] Setup training config (ragdoll_ppo.yaml)
  - [ ] First training run (balance only)
  - [ ] Debug and iterate

**Success Check:** Ragdoll attempts to balance, training runs without errors

### Day 2 (Sept 18) - Balance Mastery & First Steps

**Target:** Consistent standing + first forward movement

- [ ] **Morning (2-3 hours)**

  - [ ] Tune PD controller gains for stability
  - [ ] Refine reward function based on Day 1 results
  - [ ] Add forward velocity reward (small weight)
  - [ ] Continue training from best checkpoint

- [ ] **Afternoon (2-3 hours)**
  - [ ] Add basic curriculum (stand → shuffle → step)
  - [ ] Implement episode termination on fall
  - [ ] Test different training hyperparameters
  - [ ] Document what's working vs not working

**Success Check:** Ragdoll stands consistently, occasional forward steps

### Day 3 (Sept 19) - Walking Locomotion

**Target:** Coordinated walking gait for 5+ meters

- [ ] **Morning (2-3 hours)**

  - [ ] Increase forward velocity reward weight
  - [ ] Add gait smoothness rewards
  - [ ] Implement domain randomization (mass, friction)
  - [ ] Train with longer episodes (more steps)

- [ ] **Afternoon (2-3 hours)**
  - [ ] Fine-tune reward balance (speed vs stability)
  - [ ] Add perturbation resistance training
  - [ ] Test multiple model checkpoints
  - [ ] Create inference testing setup

**Success Check:** Ragdoll walks forward 5+ meters without falling

### Day 4 (Sept 20) - Polish & Demo

**Target:** Robust walking + compelling demo recording

- [ ] **Morning (1-2 hours)**

  - [ ] Final training refinements
  - [ ] Select best model checkpoint
  - [ ] Add directional control (optional stretch goal)
  - [ ] Optimize for demo recording

- [ ] **Afternoon (2-3 hours)**
  - [ ] Record demo videos (walking showcase)
  - [ ] Create comparison: ragdoll vs cube agents
  - [ ] Document lessons learned
  - [ ] Prepare presentation/sharing content

**Success Check:** High-quality demo of ragdoll locomotion ready to share

---

## 🔧 Technical Implementation (Compressed)

### Minimal Viable Ragdoll

```csharp
public class RagdollAgent : Agent
{
    // Simplified observations (12-16 total)
    // - Pelvis velocity (3)
    // - Pelvis orientation (4)
    // - Key joint angles: hips, knees (6)
    // - Ground contact (2-4)

    // Simplified actions (6-8 total)
    // - Hip targets (2)
    // - Knee targets (2)
    // - Ankle targets (2)
    // - Optional: spine/arms (2)

    // Focused rewards
    // - Upright bonus (high weight early)
    // - Forward velocity (increase over time)
    // - Energy penalty (prevent wild movements)
    // - Fall penalty (episode termination)
}
```

### Accelerated Training Config

```yaml
# ragdoll_ppo.yaml - Optimized for fast iteration
behaviors:
  RagdollAgent:
    trainer_type: ppo
    hyperparameters:
      batch_size: 1024 # Smaller for faster iteration
      buffer_size: 10240
      learning_rate: 5.0e-4 # Slightly higher for faster learning
      num_epoch: 3
    network_settings:
      hidden_units: 256 # Smaller network for faster training
      num_layers: 2
    max_steps: 1000000 # Shorter training cycles
    time_horizon: 256 # Shorter episodes initially
    summary_freq: 5000 # More frequent checkpoints
```

### Curriculum Strategy (Rapid)

```csharp
// Day 1: Stand curriculum
Episode 0-50k:   Reward = 100% upright + 0% velocity
Episode 50k+:    Reward = 80% upright + 20% velocity

// Day 2-3: Walk curriculum
Episode 100k+:   Reward = 60% upright + 40% velocity
Episode 200k+:   Reward = 40% upright + 60% velocity

// Day 4: Performance curriculum
Episode 300k+:   Add smoothness + efficiency rewards
```

---

## ⚡ Hackathon Optimizations

### Time-Saving Strategies

1. **Use Unity Ragdoll Wizard** - Don't build from scratch
2. **Start Simple** - Fewer joints, basic rewards first
3. **Parallel Training** - Multiple ragdolls per scene (4-8)
4. **Quick Iterations** - 2-3 hour training cycles, not overnight
5. **Leverage Cube Success** - Apply same debugging/monitoring approach

### Risk Mitigation (Compressed Timeline)

1. **Physics Issues** → Use conservative joint limits, start with stable config
2. **Slow Learning** → Aggressive curriculum, higher learning rates
3. **Complexity Creep** → Stick to walking goal, no fancy moves
4. **Technical Debt** → Document as you go, don't over-engineer

### Success Fallbacks

- **Minimal Success:** Ragdoll stands and shuffles forward
- **Good Success:** Ragdoll walks 5+ meters consistently
- **Great Success:** Ragdoll walks + turns on command
- **Amazing Success:** Multi-ragdoll scenarios

---

## 📊 Daily Progress Tracking

### Day 1 Metrics

- [ ] Training runs without crashes: ✅/❌
- [ ] Ragdoll attempts to stand: ✅/❌
- [ ] Balance duration: \_\_\_ seconds (target: 5+)
- [ ] Reward curve trending up: ✅/❌

### Day 2 Metrics

- [ ] Consistent standing (8/10 episodes): ✅/❌
- [ ] Forward steps attempted: ✅/❌
- [ ] Distance covered: \_\_\_ meters (target: 1+)
- [ ] Fall recovery attempts: ✅/❌

### Day 3 Metrics

- [ ] Walking gait visible: ✅/❌
- [ ] Distance walked: \_\_\_ meters (target: 5+)
- [ ] Stability under perturbation: ✅/❌
- [ ] Multiple checkpoints working: ✅/❌

### Day 4 Metrics

- [ ] Demo-ready model: ✅/❌
- [ ] Recording completed: ✅/❌
- [ ] Comparison video made: ✅/❌
- [ ] Documentation complete: ✅/❌

---

## 🎥 Demo Goals

### Minimum Viable Demo

- Ragdoll standing and taking steps forward
- Side-by-side with cube agents (variety showcase)
- ML-Agents status showing "InferenceOnly" mode

### Stretch Demo Goals

- Before/after: untrained vs trained ragdoll
- Multiple ragdolls walking simultaneously
- Debug visualization showing joint control
- Perturbation resistance (push recovery)

---

## 📝 Notes & Lessons

### Day 1 Learnings

- What worked:
- What didn't:
- Tomorrow's focus:

### Day 2 Learnings

- What worked:
- What didn't:
- Tomorrow's focus:

### Day 3 Learnings

- What worked:
- What didn't:
- Tomorrow's focus:

### Day 4 Results

- Final achievement:
- Key insights:
- Next steps (post-hackathon):

---

**🚀 Ready for Ragdoll Sprint!**
_Aggressive timeline, focused scope, maximum impact in minimum time._
