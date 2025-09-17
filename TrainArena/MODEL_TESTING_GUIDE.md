# 🧠 Model Testing Guide

## 📋 Quick Start - Testing Your Trained CubeAgent

### **Step 1: Open Test Scene**

1. In Unity, open `Assets/Scenes/TestScene.unity`
2. This scene has **one CubeAgent** in a clean arena for easy testing

### **Step 2: Load Your Trained Model**

1. Find the **CubeAgent** in the scene hierarchy
2. In the Inspector, locate **Behavior Parameters** component
3. **Drag and drop** `Results/cube_run_20250916_155432/CubeAgent.onnx` into the **Model** field
4. **Set Behavior Type** to `Inference Only`

### **Step 3: Test Your Agent!**

1. **Press Play** in Unity
2. Watch your trained agent navigate to the green goal! 🎯
3. Use **keyboard controls**:
   - **R** = Reset agent to new position
   - **M** = Switch between modes (Inference/Heuristic/Default)

---

## 🔬 Understanding Model Performance

### **What to Look For:**

- **Direct Movement**: Agent should move somewhat directly toward the goal
- **Obstacle Avoidance**: Agent should navigate around the arena edges
- **Success Rate**: How often does it reach the goal?
- **Efficiency**: Does it take reasonable paths?

### **Behavior Modes to Test:**

1. **Inference Only** 🧠 = Your trained AI model
2. **Heuristic Only** 🎮 = Manual WASD controls (for comparison)
3. **Default** 🔄 = Training mode (will try to connect to trainer)

---

## 📊 Comparing Different Checkpoints

### **Try Different Models:**

Your training generated several checkpoints - test them to see which performs best:

1. **`CubeAgent.onnx`** ← Start here (final model)
2. **`CubeAgent-349999.onnx`** (350K steps)
3. **`CubeAgent-399989.onnx`** (400K steps)
4. **`CubeAgent-449968.onnx`** (450K steps)
5. **`CubeAgent-499958.onnx`** (500K steps)

### **How to Compare:**

1. Load one model, test for 5-10 episodes (press R to reset)
2. Note: success rate, path quality, time to goal
3. Switch to different model and repeat
4. **Keep the best performing one!**

---

## 🎯 Expected vs. Reality Check

### **What You Should See (Success Indicators):**

✅ Agent moves toward goal (not random wandering)  
✅ Reaches goal within reasonable time (10-30 seconds)  
✅ Shows some learning (better than random movement)  
✅ Responds to environment (doesn't get stuck in corners)

### **Potential Issues & Solutions:**

❌ **Agent moves randomly** → Model may need more training  
❌ **Agent gets stuck** → Try different checkpoint model  
❌ **Agent doesn't move** → Check Behavior Type = "Inference Only"  
❌ **No model loaded** → Drag .onnx file to Model field

---

## 🚀 Next Steps After Testing

### **If Model Works Well:**

1. **Celebrate!** 🎉 You've successfully trained an AI agent!
2. **Document performance** in training logs
3. **Try curriculum learning** (harder goals, obstacles)
4. **Move to Day 2 tasks** (moving hazards, improvements)

### **If Model Needs Improvement:**

1. **Try different checkpoint** (sometimes earlier ones work better)
2. **Run more training** (extend to 1M+ steps)
3. **Adjust hyperparameters** in `cube_ppo.yaml`
4. **Analyze TensorBoard** logs for training insights

### **Advanced Testing Ideas:**

- **Multiple goals** in sequence
- **Moving obstacles** to avoid
- **Performance metrics** (average time to goal)
- **Stress testing** (unusual starting positions)

---

## 💡 Understanding Training Success

### **Your Training Achievements:**

- **500K+ steps** of training completed ✅
- **Ultra-short episodes** prevented Unity hanging ✅
- **Multiple checkpoints** for comparison ✅
- **Clean training environment** with 16 parallel agents ✅
- **Optimized PPO configuration** for learning ✅

### **This Training Was Successful If:**

- Agent performs better than random movement
- Shows goal-seeking behavior
- Completes episodes more efficiently than heuristic baseline
- Demonstrates learning over time

**Remember**: Even if the agent isn't perfect, completing 500K steps of ML-Agents training is a significant achievement! 🏆
