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
    /// Configurable parameters for ragdoll creation and AI training
    /// </summary>
    [System.Serializable]
    public class RagdollConfiguration
    {
        [Header("Body Part Dimensions - ANATOMICALLY CORRECT HUMAN PROPORTIONS")]
        // Based on real human anatomy: average adult male proportions
        public Vector3 pelvisScale = new Vector3(0.35f, 0.25f, 0.25f);    // Human pelvis width ~35cm, height ~25cm  
        public Vector3 thighScale = new Vector3(0.15f, 0.45f, 0.15f);     // Thigh diameter ~15cm, length ~45cm (femur)
        public Vector3 shinScale = new Vector3(0.12f, 0.40f, 0.12f);      // Shin diameter ~12cm, length ~40cm (tibia) 
        public Vector3 footScale = new Vector3(0.26f, 0.06f, 0.10f);      // Foot length ~26cm, height ~6cm, width ~10cm
        
        [Header("Joint Connection Points - ANATOMICALLY CORRECT")]
        // These define where each body part connects to its parent (at the parent's bottom)
        public float hipSocketDepth = 0.125f;      // How deep into pelvis the hip joint goes
        public float kneeConnectionHeight = 0.225f; // Distance from thigh center to knee joint
        public float ankleConnectionHeight = 0.20f;  // Distance from shin center to ankle joint
        
        [Header("Mass Distribution (AI Training Optimized)")]
        public float pelvisMass = 20f;      // Heavier pelvis for stability
        public float thighMass = 10f;       // Proportional thigh mass
        public float shinMass = 6f;         // Realistic shin mass
        public float footMass = 3f;         // Heavier feet for ground contact stability
        
        [Header("Physics Settings")]
        public float linearDamping = 0.1f;
        public float angularDamping = 3f;
        public RigidbodyInterpolation interpolation = RigidbodyInterpolation.Interpolate;
        
        [Header("Physics Material")]
        public float dynamicFriction = 0.6f;
        public float staticFriction = 0.6f;
        public float bounciness = 0.1f;
        
        [Header("Joint Limits (REALISTIC HUMAN RANGE)")]
        public float hipJointLimit = 70f;      // Realistic hip flexion/extension range
        public float kneeJointLimit = 120f;    // Natural knee bending range
        public float ankleJointLimit = 30f;    // Realistic ankle dorsiflexion/plantarflexion
        
        [Header("Joint Physics (Muscle-like Behavior)")]
        public float jointBounciness = 0.1f;
        public float jointSpring = 500f;       // Muscle-like spring response
        public float jointDamper = 50f;        // Stability damping
        public float jointMaxForce = 1000f;    // Maximum force application
        
        [Header("PD Controller (AI Training)")]
        public float pdKp = 400f;              // Higher proportional gain for stable control
        public float pdKd = 30f;               // Increased damping for smoother movement
        
        [Header("ML-Agents Configuration")]
        public int vectorObservationSize = 4;  // Will be calculated automatically
        public int continuousActions = 6;      // 6 joints for bipedal locomotion
        
        /// <summary>
        /// Default configuration optimized for AI training
        /// </summary>
        public static RagdollConfiguration AITraining()
        {
            var config = new RagdollConfiguration(); // Uses default values above
            TrainArenaDebugManager.Log($"📋 AI_TRAINING CONFIG CREATED - Pelvis: {config.pelvisScale}, Thigh: {config.thighScale}, Shin: {config.shinScale}, Foot: {config.footScale}", TrainArenaDebugManager.DebugLogLevel.Important);
            return config;
        }
        
        /// <summary>
        /// Configuration for realistic human proportions
        /// </summary>
        public static RagdollConfiguration RealisticHuman()
        {
            var config = new RagdollConfiguration();
            // Use the base functional proportions - they're already realistic
            // Just fine-tune slightly
            config.pelvisScale = new Vector3(0.45f, 0.28f, 0.28f);         // Slightly smaller pelvis
            config.thighScale = new Vector3(0.20f, 0.75f, 0.20f);          // Proportional thigh
            config.shinScale = new Vector3(0.16f, 0.65f, 0.16f);           // Proportional shin
            config.footScale = new Vector3(0.22f, 0.08f, 0.4f);            // Realistic flat foot
            config.hipJointLimit = 65f;     // Conservative hip range
            config.kneeJointLimit = 110f;   // Natural knee range
            config.ankleJointLimit = 25f;   // Conservative ankle range
            config.pdKp = 350f;             // Moderate control strength
            config.pdKd = 25f;              // Balanced damping
            return config;
        }
        
        /// <summary>
        /// Configuration matching the inspiration images - very elongated human proportions
        /// </summary>
        public static RagdollConfiguration InspirationMatch()
        {
            var config = new RagdollConfiguration();
            // Based on inspiration images but with functional proportions
            config.pelvisScale = new Vector3(0.4f, 0.25f, 0.25f);          // Compact but functional pelvis
            config.thighScale = new Vector3(0.18f, 0.9f, 0.18f);           // Elongated but sturdy thighs
            config.shinScale = new Vector3(0.15f, 0.8f, 0.15f);            // Elongated but sturdy shins
            config.footScale = new Vector3(0.2f, 0.06f, 0.35f);            // Flat, functional feet
            // Anatomical connection settings for elongated limbs
            config.hipSocketDepth = 0.06f;      // Shallow hip socket for elongated look
            config.kneeConnectionHeight = 0.20f; // Extended knee position for longer thigh
            config.ankleConnectionHeight = 0.18f; // Extended ankle position for longer shin
            config.hipJointLimit = 70f;
            config.kneeJointLimit = 120f;
            config.ankleJointLimit = 30f;
            config.pdKp = 400f;
            config.pdKd = 30f;
            return config;
        }

        /// <summary>
        /// Anatomically correct human proportions based on real human anatomy
        /// </summary>
        public static RagdollConfiguration AnatomicalHuman()
        {
            var config = new RagdollConfiguration();
            // Perfect human proportions based on anatomical data
            config.pelvisScale = new Vector3(0.35f, 0.25f, 0.25f);        // Real human pelvis proportions
            config.thighScale = new Vector3(0.15f, 0.45f, 0.15f);         // Real human thigh (femur) proportions
            config.shinScale = new Vector3(0.12f, 0.40f, 0.12f);          // Real human shin (tibia) proportions  
            config.footScale = new Vector3(0.26f, 0.06f, 0.10f);          // Real human foot proportions
            // Anatomical connection settings
            config.hipSocketDepth = 0.08f;      // Realistic hip socket depth
            config.kneeConnectionHeight = 0.15f; // Natural knee position
            config.ankleConnectionHeight = 0.12f; // Natural ankle position
            // Joint limits based on human anatomy
            config.hipJointLimit = 70f;         // Natural hip flexion range
            config.kneeJointLimit = 120f;       // Natural knee flexion range
            config.ankleJointLimit = 30f;       // Natural ankle range
            config.pdKp = 400f;                 // Strong control for stability
            config.pdKd = 35f;                  // Good damping
            return config;
        }

        /// <summary>
        /// Configuration optimized specifically for stable walking and locomotion
        /// </summary>
        public static RagdollConfiguration FunctionalWalker()
        {
            var config = new RagdollConfiguration();
            // Optimized for walking stability and balance
            config.pelvisScale = new Vector3(0.5f, 0.3f, 0.3f);            // Stable pelvis for balance
            config.thighScale = new Vector3(0.25f, 0.7f, 0.25f);           // Strong thighs for power
            config.shinScale = new Vector3(0.2f, 0.65f, 0.2f);             // Sturdy shins for support
            config.footScale = new Vector3(0.3f, 0.08f, 0.5f);             // Large flat feet for stability
            // Anatomical connection settings optimized for walking stability
            config.hipSocketDepth = 0.10f;      // Deeper hip socket for stability
            config.kneeConnectionHeight = 0.18f; // Natural knee position
            config.ankleConnectionHeight = 0.15f; // Natural ankle position
            // Optimized for learning to walk
            config.hipJointLimit = 80f;     // Good range for walking
            config.kneeJointLimit = 130f;   // Full walking range
            config.ankleJointLimit = 35f;   // Good ankle flexibility
            config.pdKp = 450f;             // Strong control for stability
            config.pdKd = 35f;              // Good damping for smooth movement
            return config;
        }

        /// <summary>
        /// Configuration for high-performance athletic movement
        /// </summary>
        public static RagdollConfiguration Athletic()
        {
            var config = new RagdollConfiguration();
            config.hipJointLimit = 120f;
            config.kneeJointLimit = 160f;
            config.ankleJointLimit = 60f;
            config.pdKp = 400f;
            config.pdKd = 25f;
            config.jointMaxForce = 1500f;
            return config;
        }
    }
    
    // Default configuration - can be overridden
    private static RagdollConfiguration _currentConfig = null;
    public static RagdollConfiguration CurrentConfig 
    { 
        get 
        { 
            if (_currentConfig == null) 
            {
                TrainArenaDebugManager.Log($"⚠️ CurrentConfig was NULL, creating default AITraining config", TrainArenaDebugManager.DebugLogLevel.Important);
                _currentConfig = RagdollConfiguration.AITraining();
            }
            return _currentConfig;
        } 
        set 
        { 
            TrainArenaDebugManager.Log($"🔄 CurrentConfig being set to new value", TrainArenaDebugManager.DebugLogLevel.Important);
            _currentConfig = value; 
        } 
    }
    
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
    /// <summary>
    /// Creates a configurable ragdoll structure for ML-Agents training
    /// </summary>
    public static GameObject CreateRagdoll(string name = "RagdollAgent", Vector3 position = default)
    {
        TrainArenaDebugManager.Log($"🎯 RAGDOLL CREATION ENTRY POINT - Using CurrentConfig: {CurrentConfig != null}", TrainArenaDebugManager.DebugLogLevel.Important);
        if (CurrentConfig == null)
        {
            TrainArenaDebugManager.Log($"❌ CurrentConfig is NULL! Creating default...", TrainArenaDebugManager.DebugLogLevel.Important);
            CurrentConfig = RagdollConfiguration.AITraining();
        }
        TrainArenaDebugManager.Log($"🎯 DELEGATING TO CreateRagdoll with config...", TrainArenaDebugManager.DebugLogLevel.Important);
        return CreateRagdoll(name, position, CurrentConfig);
    }
    
    /// <summary>
    /// Creates a ragdoll with custom configuration
    /// </summary>
    public static GameObject CreateRagdoll(string name, Vector3 position, RagdollConfiguration config)
    {
        TrainArenaDebugManager.Log($"🚀 CREATING ANATOMICALLY CORRECT RAGDOLL: {name}", TrainArenaDebugManager.DebugLogLevel.Important);

        var root = new GameObject(name);
        root.transform.position = position;
        
        // Create pelvis at root (center of mass)
        var pelvis = CreateAnatomicalBodyPart("Pelvis", Vector3.zero, config.pelvisScale, root.transform, config);
        
        // Calculate anatomically correct positions for leg attachment
        float pelvisBottom = -config.pelvisScale.y * 0.5f;  // Bottom of pelvis capsule
        float hipWidth = config.pelvisScale.x * 0.35f;      // Hip socket width (slightly inset from pelvis edge)
        
        // LEFT LEG CHAIN - each part connects to the bottom of its parent
        Vector3 leftHipPos = new Vector3(-hipWidth, pelvisBottom - config.hipSocketDepth, 0);
        var leftThigh = CreateAnatomicalBodyPart("LeftThigh", leftHipPos, config.thighScale, pelvis.transform, config);
        
        float thighBottom = leftHipPos.y - config.thighScale.y * 0.5f;
        Vector3 leftKneePos = new Vector3(0, thighBottom - config.kneeConnectionHeight, 0);
        var leftShin = CreateAnatomicalBodyPart("LeftShin", leftKneePos, config.shinScale, leftThigh.transform, config);
        
        float shinBottom = leftKneePos.y - config.shinScale.y * 0.5f; 
        Vector3 leftAnklePos = new Vector3(0, shinBottom - config.ankleConnectionHeight, 0);
        var leftFoot = CreateAnatomicalBodyPart("LeftFoot", leftAnklePos, config.footScale, leftShin.transform, config);
        
        // RIGHT LEG CHAIN - mirror the left leg
        Vector3 rightHipPos = new Vector3(hipWidth, pelvisBottom - config.hipSocketDepth, 0);
        var rightThigh = CreateAnatomicalBodyPart("RightThigh", rightHipPos, config.thighScale, pelvis.transform, config);
        
        Vector3 rightKneePos = new Vector3(0, thighBottom - config.kneeConnectionHeight, 0);
        var rightShin = CreateAnatomicalBodyPart("RightShin", rightKneePos, config.shinScale, rightThigh.transform, config);
        
        Vector3 rightAnklePos = new Vector3(0, shinBottom - config.ankleConnectionHeight, 0);
        var rightFoot = CreateAnatomicalBodyPart("RightFoot", rightAnklePos, config.footScale, rightShin.transform, config);

        // Create anatomically correct joints - child's top connects to parent's bottom
        TrainArenaDebugManager.Log("🔧 Creating ANATOMICALLY CORRECT JOINTS", TrainArenaDebugManager.DebugLogLevel.Important);
        
        // Hip joints - thigh top connects to pelvis hip socket  
        AddAnatomicalJoint(leftThigh, pelvis, Vector3.up * 0.5f, Vector3.down * 0.5f, new Vector3(config.hipJointLimit, 0, 0), config);
        AddAnatomicalJoint(rightThigh, pelvis, Vector3.up * 0.5f, Vector3.down * 0.5f, new Vector3(config.hipJointLimit, 0, 0), config);
        
        // Knee joints - shin top connects to thigh bottom
        AddAnatomicalJoint(leftShin, leftThigh, Vector3.up * 0.5f, Vector3.down * 0.5f, new Vector3(config.kneeJointLimit, 0, 0), config);
        AddAnatomicalJoint(rightShin, rightThigh, Vector3.up * 0.5f, Vector3.down * 0.5f, new Vector3(config.kneeJointLimit, 0, 0), config);
        
        // Ankle joints - foot connects to shin bottom
        AddAnatomicalJoint(leftFoot, leftShin, Vector3.up * 0.5f, Vector3.down * 0.5f, new Vector3(config.ankleJointLimit, 0, 0), config);
        AddAnatomicalJoint(rightFoot, rightShin, Vector3.up * 0.5f, Vector3.down * 0.5f, new Vector3(config.ankleJointLimit, 0, 0), config);

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
        
        TrainArenaDebugManager.Log($"🎭 Ragdoll created: {name} with improved proportions - 6 joints, {behaviorParams.BrainParameters.ActionSpec.NumContinuousActions} actions", 
            TrainArenaDebugManager.DebugLogLevel.Important);
        
        // Collect all PDJointControllers and assign to agent
        var joints = new System.Collections.Generic.List<PDJointController>();
        joints.AddRange(root.GetComponentsInChildren<PDJointController>());
        ragdollAgent.joints = joints;

        return root;
    }

    /// <summary>
    /// Creates an anatomically correct body part with proper joint connections
    /// </summary>
    private static GameObject CreateAnatomicalBodyPart(string name, Vector3 localPosition, Vector3 size, Transform parent, RagdollConfiguration config)
    {
        var bodyPart = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        bodyPart.name = name;
        bodyPart.transform.parent = parent;
        bodyPart.transform.localPosition = localPosition;
        bodyPart.transform.localScale = size;
        
        // Rotate feet to be flat on ground
        if (name.Contains("Foot"))
        {
            bodyPart.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }
        
        // Add physics components
        var rb = bodyPart.AddComponent<Rigidbody>();
        rb.mass = GetAnatomicalMass(name, config);
        rb.linearDamping = config.linearDamping;
        rb.angularDamping = config.angularDamping;
        rb.interpolation = config.interpolation;
        
        // Configure collider with physics material
        var collider = bodyPart.GetComponent<CapsuleCollider>();
        collider.material = CreatePhysicsMaterial(config);
        
        TrainArenaDebugManager.Log($"🦴 Created {name}: pos={localPosition}, size={size}", TrainArenaDebugManager.DebugLogLevel.Verbose);
        
        return bodyPart;
    }
    
    /// <summary>
    /// Get anatomically appropriate mass for body part
    /// </summary>
    private static float GetAnatomicalMass(string partName, RagdollConfiguration config)
    {
        if (partName.Contains("Pelvis")) return config.pelvisMass;
        if (partName.Contains("Thigh")) return config.thighMass;
        if (partName.Contains("Shin")) return config.shinMass;
        if (partName.Contains("Foot")) return config.footMass;
        return 1f; // Default
    }

    /// <summary>
    /// LEGACY METHOD - Creates a single body part (capsule with rigidbody and collider)
    /// </summary>
    private static GameObject CreateRagdollBodyPart(string name, Vector3 localPosition, Vector3 size, Transform parent, RagdollConfiguration config)
    {
        TrainArenaDebugManager.Log($"🔨 CREATING BODY PART: {name}", TrainArenaDebugManager.DebugLogLevel.Important);
        TrainArenaDebugManager.Log($"🔨 INPUT - localPosition: {localPosition}, size: {size}, parent: {parent.name}", TrainArenaDebugManager.DebugLogLevel.Important);
        
        var bodyPart = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        TrainArenaDebugManager.Log($"🔨 CREATED PRIMITIVE - name before: {bodyPart.name}", TrainArenaDebugManager.DebugLogLevel.Important);
        
        bodyPart.name = name;
        TrainArenaDebugManager.Log($"🔨 SET NAME - name after: {bodyPart.name}", TrainArenaDebugManager.DebugLogLevel.Important);
        
        bodyPart.transform.parent = parent;
        TrainArenaDebugManager.Log($"🔨 SET PARENT - parent: {bodyPart.transform.parent.name}, position before setting local: {bodyPart.transform.position}", TrainArenaDebugManager.DebugLogLevel.Important);
        
        bodyPart.transform.localPosition = localPosition;
        TrainArenaDebugManager.Log($"� SET LOCAL POSITION - requested: {localPosition}, actual local: {bodyPart.transform.localPosition}, world: {bodyPart.transform.position}", TrainArenaDebugManager.DebugLogLevel.Important);
        
        bodyPart.transform.localScale = size;
        TrainArenaDebugManager.Log($"🔨 SET SCALE - requested: {size}, actual: {bodyPart.transform.localScale}", TrainArenaDebugManager.DebugLogLevel.Important);
        
        // Rotate feet to be horizontal (flat on ground)
        if (name.Contains("Foot"))
        {
            bodyPart.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            TrainArenaDebugManager.Log($"🔨 ROTATED FOOT - {name} rotated to be flat on ground", TrainArenaDebugManager.DebugLogLevel.Important);
        }
        
        // Add rigidbody using configuration settings
        var rb = bodyPart.AddComponent<Rigidbody>();
        // Set mass based on body part name
        rb.mass = name == "Pelvis" ? config.pelvisMass : 
                 (name.Contains("Thigh") ? config.thighMass : 
                 (name.Contains("Shin") ? config.shinMass : config.footMass));
        rb.linearDamping = config.linearDamping;
        rb.angularDamping = config.angularDamping;
        rb.interpolation = config.interpolation;
        
        // Configure collider
        var collider = bodyPart.GetComponent<CapsuleCollider>();
        collider.material = CreatePhysicsMaterial(config);
        
        return bodyPart;
    }

    /// <summary>
    /// Creates an anatomically correct joint connection between body parts
    /// </summary>
    private static void AddAnatomicalJoint(GameObject child, GameObject parent, Vector3 childAnchor, Vector3 parentAnchor, Vector3 limits, RagdollConfiguration config)
    {
        var joint = child.AddComponent<ConfigurableJoint>();
        joint.connectedBody = parent.GetComponent<Rigidbody>();
        
        // ANATOMICALLY CORRECT ANCHORS - child's top connects to parent's bottom
        joint.anchor = childAnchor;                    // Top of child capsule
        joint.connectedAnchor = parentAnchor;          // Bottom of parent capsule
        joint.autoConfigureConnectedAnchor = false;    // Manual control for precision
        
        // Configure realistic joint movement
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
        joint.angularXMotion = ConfigurableJointMotion.Limited;  // Primary flexion/extension
        joint.angularYMotion = ConfigurableJointMotion.Locked;   // No side bending for now  
        joint.angularZMotion = ConfigurableJointMotion.Locked;   // No twisting for now
        
        // Set anatomical joint limits
        var lowLimit = joint.lowAngularXLimit;
        lowLimit.limit = -limits.x;
        lowLimit.bounciness = config.jointBounciness;
        joint.lowAngularXLimit = lowLimit;

        var highLimit = joint.highAngularXLimit;
        highLimit.limit = limits.x;
        highLimit.bounciness = config.jointBounciness;
        joint.highAngularXLimit = highLimit;
        
        // Configure joint drive for muscle-like behavior
        var drive = joint.angularXDrive;
        drive.positionSpring = config.jointSpring;
        drive.positionDamper = config.jointDamper;
        drive.maximumForce = config.jointMaxForce;
        joint.angularXDrive = drive;
        
        // Add PD controller for ML-Agents
        var pdController = child.AddComponent<PDJointController>();
        pdController.joint = joint;
        pdController.minAngle = -limits.x;
        pdController.maxAngle = limits.x;
        pdController.kp = config.pdKp;
        pdController.kd = config.pdKd;
        
        TrainArenaDebugManager.Log($"🔗 Anatomical joint: {child.name} -> {parent.name} (child anchor: {childAnchor}, parent anchor: {parentAnchor})", 
            TrainArenaDebugManager.DebugLogLevel.Verbose);
    }

    /// <summary>
    /// LEGACY METHOD - Adds a ConfigurableJoint and PDJointController optimized for AI training
    /// </summary>
    private static void AddRagdollJoint(GameObject child, GameObject parent, Vector3 axis, Vector3 limits, RagdollConfiguration config)
    {
        var joint = child.AddComponent<ConfigurableJoint>();
        joint.connectedBody = parent.GetComponent<Rigidbody>();
        
        // PROPER ANATOMICAL JOINT ANCHORING - prevents collapse and ensures realistic articulation
        Vector3 childAnchor = Vector3.up * 0.5f;      // Top of child capsule (connection point)
        Vector3 parentAnchor = Vector3.down * 0.5f;   // Bottom of parent capsule (connection point)
        
        joint.anchor = childAnchor;                    // Connect at top of child
        joint.connectedAnchor = parentAnchor;          // Connect at bottom of parent
        joint.autoConfigureConnectedAnchor = false;    // Manual control for precision
        
        // DEBUG: Log joint configuration
        TrainArenaDebugManager.Log($"🔗 Created joint: {child.name} -> {parent.name} with anatomical anchors (child top to parent bottom)", TrainArenaDebugManager.DebugLogLevel.Verbose);
        
        // Configure motion constraints
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
        joint.angularXMotion = ConfigurableJointMotion.Limited;
        joint.angularYMotion = ConfigurableJointMotion.Locked;
        joint.angularZMotion = ConfigurableJointMotion.Locked;
        
        // Set joint limits using configuration
        var lowLimit = joint.lowAngularXLimit;
        lowLimit.limit = -limits.x;
        lowLimit.bounciness = config.jointBounciness;
        joint.lowAngularXLimit = lowLimit;

        var highLimit = joint.highAngularXLimit;
        highLimit.limit = limits.x;
        highLimit.bounciness = config.jointBounciness;
        joint.highAngularXLimit = highLimit;
        
        // Configure joint drive using configuration
        var drive = joint.angularXDrive;
        drive.positionSpring = config.jointSpring;
        drive.positionDamper = config.jointDamper;
        drive.maximumForce = config.jointMaxForce;
        joint.angularXDrive = drive;
        
        // Add PD controller using configuration
        var pdController = child.AddComponent<PDJointController>();
        pdController.joint = joint;
        pdController.minAngle = -limits.x;
        pdController.maxAngle = limits.x;
        pdController.kp = config.pdKp;
        pdController.kd = config.pdKd;
        
        TrainArenaDebugManager.Log($"Created joint: {child.name} -> {parent.name}, limits: ±{limits.x}°", 
            TrainArenaDebugManager.DebugLogLevel.Verbose);
    }

    /// <summary>
    /// Creates physics material for ragdoll parts using configuration
    /// </summary>
    private static PhysicsMaterial CreatePhysicsMaterial(RagdollConfiguration config)
    {
        var material = new PhysicsMaterial("RagdollMaterial");
        material.dynamicFriction = config.dynamicFriction;
        material.staticFriction = config.staticFriction;
        material.bounciness = config.bounciness;
        return material;
    }

    // Unity Editor menu items for ragdoll configuration
    #if UNITY_EDITOR
   /* [UnityEditor.MenuItem("TrainArena/Ragdoll Config/Set AI Training Preset")]
    public static void SetAITrainingPreset()
    {
        CurrentConfig = RagdollConfiguration.AITraining();
        UnityEngine.Debug.Log("Ragdoll configuration set to AI Training preset");
    }

    [UnityEditor.MenuItem("TrainArena/Ragdoll Config/Set Realistic Human Preset")]
    public static void SetRealisticHumanPreset()
    {
        CurrentConfig = RagdollConfiguration.RealisticHuman();
        UnityEngine.Debug.Log("Ragdoll configuration set to Realistic Human preset");
    }

    [UnityEditor.MenuItem("TrainArena/Ragdoll Config/Set Athletic Preset")]
    public static void SetAthleticPreset()
    {
        CurrentConfig = RagdollConfiguration.Athletic();
        UnityEngine.Debug.Log("Ragdoll configuration set to Athletic preset");
    }

    [UnityEditor.MenuItem("TrainArena/Ragdoll Config/Create Anatomical Human Ragdoll")]
    public static void CreateAnatomicalHumanRagdoll()
    {
        CurrentConfig = RagdollConfiguration.AnatomicalHuman();
        var ragdoll = CreateRagdoll("AnatomicalHumanRagdoll", Vector3.zero);
        UnityEditor.Selection.activeGameObject = ragdoll;
        TrainArenaDebugManager.Log($"🧬 Created ANATOMICALLY CORRECT human ragdoll - Real human proportions and joint connections!", TrainArenaDebugManager.DebugLogLevel.Important);
    }

    [UnityEditor.MenuItem("TrainArena/Ragdoll Config/Create Test Ragdoll (Current Config)")]
    public static void CreateTestRagdoll()
    {
        var ragdoll = CreateRagdoll("TestRagdoll", Vector3.zero);
        UnityEditor.Selection.activeGameObject = ragdoll;
        TrainArenaDebugManager.Log($"🎭 Created test ragdoll with current configuration. Joints: {ragdoll.GetComponentsInChildren<PDJointController>().Length}", TrainArenaDebugManager.DebugLogLevel.Important);
    }*/

    [UnityEditor.MenuItem("TrainArena/Ragdoll Config/Create Realistic Human Ragdoll")]
    public static void CreateRealisticHumanRagdoll()
    {
        CurrentConfig = RagdollConfiguration.RealisticHuman();
        var ragdoll = CreateRagdoll("RealisticHumanRagdoll", Vector3.zero);
        UnityEditor.Selection.activeGameObject = ragdoll;
        TrainArenaDebugManager.Log($"🎭 Created realistic human ragdoll - Ready for locomotion training!", TrainArenaDebugManager.DebugLogLevel.Important);
    }

    [UnityEditor.MenuItem("TrainArena/Ragdoll Config/Create Functional Walker Ragdoll")]
    public static void CreateFunctionalWalkerRagdoll()
    {
        CurrentConfig = RagdollConfiguration.FunctionalWalker();
        var ragdoll = CreateRagdoll("FunctionalWalkerRagdoll", Vector3.zero);
        UnityEditor.Selection.activeGameObject = ragdoll;
        TrainArenaDebugManager.Log($"🚶 Created functional walker ragdoll - Optimized for stable locomotion!", TrainArenaDebugManager.DebugLogLevel.Important);
    }

    [UnityEditor.MenuItem("TrainArena/Ragdoll Config/Create Inspiration Match Ragdoll")]
    public static void CreateInspirationMatchRagdoll()
    {
        CurrentConfig = RagdollConfiguration.InspirationMatch();
        var ragdoll = CreateRagdoll("InspirationMatchRagdoll", Vector3.zero);
        UnityEditor.Selection.activeGameObject = ragdoll;
        TrainArenaDebugManager.Log($"🎯 Created inspiration-matched ragdoll - Elongated human proportions like reference images!", TrainArenaDebugManager.DebugLogLevel.Important);
    }

    [UnityEditor.MenuItem("TrainArena/Ragdoll Config/Create Athletic Ragdoll")]
    public static void CreateAthleticRagdoll()
    {
        CurrentConfig = RagdollConfiguration.Athletic();
        var ragdoll = CreateRagdoll("AthleticRagdoll", Vector3.zero);
        UnityEditor.Selection.activeGameObject = ragdoll;
        TrainArenaDebugManager.Log($"🎭 Created athletic ragdoll - Enhanced performance capabilities!", TrainArenaDebugManager.DebugLogLevel.Important);
    }

    [UnityEditor.MenuItem("TrainArena/Ragdoll Config/Create AI Training Ragdoll")]
    public static void CreateAITrainingRagdoll()
    {
        CurrentConfig = RagdollConfiguration.AITraining();
        var ragdoll = CreateRagdoll("AITrainingRagdoll", Vector3.zero);
        UnityEditor.Selection.activeGameObject = ragdoll;
        TrainArenaDebugManager.Log($"🎭 Created AI training ragdoll - Optimized for machine learning!", TrainArenaDebugManager.DebugLogLevel.Important);
    }
    #endif
}