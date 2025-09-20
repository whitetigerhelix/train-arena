# 🤖 TrainArena ML-Agents System

## Overview

Complete ML-Agents training environment with automated scene building, model management, and training workflow utilities.

## 🚀 Quick Start

> **💡 For comprehensive training tools guide, see [Training Tools Guide](TRAINING_TOOLS_GUIDE.md)**

### **Option 1: Integrated Training Dashboard** (Recommended)

```
TrainArena → Dashboard → Training Dashboard
```

**Complete Workflow:**

1. **Scene Builder Tab**: Create cube or ragdoll training environments
2. **Training Tab**: Prepare standardized training runs
3. **Model Manager Tab**: Apply and manage trained models
4. **Settings Tab**: Configure auto-refresh and debug options

### **Option 2: Menu-Based Workflow**

#### 1. Build Training Scene

```
TrainArena → Scenes → Build [Agent] Training Scene
```

#### 2. Prepare Training Run

```
TrainArena → Training → Prepare [Agent] Training Run
```

#### 3. Apply Models

```
TrainArena → Models → Apply Newest [Agent] Model
```

### **Option 3: Script-Based Training** (Automated)

```powershell
# Cube training (with auto-environment activation)
.\Scripts\train_cube.ps1

# Ragdoll training (with auto-environment activation)
.\Scripts\train_ragdoll.ps1
```

### **Option 4: Hot-Reload Development**

```
TrainArena → Models → Model Hot-Reload
```

- **Rapid Iteration**: Import newest .onnx and apply instantly
- **Perfect for Development**: Quick testing of training progress

## 📁 Folder Structure

```
Assets/ML-Agents/
├── Models/                     # Trained .onnx model files
│   ├── CubeAgent_*.onnx       # Cube agent models
│   └── RagdollAgent_*.onnx    # Ragdoll agent models
└── TrainingRuns/              # Training run organization
    ├── CubeAgent_20241201_143022/
    │   ├── config/
    │   │   └── CubeAgent_config.yaml
    │   ├── results/           # Training output goes here
    │   ├── models/           # Backup model storage
    │   └── training_metadata.json
    └── RagdollAgent_20241201_143055/
        └── ...
```

## 🛠 System Components

### Scene Builder (`SceneBuilder.cs`)

- **Cube Training Scene**: Multi-arena cube agent environment with goals and obstacles
- **Ragdoll Training Scene**: Physics-based ragdoll locomotion environment
- **Auto-Enhancement**: Applies post-processing, skybox, lighting, camera settings
- **Model Integration**: Automatically applies newest models during scene creation

### Model Manager (`ModelManager.cs`)

- **Auto-Detection**: Scans `Assets/ML-Agents/Models/` for `.onnx` files
- **Agent Matching**: Determines agent type from filename (CubeAgent vs RagdollAgent)
- **Newest Selection**: Automatically applies most recent models by timestamp
- **Manual Selection**: Editor window for choosing specific models

### Training Workflow (`TrainingWorkflow.cs`)

- **Run Organization**: Creates timestamped folders with metadata
- **Config Generation**: Produces optimized ML-Agents YAML configs
- **Standardized Naming**: Consistent model and run naming conventions
- **Result Processing**: Utilities for copying trained models

### Scene Enhancer (`SceneEnhancer.cs`)

- **Post-Processing**: Automatic URP post-processing volume setup
- **Dynamic Skybox**: Procedural skybox generation with time-of-day
- **Lighting Setup**: Optimized lighting for training environments
- **Camera Enhancement**: Training-optimized camera settings

### Training Dashboard (`TrainingDashboard.cs`)

- **Unified Interface**: All-in-one training management window
- **Scene Info**: Real-time agent and model status display
- **Quick Actions**: One-click scene building and model application
- **Training Guidance**: Step-by-step instructions and example commands

## 🎯 Agent Types

### CubeAgent

- **Goal**: Navigate to randomly positioned target cubes
- **Observations**: Position, velocity, goal position, obstacle proximity
- **Actions**: Movement (forward/back, left/right, rotate)
- **Rewards**: Distance-based goal seeking, collision penalties
- **Training**: Typically 300k steps, smaller neural network

### RagdollAgent

- **Goal**: Bipedal locomotion and balance
- **Observations**: Joint positions, velocities, ground contacts, target direction
- **Actions**: Joint torques for hip, knee, ankle controls
- **Rewards**: Forward movement, balance maintenance, energy efficiency
- **Training**: Typically 1M+ steps, larger neural network

## ⚙️ Configuration

### Hyperparameters

**CubeAgent Defaults:**

- Batch Size: 1024
- Buffer Size: 10240
- Learning Rate: 3e-4
- Max Steps: 300,000
- Time Horizon: 64
- Hidden Units: 128

**RagdollAgent Defaults:**

- Batch Size: 2048
- Buffer Size: 20,480
- Learning Rate: 3e-4
- Max Steps: 1,000,000
- Time Horizon: 1000
- Hidden Units: 256

### Environment Settings

- **Arena Count**: Configurable per agent type (Cube: 16, Ragdoll: 4)
- **Physics**: 50Hz fixed timestep for consistent training
- **Rendering**: Enhanced with post-processing and dynamic lighting
- **Observation Space**: Normalized vectors optimized for neural networks

## 🐛 Troubleshooting

### Common Issues

**Models Not Found:**

- Ensure `.onnx` files are in `Assets/ML-Agents/Models/`
- Check file names contain "Cube" or "Ragdoll" for auto-detection
- Use "Refresh Models" button in Training Dashboard

**Agents Not Moving:**

- Verify `RequestDecision()` is called (CubeAgent requires manual calls)
- Check BehaviorParameters has model assigned and inference mode set
- Ensure Academy is initialized in scene

**Training Not Starting:**

- Verify ML-Agents Python package is installed: `pip install mlagents`
- Check Unity scene has ML-Agents Academy component
- Ensure config file paths are correct in mlagents-learn command

**Performance Issues:**

- Reduce arena count for slower systems
- Lower Time Scale in Academy for faster training
- Use GPU acceleration if available: `--torch-device=cuda`

### Debug Tools

**Debug Manager:**

- Press `H` during play mode for debug controls
- Toggle time scale, physics visualization
- Monitor agent performance metrics

**Console Logging:**

- Comprehensive logging with emoji indicators
- Debug levels: Important, Info, Verbose
- Real-time training progress updates

## 📋 Menu Reference

```
Tools → ML Hack →
├── Scene Building →
│   ├── Build Cube Training Scene
│   ├── Build Ragdoll Training Scene
│   └── Build Simple Testing Scene
├── Models →
│   ├── Apply Newest Cube Model
│   ├── Apply Newest Ragdoll Model
│   ├── Open Models Folder
│   └── Show Model Selection Window
├── Training →
│   ├── Prepare Cube Training Run
│   ├── Prepare Ragdoll Training Run
│   └── Open Training Runs Folder
└── Training Dashboard
```

## 🎮 Workflow Summary

1. **🏗 Build Scene**: Use Scene Builder to create training environment
2. **⚙️ Prepare Training**: Generate config files and folder structure
3. **🏃 Train Agent**: Run mlagents-learn with generated config
4. **📦 Deploy Model**: Copy `.onnx` to Models folder, auto-apply to scene
5. **🔄 Iterate**: Repeat with improved hyperparameters or environment changes

## 🔗 Dependencies

- **Unity 6.2+**: Latest Unity version with URP support
- **ML-Agents 2.0+**: Unity ML-Agents package
- **Python ML-Agents**: `pip install mlagents`
- **URP**: Universal Render Pipeline for enhanced visuals
- **Post-Processing**: Unity Post-Processing package

---

_Happy Training! 🚀🤖_
