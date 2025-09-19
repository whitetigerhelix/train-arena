using Unity.MLAgents.Policies;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Centralized primitive and material creation system for TrainArena.
/// Ensures consistent URP materials and visual quality across all objects.
/// Eliminates code duplication between SceneBuilder, EnvInitializer, and other builders.
/// </summary>
public static class PrimitiveBuilder
{
    #region Ragdoll

    /// <summary>
    /// Creates ragdoll using the stable BlockmanRagdollBuilder system
    /// This is the main entry point for ragdoll creation - delegates to BlockmanRagdollBuilder
    /// </summary>
    public static GameObject CreateRagdoll(string name = "Ragdoll", Vector3 position = default)
    {
        TrainArenaDebugManager.Log($"🎯 Creating ragdoll using BlockmanRagdollBuilder system", TrainArenaDebugManager.DebugLogLevel.Important);
        
        // Use BlockmanRagdollBuilder for consistent, stable ragdoll creation
        var ragdoll = BlockmanRagdollBuilder.Build(position, new BlockmanRagdollBuilder.Cfg());
        ragdoll.name = name;
        
        // Add ML-Agents components if not already present
        AddMLAgentsToBlockman(ragdoll);
        
        TrainArenaDebugManager.Log($"✅ Ragdoll created using BlockmanRagdollBuilder: {name}", TrainArenaDebugManager.DebugLogLevel.Important);
        return ragdoll;
    }
    
    /// <summary>
    /// Adds ML-Agents components to a blockman ragdoll if they're not already present
    /// </summary>
    private static void AddMLAgentsToBlockman(GameObject ragdoll)
    {
        var pelvis = ragdoll.transform.Find("Pelvis")?.gameObject;
        if (pelvis == null)
        {
            TrainArenaDebugManager.LogError("No Pelvis found in blockman ragdoll - ML-Agents components not added");
            return;
        }
        
        // Add RagdollAgent if not present
        var ragdollAgent = pelvis.GetComponent<RagdollAgent>();
        if (ragdollAgent == null)
        {
            ragdollAgent = pelvis.AddComponent<RagdollAgent>();
            ragdollAgent.pelvis = pelvis.transform;
            
            // Find leg joints only for ML-Agents control (6 joints for locomotion)
            var joints = new List<PDJointController>();
            var configJoints = ragdoll.GetComponentsInChildren<ConfigurableJoint>();
            
            foreach (var configJoint in configJoints)
            {
                var partName = configJoint.gameObject.name.ToLower();
                
                // Only add PDJointController to leg joints for locomotion
                bool isLegJoint = partName.Contains("upperleg") || partName.Contains("lowerleg") || partName.Contains("foot");
                
                if (isLegJoint)
                {
                    // Add PDJointController if not present
                    var pdController = configJoint.GetComponent<PDJointController>();
                    if (pdController == null)
                    {
                        pdController = configJoint.gameObject.AddComponent<PDJointController>();
                        pdController.joint = configJoint;
                        pdController.kp = 80f;
                        pdController.kd = 8f;
                        
                        // Set reasonable angle limits based on body part
                        if (partName.Contains("upperleg"))
                        {
                            pdController.minAngle = -45f;
                            pdController.maxAngle = 45f;
                        }
                        else if (partName.Contains("lowerleg"))
                        {
                            pdController.minAngle = -90f;
                            pdController.maxAngle = 90f;
                        }
                        else if (partName.Contains("foot"))
                        {
                            pdController.minAngle = -30f;
                            pdController.maxAngle = 30f;
                        }
                    }
                    
                    joints.Add(pdController);
                }
            }
            
            ragdollAgent.joints = joints;
            TrainArenaDebugManager.Log($"Added {joints.Count} PDJointControllers to RagdollAgent", TrainArenaDebugManager.DebugLogLevel.Important);
        }
        
        // Add BehaviorParameters if not present
        if (pelvis.GetComponent<BehaviorParameters>() == null)
        {
            var behaviorParameters = pelvis.AddComponent<BehaviorParameters>();
            behaviorParameters.BehaviorName = "RagdollAgent";
            
            // Use 6 continuous actions for leg joints (hips, knees, ankles)
            behaviorParameters.BrainParameters.ActionSpec = Unity.MLAgents.Actuators.ActionSpec.MakeContinuous(6);
            
            TrainArenaDebugManager.Log($"Added BehaviorParameters with {ragdollAgent.joints.Count} continuous actions", TrainArenaDebugManager.DebugLogLevel.Important);
        }
        
        // Add AutoBehaviorSwitcher if not present
        if (pelvis.GetComponent<AutoBehaviorSwitcher>() == null)
        {
            pelvis.AddComponent<AutoBehaviorSwitcher>();
        }
        
        // Add blinking eyes to head if not already present
        var head = ragdoll.transform.Find("Head")?.gameObject;
        if (head != null && head.GetComponent<EyeBlinkAnimator>() == null)
        {
            AddBlinkingEyes(head);
        }
    }

    // Unity Editor menu items for ragdoll creation
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/ML Hack/Create Test Ragdoll")]
    public static void CreateTestRagdoll()
    {
        var ragdoll = CreateRagdoll("TestRagdoll", Vector3.zero);
        UnityEditor.Selection.activeGameObject = ragdoll;
        TrainArenaDebugManager.Log($"🎭 Created test ragdoll using BlockmanRagdollBuilder system. Joints: {ragdoll.GetComponentsInChildren<PDJointController>().Length}", TrainArenaDebugManager.DebugLogLevel.Important);
    }
#endif

    #endregion Ragdoll

    #region Materials

    /// <summary>
    /// Creates a material compatible with Unity 6.2 URP with specified surface properties
    /// </summary>
    public static Material CreateURPMaterial(float smoothness = 0.0f, float metallic = 0.0f)
    {
        // Try URP Lit shader first (Unity 6.2 default), fall back to Standard for Built-in
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }
        if (shader == null)
        {
            // Final fallback to built-in unlit
            shader = Shader.Find("Unlit/Color");
        }
        
        Material mat = new Material(shader);
        
        // Set surface properties for URP/Lit or Standard shader
        if (shader.name.Contains("Universal Render Pipeline/Lit"))
        {
            // URP Lit shader properties
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic", metallic);
        }
        else if (shader.name == "Standard")
        {
            // Built-in Standard shader properties
            mat.SetFloat("_Glossiness", smoothness);
            mat.SetFloat("_Metallic", metallic);
        }
        
        return mat;
    }

    #endregion Materials

    #region Agents and Object Creation

    /// <summary>
    /// Creates a complete CubeAgent with proper materials, physics, and trail
    /// </summary>
    public static GameObject CreateCubeAgent(string name = "CubeAgent")
    {
        return CreateCubeAgent(name, Vector3.zero, new Color(0.216f, 0.490f, 1.0f)); // Default blue color - #377DFF in linear space
    }
    
    /// <summary>
    /// Creates a complete CubeAgent with proper materials, physics, and trail at specified position and color
    /// </summary>
    public static GameObject CreateCubeAgent(string name, Vector3 position, Color color)
    {
        var agent = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.DestroyImmediate(agent.GetComponent<Collider>());
        agent.name = name;
        agent.transform.position = position;
        
        // Set up material for the agent (smooth finish for nice look)
        var mr = agent.GetComponent<Renderer>();
        Material mat = CreateURPMaterial(smoothness: 0.6f, metallic: 0.1f);
        mat.color = color;
        mat.name = "AgentMaterial";
        mr.sharedMaterial = mat;
        
        // Use BoxCollider to match cube shape perfectly
        var col = agent.AddComponent<BoxCollider>();
        col.center = Vector3.zero; // Centered on the cube
        col.size = Vector3.one; // 1x1x1 cube

        var rb = agent.AddComponent<Rigidbody>();
        rb.mass = 1f;
        
        // Add trail for path visualization
        AddTrail(agent);
        
        // Add simple face to show agent orientation
        AddAgentFace(agent);
        
        return agent;
    }
    
    /// <summary>
    /// Creates a goal sphere with proper materials and effects
    /// </summary>
    public static GameObject CreateGoal(float height, string name = "Goal")
    {
        var goal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        goal.transform.localScale = Vector3.one * height;
        var mr = goal.GetComponent<Renderer>();
        
        // Goal material with emission for visibility
        Material mat = CreateURPMaterial(smoothness: 0.3f, metallic: 0.0f);
        var goldColor = new Color(1.0f, 0.835f, 0.290f); // #FFD54A in linear space
        mat.color = goldColor;
        mat.name = "GoalMaterial";
        
        // Add emission for better visibility
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.SetColor("_EmissionColor", goldColor * 2f);
            mat.EnableKeyword("_EMISSION");
        }
        
        mr.sharedMaterial = mat;
        goal.name = name;
        
        return goal;
    }
    
    /// <summary>
    /// Creates an obstacle cube with matte materials
    /// </summary>
    public static GameObject CreateObstacle(float height, float width = 0.8f, float depth = 0.8f, string name = "Obstacle")
    {
        var obs = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obs.transform.localScale = new Vector3(width, height, depth);
        var mr = obs.GetComponent<Renderer>();
        
        // Matte finish for obstacles as requested
        Material mat = CreateURPMaterial(smoothness: 0.0f, metallic: 0.0f);
        mat.color = new Color(0.906f, 0.298f, 0.235f); // #E74C3C in linear space
        mat.name = "ObstacleMaterial";
        mr.sharedMaterial = mat;
        
        obs.name = name;
        obs.tag = "Obstacle"; // Ensure proper tag for agent detection
        
        return obs;
    }
    
    /// <summary>
    /// Creates a ground plane with appropriate materials
    /// </summary>
    public static GameObject CreateGround(string name = "Ground")
    {
        return CreateGround(Vector3.zero, Vector3.one, new Color(0.788f, 0.788f, 0.788f), name); // Default ground color - #C9C9C9 in linear space
    }
    
    /// <summary>
    /// Creates a ground plane with appropriate materials at specified position and scale
    /// </summary>
    public static GameObject CreateGround(Vector3 position, Vector3 scale, Color color, string name = "Ground")
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = name;
        ground.transform.position = position;
        ground.transform.localScale = scale;
        
        // Slightly rough surface for ground
        Material groundMat = CreateURPMaterial(smoothness: 0.2f, metallic: 0.0f);
        groundMat.name = "GroundMaterial";
        ground.GetComponent<Renderer>().sharedMaterial = groundMat;
        
        ground.isStatic = true; // Ground should be static for performance
        
        return ground;
    }

    #endregion Agents and Object Creation

    #region Polish and Effects

    /// <summary>
    /// Adds trail renderer to agents for path visualization
    /// </summary>
    public static void AddTrail(GameObject agent)
    {
        var tr = agent.AddComponent<TrailRenderer>();
        tr.time = 0.35f; 
        tr.startWidth = 0.25f; 
        tr.endWidth = 0.02f;
        
        // Create and configure material before assigning to avoid edit mode leaks
        var trailMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        trailMat.SetColor("_BaseColor", new Color(0.25f, 0.7f, 1f, 0.6f));
        trailMat.name = "AgentTrailMaterial";
        tr.sharedMaterial = trailMat;
        
        tr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        
        TrainArenaDebugManager.Log($"Added trail to {agent.name}", TrainArenaDebugManager.DebugLogLevel.Verbose);
    }
    
    /// <summary>
    /// Adds simple "eyes" to show agent forward direction
    /// </summary>
    public static void AddAgentFace(GameObject agent)
    {
        // Create left eye - positioned ON the cube surface, not inside
        var leftEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leftEye.name = "LeftEye";
        leftEye.transform.parent = agent.transform;
        leftEye.transform.localPosition = new Vector3(-0.2f, 0.2f, 0.51f); // Outside cube surface (0.5 + 0.01 margin)
        leftEye.transform.localScale = Vector3.one * 0.15f; // Slightly larger for visibility
        
        // Create right eye  
        var rightEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rightEye.name = "RightEye";
        rightEye.transform.parent = agent.transform;
        rightEye.transform.localPosition = new Vector3(0.2f, 0.2f, 0.51f); // Outside cube surface
        rightEye.transform.localScale = Vector3.one * 0.15f;
        
        // Make eyes white with slight gloss for realism
        var eyeMaterial = CreateURPMaterial(smoothness: 0.8f, metallic: 0.0f);
        eyeMaterial.color = Color.white;
        eyeMaterial.name = "EyeMaterial";
        
        leftEye.GetComponent<Renderer>().sharedMaterial = eyeMaterial;
        rightEye.GetComponent<Renderer>().sharedMaterial = eyeMaterial;
        
        // Remove colliders from eyes so they don't interfere with physics
        Object.DestroyImmediate(leftEye.GetComponent<Collider>());
        Object.DestroyImmediate(rightEye.GetComponent<Collider>());
    }

    /// <summary>
    /// Adds animated blinking eyes to the head sphere
    /// </summary>
    private static void AddBlinkingEyes(GameObject head)
    {
        // Create left eye
        var leftEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leftEye.name = "LeftEye";
        leftEye.transform.parent = head.transform;
        leftEye.transform.localPosition = new Vector3(-0.15f, 0.1f, 0.4f);
        leftEye.transform.localScale = new Vector3(0.15f, 0.15f, 0.1f);

        // Remove collider from eye (decorative only)
        Object.DestroyImmediate(leftEye.GetComponent<Collider>());

        // Make eye black
        var leftEyeMat = CreateURPMaterial(smoothness: 0.9f, metallic: 0.0f);
        leftEyeMat.color = Color.black;
        leftEyeMat.name = "LeftEyeMaterial";
        leftEye.GetComponent<Renderer>().sharedMaterial = leftEyeMat;

        // Create right eye
        var rightEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rightEye.name = "RightEye";
        rightEye.transform.parent = head.transform;
        rightEye.transform.localPosition = new Vector3(0.15f, 0.1f, 0.4f);
        rightEye.transform.localScale = new Vector3(0.15f, 0.15f, 0.1f);

        // Remove collider from eye (decorative only)
        Object.DestroyImmediate(rightEye.GetComponent<Collider>());

        // Make eye black
        var rightEyeMat = CreateURPMaterial(smoothness: 0.9f, metallic: 0.0f);
        rightEyeMat.color = Color.black;
        rightEyeMat.name = "RightEyeMaterial";
        rightEye.GetComponent<Renderer>().sharedMaterial = rightEyeMat;

        // Add blinking animation component
        var blinkAnimator = head.AddComponent<EyeBlinkAnimator>();
        blinkAnimator.leftEye = leftEye.transform;
        blinkAnimator.rightEye = rightEye.transform;
    }

    #endregion Polish and Effects

    #region Utilities and Helpers

    /// <summary>
    /// Ensures a tag exists in Unity's TagManager
    /// </summary>
    public static void EnsureTagExists(string tagName)
    {
#if UNITY_EDITOR
        // First check if tag already exists
        if (UnityEditorInternal.InternalEditorUtility.tags.Contains(tagName))
            return;
            
        // Add the tag using Unity's built-in method
        UnityEditorInternal.InternalEditorUtility.AddTag(tagName);
#endif
    }

    #endregion Utilities and Helpers
}

/// <summary>
/// Simple eye blinking animation component for ragdoll heads
/// </summary>
public class EyeBlinkAnimator : MonoBehaviour
{
    [Header("Eye References")]
    public Transform leftEye;
    public Transform rightEye;
    
    [Header("Blink Settings")]
    public float blinkInterval = 3.0f;      // Time between blinks
    public float blinkDuration = 0.15f;     // How long each blink lasts
    public float blinkVariation = 1.5f;     // Random variation in blink timing
    
    private Vector3 leftEyeOriginalScale;
    private Vector3 rightEyeOriginalScale;
    private bool isBlinking = false;
    
    void Start()
    {
        if (leftEye != null) leftEyeOriginalScale = leftEye.localScale;
        if (rightEye != null) rightEyeOriginalScale = rightEye.localScale;
        
        StartCoroutine(BlinkRoutine());
    }
    
    IEnumerator BlinkRoutine()
    {
        while (true)
        {
            // Wait for random interval before next blink
            float waitTime = blinkInterval + Random.Range(-blinkVariation, blinkVariation);
            yield return new WaitForSeconds(waitTime);
            
            // Perform blink
            if (!isBlinking)
            {
                StartCoroutine(PerformBlink());
            }
        }
    }
    
    IEnumerator PerformBlink()
    {
        isBlinking = true;
        
        float halfDuration = blinkDuration * 0.5f;
        
        // Blink close (scale Y to 0)
        float elapsedTime = 0;
        while (elapsedTime < halfDuration)
        {
            float t = elapsedTime / halfDuration;
            float scaleY = Mathf.Lerp(1f, 0.05f, t); // Almost close, not completely flat
            
            if (leftEye != null)
                leftEye.localScale = new Vector3(leftEyeOriginalScale.x, leftEyeOriginalScale.y * scaleY, leftEyeOriginalScale.z);
            if (rightEye != null)
                rightEye.localScale = new Vector3(rightEyeOriginalScale.x, rightEyeOriginalScale.y * scaleY, rightEyeOriginalScale.z);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Blink open (scale Y back to 1)
        elapsedTime = 0;
        while (elapsedTime < halfDuration)
        {
            float t = elapsedTime / halfDuration;
            float scaleY = Mathf.Lerp(0.05f, 1f, t);
            
            if (leftEye != null)
                leftEye.localScale = new Vector3(leftEyeOriginalScale.x, leftEyeOriginalScale.y * scaleY, leftEyeOriginalScale.z);
            if (rightEye != null)
                rightEye.localScale = new Vector3(rightEyeOriginalScale.x, rightEyeOriginalScale.y * scaleY, rightEyeOriginalScale.z);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure eyes are fully open
        if (leftEye != null) leftEye.localScale = leftEyeOriginalScale;
        if (rightEye != null) rightEye.localScale = rightEyeOriginalScale;
        
        isBlinking = false;
    }
}