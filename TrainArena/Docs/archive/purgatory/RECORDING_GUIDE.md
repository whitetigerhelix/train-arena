# Recording Guide - Train Arena

## Overview

Train Arena includes multiple recording systems to capture and share your trained ML-Agents models in action. This guide covers all available recording methods and their best use cases.

## Quick Start - Record Your Trained Models

### Method 1: Unity Recorder (Recommended for High Quality)

**Best for**: Professional quality videos, demos, presentations

1. **Setup Unity Recorder**

   - Open Unity Editor
   - Load your scene with trained agents running in inference mode
   - Go to `Tools ? ML Hack ? Start Recording`

2. **Recording Process**

   ```
   Unity Menu: Tools ? ML Hack ? Start Recording
   [Play your scene - agents will perform with trained models]
   Unity Menu: Tools ? ML Hack ? Stop Recording
   ```

3. **Output Location**
   - Files saved to: `Recordings/Hackathon_YYYYMMDD_HHMMSS.mp4`
   - High quality 1920x1080 MP4 with audio

### Method 2: Simple Frame Recorder (Best for Development)

**Best for**: Quick captures, debugging, lightweight recording

1. **Add Simple Recorder to Scene**

   - Add `SimpleRecorder` component to your Main Camera
   - Configure settings in inspector:
     - `fps`: 30 (default) or 60 for smoother playback
     - `outputDir`: "Recordings" (default)

2. **Recording Process**

   ```
   [Enter Play Mode]
   Press 'R' key ? Recording starts
   [Let your agents perform for desired duration]
   Press 'R' key ? Recording stops
   ```

3. **Convert to Video**

   - **Windows**: Run `Tools/make_gif.ps1`

   ```powershell
   ./Tools/make_gif.ps1 -InputDir Recordings -OutMp4 demo.mp4 -OutGif demo.gif -Fps 30
   ```

   - **macOS/Linux**: Run `Tools/make_gif.sh`

   ```bash
   ./Tools/make_gif.sh Recordings demo.mp4 demo.gif 30
   ```

## Recording Your Trained Models

### Perfect Setup for Your Success Story ⭐

1. **Prepare Your Inference Scene**

   ```csharp
   // Your models are already working perfectly! Just ensure:
   var behaviorParams = agent.GetComponent<BehaviorParameters>();
   behaviorParams.BehaviorType = BehaviorType.InferenceOnly;
   behaviorParams.Model = CubeAgent; // Your trained .onnx file

   // Models to showcase:
   // CubeAgent.onnx (final - best performance)
   // CubeAgent-349999.onnx, CubeAgent-449968.onnx (earlier checkpoints)
   ```

2. **Optimal Camera Setup for Cube Success**

   - **Single Agent Focus**: `Position(10, 8, -10)` for close-up goal navigation
   - **Multi-Agent Overview**: `Position(20, 22, -20)` looking at origin
   - **Side-by-Side Comparison**: `Position(15, 15, 0)` looking down at arena grid
   - **Debug Visualization**: Enable ML-Agents status (M key) for educational value

3. **Recording Settings for Maximum Impact**
   - **Frame Rate**: 60 FPS for smooth goal-seeking movement
   - **Resolution**: 1920x1080 for crisp agent movement detail
   - **Duration**:
     - 15-30 seconds for individual agent success
     - 45-60 seconds for multi-agent showcase
     - 5-10 seconds for quick social media clips

### Demo Scenarios to Record

#### Cube Navigation Success Stories 🎯

```csharp
// Perfect demo scenarios for your trained models:
// 1. ⭐ Cubes beelining directly to goals (your current success!)
// 2. Side-by-side: Random vs Heuristic vs Trained models
// 3. Multiple agents with different training checkpoints
// 4. Model comparison: CubeAgent-349999.onnx vs CubeAgent.onnx (final)
// 5. Debug overlay showing agent "thinking" (raycast + ML-Agents status)
// 6. Multi-arena showcase - 4+ agents performing simultaneously
```

#### Debug Visualization Recording

```csharp
// Enable debug features for educational recordings:
// Press 'R' - Show raycast visualization
// Press 'V' - Show velocity vectors
// Press 'A' - Show arena boundaries
// Press 'O' - Show observations data
// Press 'M' - Show ML-Agents status (behavior type, model info) ⭐ NEW!
// Press 'I' - Show individual agent debug info
```

## Recording Configurations

### High Quality Demo Recording

```csharp
// Unity Recorder Settings (via Tools ? ML Hack ? Start Recording)
Resolution: 1920x1080
Quality: High
Audio: Enabled
Format: MP4
Codec: H.264
```

### Quick Development Recording

```csharp
// SimpleRecorder Component Settings
fps: 30
outputDir: "Recordings"
recording: false (toggle with 'R' key)

// Converts to:
// - PNG sequence ? MP4 ? GIF
// - Configurable quality and compression
```

### Performance Considerations

#### For Smooth Recording

```csharp
// Optimize for recording:
Time.timeScale = 1.0f;              // Normal speed
Application.targetFrameRate = 60;    // Consistent framerate
QualitySettings.vSyncCount = 1;      // Prevent tearing

// Disable expensive debug features during recording:
TrainArenaDebugManager.ShowRaycastVisualization = false;
TrainArenaDebugManager.ShowObservations = false;
```

## Common Recording Workflows

### 1. Model Comparison Demo

```csharp
// Record multiple agents with different models:
// Agent 1: Heuristic (WASD/Random)
// Agent 2: Early training checkpoint
// Agent 3: Final trained model
// Agent 4: Overtrained model (if available)

// Show side-by-side performance in same environment
```

### 2. Training Progress Showcase

```csharp
// Record same scenario with models from different training steps:
// CubeAgent-100000.onnx  (early learning)
// CubeAgent-300000.onnx  (improving)
// CubeAgent-500000.onnx  (final)

// Demonstrates learning progression
```

### 3. Curriculum Demonstration

```csharp
// Record agent performance across difficulty levels:
// Easy: Large arena, no obstacles
// Medium: Normal arena, few obstacles
// Hard: Small arena, many obstacles
// Extreme: Moving obstacles, changing goals
```

## Sharing Your Recordings

### For Social Media/Web

- **Format**: MP4 or GIF
- **Duration**: 15-30 seconds
- **Resolution**: 1280x720 or 1920x1080
- **Add captions**: Explain what the agent learned

### For Presentations

- **Format**: MP4 with audio
- **Duration**: 60-120 seconds
- **Quality**: Highest available
- **Include**: Debug visualizations, metrics overlays

### For Documentation

- **Format**: GIF for inline embedding
- **Duration**: 5-15 seconds
- **Size**: Optimized for web (<2MB)
- **Focus**: Single concept demonstration

## Troubleshooting

### Recording Issues

**Problem**: Choppy/laggy recording

```csharp
// Solutions:
// 1. Reduce arena count (fewer parallel agents)
// 2. Disable debug visualizations
// 3. Lower resolution
// 4. Set consistent framerate
Application.targetFrameRate = 30;
```

**Problem**: Large file sizes

```csharp
// Solutions:
// 1. Use SimpleRecorder ? convert to compressed MP4
// 2. Record shorter clips
// 3. Lower resolution
// 4. Reduce framerate to 24-30 FPS
```

**Problem**: Model not performing as expected in recording

```csharp
// Checklist:
// 1. Verify BehaviorType.InferenceOnly is set
// 2. Check model file is properly assigned
// 3. Ensure scene setup matches training environment
// 4. Verify Time.timeScale = 1.0f
// 5. Check for domain randomization interference
```

## Advanced Recording Features

### Custom Recording Scripts

```csharp
// Extend SimpleRecorder for automated recordings:
public class AutoRecorder : SimpleRecorder
{
    public float recordDuration = 30f;
    public bool autoStart = true;

    void Start()
    {
        if (autoStart)
        {
            recording = true;
            Invoke("StopRecording", recordDuration);
        }
    }

    void StopRecording()
    {
        recording = false;
        Debug.Log($"Auto-recording complete: {frameIdx} frames");
    }
}
```

### Batch Recording Multiple Models

```csharp
// Script to automatically test and record multiple model checkpoints:
// 1. Load model checkpoint
// 2. Reset environment
// 3. Record performance
// 4. Switch to next model
// 5. Repeat
```

## File Organization

```
TrainArena/
??? Recordings/              # All recordings output here
?   ??? frame_000001.png    # SimpleRecorder PNG sequence
?   ??? demo.mp4           # Converted video
?   ??? demo.gif           # Converted GIF
?   ??? Hackathon_20250316_143022.mp4  # Unity Recorder output
??? Tools/                 # Conversion scripts
?   ??? make_gif.ps1      # Windows conversion
?   ??? make_gif.sh       # macOS/Linux conversion
??? Assets/
    ??? Scripts/Recording/ # Recording components
    ??? Editor/           # Recording utilities
```

## Example Recording Session

1. **Load your best trained model**
2. **Set up scene**: `Tools ? ML Hack ? Build Cube Training Scene`
3. **Assign model** to agents' BehaviorParameters
4. **Position camera** for good view
5. **Start recording**: `Tools ? ML Hack ? Start Recording` or press 'R'
6. **Let agents perform** for 30-60 seconds
7. **Stop recording**: `Tools ? ML Hack ? Stop Recording` or press 'R'
8. **Convert if needed**: Run make_gif script for SimpleRecorder
9. **Share your results!**

Your trained models are ready to show off! ????
