# 📋 Ragdoll Training Plan Tracker

## **Training Overview**

**Strategy**: Incremental training with 1M step phases for faster iteration and risk mitigation  
**Total Expected Duration**: 36-45 hours across 2-3 phases  
**Hardware Requirements**: 8GB+ RAM, GTX 1060+ GPU recommended

---

## **Phase 1: Foundation Training**

**Status**: 🟡 Ready to Launch  
**Duration**: ~18 hours  
**Steps**: 1,000,000  
**Goal**: Basic standing balance and weight shifting

### **Configuration**

```yaml
max_steps: 1.0e6
learning_rate: 3.0e-4
batch_size: 2048
time_horizon: 512
checkpoint_interval: 100000
```

### **Command**

```powershell
.\Scripts\train_ragdoll.ps1 -RunId "ragdoll_foundation"
```

### **Expected Milestones**

- **0-250K steps (0-4.5 hours)**: Random movement → basic balance attempts
- **250K-500K steps (4.5-9 hours)**: Standing stability → weight shifting
- **500K-750K steps (9-13.5 hours)**: Coordinated limb movement → stepping attempts
- **750K-1M steps (13.5-18 hours)**: Forward locomotion → basic walking patterns

### **Success Criteria**

- [ ] Agent can stand upright for >10 seconds consistently
- [ ] Demonstrates coordinated limb movement
- [ ] Shows forward locomotion attempts
- [ ] Episode length increases over training (more time upright)

### **Checkpoints to Evaluate**

- [ ] **100K steps** (1.8 hours): Basic balance attempts
- [ ] **500K steps** (9 hours): Coordinated movement
- [ ] **1M steps** (18 hours): Foundation complete

---

## **Phase 2: Locomotion Training**

**Status**: 🔄 Pending Phase 1 Completion  
**Duration**: ~18 hours  
**Steps**: 1,000,000 (additional)  
**Goal**: Forward stepping and coordinated movement

### **Configuration Options**

#### **Option A: Continue with Same Settings (Recommended)**

```powershell
.\Scripts\train_ragdoll.ps1 -RunId "ragdoll_foundation" -Resume
```

#### **Option B: New Run with Adjusted Settings**

```yaml
max_steps: 1.0e6
learning_rate: 1.0e-4 # Reduced for fine-tuning
beta: 0.005 # Less exploration, more exploitation
```

### **Expected Outcomes**

- Consistent forward walking for >5 meters
- Recovery from minor perturbations
- Smooth gait transitions
- Improved reward curves and episode lengths

### **Success Criteria**

- [ ] Agent walks forward consistently
- [ ] Maintains balance during movement
- [ ] Shows gait improvement over training
- [ ] Handles minor environment variations

---

## **Phase 3: Refinement Training** _(Optional)_

**Status**: 🔄 Pending Phase 2 Results  
**Duration**: ~9 hours  
**Steps**: 500,000  
**Goal**: Polish movement and robustness

### **Trigger Conditions**

- Phase 2 achieves basic walking but needs refinement
- Want to add domain randomization
- Need better perturbation recovery

### **Configuration**

```yaml
max_steps: 5.0e5
learning_rate: 5.0e-5 # Very low for polish
beta: 0.002 # Minimal exploration
```

### **Enhancements to Consider**

- Domain randomization (mass, friction variations)
- Perturbation forces during training
- More complex reward shaping
- Curriculum learning with obstacles

---

## **Training Monitoring**

### **TensorBoard Metrics to Track**

- **Environment/Cumulative Reward**: Should trend upward
- **Environment/Episode Length**: Should increase (more time upright)
- **Policy/Entropy**: Should decrease gradually
- **Policy/Learning Rate**: Monitor adaptive scheduling
- **Losses/Policy Loss**: Should stabilize after initial learning

### **Real-time Evaluation**

```powershell
# Monitor training progress
tensorboard --logdir=results

# Test intermediate models during training
# TrainArena → Models → Model Hot-Reload → Import Newest → Assign To All
```

### **Checkpoint Testing Protocol**

1. **Load checkpoint model** using ModelManager
2. **Switch agent behavior** to Inference Only
3. **Observe for 30 seconds** in Unity Play mode
4. **Document improvements** from previous checkpoint
5. **Continue training** or adjust hyperparameters

---

## **File Structure**

```
Results/
├── ragdoll_foundation/           # Phase 1 training run
│   ├── RagdollAgent-100000.onnx
│   ├── RagdollAgent-500000.onnx
│   ├── RagdollAgent-1000000.onnx
│   ├── RagdollAgent.onnx        # Latest model
│   ├── configuration.yaml
│   └── events.out.tfevents.*    # TensorBoard data
├── ragdoll_locomotion/          # Phase 2 (if separate run)
└── ragdoll_refined/             # Phase 3 (if needed)
```

---

## **Progress Tracking**

### **Phase 1 Progress**

**Started**: ****\_\_\_****  
**Completed**: ****\_\_\_****  
**Duration**: ****\_\_\_**** hours  
**Final Reward**: ****\_\_\_****  
**Notes**:

-
-
-

### **Phase 2 Progress**

**Started**: ****\_\_\_****  
**Completed**: ****\_\_\_****  
**Duration**: ****\_\_\_**** hours  
**Final Reward**: ****\_\_\_****  
**Notes**:

-
-
-

### **Phase 3 Progress**

**Started**: ****\_\_\_****  
**Completed**: ****\_\_\_****  
**Duration**: ****\_\_\_**** hours  
**Final Reward**: ****\_\_\_****  
**Notes**:

-
-
- ***

## **Decision Points & Adaptations**

### **After Phase 1: Foundation Results**

**If successful** (agent stands and shows movement):

- ✅ Proceed to Phase 2 with resume training
- ✅ Consider slight learning rate reduction

**If marginal** (some balance but inconsistent):

- 🔄 Continue Phase 1 for another 500K steps
- 🔧 Adjust reward function for better balance incentives

**If unsuccessful** (no meaningful progress):

- 🛠️ Review reward function and physics settings
- 🔧 Consider reduced learning rate or different hyperparameters

### **After Phase 2: Locomotion Results**

**If successful** (consistent forward walking):

- ✅ Training complete! Deploy final model
- ✅ Consider Phase 3 for robustness improvements

**If marginal** (intermittent walking):

- 🔄 Continue Phase 2 training
- 🔧 Add curriculum learning elements

**If unsuccessful** (no forward progress):

- 🛠️ Analyze reward balance (forward vs upright)
- 🔧 Consider different network architecture

---

## **Backup & Recovery**

### **Model Preservation**

- **Automatic backups**: Checkpoints every 100K steps
- **Manual backups**: Copy key models to safe location
- **Version control**: Document which models correspond to which configurations

### **Training Recovery**

```powershell
# If training interrupted, resume from last checkpoint
.\Scripts\train_ragdoll.ps1 -RunId "ragdoll_foundation" -Resume

# If resume fails, start new run and manually copy previous model
# Copy latest .onnx to new run directory before starting
```

### **Configuration Tracking**

- Keep copies of YAML configs used for each phase
- Document any manual hyperparameter changes
- Note Unity version, ML-Agents version, hardware specs

---

## **Success Metrics Summary**

| Phase       | Primary Goal     | Success Indicator         | Failure Signal                 | Next Action                       |
| ----------- | ---------------- | ------------------------- | ------------------------------ | --------------------------------- |
| **Phase 1** | Standing Balance | >10s upright consistently | Still falling after 750K steps | Extend training or adjust physics |
| **Phase 2** | Walking Motion   | >5m forward progress      | No forward movement by 750K    | Review reward balance             |
| **Phase 3** | Robustness       | Handles perturbations     | Falls from minor disturbances  | Add curriculum/randomization      |

---

## **Contact & Notes**

**Training Start Date**: ****\_\_\_****  
**Hardware Used**: ****\_\_\_****  
**Unity Version**: ****\_\_\_****  
**ML-Agents Version**: ****\_\_\_****

**Additional Notes**:

-
-
-

---

**Training Command Quick Reference**:

```powershell
# Phase 1: Start foundation training
.\Scripts\train_ragdoll.ps1 -RunId "ragdoll_foundation"

# Phase 2: Resume/continue training
.\Scripts\train_ragdoll.ps1 -RunId "ragdoll_foundation" -Resume

# Monitor progress
tensorboard --logdir=results

# Test models during training
# TrainArena → Dashboard → Training Dashboard → Model Manager → Apply Models
```
