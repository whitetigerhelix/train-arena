# 🛠️ Training Tools & Model Management Guide

Complete guide to TrainArena's integrated training tools for professional ML-Agents development workflows.

## 🎛️ **Training Dashboard**

_Menu: TrainArena/Dashboard/Training Dashboard_

### **Overview**

Comprehensive ML-Agents training dashboard that combines scene building, model management, and training workflow into one unified interface.

### **Tabs & Features**

#### **🏗️ Scene Builder Tab**

- **Quick Scene Creation**: One-click cube and ragdoll arena generation
- **Configurable Settings**: Arena counts, camera setup options
- **Current Scene Info**: Real-time agent and behavior status

#### **🤖 Model Manager Tab**

- **Auto Model Detection**: Scans Assets/ML-Agents/Models/ for .onnx files
- **Quick Apply**: One-click model assignment to matching agents
- **Model Browser**: View all available models with timestamps and types

#### **🏃 Training Workflow Tab**

- **Run Preparation**: Create standardized training folder structures
- **Config Generation**: Auto-generate ML-Agents YAML configurations
- **Folder Shortcuts**: Quick access to training runs and models
- **Command Examples**: Copy-paste training commands

#### **⚙️ Settings Tab**

- **Auto-refresh**: Automatic model discovery
- **Debug Controls**: Console management and log levels

### **Usage Workflow**

1. **Build Scene**: Use Scene Builder to create training environment
2. **Prepare Training**: Generate config files and folder structure
3. **Run Training**: Use generated commands in terminal
4. **Apply Models**: Auto-assign trained models to agents

---

## 🔥 **Model Hot-Reload Window**

_Menu: TrainArena/Models/Model Hot-Reload_

### **Purpose**

Rapid iteration tool for applying the newest trained models without manual file management.

### **Features**

- **Auto-Import**: Finds newest .onnx in results directory
- **Smart Assignment**: Applies models to all matching ModelSwitcher components
- **One-Click Workflow**: Import + assign in single button press

### **Usage**

```
1. Train your model (results saved to "results/" folder)
2. Open Hot-Reload window
3. Click "Import Newest .onnx"
4. Click "Assign To All ModelSwitchers"
5. Test immediately in Unity!
```

---

## 🤖 **Model Manager System**

_Menu: TrainArena/Models/_

### **Core Features**

#### **Automatic Model Discovery**

- Scans `Assets/ML-Agents/Models/` for .onnx files
- Detects agent type from filename (Cube/Ragdoll)
- Tracks modification timestamps
- Provides standardized naming conventions

#### **Smart Model Assignment**

```csharp
ModelManager.ApplyNewestModelToAgents("Cube");     // Apply newest cube model
ModelManager.ApplyNewestModelToAgents("Ragdoll");  // Apply newest ragdoll model
```

#### **Model Selection Window**

- Visual browser for all available models
- Filter by agent type (Cube/Ragdoll/All)
- One-click apply to matching agents
- Auto-refresh every 5 seconds

### **Menu Commands**

- **Apply Newest Cube Model**: Auto-assign latest cube model
- **Apply Newest Ragdoll Model**: Auto-assign latest ragdoll model
- **Open Models Folder**: File explorer to models directory
- **Show Model Selection Window**: Visual model browser

---

## 🏃 **Training Workflow System**

### **Standardized Training Runs**

#### **Automated Folder Structure**

```
Assets/ML-Agents/TrainingRuns/
├── CubeAgent_20250919_143022/
│   ├── config/
│   │   └── CubeAgent_config.yaml
│   ├── results/
│   ├── models/
│   └── training_metadata.json
└── RagdollAgent_20250919_143125/
    ├── config/
    │   └── RagdollAgent_config.yaml
    └── ...
```

#### **Training Run Preparation**

```csharp
// Menu: TrainArena/Training/Prepare Cube Training Run
TrainingWorkflow.PrepareCubeTrainingRun();

// Menu: TrainArena/Training/Prepare Ragdoll Training Run
TrainingWorkflow.PrepareRagdollTrainingRun();
```

#### **Generated Training Commands**

After preparation, use generated commands:

```bash
# Cube Training
mlagents-learn Assets/ML-Agents/TrainingRuns/CubeAgent_20250919_143022/config/CubeAgent_config.yaml --run-id=CubeAgent_MyRun

# Ragdoll Training
mlagents-learn Assets/ML-Agents/TrainingRuns/RagdollAgent_20250919_143125/config/RagdollAgent_config.yaml --run-id=RagdollAgent_MyRun
```

### **Metadata Tracking**

Each training run includes:

- **Run ID** with timestamp
- **Agent type** and **Unity version**
- **Scene name** and **start time**
- **Custom notes** for experiment tracking

---

## 🎯 **Integration with Current System**

### **Compatibility Status** ✅

All training tools are **fully compatible** with the current TrainArena agent system:

- ✅ **CubeAgent Integration**: Automatic detection and model assignment
- ✅ **RagdollAgent Integration**: Supports 6-joint locomotion system
- ✅ **BehaviorParameters**: Works with Unity ML-Agents v4.0.0
- ✅ **Model Detection**: Recognizes both agent types from filenames

### **No Updates Required**

The training tools are designed to work with:

- Current `BehaviorParameters` setup
- Existing agent naming conventions (`CubeAgent`, `RagdollAgent`)
- Standard ML-Agents workflow
- Unity 6.2 + ML-Agents Package v4.0.0

---

## 🚀 **Quick Start Workflow**

### **For New Training**

1. **Open Training Dashboard**: `TrainArena/Dashboard/Training Dashboard`
2. **Build Scene**: Scene Builder tab → "🎯 Build Cube Arena" or "🎭 Build Ragdoll Arena"
3. **Prepare Training**: Training tab → "Prepare Cube/Ragdoll Training"
4. **Run Training**: Use generated command in terminal
5. **Apply Model**: Model Manager tab → "Apply Newest Model"

### **For Model Iteration**

1. **Train Model**: Use existing scripts or commands
2. **Hot Reload**: `TrainArena/Models/Model Hot-Reload` → "Import Newest .onnx"
3. **Test Immediately**: Models auto-assigned to agents
4. **Iterate**: Repeat for rapid testing cycles

### **For Model Management**

1. **Browse Models**: `TrainArena/Models/Show Model Selection Window`
2. **Apply Specific Model**: Select and click "Apply"
3. **Organize Models**: Models automatically named with timestamps
4. **Quick Access**: `TrainArena/Models/Open Models Folder`

---

## 🎛️ **Advanced Configuration**

### **Training Config Settings**

The workflow system generates optimized configs:

#### **Cube Agent Defaults**

```yaml
batch_size: 1024
buffer_size: 10240
learning_rate: 3e-4
max_steps: 300000
time_horizon: 64
```

#### **Ragdoll Agent Defaults**

```yaml
batch_size: 2048
buffer_size: 20480
learning_rate: 3e-4
max_steps: 1000000
time_horizon: 1000
hidden_units: 256
num_layers: 3
```

### **Customization**

Modify `TrainingConfigSettings.DefaultCube()` and `TrainingConfigSettings.DefaultRagdoll()` for different hyperparameters.

---

## 🔧 **Troubleshooting**

### **Models Not Detected**

- Ensure .onnx files are in `Assets/ML-Agents/Models/`
- Check filename contains "Cube" or "Ragdoll"
- Click "Refresh Models" in any tool

### **Assignment Not Working**

- Verify agents have `BehaviorParameters` component
- Check `BehaviorName` matches "CubeAgent" or "RagdollAgent"
- Ensure scene has active agent GameObjects

### **Training Workflow Issues**

- Run from Unity project root directory
- Ensure ML-Agents Python environment is active
- Check that config files are generated properly

---

## 📊 **Best Practices**

### **Model Organization**

- Use standardized naming: `AgentType_RunId_Timestamp_Steps.onnx`
- Keep models in `Assets/ML-Agents/Models/` for auto-detection
- Archive old models in subfolders if needed

### **Training Workflow**

- Always use Training Dashboard to prepare runs
- Include descriptive notes in training metadata
- Copy successful models to Models folder for permanent storage

### **Development Workflow**

- Use Hot-Reload for rapid iteration during development
- Use Model Manager for organized production workflows
- Use Training Dashboard for comprehensive project management

This integrated system provides a complete professional workflow from scene creation through training to model deployment! 🚀
