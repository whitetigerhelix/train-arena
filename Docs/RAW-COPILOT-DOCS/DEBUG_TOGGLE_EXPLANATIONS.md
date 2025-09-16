## Debug Toggle Explanations

### **'R' Key - Raycast Visualization**

- **ON**: Shows 8-directional raycasts from ALL agents simultaneously
- **Color coding**:
  - Red: Rays hitting obstacles
  - Cyan: Clear paths
  - Yellow spheres: Hit points
  - Magenta arrow: Direction to goal

### **'I' Key - Agent Debug Info**

- **ON**: Shows debug text overlay for the FIRST agent only (`CubeAgent_Arena_0`)
- **Displays**:
  - Agent name
  - Current velocity magnitude
  - Distance to goal
  - When observations toggle is also ON, shows full ML-Agents observation vector

### **'O' Key - Observations Display** ✨ _NEW_

- **ON**: Shows detailed ML-Agents observations for debugging training
- **Includes**:
  - Local velocity vector (3 values)
  - Local goal direction (3 values)
  - Raycast distances for all 8 directions (8 values)
  - Applied force visualization (red arrows)

### **Movement System Explanation**

Your cube agents move using **physics-based force application**:

```csharp
// Agent receives 2 continuous actions: moveX, moveZ (range -1 to +1)
Vector3 localMove = new Vector3(moveX, 0f, moveZ);
Vector3 worldForce = transform.TransformDirection(localMove) * moveAccel;
rb.AddForce(worldForce, ForceMode.Acceleration);
```

**Why this is good:**

- ✅ **Realistic physics**: Natural momentum and inertia
- ✅ **Simple action space**: Only 2 continuous values needed
- ✅ **Emergent behaviors**: Agent learns to manage momentum
- ✅ **Force application**: Applied at center of mass (not corners)

**Alternative force application methods you mentioned:**

- **Corner forces**: More complex (8 force points) but allows rotation control
- **Impulse-based**: More immediate response but less realistic
- **Direct velocity**: Bypasses physics entirely

**Current setup is optimal for learning** because:

1. **Simple action space** = faster training
2. **Physics realism** = transferable behaviors
3. **Center-of-mass forces** = stable movement

**Testing the movement:**

1. Build scene in Unity
2. Enter Play mode
3. Use **WASD keys** to control any agent (set to HeuristicOnly mode)
4. Press **'O'** to see force vectors and observations
5. Press **'I'** to see velocity and goal distance

The agents should now move properly with WASD control!
