# 🎬 Recording & Demo Guide

Complete guide for capturing, creating, and sharing demonstrations of your trained ML-Agents models.

## 📋 Table of Contents

- [Recording Methods Overview](#-recording-methods-overview)
- [Unity Recorder (Professional Quality)](#-unity-recorder-professional-quality)
- [Simple Frame Recorder (Development)](#-simple-frame-recorder-development)
- [Video Conversion & Processing](#-video-conversion--processing)
- [Demo Best Practices](#-demo-best-practices)
- [Sharing & Export Options](#-sharing--export-options)

---

## 📊 Recording Methods Overview

TrainArena provides multiple recording systems optimized for different use cases:

| Method                    | Best For               | Quality               | Ease of Use | Performance Impact |
| ------------------------- | ---------------------- | --------------------- | ----------- | ------------------ |
| **Unity Recorder**        | Presentations, demos   | High (1080p MP4)      | Easy        | Medium             |
| **Simple Frame Recorder** | Development, debugging | Medium (PNG sequence) | Very Easy   | Low                |
| **TensorBoard Plots**     | Training analysis      | Data visualization    | Easy        | None               |

---

## 🎥 Unity Recorder (Professional Quality)

**Best for**: Professional presentations, demo reels, marketing content

### Quick Start

```csharp
// Unity Menu
Tools → ML Hack → Start Recording

// Record your scene
[Play your scene - agents perform with trained models]

// Stop recording
Tools → ML Hack → Stop Recording
```

### Setup Process

1. **Load Trained Models:**

   - Use [Model Hot-Reload](ADVANCED_FEATURES.md#-model-hot-reload-system) for quick model loading
   - Set agents to `Inference Only` mode
   - Verify agents are performing well

2. **Configure Scene:**

   - Position camera for best viewing angle
   - Enable debug visualization if desired (press `R` for raycasts)
   - Set appropriate lighting

3. **Start Recording:**
   - Use menu option or Unity Recorder window
   - Choose output settings (1920x1080 MP4 recommended)

### Output Configuration

**Default Settings:**

- **Resolution**: 1920x1080 (Full HD)
- **Format**: MP4 with H.264 compression
- **Frame Rate**: 30 FPS (smooth playback)
- **Audio**: Included if present
- **Location**: `Recordings/Hackathon_YYYYMMDD_HHMMSS.mp4`

**Custom Settings:**

```csharp
// Unity Recorder window configuration
Resolution: 1920x1080 (or higher for 4K)
Frame Rate: 30 FPS (or 60 for smoother action)
Format: MP4 (universal compatibility)
Quality: High (good balance of size/quality)
```

### Advanced Unity Recorder Features

**Multiple Camera Angles:**

- Set up multiple cameras for different perspectives
- Record simultaneously for multi-angle demos
- Switch between overhead, agent POV, and overview shots

**Timeline Integration:**

- Use Unity Timeline for complex recording sequences
- Choreograph camera movements
- Synchronize with agent demonstrations

---

## 📷 Simple Frame Recorder (Development)

**Best for**: Quick captures, debugging sessions, lightweight recording

### Setup

```csharp
// Add to Main Camera in scene
1. Select Main Camera
2. Add Component → Simple Recorder
3. Configure settings in Inspector
```

### Configuration Options

```csharp
public class SimpleRecorder : MonoBehaviour
{
    [Header("Recording Settings")]
    public int fps = 30;                    // Frame rate
    public string outputDir = "Recordings"; // Output directory
    public KeyCode recordKey = KeyCode.R;   // Toggle key

    [Header("Quality Settings")]
    public int resolutionScale = 1;         // 1=native, 2=2x, etc.
    public bool includeUI = false;          // Capture UI elements
}
```

### Recording Process

```csharp
// In Play Mode
Press 'R' key → Recording starts (red indicator appears)
[Let agents perform for desired duration]
Press 'R' key → Recording stops

// Output: PNG sequence in Recordings/ folder
// Files: frame_0001.png, frame_0002.png, etc.
```

### PNG Sequence Benefits

- **Low Performance Impact**: Minimal effect on Unity performance
- **High Quality**: Lossless PNG format
- **Flexible Processing**: Convert to any video format later
- **Frame-by-Frame Access**: Easy to extract specific frames
- **Lightweight**: Small storage footprint during recording

---

## 🔄 Video Conversion & Processing

Convert PNG sequences and create shareable video formats.

### Automated Conversion Scripts

**Windows PowerShell:**

```powershell
# Convert PNG sequence to MP4 and GIF
.\Tools\make_gif.ps1 -InputDir Recordings -OutMp4 demo.mp4 -OutGif demo.gif -Fps 30

# Parameters:
# -InputDir: Folder containing PNG sequence
# -OutMp4: Output MP4 filename
# -OutGif: Output GIF filename
# -Fps: Frame rate for output videos
```

**macOS/Linux:**

```bash
# Convert PNG sequence to MP4 and GIF
./Tools/make_gif.sh Recordings demo.mp4 demo.gif 30
```

### Manual FFmpeg Commands

**PNG Sequence to MP4:**

```bash
ffmpeg -framerate 30 -pattern_type glob -i "Recordings/*.png" -c:v libx264 -pix_fmt yuv420p demo.mp4
```

**MP4 to GIF (High Quality):**

```bash
# Generate palette for better quality
ffmpeg -i demo.mp4 -vf "fps=15,scale=720:-1:flags=lanczos,palettegen" palette.png

# Create GIF with palette
ffmpeg -i demo.mp4 -i palette.png -filter_complex "fps=15,scale=720:-1:flags=lanczos[x];[x][1:v]paletteuse" demo.gif
```

**Optimization Options:**

```bash
# Reduce file size
ffmpeg -i input.mp4 -crf 28 -preset fast output_compressed.mp4

# Create web-optimized GIF
ffmpeg -i input.mp4 -vf "fps=10,scale=480:-1:flags=lanczos" -loop 0 output_web.gif
```

---

## 🎯 Demo Best Practices

### Content Planning

**Effective Demo Structure:**

1. **Setup Shot** (2-3 seconds): Show the environment and challenge
2. **Learning Progress** (10-15 seconds): Show agents attempting task
3. **Success Demonstration** (10-20 seconds): Show trained agents succeeding
4. **Comparison** (optional): Untrained vs trained behavior

**Multi-Agent Demos:**

- Show individual agent capabilities first
- Then demonstrate interactions (cooperation/competition)
- Highlight emergent behaviors and strategies

### Visual Enhancement

**Camera Work:**

- **Overview shots**: Show complete environment and multiple agents
- **Follow shots**: Track individual agents during key actions
- **Close-ups**: Highlight precise navigation and decision-making

**Debug Visualization:**

```csharp
// Effective debug visualization for demos
Press 'R': Show raycast sensing (helps viewers understand AI perception)
Press 'V': Show velocity vectors (demonstrates movement intent)
Press 'A': Show arena boundaries (clarifies constraints)
```

**Lighting and Aesthetics:**

- Use good lighting to make agents and environment clearly visible
- Consider post-processing effects for professional appearance
- Ensure color contrast between agents, goals, and obstacles

### Timing and Pacing

**Episode Selection:**

- Record multiple episodes and select best examples
- Include both successes and interesting failure cases
- Show variety in initial conditions and outcomes

**Editing Tips:**

- Speed up boring/repetitive sections (2x-4x)
- Slow down key decision moments
- Use cuts to show multiple agents or perspectives
- Add titles/annotations for context

---

## 📈 Training Progress Documentation

### Checkpoint Comparison Videos

**Progressive Learning Demo:**

```csharp
// 1. Load early checkpoint (e.g., 100K steps)
// Record: Clumsy, inefficient behavior

// 2. Load middle checkpoint (e.g., 300K steps)
// Record: Improved but not optimal behavior

// 3. Load final model (500K steps)
// Record: Smooth, efficient behavior

// 4. Edit together showing progression
```

**Side-by-Side Comparison:**

- Split screen showing untrained vs trained
- Multiple agents with different model checkpoints
- Before/after training comparison

### TensorBoard Integration

**Training Curve Screenshots:**

- Capture key metrics from TensorBoard
- Show learning curves alongside video demonstrations
- Correlate performance improvements with visual behavior

**Performance Metrics:**

- Include episode success rate
- Show training time and computational requirements
- Document hyperparameters and configuration used

---

## 🚀 Sharing & Export Options

### Video Format Recommendations

**YouTube/Social Media:**

- **Format**: MP4 (H.264)
- **Resolution**: 1920x1080 or 1280x720
- **Frame Rate**: 30 FPS
- **Bitrate**: 8-12 Mbps for high quality

**Twitter/Short Form:**

- **Format**: MP4 (optimized)
- **Duration**: < 2 minutes for Twitter
- **Resolution**: 720p (faster upload)
- **Size**: < 512MB for most platforms

**Technical Documentation:**

- **Format**: MP4 with lossless compression
- **Resolution**: Native Unity resolution
- **Frame Rate**: 60 FPS for smooth analysis
- **Include**: Debug overlays and technical info

### GIF Optimization

**High Quality GIFs:**

```bash
# Use palette generation for better colors
ffmpeg -i input.mp4 -vf "fps=15,scale=720:-1:flags=lanczos,palettegen" palette.png
ffmpeg -i input.mp4 -i palette.png -filter_complex "fps=15,scale=720:-1:flags=lanczos[x];[x][1:v]paletteuse" output.gif
```

**Web-Optimized GIFs:**

- Max 480p resolution for fast loading
- 10-15 FPS for reasonable file size
- 5-10 second loops for engagement
- < 5MB file size for social media

### Platform-Specific Considerations

**GitHub README/Documentation:**

- Use GIFs for quick visual examples
- Host large videos externally (YouTube links)
- Include setup/result screenshots

**Portfolio/Demo Reel:**

- High-quality MP4 with professional editing
- Include context and explanation text overlays
- Show both technical details and results

**Research/Academic:**

- Include statistical overlays
- Document experimental conditions
- Provide multiple viewing angles
- Show quantitative results alongside qualitative behavior

---

## 🔗 Integration with Other Systems

### Model Hot-Reload Workflow

```csharp
// Rapid iteration for demo creation
1. Train model checkpoint
2. Hot-reload latest model (Tools → ML Hack → Model Hot-Reload)
3. Start recording
4. Capture demonstration
5. Repeat for next checkpoint
```

### Debug System Integration

**Recording with Debug Info:**

- Enable raycast visualization for AI perception demos
- Show ML-Agents status for technical audiences
- Include performance metrics overlays

### Advanced Features Recording

**Self-Play Demonstrations:**

- Record competitive agent interactions
- Show strategy evolution over time
- Capture emergent behaviors

**Domain Randomization:**

- Record agents in varied environments
- Demonstrate robustness across conditions
- Show adaptation capabilities

---

## 🛠️ Troubleshooting Recording Issues

### Performance Problems

**Unity Recorder Performance:**

- Reduce resolution if recording causes frame drops
- Close unnecessary applications during recording
- Use `--no-graphics` mode for training, windowed for recording

**Simple Recorder Issues:**

- Check available disk space (PNG sequences can be large)
- Verify write permissions to output directory
- Monitor Unity console for error messages

### Quality Issues

**Blurry or Low-Quality Output:**

- Increase recording resolution
- Check Unity Quality Settings
- Ensure adequate lighting in scene

**Choppy Playback:**

- Record at consistent frame rate
- Use fixed timestep in Unity (Edit → Project Settings → Time)
- Avoid frame rate fluctuations during recording

### File Format Problems

**Compatibility Issues:**

- Use H.264 MP4 for maximum compatibility
- Test playback on target platforms
- Include audio codec if using audio

**Large File Sizes:**

- Adjust compression settings (CRF 18-28)
- Use shorter recording durations
- Consider two-pass encoding for better compression

---

## 🔗 Related Documentation

- **Model testing**: [Quick Start Guide](QUICK_START.md#-test-your-trained-model)
- **Advanced features**: [Advanced Features Guide](ADVANCED_FEATURES.md#-model-hot-reload-system)
- **Debug visualization**: [Debug & Troubleshooting](DEBUG_AND_TROUBLESHOOTING.md#-debug-visualization-system)
- **API integration**: [API Reference](API_REFERENCE.md)
