# Ragdoll Locomotion - Hackathon Sprint Plan

## 🎯 Mission: Ragdoll Walking in 4 Days (UPDATED)

**Timeline:** September 17-20, 2025  
**Goal:** From zero to basic ragdoll locomotion  
**Success Metric:** Ragdoll stands upright and attempts forward movement consistently

### 🚨 **REALITY CHECK (Sept 17, 2025)**

**Current State:**

- ✅ Code foundation exists (`RagdollAgent.cs`, `PDJointController.cs`)
- ✅ Training infrastructure ready (Python + ML-Agents working)
- ❌ **NO ACTUAL RAGDOLL EXISTS** - need to build from scratch
- ❌ No scene builder integration for ragdolls
- ❌ No multi-arena training setup

**Adjusted Expectations:**

- Day 1-2: Build actual ragdoll + basic training setup
- Day 3: Get standing balance working
- Day 4: Attempt forward movement (stretch: short steps)

---

## 📅 Daily Sprint Breakdown

### Day 1 (Sept 17) - **RAGDOLL CREATION + BASIC SETUP** ⚡

**Target:** Build actual ragdoll and get training pipeline running

- [ ] **Morning (2-3 hours) - BUILD THE RAGDOLL**

  - [ ] **Create ragdoll using Unity Ragdoll Wizard** (humanoid capsules)
  - [ ] **Add ConfigurableJoints** to key body parts (6-8 joints max)
  - [ ] **Integrate PDJointController** components on each joint
  - [ ] **Wire up RagdollAgent** with joint references and pelvis transform

- [ ] **Afternoon (2-3 hours) - INTEGRATION**
  - [ ] **Enhance SceneBuilder** with `CreateRagdoll()` method
  - [ ] **Create RagdollAgent prefab** for reuse
  - [ ] **Test heuristic mode** (manual wiggle) to verify joint control
  - [ ] **First training attempt** - aim for "no crashes" not "good results"

**Success Check:** Training starts without errors, ragdoll responds to actions

### Day 2 (Sept 18) - **MULTI-ARENA + BALANCE FOCUS** 🏟️

**Target:** Scale training setup + focus on standing balance

- [ ] **Morning (2-3 hours) - SCALING UP**

  - [ ] **Add ragdoll support to EnvInitializer** (multi-arena training)
  - [ ] **Create ragdoll training scene builder** (4x4 or 2x2 arenas)
  - [ ] **Optimize physics settings** for faster training iteration
  - [ ] **Tune PD controller gains** based on Day 1 learnings

- [ ] **Afternoon (2-3 hours) - REWARD ENGINEERING**
  - [ ] **Focus reward function on balance only** (ignore forward movement)
  - [ ] **Implement proper episode termination** (fall detection)
  - [ ] **Start 2-4 hour training runs** with balance-only rewards
  - [ ] **Monitor training with TensorBoard** and iterate

**Success Check:** Training shows reward improvement, ragdoll attempts to stay upright

### Day 3 (Sept 19) - **STANDING SUCCESS + MOVEMENT INTRODUCTION** 🧍

**Target:** Reliable standing + introduce forward movement rewards

- [ ] **Morning (2-3 hours) - BALANCE MASTERY**

  - [ ] **Verify standing balance works reliably** (8/10 episodes)
  - [ ] **Add curriculum progression**: standing → weight shifting
  - [ ] **Introduce small forward velocity reward** (5% of total reward)
  - [ ] **Continue training with mixed balance/movement rewards**

- [ ] **Afternoon (2-3 hours) - MOVEMENT EXPERIMENTS**
  - [ ] **Gradually increase forward movement reward weight**
  - [ ] **Monitor for balance vs movement tradeoffs**
  - [ ] **Test different episode lengths** (extend if making progress)
  - [ ] **Create inference testing setup** for model evaluation

**Success Check:** Ragdoll maintains balance AND shows forward movement attempts

### Day 4 (Sept 20) - **DEMO + DOCUMENTATION** 🎬

**Target:** Best possible demo + comprehensive documentation

- [ ] **Morning (2-3 hours) - OPTIMIZATION & POLISH**

  - [ ] **Select best performing model checkpoint**
  - [ ] **Test inference mode** with trained models
  - [ ] **Optimize ragdoll for demo recording** (visual improvements)
  - [ ] **Compare multiple training approaches** if time permits

- [ ] **Afternoon (2-3 hours) - DEMO & SHARING**
  - [ ] **Record demo videos** showing ragdoll progress (before/after training)
  - [ ] **Create side-by-side comparison** (untrained vs trained ragdoll)
  - [ ] **Document lessons learned** and training insights
  - [ ] **Update project README** with ragdoll training guide

**Success Check:** Working ragdoll demo ready to share, even if just standing/balancing

---

## 🔧 Technical Implementation (UPDATED + REALISTIC)

### **Day 1: Ragdoll Creation Checklist**

1. **Unity Ragdoll Wizard Setup**

   - Use Unity's built-in Ragdoll Wizard (Window → Ragdoll Wizard)
   - Create simple humanoid with capsule colliders
   - Focus on: pelvis, thighs, shins, feet (6 joints max initially)

2. **Integration Tasks**
   - Add `PDJointController` to each ConfigurableJoint
   - Set up `RagdollAgent` on pelvis with joint references
   - Create ragdoll prefab for reuse

### Minimal Viable Ragdoll (EXISTING CODE ANALYSIS)

```csharp
// ✅ RagdollAgent.cs ALREADY EXISTS - good foundation
public class RagdollAgent : Agent
{
    // Current observations (4 + 2*joints total)
    // ✅ Pelvis uprightness (1) - dot product with up vector
    // ✅ Pelvis velocity (3) - transformed to local space
    // ✅ Joint angles + angular velocities (2 per joint)

    // Current actions (1 per joint)
    // ✅ Joint target angles mapped from [-1,1] to joint limits

    // Current rewards (BALANCE-FOCUSED - GOOD!)
    // ✅ Upright bonus (high weight) - (uprightness - 0.8) * 0.01
    // ✅ Forward velocity (low weight) - clamped forward velocity * 0.02
    // ✅ Energy penalty - sum of squared actions * 0.001
    // ✅ Fall detection - uprightness < 0.4 or height < 0.2
}
```

### **Missing Implementation Tasks**

1. **SceneBuilder Integration** - Add `CreateRagdoll()` method
2. **EnvInitializer Support** - Multi-arena ragdoll training
3. **Prefab Creation** - Reusable ragdoll with proper setup
4. **Training Scene** - Ragdoll equivalent of cube training scene

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

## ⚡ Hackathon Optimizations (UPDATED REALITY)

### Time-Saving Strategies

1. **Use Unity Ragdoll Wizard** - Don't build from scratch ✅ **PRIORITY 1**
2. **Start Simple** - 6 joints max (hips, knees, ankles), basic rewards first
3. **Parallel Training** - Multiple ragdolls per scene (2x2 = 4 initially)
4. **Quick Iterations** - 2-3 hour training cycles, not overnight ✅ **LEARNED FROM CUBES**
5. **Leverage Cube Success** - Apply same SceneBuilder/EnvInitializer patterns

### Risk Mitigation (Compressed Timeline)

1. **Physics Issues** → Use conservative joint limits, start with stable config
2. **Slow Learning** → Aggressive curriculum, higher learning rates
3. **Complexity Creep** → Stick to walking goal, no fancy moves
4. **Technical Debt** → Document as you go, don't over-engineer

### Success Fallbacks (REALISTIC EXPECTATIONS)

- **Minimal Success:** Ragdoll created, training runs without crashing ✅ **Day 1 TARGET**
- **Good Success:** Ragdoll stands upright for 5+ seconds consistently ✅ **Day 2-3 TARGET**
- **Great Success:** Ragdoll shows forward movement attempts (shuffling/stepping)
- **Amazing Success:** Ragdoll takes multiple coordinated steps forward

### 🎯 **PRIMARY GOAL: Standing Balance**

Getting a ragdoll to reliably stand upright is already a significant ML achievement. Walking is a stretch goal.

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
