# 🧠 **Inference Model Debugging Guide**

## **🚨 Issue: Trained Model Doesn't Move Cube**

### **Enhanced Debugging Added:**

The CubeAgent now has **detailed inference logging** to help diagnose the issue.

---

## **📋 Step-by-Step Diagnosis:**

### **Step 1: Verify Model Loading**

1. **Load your model** (drag `CubeAgent.onnx` to Model field)
2. **Set Behavior Type** to `Inference Only`
3. **Press Play**
4. **Look for this log**:

```
🤖 CubeAgent_Arena_1: STUCK | InferenceOnly | Model:CubeAgent(True) | Vel=0.00 | Goal=4.5 | Actions=0 | Step=1/500
```

**Key Indicators:**

- ✅ `InferenceOnly` = Correct mode
- ✅ `Model:CubeAgent(True)` = Model loaded successfully
- ❌ `Model:NO_MODEL(False)` = Model not loaded properly

---

### **Step 2: Check Action Generation**

**Look for inference action logs:**

```
🧠 INFERENCE ACTION #1: Raw=(0.1234, -0.5678) | Clamped=(0.1234, -0.5678)
🎮 CubeAgent_Arena_1 [INFERENCE] MODEL:CubeAgent ACTION: (0.123,-0.568) → Force=28.4 | Vel=0.15
```

**Diagnosis:**

- ✅ **Actions Generated**: Model is producing outputs
- ❌ **No Action Logs**: Model might be outputting zeros
- ❌ **Actions Always (0,0)**: Model learned to do nothing

---

### **Step 3: Common Issues & Solutions**

#### **Issue A: Model Not Loaded**

**Symptoms**: `Model:NO_MODEL(False)`
**Solution**:

1. Ensure `.onnx` file is in `Results/cube_run_XXXXXX/` folder
2. Drag the **correct .onnx file** to Model field
3. Must be `CubeAgent.onnx` or `CubeAgent-XXXXX.onnx`

#### **Issue B: Wrong Behavior Type**

**Symptoms**: Shows `Default` or `HeuristicOnly` instead of `InferenceOnly`
**Solution**:

1. Select CubeAgent in scene
2. Find **BehaviorParameters** component
3. Set **Behavior Type** dropdown to `Inference Only`

#### **Issue C: Model Learned to Do Nothing**

**Symptoms**: Actions always `(0.000, 0.000)`
**Solution**:

1. **Try different checkpoint**: Use `CubeAgent-349999.onnx` instead of final model
2. **Retrain**: Model may have been undertrained or overtrained
3. **Check reward system**: Agent might have learned that not moving = best reward

#### **Issue D: Action Scale Mismatch**

**Symptoms**: Very tiny actions like `(0.001, 0.002)`
**Solution**: This might be a normalization issue from training

---

## **🔧 Quick Fixes to Try:**

### **Fix 1: Test Different Model Checkpoints**

```
Try these in order:
1. CubeAgent.onnx (final model)
2. CubeAgent-499958.onnx (late checkpoint)
3. CubeAgent-449968.onnx (earlier checkpoint)
4. CubeAgent-349999.onnx (mid-training)
```

### **Fix 2: Force Action Scaling (if actions are tiny)**

If actions are like `(0.001, 0.002)`, add this temporary fix to `OnActionReceived`:

```csharp
// Temporary fix for tiny model outputs
float moveX = Mathf.Clamp(actions.ContinuousActions[0] * 10f, -1f, 1f);  // 10x scale
float moveZ = Mathf.Clamp(actions.ContinuousActions[1] * 10f, -1f, 1f);  // 10x scale
```

### **Fix 3: Compare with Heuristic Mode**

1. **Test Heuristic**: Set Behavior Type to `Heuristic Only`
2. **Use WASD**: Agent should move with keyboard
3. **If Heuristic works**: Problem is with the model
4. **If Heuristic doesn't work**: Problem is with physics setup

---

## **🎯 Testing Workflow:**

### **Single Arena Test Scene:**

1. **Set EnvInitializer** preset to `SingleArena`
2. **Load model** in CubeAgent
3. **Set to InferenceOnly**
4. **Press Play**
5. **Watch console** for debug logs
6. **Check cube movement** - should see navigation toward green goal

### **Expected Logs (Success):**

```
🧠 INFERENCE ACTION #1: Raw=(0.4123, -0.2567) | Clamped=(0.4123, -0.2567)
🎮 CubeAgent_Arena_1 [INFERENCE] MODEL:CubeAgent ACTION: (0.412,-0.257) → Force=20.6 | Vel=0.85
🤖 CubeAgent_Arena_1: MOVING | InferenceOnly | Model:CubeAgent(True) | Vel=0.85 | Goal=3.2 | Actions=45 | Step=45/500
```

### **Problem Logs (Failure):**

```
🤖 CubeAgent_Arena_1: STUCK | InferenceOnly | Model:CubeAgent(True) | Vel=0.00 | Goal=4.5 | Actions=0 | Step=100/500
// No inference action logs = Model producing zeros
```

---

## **💡 Next Steps:**

1. **Run the enhanced debugging** and share the logs
2. **We'll identify** exactly what the model is outputting
3. **Apply specific fix** based on the diagnosis

**The new logging should immediately show us what's wrong!** 🕵️‍♂️
