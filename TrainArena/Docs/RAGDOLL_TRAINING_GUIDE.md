# 🎭 Ragdoll Training Quick Start

## Prerequisites

1. **Activate Python Environment First:**

   ```powershell
   cd D:\train-arena\TrainArena
   .\Scripts\activate_mlagents_py310.ps1
   ```

2. **Verify Environment:**
   ```powershell
   .\Scripts\check_environment.ps1
   ```

## Start Training

### Option 1: Basic Training

```powershell
.\Scripts\train_ragdoll.ps1
```

### Option 2: Custom Configuration

```powershell
.\Scripts\train_ragdoll.ps1 -RunId "my_ragdoll_test" -TimeScale 10 -Resume
```

### Option 3: Skip TensorBoard

```powershell
.\Scripts\train_ragdoll.ps1 -SkipTensorBoard
```

## Training Process

1. Script will launch and show "Listening on port 5004"
2. **Switch to Unity** and press **PLAY** within 300 seconds
3. Watch ragdoll agents switch from Heuristic to Default behavior
4. Monitor progress at http://localhost:6006 (TensorBoard)

## Expected Timeline

- **Phase 1 (0-500k steps):** Learning to balance and not fall immediately
- **Phase 2 (500k-2M steps):** Basic locomotion patterns emerge
- **Phase 3 (2M+ steps):** Coordinated walking and navigation
- **Total time:** 2-6 hours depending on hardware

## Troubleshooting

- Make sure you're in the **TrainArena** directory (not Scripts)
- Ensure Unity has ragdoll training scene loaded
- Check that Python environment is activated
- Verify ragdoll agents have proper joint configurations

## Recent Improvements ✅

- Enhanced joint limits for natural movement
- Improved PD controller gains (kp=200f, kd=20f)
- Centralized configuration system
- Sophisticated training script with proper error handling
- Better heuristic patterns for coordinated locomotion

Happy training! 🏃‍♂️
