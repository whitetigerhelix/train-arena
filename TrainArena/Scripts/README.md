# ML-Agents Training Scripts - Clean Workflow

Simple, reliable workflow for ML-Agents PPO training with Unity.

## 🎯 Core Scripts (Only These 4)

### 1. **Setup** - `Scripts\setup_python311.ps1`

Creates Python 3.11 virtual environment with ML-Agents and all compatibility fixes.

```powershell
.\Scripts\setup_python311.ps1        # Create new environment
.\Scripts\setup_python311.ps1 -Force # Recreate if exists
```

### 2. **Activate** - `activate_mlagents_py311.ps1`

Activates the Python environment and sets compatibility variables.

```powershell
.\activate_mlagents_py311.ps1
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

## 🚀 Complete Workflow

**First time setup:**

```powershell
# 1. Create environment (one time)
.\Scripts\setup_python311.ps1

# 2. Activate environment
.\activate_mlagents_py311.ps1

# 3. Verify everything works
.\Scripts\check_environment.ps1

# 4. Start training
.\Scripts\train_cube.ps1
# Then open Unity and press PLAY
```

**Daily usage (after setup):**

```powershell
# 1. Activate environment
.\activate_mlagents_py311.ps1

# 2. Start training
.\Scripts\train_cube.ps1
# Then open Unity and press PLAY
```

## 🔧 Built-in Compatibility Fixes

All scripts include automatic fixes for:

- ✅ **Protobuf compatibility** (sets PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION=python)
- ✅ **Python 3.11 optimization** (best compatibility with ML-Agents)
- ✅ **Pip corruption handling** (downloads fresh installer if needed)
- ✅ **Dependency order** (installs protobuf 3.20.3 before ML-Agents)
- ✅ **Environment validation** (checks everything before proceeding)

## 📁 File Structure

```
Scripts/
├── setup_python311.ps1    # Primary setup script
├── check_environment.ps1   # Verification script
├── train_cube.ps1         # Training launcher
└── purgatory/             # Archived fix scripts (ignore)

activate_mlagents_py311.ps1  # Environment activation
venv/mlagents-py311/         # Python virtual environment
```

## 🆘 Troubleshooting

**Environment not working?**

```powershell
# Clean slate approach
Remove-Item -Recurse -Force venv\mlagents-py311
.\Scripts\setup_python311.ps1
```

**mlagents-learn command fails?**

```powershell
# Manual install after activation
.\activate_mlagents_py311.ps1
pip install protobuf==3.20.3 mlagents --force-reinstall
```

**Need help?**

- Check `TRAINING_GUIDE.md` for detailed explanations
- All fix scripts are archived in `Scripts/purgatory/` if needed

## ✨ Key Improvements

- **Single setup script** instead of multiple confusing options
- **Automatic compatibility** fixes built into core workflow
- **Clear error messages** with specific fix instructions
- **Clean file structure** with archived experimental scripts
- **Self-contained** - each script includes all necessary fixes
