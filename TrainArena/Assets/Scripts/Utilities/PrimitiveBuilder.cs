using System.Linq;
using UnityEngine;

/// <summary>
/// Centralized primitive and material creation system for TrainArena.
/// Ensures consistent URP materials and visual quality across all objects.
/// Eliminates code duplication between SceneBuilder, EnvInitializer, and other builders.
/// </summary>
public static class PrimitiveBuilder
{
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
    
    /// <summary>
    /// Creates a complete CubeAgent with proper materials, physics, and trail
    /// </summary>
    public static GameObject CreateAgent(string name = "CubeAgent")
    {
        return CreateAgent(name, Vector3.zero, new Color(0.216f, 0.490f, 1.0f)); // Default blue color - #377DFF in linear space
    }
    
    /// <summary>
    /// Creates a complete CubeAgent with proper materials, physics, and trail at specified position and color
    /// </summary>
    public static GameObject CreateAgent(string name, Vector3 position, Color color)
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

    /// <summary>
    /// Creates a simple ragdoll structure for ML-Agents training.
    /// This creates a minimal humanoid with capsule colliders and configurable joints.
    /// </summary>
    public static GameObject CreateRagdoll(string name = "RagdollAgent", Vector3 position = default)
    {
        var root = new GameObject(name);
        root.transform.position = position;

        // Create pelvis (root body - no joint, has RagdollAgent) - more realistic proportions
        var pelvis = CreateRagdollBodyPart("Pelvis", Vector3.zero, new Vector3(0.4f, 0.3f, 0.25f), root.transform);
        
        // Create legs with better spacing and proportions
        var leftThigh = CreateRagdollBodyPart("LeftThigh", new Vector3(-0.15f, -0.4f, 0), new Vector3(0.2f, 0.4f, 0.2f), pelvis.transform);
        var leftShin = CreateRagdollBodyPart("LeftShin", new Vector3(0, -0.4f, 0), new Vector3(0.15f, 0.35f, 0.15f), leftThigh.transform);
        var leftFoot = CreateRagdollBodyPart("LeftFoot", new Vector3(0, -0.2f, 0.15f), new Vector3(0.12f, 0.1f, 0.3f), leftShin.transform);

        var rightThigh = CreateRagdollBodyPart("RightThigh", new Vector3(0.15f, -0.4f, 0), new Vector3(0.2f, 0.4f, 0.2f), pelvis.transform);
        var rightShin = CreateRagdollBodyPart("RightShin", new Vector3(0, -0.4f, 0), new Vector3(0.15f, 0.35f, 0.15f), rightThigh.transform);
        var rightFoot = CreateRagdollBodyPart("RightFoot", new Vector3(0, -0.2f, 0.15f), new Vector3(0.12f, 0.1f, 0.3f), rightShin.transform);

        // Add joints to create ragdoll hierarchy with realistic human joint limits
        AddRagdollJoint(leftThigh, pelvis, new Vector3(0, 0, 0), new Vector3(60, 0, 0));   // Hip: 60° forward/back
        AddRagdollJoint(leftShin, leftThigh, new Vector3(0, 0, 0), new Vector3(120, 0, 0)); // Knee: 120° bend
        AddRagdollJoint(leftFoot, leftShin, new Vector3(0, 0, 0), new Vector3(30, 0, 0));   // Ankle: 30° flex
        
        AddRagdollJoint(rightThigh, pelvis, new Vector3(0, 0, 0), new Vector3(60, 0, 0));   // Hip: 60° forward/back  
        AddRagdollJoint(rightShin, rightThigh, new Vector3(0, 0, 0), new Vector3(120, 0, 0)); // Knee: 120° bend
        AddRagdollJoint(rightFoot, rightShin, new Vector3(0, 0, 0), new Vector3(30, 0, 0));   // Ankle: 30° flex

        // Add RagdollAgent to pelvis
        var ragdollAgent = pelvis.AddComponent<RagdollAgent>();
        ragdollAgent.pelvis = pelvis.transform;
        
        // Add BehaviorParameters for ML-Agents
        var behaviorParams = pelvis.AddComponent<Unity.MLAgents.Policies.BehaviorParameters>();
        behaviorParams.BehaviorName = "RagdollAgent";
        behaviorParams.BrainParameters.VectorObservationSize = 4; // Will be calculated automatically
        behaviorParams.BrainParameters.NumStackedVectorObservations = 1;
        behaviorParams.BrainParameters.ActionSpec = Unity.MLAgents.Actuators.ActionSpec.MakeContinuous(6); // 6 joints
        behaviorParams.BehaviorType = Unity.MLAgents.Policies.BehaviorType.Default;
        
        // Collect all PDJointControllers and assign to agent
        var joints = new System.Collections.Generic.List<PDJointController>();
        joints.AddRange(root.GetComponentsInChildren<PDJointController>());
        ragdollAgent.joints = joints;

        return root;
    }

    /// <summary>
    /// Creates a single body part (capsule with rigidbody and collider)
    /// </summary>
    private static GameObject CreateRagdollBodyPart(string name, Vector3 localPosition, Vector3 size, Transform parent)
    {
        var bodyPart = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        bodyPart.name = name;
        bodyPart.transform.parent = parent;
        bodyPart.transform.localPosition = localPosition;
        bodyPart.transform.localScale = size;
        
        // Add rigidbody with appropriate settings and realistic mass distribution
        var rb = bodyPart.AddComponent<Rigidbody>();
        rb.mass = name == "Pelvis" ? 20f : (name.Contains("Thigh") ? 10f : (name.Contains("Shin") ? 6f : 3f));
        rb.linearDamping = 0.05f;        // Lower drag for more natural movement
        rb.angularDamping = 5f;    // Higher angular drag for stability
        
        // Configure collider
        var collider = bodyPart.GetComponent<CapsuleCollider>();
        collider.material = CreatePhysicsMaterial();
        
        return bodyPart;
    }

    /// <summary>
    /// Adds a ConfigurableJoint and PDJointController to connect two body parts
    /// </summary>
    private static void AddRagdollJoint(GameObject child, GameObject parent, Vector3 axis, Vector3 limits)
    {
        var joint = child.AddComponent<ConfigurableJoint>();
        joint.connectedBody = parent.GetComponent<Rigidbody>();
        
        // Configure joint as hinge for simplicity
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
        joint.angularXMotion = ConfigurableJointMotion.Limited;
        joint.angularYMotion = ConfigurableJointMotion.Locked;
        joint.angularZMotion = ConfigurableJointMotion.Locked;
        
        // Set joint limits
        var lowLimit = joint.lowAngularXLimit;
        lowLimit.limit = -limits.x;
        joint.lowAngularXLimit = lowLimit;

        var highLimit = joint.highAngularXLimit;
        highLimit.limit = limits.x;
        joint.highAngularXLimit = highLimit;
        
        // Add PD controller
        var pdController = child.AddComponent<PDJointController>();
        pdController.joint = joint;
        pdController.minAngle = -limits.x;
        pdController.maxAngle = limits.x;
        pdController.kp = 200f;
        pdController.kd = 10f;
    }

    /// <summary>
    /// Creates physics material for ragdoll parts
    /// </summary>
    private static PhysicsMaterial CreatePhysicsMaterial()
    {
        var material = new PhysicsMaterial("RagdollMaterial");
        material.dynamicFriction = 0.6f;
        material.staticFriction = 0.6f;
        material.bounciness = 0.1f;
        return material;
    }
}