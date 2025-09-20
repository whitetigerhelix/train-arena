# 🕐 Training Duration & Configuration Guide

## **Training Time Estimates**

### **Current Configuration Analysis**

| Agent Type            | Max Steps | Arena Count     | Time Horizon | Estimated Duration |
| --------------------- | --------- | --------------- | ------------ | ------------------ |
| **Cube**              | 500,000   | 4x4 = 16 arenas | 500 steps    | **~6 hours**       |
| **Ragdoll**           | 2,000,000 | 2x2 = 4 arenas  | 512 steps    | **~36 hours**      |
| **Ragdoll (Reduced)** | 1,000,000 | 2x2 = 4 arenas  | 512 steps    | **~18 hours**      |

### **Duration Calculation Formula**

```
Training Duration ≈ max_steps / (arenas × steps_per_second × time_scale)

Where:
- max_steps: Total training steps from YAML config
- arenas: Number of parallel environments (4x4=16 for cube, 2x2=4 for ragdoll)
- steps_per_second: Hardware dependent (~15-25 for ragdoll, ~50-80 for cube)
- time_scale: Unity physics multiplier (20x default)
```

### **Factors Affecting Training Speed**

#### **Performance Factors:**

- **GPU**: Barracuda inference speed affects neural network evaluation
- **CPU**: Physics simulation and multi-arena coordination
- **RAM**: Large batch sizes and buffer memory requirements
- **Unity Time Scale**: Set to 20x for accelerated training

#### **Configuration Factors:**

- **Arena Count**: More arenas = faster step collection but higher resource usage
- **Batch Size**: Larger batches = more memory but potentially faster learning
- **Time Horizon**: Episode length affects step frequency and learning dynamics
- **Checkpoint Frequency**: More frequent saves = slight performance impact

---

## **Configuration Files**

### **Cube Agent (cube_ppo.yaml)**

```yaml
max_steps: 500000 # 500K steps ≈ 6 hours
time_horizon: 500 # Episode length
batch_size: 1024 # Memory efficient
checkpoint_interval: 50000 # Every hour checkpoint
```

### **Ragdoll Agent (ragdoll_ppo.yaml)**

```yaml
max_steps: 2000000 # 2M steps ≈ 36 hours
time_horizon: 512 # Longer episodes for complex movement
batch_size: 2048 # Larger batch for stability
checkpoint_interval: 100000 # Every ~2 hour checkpoint
```

---

## **Incremental Training Strategy**

### **Curriculum Learning Approach**

#### **Phase 1: Foundation (1M steps, ~18 hours)**

- **Goal**: Learn basic standing and balance
- **Expected Results**: Agent maintains upright posture, basic weight shifting
- **Configuration**: Standard ragdoll_ppo.yaml with `max_steps: 1000000`

#### **Phase 2: Locomotion (Resume from Phase 1 + 1M steps)**

- **Goal**: Develop forward movement and coordination
- **Expected Results**: Coordinated stepping, forward progress
- **Configuration**: Resume training with `--resume` flag

#### **Phase 3: Robustness (Resume from Phase 2 + 500K steps)**

- **Goal**: Handle perturbations and varied environments
- **Expected Results**: Stable walking, recovery from falls
- **Configuration**: Add domain randomization, continue training

### **Benefits of Incremental Training:**

- **Faster Initial Results**: See progress in 18 hours vs 36 hours
- **Iterative Improvement**: Adjust hyperparameters between phases
- **Risk Mitigation**: Avoid losing 36 hours if configuration issues arise
- **Curriculum Benefits**: Easier tasks first, complex behaviors later

---

## **Resuming Training Workflows**

### **Method 1: ML-Agents Built-in Resume**

```powershell
# First training run
.\Scripts\train_ragdoll.ps1 -RunId "ragdoll_phase1"

# Resume from checkpoint (automatic detection)
.\Scripts\train_ragdoll.ps1 -RunId "ragdoll_phase1" -Resume
```

### **Method 2: Initialize from Previous Model**

```powershell
# Train Phase 1
.\Scripts\train_ragdoll.ps1 -RunId "ragdoll_phase1"

# Start Phase 2 with Phase 1's final model
# Copy: Results/ragdoll_phase1/RagdollAgent.onnx → Results/ragdoll_phase2/RagdollAgent/checkpoint.pt
.\Scripts\train_ragdoll.ps1 -RunId "ragdoll_phase2" -Resume
```

### **Method 3: Curriculum Configuration**

```yaml
# Phase 2 config: Continue from Phase 1 with adjusted parameters
behaviors:
  RagdollAgent:
    trainer_type: ppo
    max_steps: 1000000 # Additional 1M steps
    learning_rate: 1.0e-4 # Lower learning rate for fine-tuning
    # ... other parameters
```

---

## **Training Monitoring & Checkpoints**

### **Progress Indicators**

#### **Early Training (0-200K steps):**

- **Expected**: Random flailing, frequent falls
- **Success Metrics**: Reduced fall frequency, occasional balance

#### **Mid Training (200K-600K steps):**

- **Expected**: Basic balance, weight shifting
- **Success Metrics**: Standing for >10 seconds, coordinated limb movement

#### **Late Training (600K-1M steps):**

- **Expected**: Forward locomotion attempts
- **Success Metrics**: Forward progress >1m, recovery from minor perturbations

### **TensorBoard Metrics to Watch**

- **Policy/Entropy**: Should decrease gradually (exploration → exploitation)
- **Policy/Learning Rate**: Adaptive scheduling if configured
- **Environment/Episode Length**: Should increase as agent learns to stay upright
- **Environment/Cumulative Reward**: Upward trend indicates learning progress

### **Model Checkpoints**

```
Results/ragdoll_run_YYYYMMDD_HHMMSS/
├── RagdollAgent-100000.onnx    # 2-hour checkpoint
├── RagdollAgent-200000.onnx    # 4-hour checkpoint
├── RagdollAgent-500000.onnx    # 10-hour checkpoint
├── RagdollAgent-1000000.onnx   # Final 18-hour model
└── RagdollAgent.onnx           # Latest (symlink to final)
```

---

## **Hardware Recommendations**

### **Minimum Requirements**

- **GPU**: GTX 1060 6GB or equivalent
- **CPU**: 4+ cores for multi-arena physics
- **RAM**: 8GB+ (16GB recommended for ragdoll training)
- **Expected Speed**: ~36 hours for full ragdoll training

### **Recommended Setup**

- **GPU**: RTX 3060 or better
- **CPU**: 8+ cores for optimal multi-threading
- **RAM**: 16GB+ for large batch sizes
- **Expected Speed**: ~18-24 hours for full ragdoll training

### **High-Performance Setup**

- **GPU**: RTX 4070+ or equivalent
- **CPU**: 12+ cores with high single-thread performance
- **RAM**: 32GB for maximum batch sizes
- **Expected Speed**: ~12-18 hours for full ragdoll training

---

## **Troubleshooting Training Speed**

### **If Training is Too Slow:**

1. **Reduce Arena Count**: Lower grid dimensions in SceneConfiguration
2. **Decrease Batch Size**: Reduce memory pressure
3. **Lower Time Horizon**: Shorter episodes = faster step collection
4. **Increase Time Scale**: Higher than 20x (if physics remain stable)

### **If Training is Unstable:**

1. **Reduce Learning Rate**: Slower but more stable learning
2. **Increase Batch Size**: More stable gradient estimates
3. **Lower Time Scale**: More accurate physics simulation
4. **Add Curriculum**: Start with easier tasks

### **Memory Issues:**

1. **Reduce Buffer Size**: Lower `buffer_size` in YAML
2. **Smaller Batches**: Reduce `batch_size` parameter
3. **Fewer Arenas**: Modify SceneConfiguration grid dimensions
4. **Close Other Applications**: Free up system memory

---

## **Quick Reference Commands**

```powershell
# Start fresh training (18-hour duration)
.\Scripts\train_ragdoll.ps1

# Resume interrupted training
.\Scripts\train_ragdoll.ps1 -Resume

# Monitor training progress
tensorboard --logdir=results

# Apply latest model during training
# TrainArena → Models → Model Hot-Reload → Import Newest → Assign To All
```
