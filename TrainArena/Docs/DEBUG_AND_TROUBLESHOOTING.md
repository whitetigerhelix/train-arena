# 🔧 Debug & Troubleshooting Guide

Comprehensive guide to TrainArena's debug systems, common issues, and solutions.

## 📋 Table of Contents

- [Debug Visualization System](#-debug-visualization-system)
- [ML-Agents Status Monitoring](#-ml-agents-status-monitoring)
- [Common Issues & Solutions](#-common-issues--solutions)
- [Scene Generation Problems](#-scene-generation-problems)
- [Training Connection Issues](#-training-connection-issues)
- [Performance Problems](#-performance-problems)

---

## 🎮 Debug Visualization System

TrainArena includes a comprehensive debug system with keyboard shortcuts for real-time monitoring.

### Keyboard Controls

| Key   | Function              | Description                                |
| ----- | --------------------- | ------------------------------------------ |
| **R** | Raycast Visualization | Show/hide raycast lines for ALL agents     |
| **I** | ML-Agents Info        | Toggle connection status display           |
| **V** | Velocity Vectors      | Show agent velocity visualization          |
| **A** | Arena Bounds          | Display arena boundaries                   |
| **M** | Mode Cycling          | Switch between Inference/Heuristic/Default |
| **H** | Heuristic Controls    | WASD manual control when in Heuristic mode |

### Debug Manager Features

**Global vs Individual Control:**

- **Global Toggle (R key)**: Shows/hides visualization for ALL agents simultaneously
- **Individual Selection**: Click any agent to see debug info (works even when global is OFF)
- **Smooth Integration**: Both modes work together without conflicts

**Centralized Debug Control:**

```csharp
TrainArenaDebugManager.ShowRaycastVisualization  // Global raycast toggle
TrainArenaDebugManager.ShowDebugInfo            // General debug info
TrainArenaDebugManager.CurrentLogLevel         // Filtered logging
```

### Visual Debug Elements

**Raycast Visualization:**

- **Green Lines**: Clear path (no obstacles detected)
- **Red Lines**: Obstacle detected within range
- **Line Length**: Proportional to detection distance
- **8 Directions**: Complete 360° obstacle sensing

**Status Information:**

```
🤖 CubeAgent_Arena_1: SUCCESS | Inference | Vel=1.23 | Goal=0.45 | Actions=127 | Step=156/500
```

**UI Debug Panel:**

- **Behavior Type**: Default/Inference/Heuristic
- **Model Status**: Loaded model information
- **Connection State**: ML-Agents trainer connection
- **Episode Info**: Steps, rewards, episode outcome

---

## 📡 ML-Agents Status Monitoring

### Connection Status Indicators

**✅ Connected (Training Mode):**

```
Status: Connected | Mode: Default | Model: None | Trainer: Active
```

**🧠 Inference Mode:**

```
Status: Inference | Mode: Inference | Model: CubeAgent.onnx | Trainer: None
```

**🎮 Manual Control:**

```
Status: Heuristic | Mode: Heuristic | Model: None | Controls: WASD
```

**❌ Disconnected:**

```
Status: Waiting | Mode: Default | Model: None | Trainer: Not Found
```

### Action Counter Tracking

Monitor if agents are receiving commands from ML-Agents:

```csharp
// Action tracking in debug logs
totalActionsReceived++; // When |moveX| > 0.01 or |moveZ| > 0.01
```

**What to Look For:**

- **Actions > 0**: Agent receiving ML-Agents commands ✅
- **Actions = 0**: No ML-Agents connection ❌
- **Vel > 0.1**: Forces being applied properly ✅
- **Vel ≈ 0**: Physics or force application issues ❌

---

## ⚠️ Common Issues & Solutions

### Python Environment Issues

**Problem**: `mlagents-learn` command not found

```powershell
# Solution: Recreate environment
.\Scripts\surgical_cleanup.ps1 -PythonEnv -Force
.\Scripts\setup_python310.ps1
.\Scripts\check_environment.ps1
```

**Problem**: Protobuf compatibility errors

```powershell
# Solution: Environment includes automatic fix
$env:PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION = "python"
# This is set automatically by activate_mlagents_py310.ps1
```

**Problem**: Module import errors

```powershell
# Solution: Verify environment activation
.\Scripts\activate_mlagents_py310.ps1
python -c "import mlagents; print('ML-Agents working!')"
```

### Unity ML-Agents Package Issues

**Problem**: ML-Agents package missing

```
Solution: Package Manager → Add by Name → com.unity.ml-agents
```

**Problem**: Barracuda package missing

```
Solution: Package Manager → Add by Name → com.unity.barracuda
```

**Problem**: Package version conflicts

```
Solution: Use ML-Agents v4.0.0 (verified compatible with Unity 6.2)
```

### Training Connection Problems

**Problem**: Unity won't connect to trainer

**Check 1 - Behavior Type:**

- Select CubeAgent in scene
- Behavior Parameters → Behavior Type must be **"Default"**
- Not "Inference Only" or "Heuristic Only"

**Check 2 - Scene Setup:**

- Scene must have active CubeAgent objects
- Use: `Tools → ML Hack → Build Cube Training Scene`
- Ensure agents are not disabled

**Check 3 - Training Command:**

```powershell
# Correct command format
mlagents-learn Assets/ML-Agents/Configs/cube_ppo.yaml --run-id=test_run --train

# Common mistake - missing --train flag
```

**Check 4 - Port Conflicts:**

```powershell
# Check if port 5004 is in use
netstat -an | Select-String "5004"

# Kill existing trainer processes
Get-Process "python" | Where-Object {$_.ProcessName -eq "python"} | Stop-Process -Force
```

---

## 🏗️ Scene Generation Problems

### Fixed Issues in Current Version

**✅ Prefab Disabling Fix:**

```csharp
// BEFORE: Prefabs disabled during creation (broke spawned instances)
cubeAgentPrefab.SetActive(false);  // Called mid-generation - BAD

// AFTER: Prefabs disabled after scene complete (spawned instances remain active)
// Moved to end of BuildCubeScene() method - GOOD
```

**✅ Arena Distribution Fix:**

- Fixed agent clustering issue
- Now properly distributed across 3x3 grid
- Proper 20-unit spacing between arena centers
- Each arena gets exactly 1 agent, 1 goal, 3-4 obstacles

### Current Scene Generation Process

1. **Ground Plane**: 200x200 unit base terrain
2. **Arena Grid**: 3x3 layout with 20-unit spacing
3. **Per Arena**: 1 agent + 1 goal + 3-4 obstacles
4. **Agent Setup**: Proper physics and ML-Agents components
5. **Debug Setup**: TrainArenaDebugManager initialization

### Troubleshooting Scene Issues

**Problem**: Scene appears empty after generation

```csharp
// Check in Hierarchy for:
// - Ground
// - EnvInitializer
// - CubeAgent_Arena_0, CubeAgent_Arena_1, etc.
```

**Problem**: Agents spawn in same location

```csharp
// Verify arena spacing in SceneBuilder.cs:
private const int TrainingArenaGridSize = 3;
private const float ArenaSpacing = 20f;
```

**Problem**: Debug visualization not working

```csharp
// Ensure TrainArenaDebugManager exists in scene
// Regenerate scene: Tools → ML Hack → Build Cube Training Scene
```

---

## 🚀 Performance Problems

### Unity Performance Issues

**Problem**: Choppy playback, 337s timeouts

**Solution 1 - Reduce Arena Count:**

```csharp
// In SceneBuilder.cs
private const int TrainingArenaGridSize = 3;  // Instead of 4 (9 vs 16 arenas)
```

**Solution 2 - Optimize Episode Length:**

```yaml
# In cube_ppo.yaml
time_horizon: 500 # Down from 1000+ for better performance
```

**Solution 3 - Disable Debug During Training:**

```csharp
// Press 'R' to turn off raycast visualization during training
// Use --no-graphics flag for headless training
```

### Agent Movement Issues

**Problem**: Agents not moving/stuck

**Debug Steps:**

1. **Check Action Counter**: Should be > 0 if receiving ML-Agents commands
2. **Check Velocity**: Should be > 0.1 if physics working
3. **Check Behavior Type**: Must be "Default" for training
4. **Check ML-Agents Connection**: Look for trainer connection status

**Force Settings Fix:**

```csharp
// Optimized CubeAgent settings
[SerializeField] private float moveAccel = 50f;  // Enhanced responsiveness
// Rigidbody: drag = 0.5f, angularDrag = 5f
```

### Training Performance Optimization

**Faster Training:**

```powershell
# Use headless mode
mlagents-learn config.yaml --run-id=test --train --no-graphics

# Increase time scale
mlagents-learn config.yaml --run-id=test --train --time-scale=20
```

**Monitor Resources:**

```powershell
# Check CPU/Memory usage
Get-Process "Unity*" | Select-Object ProcessName,CPU,WorkingSet64
Get-Process "python*" | Select-Object ProcessName,CPU,WorkingSet64
```

---

## 🛠️ Advanced Debugging Tools

### Console Logging Levels

Press `L` key to cycle through log levels:

- **None**: Minimal logging
- **Info**: Normal operation info
- **Warning**: Potential issues
- **Error**: Problems requiring attention

### Debug Log Interpretation

**Successful Episode:**

```
🤖 CubeAgent_Arena_2: SUCCESS | Inference | Vel=0.85 | Goal=0.12 | Actions=89 | Step=145/500
```

**Stuck Agent:**

```
🤖 CubeAgent_Arena_1: STUCK | Default | Vel=0.02 | Goal=4.5 | Actions=127 | Step=495/500
```

**No ML-Agents Connection:**

```
🤖 CubeAgent_Arena_0: WAITING | Default | Vel=0.00 | Goal=3.2 | Actions=0 | Step=25/500
```

### Physics Debugging

**Rigidbody Settings Check:**

```csharp
// Optimal settings for responsive movement
rb.mass = 1f;
rb.drag = 0.5f;           // Balanced momentum
rb.angularDrag = 5f;      // Reduced spinning
rb.useGravity = true;
rb.isKinematic = false;
```

**Collision Detection:**

```csharp
// Ensure proper collision detection
rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
```

---

## 🆘 Emergency Fixes

### Complete Environment Reset

If everything breaks:

```powershell
# Nuclear option - clean everything
.\Scripts\surgical_cleanup.ps1 -All -Force

# Fresh start
.\Scripts\setup_python310.ps1
.\Scripts\check_environment.ps1
```

### Unity Project Reset

If Unity issues persist:

```powershell
# Close Unity completely
# Delete Library/ and Temp/ folders
# Reopen project - Unity will regenerate
```

### Quick Diagnostic Script

```powershell
# Run comprehensive diagnostics
.\Scripts\check_environment.ps1

# Should show:
# ✅ Python 3.10.11 found
# ✅ ML-Agents 1.1.0 installed
# ✅ mlagents-learn command working
# ✅ TensorBoard available
```

---

## 🔗 Related Documentation

- **Setup help**: [Quick Start Guide](QUICK_START.md)
- **Training details**: [Training Guide](TRAINING_GUIDE.md)
- **Advanced features**: [Advanced Features Guide](ADVANCED_FEATURES.md)
- **Code reference**: [API Reference](API_REFERENCE.md)
