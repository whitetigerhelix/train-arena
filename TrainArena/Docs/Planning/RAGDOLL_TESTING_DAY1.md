# 🎭 Ragdoll Testing Instructions (Day 1)

## ✅ What We've Built

1. **Programmatic ragdoll creation** - `PrimitiveBuilder.CreateRagdoll()`
2. **Enhanced scene builders** - Two new menu items:
   - `Tools → ML Hack → Build Ragdoll Test Scene` (single ragdoll)
   - `Tools → ML Hack → Build Ragdoll Training Scene` (2x2 grid, 4 ragdolls)
3. **Training automation** - `Scripts/train_ragdoll_sprint.ps1`

## 🧪 Testing Steps

### Step 1: Build Test Scene

1. **Open Unity Editor** with TrainArena project
2. **Menu: Tools → ML Hack → Build Ragdoll Test Scene**
3. **Press Play** - ragdoll should fall and respond to physics
4. **Press 'H'** to enable heuristic mode (wiggling joints)

### Step 2: Verify Joint Control

- **Heuristic mode** should make joints move in sine wave pattern
- **Physics** should be stable (no explosive behavior)
- **Ragdoll should maintain structure** (not fall apart)

### Step 3: Build Training Scene

1. **Menu: Tools → ML Hack → Build Ragdoll Training Scene**
2. **Verify** 4 ragdolls in 2x2 grid layout
3. **Press Play** - all 4 should respond independently

### Step 4: First Training Run

```powershell
cd d:\train-arena\TrainArena
.\Scripts\train_ragdoll_sprint.ps1
```

## 🎯 Success Criteria (Day 1)

- [ ] **Scene builds without errors**
- [ ] **Ragdoll responds to heuristic input** (H key)
- [ ] **Training starts without crashing** (runs for 1+ minutes)
- [ ] **TensorBoard shows reward data** (can be negative, that's OK)

## 🐛 Common Issues & Solutions

### "PDJointController missing"

- **Fix:** Ensure Unity compiled all scripts before building scene

### "Training hangs immediately"

- **Fix:** Check ML-Agents Academy is present in scene

### "Ragdoll explodes/falls apart"

- **Fix:** Lower PD controller gains (kp=100, kd=5)

### "No rewards showing"

- **Fix:** Verify BehaviorParameters.BehaviorName = "RagdollAgent"

---

**Next:** Once this works, we move to Day 2 (multi-arena optimization)!
