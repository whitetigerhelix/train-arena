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
            
            // Use continuous actions matching the number of joints
            behaviorParameters.BrainParameters.ActionSpec = Unity.MLAgents.Actuators.ActionSpec.MakeContinuous(ragdollAgent.joints.Count);
            
            // Configure observation space using RagdollAgent's calculation
            int totalObservations = ragdollAgent.GetTotalObservationCount();
            behaviorParameters.BrainParameters.VectorObservationSize = totalObservations;
            
            TrainArenaDebugManager.Log($"Added BehaviorParameters: {ragdollAgent.joints.Count} actions, {totalObservations} observations " +
                                     $"({RagdollAgent.UPRIGHTNESS_OBSERVATIONS} uprightness + {RagdollAgent.VELOCITY_OBSERVATIONS} velocity + {ragdollAgent.joints.Count * RagdollAgent.JOINT_STATE_OBSERVATIONS_PER_JOINT} joint states)", 
                                     TrainArenaDebugManager.DebugLogLevel.Important);
        }

        // Add blinking animation for visual polish
        var headVisualObject = ragdoll.transform.Find("Head")?.transform.Find("Visual")?.gameObject;
        if (headVisualObject != null)
        {
            headVisualObject.AddComponent<EyeBlinker>();
        }
        else
        {
            TrainArenaDebugManager.LogWarning("Failed to find head visual object for the eyes!");
        }

        // Add debug info component for development
        pelvis.AddComponent<AgentDebugInfo>();
        
        // Add domain randomization for physics testing (random mass, friction, etc.)
        var domainRandomizer = pelvis.AddComponent<DomainRandomizer>();
        domainRandomizer.randomizeMass = true;
        domainRandomizer.randomizeFriction = true;
        domainRandomizer.randomizeGravity = false; // Keep gravity stable for ragdolls
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

        agent.AddComponent<CubeAgent>();
        
        // Add BehaviorParameters if not present
        if (agent.GetComponent<BehaviorParameters>() == null)
        {
            var behaviorParameters = agent.AddComponent<BehaviorParameters>();
            behaviorParameters.BehaviorName = "CubeAgent";

            // Configure action and observation space
            if (behaviorParameters.BrainParameters.ActionSpec.NumContinuousActions == 0)
            {
                // Set up action space: 2 continuous actions (moveX, moveZ)
                behaviorParameters.BrainParameters.ActionSpec = 
                    Unity.MLAgents.Actuators.ActionSpec.MakeContinuous(2);
            }
        }

        // Add blinking animation for visual polish
        agent.AddComponent<EyeBlinker>();

        // Add debug info component for development
        agent.AddComponent<AgentDebugInfo>();
        
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
        EnsureTagExists("Obstacle");
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
