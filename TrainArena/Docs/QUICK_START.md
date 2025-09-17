# 🚀 TrainArena Quick Start

**Get from zero to trained AI agent in under 5 minutes!**

This guide gets you up and running with the essentials. For detailed explanations, see the [Complete Training Guide](TRAINING_GUIDE.md).

## ⚡ Prerequisites

- **Unity 6.2+** with ML-Agents Package v4.0.0 installed
- **Windows PowerShell** (for automated scripts)

## 🎯 4-Step Setup

### Step 1: Setup Python Environment (Automated)

```powershell
# Creates Python 3.10.11 + ML-Agents 1.1.0 environment
.\Scripts\setup_python310.ps1
```

### Step 2: Activate Environment

```powershell
# Activates Python environment with compatibility fixes
.\Scripts\activate_mlagents_py310.ps1
```

### Step 3: Verify Everything Works

```powershell
# Tests Python, ML-Agents, and mlagents-learn command
.\Scripts\check_environment.ps1
```

### Step 4: Start Training

```powershell
# Starts PPO training with automatic TensorBoard launch
.\Scripts\train_cube.ps1

# Then in Unity: Tools → ML Hack → Build Cube Training Scene → Press PLAY
```

## 🎮 Test Your Trained Model

1. **Wait for training completion** (10-20 minutes for visible results)
2. **Load your trained model:**
   - Find `Results/cube_run_*/CubeAgent.onnx`
   - In Unity scene, select a CubeAgent
   - Drag .onnx file to **Behavior Parameters > Model**
   - Set **Behavior Type** to `Inference Only`
3. **Press Play** - watch your AI navigate to goals! 🎯

## 🔗 Next Steps

- **Having issues?** → [Debug & Troubleshooting Guide](DEBUG_AND_TROUBLESHOOTING.md)
- **Want to understand training?** → [Complete Training Guide](TRAINING_GUIDE.md)
- **Ready for advanced features?** → [Advanced Features Guide](ADVANCED_FEATURES.md)
- **Want to record demos?** → [Recording & Demo Guide](RECORDING_AND_DEMO.md)

## 🆘 Quick Troubleshooting

**Environment setup fails?**

```powershell
.\Scripts\surgical_cleanup.ps1 -PythonEnv
.\Scripts\setup_python310.ps1
```

**Training won't connect?**

- Check Unity has ML-Agents package installed
- Verify BehaviorType is "Default" (not Inference Only)
- Ensure scene has CubeAgent objects

**Need more help?** See [Debug & Troubleshooting](DEBUG_AND_TROUBLESHOOTING.md) for comprehensive solutions.
