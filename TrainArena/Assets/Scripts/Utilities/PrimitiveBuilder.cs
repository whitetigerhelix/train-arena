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

        // Create pelvis as the main body (contains RagdollAgent component)
        var pelvis = CreateRagdollBodyPart("Pelvis", Vector3.zero, new Vector3(0.4f, 0.3f, 0.25f), root.transform);
        
        // Create hierarchical skeleton: Pelvis -> Thigh -> Shin -> Foot
        // Left leg chain
        var leftThigh = CreateRagdollBodyPart("LeftThigh", new Vector3(-0.15f, -0.4f, 0), new Vector3(0.2f, 0.4f, 0.2f), pelvis.transform);
        var leftShin = CreateRagdollBodyPart("LeftShin", new Vector3(0, -0.4f, 0), new Vector3(0.15f, 0.35f, 0.15f), leftThigh.transform);
        var leftFoot = CreateRagdollBodyPart("LeftFoot", new Vector3(0, -0.35f, 0.15f), new Vector3(0.12f, 0.1f, 0.3f), leftShin.transform);

        // Right leg chain  
        var rightThigh = CreateRagdollBodyPart("RightThigh", new Vector3(0.15f, -0.4f, 0), new Vector3(0.2f, 0.4f, 0.2f), pelvis.transform);
        var rightShin = CreateRagdollBodyPart("RightShin", new Vector3(0, -0.4f, 0), new Vector3(0.15f, 0.35f, 0.15f), rightThigh.transform);
        var rightFoot = CreateRagdollBodyPart("RightFoot", new Vector3(0, -0.35f, 0.15f), new Vector3(0.12f, 0.1f, 0.3f), rightShin.transform);

        // Add joints connecting the skeletal hierarchy
        // Hips: Connect thighs to pelvis
        AddRagdollJointWithAnchors(leftThigh, pelvis, new Vector3(0, 0.2f, 0), new Vector3(-0.15f, -0.15f, 0), 45f);
        AddRagdollJointWithAnchors(rightThigh, pelvis, new Vector3(0, 0.2f, 0), new Vector3(0.15f, -0.15f, 0), 45f);
        
        // Knees: Connect shins to thighs  
        AddRagdollJointWithAnchors(leftShin, leftThigh, new Vector3(0, 0.175f, 0), new Vector3(0, -0.2f, 0), 90f);
        AddRagdollJointWithAnchors(rightShin, rightThigh, new Vector3(0, 0.175f, 0), new Vector3(0, -0.2f, 0), 90f);
        
        // Ankles: Connect feet to shins
        AddRagdollJointWithAnchors(leftFoot, leftShin, new Vector3(0, 0.05f, -0.1f), new Vector3(0, -0.175f, 0), 30f);
        AddRagdollJointWithAnchors(rightFoot, rightShin, new Vector3(0, 0.05f, -0.1f), new Vector3(0, -0.175f, 0), 30f);

        // Add RagdollAgent to pelvis
        var ragdollAgent = pelvis.AddComponent<RagdollAgent>();
        ragdollAgent.pelvis = pelvis.transform;
        
        // Note: MaxStep is left at default (0 = infinite) to match cube agent behavior
        
        // Add BehaviorParameters for ML-Agents
        var behaviorParams = pelvis.AddComponent<Unity.MLAgents.Policies.BehaviorParameters>();
        behaviorParams.BehaviorName = "RagdollAgent";
        behaviorParams.BrainParameters.VectorObservationSize = 16; // 1 uprightness + 3 pelvis vel + (6 joints * 2 obs each)
        behaviorParams.BrainParameters.NumStackedVectorObservations = 1;
        behaviorParams.BrainParameters.ActionSpec = Unity.MLAgents.Actuators.ActionSpec.MakeContinuous(6); // 6 joints
        behaviorParams.BehaviorType = Unity.MLAgents.Policies.BehaviorType.HeuristicOnly; // Start with heuristic, will be configured by SceneBuilder
        
        TrainArenaDebugManager.Log($"🎭 Ragdoll created: {name} with hierarchical skeleton - 6 joints, {behaviorParams.BrainParameters.ActionSpec.NumContinuousActions} actions", 
            TrainArenaDebugManager.DebugLogLevel.Important);
        
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
        
        // Add rigidbody with balanced mass distribution for stability
        var rb = bodyPart.AddComponent<Rigidbody>();
        rb.mass = name == "Pelvis" ? 8f : (name.Contains("Thigh") ? 6f : (name.Contains("Shin") ? 4f : 2f));
        rb.linearDamping = 0.1f;         // Moderate drag for stability
        rb.angularDamping = 8f;          // Higher angular drag for stability
        rb.interpolation = RigidbodyInterpolation.Interpolate; // Smooth movement
        
        // Configure collider
        var collider = bodyPart.GetComponent<CapsuleCollider>();
        collider.material = CreatePhysicsMaterial();
        
        return bodyPart;
    }

    /// <summary>
    /// Adds a ConfigurableJoint and PDJointController with proper anchors to connect two body parts
    /// </summary>
    private static void AddRagdollJointWithAnchors(GameObject child, GameObject parent, Vector3 childAnchor, Vector3 parentAnchor, float limitDegrees)
    {
        var joint = child.AddComponent<ConfigurableJoint>();
        joint.connectedBody = parent.GetComponent<Rigidbody>();
        
        // Set anchors - where on each body part the joint connects
        joint.anchor = childAnchor;
        joint.connectedAnchor = parentAnchor;
        
        // Configure joint as hinge for simplicity
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
        joint.angularXMotion = ConfigurableJointMotion.Limited;
        joint.angularYMotion = ConfigurableJointMotion.Locked;
        joint.angularZMotion = ConfigurableJointMotion.Locked;
        
        // Set joint limits with spring for stability
        var lowLimit = joint.lowAngularXLimit;
        lowLimit.limit = -limitDegrees;
        lowLimit.bounciness = 0.1f;
        joint.lowAngularXLimit = lowLimit;

        var highLimit = joint.highAngularXLimit;
        highLimit.limit = limitDegrees; 
        highLimit.bounciness = 0.1f;
        joint.highAngularXLimit = highLimit;
        
        // Configure joint drive for natural ragdoll physics
        var drive = joint.angularXDrive;
        drive.positionSpring = 150f;      // Reduced spring for flexibility
        drive.positionDamper = 15f;       // Light damping for natural movement
        drive.maximumForce = 300f;        // Lower force limit for smoother motion
        joint.angularXDrive = drive;
        
        // Add PD controller with natural gains for locomotion
        var pdController = child.AddComponent<PDJointController>();
        pdController.joint = joint;
        pdController.minAngle = -limitDegrees;
        pdController.maxAngle = limitDegrees;
        pdController.kp = 80f;            // Moderate strength for natural movement  
        pdController.kd = 8f;             // Light damping for fluid motion
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