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
        TrainArenaDebugManager.Log($"🎯 Creating ragdoll '{name}' at position {position} using BlockmanRagdollBuilder system", TrainArenaDebugManager.DebugLogLevel.Important);
        
        GameObject ragdoll = null;
        
        try
        {
            // Use BlockmanRagdollBuilder for consistent, stable ragdoll creation
            ragdoll = BlockmanRagdollBuilder.Build(position, new BlockmanRagdollBuilder.Cfg());
            
            if (ragdoll == null)
            {
                TrainArenaDebugManager.LogError($"❌ CRITICAL: BlockmanRagdollBuilder returned null ragdoll! Creation failed.");
                return null;
            }
            
            ragdoll.name = name;
            TrainArenaDebugManager.Log($"✅ BlockmanRagdollBuilder created ragdoll structure successfully", TrainArenaDebugManager.DebugLogLevel.Verbose);
        }
        catch (System.Exception e)
        {
            TrainArenaDebugManager.LogError($"❌ CRITICAL: BlockmanRagdollBuilder failed to create ragdoll: {e.Message}");
            TrainArenaDebugManager.LogError($"Stack trace: {e.StackTrace}");
            return null;
        }
        
        // Validate basic ragdoll structure before proceeding
        if (!ValidateRagdollStructure(ragdoll))
        {
            TrainArenaDebugManager.LogError($"❌ CRITICAL: Created ragdoll '{name}' failed structure validation!");
            if (ragdoll != null)
            {
                Object.DestroyImmediate(ragdoll);
            }
            return null;
        }
        
        // Add ML-Agents components if not already present
        try
        {
            AddMLAgentsToBlockman(ragdoll);
        }
        catch (System.Exception e)
        {
            TrainArenaDebugManager.LogError($"❌ CRITICAL: Failed to add ML-Agents components to ragdoll '{name}': {e.Message}");
            TrainArenaDebugManager.LogError($"Stack trace: {e.StackTrace}");
            
            // Don't destroy the ragdoll since the structure might still be usable
            TrainArenaDebugManager.LogWarning($"⚠️ Ragdoll '{name}' created but ML-Agents integration may not work properly");
        }
        
        // Final validation
        var ragdollAgent = ragdoll.GetComponentInChildren<RagdollAgent>();
        if (ragdollAgent == null)
        {
            TrainArenaDebugManager.LogError($"❌ CRITICAL: No RagdollAgent component found in created ragdoll '{name}'!");
        }
        else
        {
            TrainArenaDebugManager.Log($"✅ Ragdoll '{name}' created successfully using BlockmanRagdollBuilder with ML-Agents integration", TrainArenaDebugManager.DebugLogLevel.Important);
            
            // Log final configuration summary
            int jointCount = ragdollAgent.joints?.Count ?? 0;
            int totalObservations = ragdollAgent.GetTotalObservationCount();
            TrainArenaDebugManager.Log($"📊 Ragdoll Summary: {jointCount} joints, {totalObservations} observations", TrainArenaDebugManager.DebugLogLevel.Important);
        }
        
        return ragdoll;
    }
    
    /// <summary>
    /// Validates basic ragdoll structure to ensure it has required components
    /// </summary>
    private static bool ValidateRagdollStructure(GameObject ragdoll)
    {
        if (ragdoll == null)
        {
            TrainArenaDebugManager.LogError("❌ Cannot validate null ragdoll structure");
            return false;
        }
        
        // Check for pelvis
        var pelvis = ragdoll.transform.Find("Pelvis");
        if (pelvis == null)
        {
            TrainArenaDebugManager.LogError($"❌ Ragdoll '{ragdoll.name}' missing required Pelvis bone");
            return false;
        }
        
        // Check pelvis has rigidbody
        var pelvisRigidbody = pelvis.GetComponent<Rigidbody>();
        if (pelvisRigidbody == null)
        {
            TrainArenaDebugManager.LogError($"❌ Ragdoll pelvis '{pelvis.name}' missing Rigidbody component");
            return false;
        }
        
        // Check for configurable joints
        var joints = ragdoll.GetComponentsInChildren<ConfigurableJoint>();
        if (joints == null || joints.Length == 0)
        {
            TrainArenaDebugManager.LogError($"❌ Ragdoll '{ragdoll.name}' has no ConfigurableJoint components");
            return false;
        }
        
        // Check for rigidbodies on joint objects
        int rigidbodyCount = ragdoll.GetComponentsInChildren<Rigidbody>().Length;
        if (rigidbodyCount < 2) // At least pelvis + one other part
        {
            TrainArenaDebugManager.LogError($"❌ Ragdoll '{ragdoll.name}' has insufficient Rigidbody components: {rigidbodyCount}");
            return false;
        }
        
        TrainArenaDebugManager.Log($"✅ Ragdoll structure validation passed: Pelvis OK, {joints.Length} joints, {rigidbodyCount} rigidbodies", 
            TrainArenaDebugManager.DebugLogLevel.Verbose);
        
        return true;
    }

    /// <summary>
    /// Adds ML-Agents components to a blockman ragdoll if they're not already present
    /// </summary>
    private static void AddMLAgentsToBlockman(GameObject ragdoll)
    {
        if (ragdoll == null)
        {
            TrainArenaDebugManager.LogError("❌ CRITICAL: Cannot add ML-Agents components to null ragdoll!");
            return;
        }
        
        var pelvis = ragdoll.transform.Find("Pelvis")?.gameObject;
        if (pelvis == null)
        {
            TrainArenaDebugManager.LogError($"❌ CRITICAL: No Pelvis found in blockman ragdoll '{ragdoll.name}'! ML-Agents components cannot be added. Ragdoll structure is invalid.");
            
            // Log available children for debugging
            TrainArenaDebugManager.Log($"Available children in '{ragdoll.name}':", TrainArenaDebugManager.DebugLogLevel.Important);
            foreach (Transform child in ragdoll.transform)
            {
                TrainArenaDebugManager.Log($"  - {child.name}", TrainArenaDebugManager.DebugLogLevel.Important);
            }
            return;
        }
        
        TrainArenaDebugManager.Log($"✅ Found Pelvis '{pelvis.name}' in ragdoll '{ragdoll.name}'", TrainArenaDebugManager.DebugLogLevel.Verbose);
        
        // Add RagdollAgent if not present
        var ragdollAgent = pelvis.GetComponent<RagdollAgent>();
        if (ragdollAgent == null)
        {
            try
            {
                ragdollAgent = pelvis.AddComponent<RagdollAgent>();
                ragdollAgent.pelvis = pelvis.transform;
                TrainArenaDebugManager.Log($"✅ Added RagdollAgent component to pelvis", TrainArenaDebugManager.DebugLogLevel.Important);
            }
            catch (System.Exception e)
            {
                TrainArenaDebugManager.LogError($"❌ CRITICAL: Failed to add RagdollAgent component: {e.Message}");
                return;
            }
        }
        else
        {
            TrainArenaDebugManager.Log($"RagdollAgent component already exists on pelvis", TrainArenaDebugManager.DebugLogLevel.Verbose);
        }
        
        // Find and configure leg joints for ML-Agents control
        var joints = new List<PDJointController>();
        var configJoints = ragdoll.GetComponentsInChildren<ConfigurableJoint>();
        
        if (configJoints == null || configJoints.Length == 0)
        {
            TrainArenaDebugManager.LogError($"❌ CRITICAL: No ConfigurableJoints found in ragdoll '{ragdoll.name}'! Cannot create PDJointControllers.");
            return;
        }
        
        TrainArenaDebugManager.Log($"Found {configJoints.Length} ConfigurableJoints in ragdoll", TrainArenaDebugManager.DebugLogLevel.Verbose);
        
        int legJointCount = 0;
        foreach (var configJoint in configJoints)
        {
            if (configJoint == null)
            {
                TrainArenaDebugManager.LogWarning($"⚠️ Null ConfigurableJoint found in ragdoll children");
                continue;
            }
            
            var partName = configJoint.gameObject.name; // Keep original case
            
			//TODO: Hardcoding the names
            // Only add PDJointController to leg joints for locomotion (matches BlockmanRagdollBuilder naming)
            // BlockmanRagdollBuilder uses: LeftUpperLeg, RightUpperLeg, LeftLowerLeg, RightLowerLeg, LeftFoot, RightFoot
            bool isLegJoint = partName.Contains("UpperLeg") || partName.Contains("LowerLeg") || partName.Contains("Foot");
            
            if (isLegJoint)
            {
                try
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
                        if (partName.Contains("UpperLeg"))
                        {
                            pdController.minAngle = -45f;
                            pdController.maxAngle = 45f;
                        }
                        else if (partName.Contains("LowerLeg"))
                        {
                            pdController.minAngle = -90f;
                            pdController.maxAngle = 90f;
                        }
                        else if (partName.Contains("Foot"))
                        {
                            pdController.minAngle = -30f;
                            pdController.maxAngle = 30f;
                        }
                        
                        TrainArenaDebugManager.Log($"✅ Added PDJointController to '{configJoint.gameObject.name}'", TrainArenaDebugManager.DebugLogLevel.Verbose);
                        legJointCount++;
                    }
                    else
                    {
                        TrainArenaDebugManager.Log($"PDJointController already exists on '{configJoint.gameObject.name}'", TrainArenaDebugManager.DebugLogLevel.Verbose);
                    }
                    
                    // Validate PDJointController configuration
                    if (pdController.joint == null)
                    {
                        TrainArenaDebugManager.LogError($"❌ PDJointController on '{configJoint.gameObject.name}' has null joint reference!");
                        continue;
                    }
                    
                    joints.Add(pdController);
                }
                catch (System.Exception e)
                {
                    TrainArenaDebugManager.LogError($"❌ Error configuring PDJointController on '{configJoint.gameObject.name}': {e.Message}");
                }
            }
        }
        
        // Validate we found appropriate leg joints
        if (joints.Count == 0)
        {
            TrainArenaDebugManager.LogError($"❌ CRITICAL: No leg joints found in ragdoll '{ragdoll.name}'! Expected parts with names containing 'UpperLeg', 'LowerLeg', or 'Foot'.");
            TrainArenaDebugManager.Log("Available joint names:", TrainArenaDebugManager.DebugLogLevel.Important);
            foreach (var joint in configJoints)
            {
                if (joint != null)
                {
                    TrainArenaDebugManager.Log($"  - {joint.gameObject.name}", TrainArenaDebugManager.DebugLogLevel.Important);
                }
            }
            return;
        }
        
        // Assign joints to RagdollAgent
        try
        {
            ragdollAgent.joints = joints;
            TrainArenaDebugManager.Log($"✅ Configured RagdollAgent with {joints.Count} leg joints ({legJointCount} newly added)", TrainArenaDebugManager.DebugLogLevel.Important);
        }
        catch (System.Exception e)
        {
            TrainArenaDebugManager.LogError($"❌ CRITICAL: Failed to assign joints to RagdollAgent: {e.Message}");
            return;
        }
        
        // Add BehaviorParameters if not present - MUST be done after joints are assigned
        var behaviorParameters = pelvis.GetComponent<BehaviorParameters>();
        if (behaviorParameters == null)
        {
            try
            {
                behaviorParameters = pelvis.AddComponent<BehaviorParameters>();
                behaviorParameters.BehaviorName = "RagdollAgent";
                
                // Use actual joint count for continuous actions (should be 6 for legs)
                int actionCount = ragdollAgent.joints.Count;
                behaviorParameters.BrainParameters.ActionSpec = Unity.MLAgents.Actuators.ActionSpec.MakeContinuous(actionCount);
                
                // Configure observation space using RagdollAgent's calculation
                int totalObservations = ragdollAgent.GetTotalObservationCount();
                if (totalObservations <= 0)
                {
                    TrainArenaDebugManager.LogError($"❌ CRITICAL: RagdollAgent returned invalid observation count: {totalObservations}");
                    return;
                }
                
                behaviorParameters.BrainParameters.VectorObservationSize = totalObservations;
                
                TrainArenaDebugManager.Log($"✅ Added BehaviorParameters: {ragdollAgent.joints.Count} actions, {totalObservations} observations " +
                                         $"({RagdollAgent.UPRIGHTNESS_OBSERVATIONS} uprightness + {RagdollAgent.VELOCITY_OBSERVATIONS} velocity + {ragdollAgent.joints.Count * RagdollAgent.JOINT_STATE_OBSERVATIONS_PER_JOINT} joint states)", 
                                         TrainArenaDebugManager.DebugLogLevel.Important);
            }
            catch (System.Exception e)
            {
                TrainArenaDebugManager.LogError($"❌ CRITICAL: Failed to add/configure BehaviorParameters: {e.Message}");
                return;
            }
        }
        else
        {
            TrainArenaDebugManager.Log($"BehaviorParameters already exists on pelvis", TrainArenaDebugManager.DebugLogLevel.Verbose);
            
            // Update existing BehaviorParameters to match joint count
            int expectedActions = ragdollAgent.joints.Count;
            int actualActions = behaviorParameters.BrainParameters.ActionSpec.NumContinuousActions;
            if (actualActions != expectedActions)
            {
                TrainArenaDebugManager.LogWarning($"⚠️ Updating BehaviorParameters action count from {actualActions} to {expectedActions}");
                behaviorParameters.BrainParameters.ActionSpec = Unity.MLAgents.Actuators.ActionSpec.MakeContinuous(expectedActions);
            }
        }

        // Add visual polish components with error handling
        try
        {
            // Add blinking animation for visual polish
            var headVisualObject = ragdoll.transform.Find("Head")?.transform.Find("Visual")?.gameObject;
            if (headVisualObject != null)
            {
                if (headVisualObject.GetComponent<EyeBlinker>() == null)
                {
                    headVisualObject.AddComponent<EyeBlinker>();
                    TrainArenaDebugManager.Log($"✅ Added EyeBlinker to head visual", TrainArenaDebugManager.DebugLogLevel.Verbose);
                }
            }
            else
            {
                TrainArenaDebugManager.LogWarning("⚠️ Could not find Head/Visual object for eye blinking animation");
            }
        }
        catch (System.Exception e)
        {
            TrainArenaDebugManager.LogWarning($"⚠️ Failed to add EyeBlinker: {e.Message}");
        }

        // Add debug and development components with error handling
        try
        {
            // Add debug info component for development
            if (pelvis.GetComponent<AgentDebugInfo>() == null)
            {
                pelvis.AddComponent<AgentDebugInfo>();
                TrainArenaDebugManager.Log($"✅ Added AgentDebugInfo component", TrainArenaDebugManager.DebugLogLevel.Verbose);
            }
            
            // Add domain randomization for physics testing
            var domainRandomizer = pelvis.GetComponent<DomainRandomizer>();
            if (domainRandomizer == null)
            {
                domainRandomizer = pelvis.AddComponent<DomainRandomizer>();
                domainRandomizer.randomizeMass = true;
                domainRandomizer.randomizeFriction = true;
                domainRandomizer.randomizeGravity = false; // Keep gravity stable for ragdolls
                TrainArenaDebugManager.Log($"✅ Added DomainRandomizer component", TrainArenaDebugManager.DebugLogLevel.Verbose);
            }
        }
        catch (System.Exception e)
        {
            TrainArenaDebugManager.LogWarning($"⚠️ Failed to add optional components: {e.Message}");
        }
        
        TrainArenaDebugManager.Log($"🎭 Successfully configured ragdoll '{ragdoll.name}' for ML-Agents training with {ragdollAgent.joints.Count} action joints", TrainArenaDebugManager.DebugLogLevel.Important);
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

        // Add CubeAgent component
        var cubeAgent = agent.AddComponent<CubeAgent>();
        
        // Add BehaviorParameters - MUST be done after CubeAgent is added
        var behaviorParameters = agent.GetComponent<BehaviorParameters>();
        if (behaviorParameters == null)
        {
            try
            {
                behaviorParameters = agent.AddComponent<BehaviorParameters>();
                behaviorParameters.BehaviorName = "CubeAgent";
                
                // CubeAgent uses exactly 2 continuous actions (moveX, moveZ)
                const int CUBE_ACTION_COUNT = 2;
                behaviorParameters.BrainParameters.ActionSpec = Unity.MLAgents.Actuators.ActionSpec.MakeContinuous(CUBE_ACTION_COUNT);
                
                // Configure observation space using CubeAgent's calculation
                int totalObservations = cubeAgent.GetTotalObservationCount();
                if (totalObservations <= 0)
                {
                    TrainArenaDebugManager.LogError($"❌ CRITICAL: CubeAgent returned invalid observation count: {totalObservations}");
                    return agent; // Return but don't fail completely
                }
                
                behaviorParameters.BrainParameters.VectorObservationSize = totalObservations;
                
                TrainArenaDebugManager.Log($"✅ Added BehaviorParameters: {CUBE_ACTION_COUNT} actions, {totalObservations} observations", 
                                         TrainArenaDebugManager.DebugLogLevel.Important);
            }
            catch (System.Exception e)
            {
                TrainArenaDebugManager.LogError($"❌ CRITICAL: Failed to add/configure BehaviorParameters for CubeAgent: {e.Message}");
                return agent; // Return but don't fail completely
            }
        }
        else
        {
            TrainArenaDebugManager.Log($"BehaviorParameters already exists on CubeAgent", TrainArenaDebugManager.DebugLogLevel.Verbose);
            
            // Update existing BehaviorParameters to ensure correct configuration
            const int EXPECTED_CUBE_ACTIONS = 2;
            int actualActions = behaviorParameters.BrainParameters.ActionSpec.NumContinuousActions;
            if (actualActions != EXPECTED_CUBE_ACTIONS)
            {
                TrainArenaDebugManager.LogWarning($"⚠️ Updating CubeAgent BehaviorParameters action count from {actualActions} to {EXPECTED_CUBE_ACTIONS}");
                behaviorParameters.BrainParameters.ActionSpec = Unity.MLAgents.Actuators.ActionSpec.MakeContinuous(EXPECTED_CUBE_ACTIONS);
            }
        }

        // Add visual and debug components with error handling
        try
        {
            // Add blinking animation for visual polish
            if (agent.GetComponent<EyeBlinker>() == null)
            {
                agent.AddComponent<EyeBlinker>();
                TrainArenaDebugManager.Log($"✅ Added EyeBlinker component", TrainArenaDebugManager.DebugLogLevel.Verbose);
            }

            // Add debug info component for development
            if (agent.GetComponent<AgentDebugInfo>() == null)
            {
                agent.AddComponent<AgentDebugInfo>();
                TrainArenaDebugManager.Log($"✅ Added AgentDebugInfo component", TrainArenaDebugManager.DebugLogLevel.Verbose);
            }

            // Add domain randomization for physics testing
            var domainRandomizer = agent.GetComponent<DomainRandomizer>();
            if (domainRandomizer == null)
            {
                domainRandomizer = agent.AddComponent<DomainRandomizer>();
                domainRandomizer.randomizeMass = true;
                domainRandomizer.randomizeFriction = true;
                domainRandomizer.randomizeGravity = false; // Keep gravity stable
                TrainArenaDebugManager.Log($"✅ Added DomainRandomizer component", TrainArenaDebugManager.DebugLogLevel.Verbose);
            }
        }
        catch (System.Exception e)
        {
            TrainArenaDebugManager.LogWarning($"⚠️ Failed to add optional components to CubeAgent: {e.Message}");
        }

        TrainArenaDebugManager.Log($"🧊 Successfully configured CubeAgent '{agent.name}' for ML-Agents training with 2 action dimensions", TrainArenaDebugManager.DebugLogLevel.Important);
        
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
