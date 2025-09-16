# ML-Agents Training Scripts - Clean Workflow

Simple, reliable workflow for ML-Agents PPO training with Unity.

## 🎯 Core Scripts (Only These 5)

### 1. **Setup** - `Scripts\setup_python310.ps1`

Creates Python 3.10.11 virtual environment with ML-Agents 1.1.0 and all compatibility fixes.

```powershell
.\Scripts\setup_python310.ps1        # Create new environment
.\Scripts\setup_python310.ps1 -Force # Recreate if exists
```

### 2. **Activate** - `activate_mlagents_py310.ps1`

Activates the Python environment and sets compatibility variables.

```powershell
.\Scripts\activate_mlagents_py310.ps1
```

### 3. **Verify** - `Scripts\check_environment.ps1`

Checks that Python, ML-Agents, and mlagents-learn command work.

```powershell
.\Scripts\check_environment.ps1
```

### 4. **Train** - `Scripts\train_cube.ps1`

Starts PPO training with automatic TensorBoard launch.

```powershell
.\Scripts\train_cube.ps1
```

### 5. **Cleanup** - `Scripts\surgical_cleanup.ps1`

Selective cleanup tool for project reset and disk space recovery.

```powershell
.\Scripts\surgical_cleanup.ps1                 # Show cleanup options
.\Scripts\surgical_cleanup.ps1 -All -ListOnly  # Preview full cleanup (6GB+)
.\Scripts\surgical_cleanup.ps1 -PythonEnv      # Clean only Python environment
.\Scripts\surgical_cleanup.ps1 -UnityLogs      # Clean only Unity cache/logs
```

## 🚀 Complete Workflow

**First time setup:**

```powershell
# 1. Create environment (one time)
.\Scripts\setup_python310.ps1

# 2. Activate environment
.\Scripts\activate_mlagents_py310.ps1

# 3. Verify everything works
.\Scripts\check_environment.ps1

# 4. Start training
.\Scripts\train_cube.ps1
# Then open Unity and press PLAY
```

**Daily usage (after setup):**

```powershell
# 1. Activate environment
.\Scripts\activate_mlagents_py310.ps1

# 2. Start training
.\Scripts\train_cube.ps1
# Then open Unity and press PLAY
```

## 🔧 Built-in Compatibility Fixes

All scripts include automatic fixes for:

- ✅ **Protobuf compatibility** (sets PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION=python)
- ✅ **Python 3.10.11 optimization** (Unity official recommendation)
- ✅ **ML-Agents 1.1.0** (latest available, fully compatible)
- ✅ **Pip corruption handling** (downloads fresh installer if needed)
- ✅ **Dependency order** (installs protobuf 3.20.3 before ML-Agents)
- ✅ **Environment validation** (checks everything before proceeding)

## 📁 File Structure

```
Scripts/
├── setup_python310.ps1         # Primary setup script
├── activate_mlagents_py310.ps1  # Environment activation
├── check_environment.ps1        # Verification script
├── train_cube.ps1              # Training launcher
├── surgical_cleanup.ps1        # Selective cleanup tool
└── purgatory/                  # Archived fix scripts (ignore)

venv/mlagents-py310/            # Python virtual environment
```

## 🆘 Troubleshooting

**Environment not working?**

```powershell
# Clean and rebuild approach
.\Scripts\surgical_cleanup.ps1 -PythonEnv
.\Scripts\setup_python310.ps1
```

**Need more disk space?**

```powershell
# See what can be cleaned (dry run)
.\Scripts\surgical_cleanup.ps1 -All -ListOnly

# Clean Unity cache/logs (recovers ~1-5GB)
.\Scripts\surgical_cleanup.ps1 -UnityLogs -Force
```

**mlagents-learn command fails?**

```powershell
# Clean rebuild of Python environment
.\Scripts\surgical_cleanup.ps1 -PythonEnv -Force
.\Scripts\setup_python310.ps1
.\Scripts\check_environment.ps1
```

**Need help?**

- Check `TRAINING_GUIDE.md` for detailed explanations
- All fix scripts are archived in `Scripts/purgatory/` if needed

## 🧹 Surgical Cleanup Features

The `surgical_cleanup.ps1` tool provides smart, selective cleanup:

- **📊 Size Analysis** - Shows actual disk usage before cleanup
- **🎯 Selective Options** - Clean only what you want (`-PythonEnv`, `-UnityLogs`, `-TrainingResults`)
- **🔍 Dry Run Mode** - Preview with `-ListOnly` before making changes
- **⚡ Smart Recovery** - Can free 6+ GB of disk space safely
- **🛡️ Safety Features** - Confirmation prompts (skip with `-Force`)
- **💡 Next Steps** - Suggests follow-up actions after cleanup

## ✨ Key Improvements

- **Single setup script** instead of multiple confusing options
- **Surgical cleanup tool** for smart project maintenance
- **Automatic compatibility** fixes built into core workflow
- **Clear error messages** with specific fix instructions
- **Clean file structure** with archived experimental scripts
- **Self-contained** - each script includes all necessary fixes
