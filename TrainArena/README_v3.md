# TrainArena v3 Add-on: Self-Play Tag + Domain Randomization + Recording

## New Editor Menu
- **Tools ▸ ML Hack ▸ Build Self-Play Tag Scene** — Runner and Tagger are both trainable.
- Domain randomization panel appears at runtime (toggle mass/friction/lighting/gravity, click Apply).

## Training both agents together
```bash
mlagents-learn Assets/ML-Agents/Configs/selfplay_combo.yaml --run-id=tag_selfplay_01 --train
```
Enter **Play** in the Self-Play Tag scene. Both behaviors will learn in one run.

## Recording
- Add `SimpleRecorder` to your camera. Press **R** to start/stop capturing PNGs to `Recordings/`.
- Convert to MP4/GIF (requires `ffmpeg`):
  - macOS/Linux:
    ```bash
    Tools/make_gif.sh Recordings out.mp4 out.gif 30
    ```
  - Windows PowerShell:
    ```powershell
    Tools/make_gif.ps1 -InputDir Recordings -OutMp4 out.mp4 -OutGif out.gif -Fps 30
    ```