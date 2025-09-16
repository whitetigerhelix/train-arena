# 🧹 Project Clean Reset Guide

This guide shows how to completely reset your ML-Agents project to a fresh state for clean testing.

## 🔄 **Complete Reset Options**

### **Option 1: Nuclear Reset (Most Thorough)**

```powershell
# WARNING: This removes ALL untracked files and directories
git clean -fdx

# This will remove:
# - All virtual environments (venv/)
# - All Python cache (__pycache__/)
# - All build artifacts
# - All temporary files
# - Any files not tracked by git
```

### **Option 2: Surgical Reset (Recommended)**

```powershell
# Remove only Python-related artifacts
Remove-Item -Recurse -Force "venv" -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force "__pycache__" -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force "*.pyc" -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force ".pytest_cache" -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force "results" -ErrorAction SilentlyContinue
Remove-Item "activate_mlagents_*.ps1" -ErrorAction SilentlyContinue

# Remove any Python installations we made (if you want to)
# Remove-Item -Recurse -Force "C:\Python310" -ErrorAction SilentlyContinue
```

### **Option 3: Virtual Environment Only**

```powershell
# Remove only virtual environments (keeps system Python)
Remove-Item -Recurse -Force "venv" -ErrorAction SilentlyContinue
Remove-Item "activate_mlagents_*.ps1" -ErrorAction SilentlyContinue
```

## ✅ **After Reset - Fresh Setup**

1. **Verify Clean State**

   ```powershell
   git status
   # Should show only tracked files, no virtual environments
   ```

2. **Fresh Setup**

   ```powershell
   .\Scripts\setup_python310.ps1 -Force
   ```

3. **Verify Installation**
   ```powershell
   .\activate_mlagents_py310.ps1
   .\Scripts\check_environment.ps1
   ```

## 📋 **What Gets Reset**

| Item                  | Option 1 (Nuclear) | Option 2 (Surgical) | Option 3 (Venv Only) |
| --------------------- | ------------------ | ------------------- | -------------------- |
| Virtual environments  | ✅                 | ✅                  | ✅                   |
| Python cache files    | ✅                 | ✅                  | ❌                   |
| Training results      | ✅                 | ✅                  | ❌                   |
| Unity build files     | ✅                 | ❌                  | ❌                   |
| System Python 3.10.11 | ❌                 | Optional            | ❌                   |
| Git tracked files     | ❌                 | ❌                  | ❌                   |

## 🎯 **Recommended Workflow**

**For testing setup script reliability:**

```powershell
# 1. Surgical reset
Remove-Item -Recurse -Force "venv" -ErrorAction SilentlyContinue
Remove-Item "activate_mlagents_*.ps1" -ErrorAction SilentlyContinue

# 2. Fresh setup
.\Scripts\setup_python310.ps1 -Force

# 3. Test
.\activate_mlagents_py310.ps1
.\Scripts\check_environment.ps1
```

**For complete fresh start:**

```powershell
git clean -fdx
.\Scripts\setup_python310.ps1
```

## ⚠️ **Important Notes**

- **System Python**: Option 2 can optionally remove `C:\Python310` if you want to test Python installation too
- **Git Safety**: All options preserve git-tracked files
- **Unity Projects**: Options 2&3 preserve Unity project files and settings
- **Training Data**: Option 1 removes training results, others preserve them

## 🔍 **Verify Clean State**

After reset, this should show a clean project:

```powershell
ls venv          # Should not exist
ls *.ps1         # Should only show git-tracked files
python --version # Should show system Python or error if removed
```
