# TrainArena

**TrainArena** is a Unity 6 + ML-Agents playground for training embodied AI agents with PPO.

Built for a one-week hackathon project, it provides a clear learning path:
- Cube → Goal task (fundamentals)
- Ragdoll locomotion (control & physics)
- Tag mini-game (multi-agent)
- Self-play Runner/Tagger training
- Domain randomization for robustness
- Recording utilities for demo reels

## Quick Start
1. Open in Unity 6.2+ (URP works fine).
2. Install ML-Agents + Barracuda packages via Package Manager.
3. Use the **Tools ▸ ML Hack** menu to build scenes (Cube, Ragdoll, Tag, Self-Play).
4. Train with configs in `Assets/ML-Agents/Configs/` using `mlagents-learn`.
5. Export `.onnx` and run inference-only in Unity.

See `PLAN.md` for detailed day-by-day roadmap.

---
