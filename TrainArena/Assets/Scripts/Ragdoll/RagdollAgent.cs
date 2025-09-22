using System.Collections;
using System.Collections.Generic;
using TrainArena.Core;
using TrainArena.Configuration;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

/// <summary>
/// ML-Agents controller for hierarchical ragdoll locomotion learning
/// 
/// Observations:
/// - Pelvis uprightness (1 value): dot product with world up vector
/// - Pelvis velocity (3 values): local-space velocity for direction awareness
/// - Joint states (2 per joint): current angle and angular velocity
/// 
/// Actions: 
/// - Continuous actions (1 per joint): target angles mapped from [-1,1] to joint limits
/// 
/// Rewards:
/// - Uprightness: Stay vertical and balanced
/// - Forward velocity: Move forward efficiently
/// - Energy efficiency: Minimize unnecessary joint movements
/// - Stability: Avoid excessive angular velocity
/// 
/// Episode termination: Ragdoll falls (low uprightness or height)
/// </summary>
public class RagdollAgent : BaseTrainArenaAgent
{
    [Header("Ragdoll Configuration")]
    [Tooltip("PDJointControllers in action order - each gets one continuous action")]
    public List<PDJointController> joints = new List<PDJointController>();
    
    [Tooltip("Main body transform for uprightness and velocity calculations")]
    public Transform pelvis;
    
    [Header("Locomotion Parameters")]
    [Tooltip("Target forward velocity for reward calculation (0 = use config default)")]
    public float targetSpeed = 0f;
    
    [Tooltip("Weight for uprightness reward component (0 = use config default)")]
    public float uprightBonus = 0f;
    
    [Header("Heuristic Configuration")]
    [Tooltip("Override base frequency for heuristic movements (0 = use default from config)")]
    public float heuristicFrequencyOverride = 0f;
    
    [Tooltip("Global amplitude multiplier for all heuristic movements (0 = use config default)")]
    [Range(0f, 2.0f)]
    public float heuristicAmplitudeMultiplier = 0f;
    
    // Observation space constants (matching CubeAgent pattern)
    public const int UPRIGHTNESS_OBSERVATIONS = 1;      // Pelvis uprightness (dot product with world up)
    public const int VELOCITY_OBSERVATIONS = 3;         // Pelvis velocity (x, y, z)
    public const int JOINT_STATE_OBSERVATIONS_PER_JOINT = 2; // Angle and angular velocity per joint
    // Total observations = UPRIGHTNESS_OBSERVATIONS + VELOCITY_OBSERVATIONS + (joints.Count * JOINT_STATE_OBSERVATIONS_PER_JOINT)
    
    // Episode management
    [Header("Episode Management")]
    [Tooltip("Minimum time before episode can end (0 = use config default)")]
    public float episodeGracePeriod = 0f;
    
    [Tooltip("Maximum episode duration before auto-reset (0 = use config default)")]
    public float maxEpisodeDuration = 0f;
    
    [Tooltip("Minimum uprightness to avoid termination (0 = use config default)")]
    public float minUprightness = 0f;
    
    [Tooltip("Minimum height before episode termination (0 = use config default)")]
    public float minHeight = 0f;
    
    // Episode reset state
    private Vector3 startPosition;
    private Quaternion startRotation;
    
    // BaseTrainArenaAgent abstract property implementations
    public override Transform MainTransform => pelvis;
    public override Rigidbody MainRigidbody => pelvis?.GetComponent<Rigidbody>();
    public override string AgentTypeIcon => "🎭";

    public float Uprightness => Vector3.Dot(pelvis.up, Vector3.up);

    public override int GetTotalObservationCount()
    {
        // Calculate total observations: uprightness + velocity + joint states
        int numObservations = UPRIGHTNESS_OBSERVATIONS + VELOCITY_OBSERVATIONS + (joints.Count * JOINT_STATE_OBSERVATIONS_PER_JOINT);
        
        // Only log during initialization, not every frame
        if (Application.isPlaying && Time.time < 5f)
        {
            TrainArenaDebugManager.Log($"🎭 {name}: Total observations = {UPRIGHTNESS_OBSERVATIONS} (uprightness) + {VELOCITY_OBSERVATIONS} (velocity) + {joints.Count} joints × {JOINT_STATE_OBSERVATIONS_PER_JOINT} = {numObservations}", 
                TrainArenaDebugManager.DebugLogLevel.Important);
            
            if (numObservations > 30)
            {
                TrainArenaDebugManager.LogError($"⚠️ {name}: {numObservations} observations may be too many! Consider reducing joints or using fewer observations per joint.");
            }
        }
        
        return numObservations;
    }

    protected override void InitializeAgent()
    {
        // Ensure pelvis reference is set
        if (pelvis == null && transform != null) 
            pelvis = transform;
        
        // Store initial pose for episode resets
        if (pelvis != null)
        {
            startPosition = pelvis.position;
            startRotation = pelvis.rotation;
        }
        
        // Auto-configure all joints if not properly set up
        AutoConfigureRagdollJoints();
        
        // Update ML-Agents configuration to match joint count
        UpdateMLAgentsConfiguration();
        
        // Log initialization success
        TrainArenaDebugManager.Log($"🎭 {name}: Initialized ragdoll with {joints.Count} joints, Activity={AgentActivity}, ObsCount={GetTotalObservationCount()}", 
            TrainArenaDebugManager.DebugLogLevel.Important);
        
        // Validate configuration
        ValidateRagdollConfiguration();
        
        // Start action diagnostic coroutine
        StartCoroutine(DiagnoseActionCalls());
    }

    /// <summary>
    /// Validate that ragdoll is properly configured for training
    /// </summary>
    private void ValidateRagdollConfiguration()
    {
        if (joints.Count == 0)
        {
            TrainArenaDebugManager.LogError($"❌ {name}: No PDJointControllers assigned! Ragdoll cannot function without joints.");
            return;
        }

        if (pelvis == null)
        {
            TrainArenaDebugManager.LogError($"❌ {name}: Pelvis transform not assigned! Required for observations and rewards.");
            return;
        }

        // Debug: List all joints found in the ragdoll hierarchy
        var allPDJoints = GetComponentsInChildren<PDJointController>();
        TrainArenaDebugManager.Log($"🔍 {name}: Found {allPDJoints.Length} total PDJointControllers in hierarchy vs {joints.Count} in joints list", 
            TrainArenaDebugManager.DebugLogLevel.Important);
        
        if (allPDJoints.Length != joints.Count)
        {
            TrainArenaDebugManager.LogError($"⚠️ {name}: Potential joint mismatch! Found {allPDJoints.Length} PDJointControllers but only {joints.Count} are assigned to joints list.");
            TrainArenaDebugManager.Log($"🔍 {name}: All PDJointControllers found:", TrainArenaDebugManager.DebugLogLevel.Important);
            for (int i = 0; i < allPDJoints.Length; i++)
            {
                bool isInList = joints.Contains(allPDJoints[i]);
                TrainArenaDebugManager.Log($"  [{i}] {allPDJoints[i].name} - {(isInList ? "✅ In joints list" : "❌ NOT in joints list")}", 
                    TrainArenaDebugManager.DebugLogLevel.Important);
            }
        }

        // Verify each joint has required components
        TrainArenaDebugManager.Log($"🔍 {name}: Validating {joints.Count} joints in joints list:", TrainArenaDebugManager.DebugLogLevel.Important);
        for (int i = 0; i < joints.Count; i++)
        {
            var joint = joints[i];
            if (joint == null)
            {
                TrainArenaDebugManager.LogError($"❌ {name}: Joint {i} is null!");
                continue;
            }

            TrainArenaDebugManager.Log($"  [{i}] {joint.name} - {(joint.joint != null ? "✅" : "❌")} Joint, {(joint.GetComponent<Rigidbody>() != null ? "✅" : "❌")} RB", 
                TrainArenaDebugManager.DebugLogLevel.Important);

            if (joint.joint == null)
            {
                TrainArenaDebugManager.LogError($"❌ {name}: Joint {i} ({joint.name}) missing ConfigurableJoint component!");
            }

            if (joint.GetComponent<Rigidbody>() == null)
            {
                TrainArenaDebugManager.LogError($"❌ {name}: Joint {i} ({joint.name}) missing Rigidbody component!");
            }
        }

        TrainArenaDebugManager.Log($"✅ {name}: Ragdoll configuration validated - {joints.Count} joints ready for training", 
            TrainArenaDebugManager.DebugLogLevel.Important);
    }
    
    /// <summary>
    /// Auto-configure all joints in the ragdoll hierarchy for proper ML-Agents training
    /// This ensures ALL joints (chest, head, arms, legs) are controlled, not just legs
    /// </summary>
    private void AutoConfigureRagdollJoints()
    {
        TrainArenaDebugManager.Log($"🔧 {name}: Auto-configuring ragdoll joints for complete body control...", 
            TrainArenaDebugManager.DebugLogLevel.Important);
        
        // Find all ConfigurableJoints in the hierarchy (these define the ragdoll structure)
        var allConfigurableJoints = GetComponentsInChildren<ConfigurableJoint>();
        var newJointsList = new List<PDJointController>();
        
        TrainArenaDebugManager.Log($"🔍 {name}: Found {allConfigurableJoints.Length} ConfigurableJoints in ragdoll hierarchy", 
            TrainArenaDebugManager.DebugLogLevel.Important);
        
        foreach (var configurableJoint in allConfigurableJoints)
        {
            // Ensure each ConfigurableJoint has a PDJointController
            var pdController = configurableJoint.GetComponent<PDJointController>();
            if (pdController == null)
            {
                // Add PDJointController component
                pdController = configurableJoint.gameObject.AddComponent<PDJointController>();
                TrainArenaDebugManager.Log($"✅ {name}: Added PDJointController to '{configurableJoint.name}'", 
                    TrainArenaDebugManager.DebugLogLevel.Important);
            }
            
            // Configure the PDJointController
            if (pdController.joint == null)
            {
                pdController.joint = configurableJoint;
            }
            
            // Apply centralized joint configuration
            (float minAngle, float maxAngle, float kp, float kd) = RagdollJointNames.GetJointControllerConfig(configurableJoint.name);
            pdController.SetTarget01(0f); // Neutral position (0 = center of range)
            
            TrainArenaDebugManager.Log($"🔧 {name}: Configured PDJointController for '{configurableJoint.name}': " +
                     $"Limits=[{minAngle:F1}, {maxAngle:F1}], Gains=Kp{kp:F1}/Kd{kd:F1}", 
                     TrainArenaDebugManager.DebugLogLevel.Verbose);
            
            // Add to our joints list
            newJointsList.Add(pdController);
            
            // Reset joint to neutral pose to fix twisted spawning
            ResetJointToNeutralPose(configurableJoint);
        }
        
        // Sort joints in a logical order for training (core to extremities)
        newJointsList.Sort((a, b) => GetJointPriority(a.name).CompareTo(GetJointPriority(b.name)));
        
        // Update the joints list
        joints.Clear();
        joints.AddRange(newJointsList);
        
        TrainArenaDebugManager.Log($"🎯 {name}: Auto-configured {joints.Count} joints in priority order:", 
            TrainArenaDebugManager.DebugLogLevel.Important);
        for (int i = 0; i < joints.Count; i++)
        {
            TrainArenaDebugManager.Log($"   [{i}] {joints[i].name}", TrainArenaDebugManager.DebugLogLevel.Important);
        }
        
        // Calculate and display the required Vector Observation Space Size
        int requiredObsSize = UPRIGHTNESS_OBSERVATIONS + VELOCITY_OBSERVATIONS + (joints.Count * JOINT_STATE_OBSERVATIONS_PER_JOINT);
        TrainArenaDebugManager.LogError($"🎯 UNITY SETUP REQUIRED: Set Vector Observation Space Size to {requiredObsSize} in the Behavior Parameters component!");
        TrainArenaDebugManager.LogError($"   Current calculation: {UPRIGHTNESS_OBSERVATIONS} + {VELOCITY_OBSERVATIONS} + ({joints.Count} × {JOINT_STATE_OBSERVATIONS_PER_JOINT}) = {requiredObsSize}");
    }

    /// <summary>
    /// Update ML-Agents BehaviorParameters to match the current joint configuration
    /// This ensures action space and observation space match the dynamic joint count
    /// </summary>
    private void UpdateMLAgentsConfiguration()
    {
        var behaviorParams = GetComponent<Unity.MLAgents.Policies.BehaviorParameters>();
        if (behaviorParams == null)
        {
            TrainArenaDebugManager.LogWarning($"⚠️ {name}: No BehaviorParameters found - ML-Agents configuration cannot be updated");
            return;
        }

        // Update action space to match joint count
        int expectedActions = joints.Count;
        int currentActions = behaviorParams.BrainParameters.ActionSpec.NumContinuousActions;
        if (currentActions != expectedActions)
        {
            behaviorParams.BrainParameters.ActionSpec = Unity.MLAgents.Actuators.ActionSpec.MakeContinuous(expectedActions);
            TrainArenaDebugManager.Log($"✅ {name}: Updated action space from {currentActions} to {expectedActions} continuous actions", 
                TrainArenaDebugManager.DebugLogLevel.Important);
        }

        // Update observation space to match calculated observations
        int expectedObservations = GetTotalObservationCount();
        int currentObservations = behaviorParams.BrainParameters.VectorObservationSize;
        if (currentObservations != expectedObservations)
        {
            behaviorParams.BrainParameters.VectorObservationSize = expectedObservations;
            TrainArenaDebugManager.Log($"✅ {name}: Updated observation space from {currentObservations} to {expectedObservations} observations", 
                TrainArenaDebugManager.DebugLogLevel.Important);
        }

        // Ensure behavior name matches centralized configuration
        if (behaviorParams.BehaviorName != AgentConfiguration.RagdollAgent.BehaviorName)
        {
            behaviorParams.BehaviorName = AgentConfiguration.RagdollAgent.BehaviorName;
            TrainArenaDebugManager.Log($"✅ {name}: Set behavior name to '{AgentConfiguration.RagdollAgent.BehaviorName}'", 
                TrainArenaDebugManager.DebugLogLevel.Important);
        }

        // Check for incompatible model and clear it if necessary
        if (behaviorParams.Model != null)
        {
            TrainArenaDebugManager.LogWarning($"⚠️ {name}: Clearing loaded model due to configuration changes - will use heuristic behavior");
            TrainArenaDebugManager.LogWarning($"   Model expects different observation space than current configuration ({expectedObservations} observations)");
            behaviorParams.Model = null;
            
            // Force heuristic mode for development
            behaviorParams.BehaviorType = Unity.MLAgents.Policies.BehaviorType.HeuristicOnly;
        }

        TrainArenaDebugManager.Log($"🎭 {name}: ML-Agents configuration - {expectedActions} actions, {expectedObservations} observations", 
            TrainArenaDebugManager.DebugLogLevel.Important);
    }
    
    /// <summary>
    /// Get joint priority for sorting using centralized configuration
    /// Core stability joints come first, then extremities
    /// </summary>
    private int GetJointPriority(string jointName)
    {
        // Use centralized joint name constants instead of hardcoded strings
        
        // Core stability (highest priority)
        if (jointName == RagdollJointNames.Chest) return 10;
        if (jointName == RagdollJointNames.Head) return 20;
        
        // Upper body balance (arms for balancing)
        if (jointName == RagdollJointNames.LeftUpperArm || jointName == RagdollJointNames.RightUpperArm) return 30;
        if (jointName == RagdollJointNames.LeftLowerArm || jointName == RagdollJointNames.RightLowerArm) return 35;
        
        // Lower body locomotion (legs for walking)
        if (jointName == RagdollJointNames.LeftUpperLeg || jointName == RagdollJointNames.RightUpperLeg) return 40;
        if (jointName == RagdollJointNames.LeftLowerLeg || jointName == RagdollJointNames.RightLowerLeg) return 45;
        if (jointName == RagdollJointNames.LeftFoot || jointName == RagdollJointNames.RightFoot) return 50;
        
        // Check if it's a locomotion joint using the centralized method
        if (RagdollJointNames.IsLocomotionJoint(jointName)) return 60;
        
        // Default priority for unrecognized joints
        return 100;
    }
    
    /// <summary>
    /// Reset joint to neutral pose to prevent twisted spawning
    /// Properly configures ConfigurableJoint drives and targets for T-pose
    /// </summary>
    private void ResetJointToNeutralPose(ConfigurableJoint joint)
    {
        if (joint == null) return;
        
        // Reset the joint's target rotation to neutral (T-pose)
        joint.targetRotation = Quaternion.identity;
        
        // Apply T-pose specific rotations for natural positioning
        ApplyTPoseRotation(joint);
        
        // Configure joint drives for natural movement using joint-specific settings
        var (springForce, dampingForce) = RagdollTPoseConfig.GetJointDriveSettings(joint.name);
        
        var angularXDrive = joint.angularXDrive;
        angularXDrive.positionSpring = springForce;
        angularXDrive.positionDamper = dampingForce;
        joint.angularXDrive = angularXDrive;
        
        var angularYZDrive = joint.angularYZDrive;
        angularYZDrive.positionSpring = springForce;
        angularYZDrive.positionDamper = dampingForce;
        joint.angularYZDrive = angularYZDrive;
        
        // If it has a rigidbody, reset velocities and ensure it's active
        var rb = joint.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            // Ensure proper mass for joint stability using centralized configuration
            if (rb.mass < RagdollTPoseConfig.MinimumRigidbodyMass) 
                rb.mass = RagdollTPoseConfig.MinimumRigidbodyMass;
        }
        
        // Force the joint to update its configuration
        joint.configuredInWorldSpace = false;
        
        TrainArenaDebugManager.Log($"🔄 Reset joint '{joint.name}' to T-pose neutral position", 
            TrainArenaDebugManager.DebugLogLevel.Verbose);
    }

    /// <summary>
    /// Reset the entire ragdoll to T-pose neutral positions
    /// Ensures consistent spawning without twisted limbs
    /// </summary>
    private void ResetRagdollToTPose()
    {
        TrainArenaDebugManager.Log($"🔄 {name}: Resetting ragdoll to T-pose", 
            TrainArenaDebugManager.DebugLogLevel.Verbose);

        // Reset all joints to neutral positions
        foreach (var joint in joints)
        {
            if (joint != null && joint.joint != null)
            {
                ResetJointToNeutralPose(joint.joint);
                
                // Also reset the PD controller target
                joint.SetTarget01(0f); // Neutral position (center of range)
            }
        }

        // Wait one physics frame for joints to settle
        StartCoroutine(WaitForJointSettling());
    }

    /// <summary>
    /// Wait for joints to settle after T-pose reset
    /// </summary>
    private System.Collections.IEnumerator WaitForJointSettling()
    {
        yield return new WaitForFixedUpdate();
        
        TrainArenaDebugManager.Log($"✅ {name}: T-pose reset complete", 
            TrainArenaDebugManager.DebugLogLevel.Verbose);
    }

    /// <summary>
    /// Apply T-pose specific rotations for natural ragdoll positioning using centralized configuration
    /// Prevents twisted limbs by setting appropriate starting rotations
    /// </summary>
    private void ApplyTPoseRotation(ConfigurableJoint joint)
    {
        if (joint == null) return;
        
        string jointName = joint.name;
        
        // Use centralized T-pose configuration
        joint.targetRotation = RagdollTPoseConfig.GetTPoseRotation(jointName);
        
        TrainArenaDebugManager.Log($"🎯 Applied T-pose rotation to '{jointName}': {joint.targetRotation.eulerAngles}", 
            TrainArenaDebugManager.DebugLogLevel.Verbose);
    }
    
    /// <summary>
    /// Diagnostic coroutine to verify ML-Agents is sending actions to this ragdoll
    /// Helps identify configuration issues early in training
    /// </summary>
    System.Collections.IEnumerator DiagnoseActionCalls()
    {
        yield return new WaitForSeconds(3f); // Allow scene initialization to complete
        
        TrainArenaDebugManager.Log($"🔍 {name}: Starting action diagnostic check...", 
            TrainArenaDebugManager.DebugLogLevel.Important);
        
        // Monitor for 10 seconds to see if actions arrive
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(1f);
            // Action reception is logged in HandleActiveActions method
        }
        
        // Check behavior parameters for additional diagnostics
        var behaviorParams = GetComponent<Unity.MLAgents.Policies.BehaviorParameters>();
        if (behaviorParams != null)
        {
            TrainArenaDebugManager.Log($"🔍 {name}: Diagnostic complete - BehaviorType={behaviorParams.BehaviorType}, TeamId={behaviorParams.TeamId}", 
                TrainArenaDebugManager.DebugLogLevel.Important);
        }
    }

    protected override void OnAgentEpisodeBegin()
    {
        // Track episode start time for grace period and timeout
        episodeStartTime = Time.time;
        
        // Reset ragdoll to initial pose with slight elevation to prevent ground clipping
        if (pelvis != null)
        {
            pelvis.position = startPosition + Vector3.up * RagdollTPoseConfig.SpawnElevationOffset;
            pelvis.rotation = startRotation;
        }
        
        // Reset all rigidbody velocities to ensure clean episode start
        foreach (var rigidbody in GetComponentsInChildren<Rigidbody>())
        {
            rigidbody.linearVelocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }
        
        // Reset all joints to neutral pose to prevent twisted spawning
        ResetRagdollToTPose();
        
        // Enhanced episode start logging with detailed diagnostics
        float effectiveGracePeriod = episodeGracePeriod > 0 ? episodeGracePeriod : RewardConfig.EpisodeGracePeriod;
        float effectiveMaxDuration = maxEpisodeDuration > 0 ? maxEpisodeDuration : RewardConfig.MaxEpisodeDuration;
        TrainArenaDebugManager.Log($"🎭 {name}: Episode {CompletedEpisodes} started - Grace={effectiveGracePeriod:F1}s, Timeout={effectiveMaxDuration:F1}s", 
            TrainArenaDebugManager.DebugLogLevel.Important);
        TrainArenaDebugManager.Log($"🎭 {name}: Ragdoll State - {joints.Count} joints, pelvis Y={pelvis.position.y:F2}, Uprightness={Uprightness:F2}", 
            TrainArenaDebugManager.DebugLogLevel.Verbose);
        
        // ML-Agents configuration check (important level for troubleshooting)
        var behaviorParams = GetComponent<Unity.MLAgents.Policies.BehaviorParameters>();
        if (behaviorParams != null && CompletedEpisodes % 100 == 0) // Only log every 100 episodes to reduce spam
        {
            TrainArenaDebugManager.Log($"🎭 {name}: ML-Agents Status - Type={behaviorParams.BehaviorType}, Model={behaviorParams.Model != null}, MaxSteps={MaxStep}", 
                TrainArenaDebugManager.DebugLogLevel.Important);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (pelvis == null) return;
        
        var pelvisRigidbody = pelvis.GetComponent<Rigidbody>();

        // Observation 1: Pelvis Uprightness (1 value)
        // Range: [-1,1] where 1 = perfectly upright, 0 = horizontal, -1 = upside down
        sensor.AddObservation(Uprightness);
        
        // Observations 2-4: Pelvis velocity in local space (3 values)
        // Provides direction-aware velocity information for movement learning
        Vector3 localVelocity = pelvis.InverseTransformDirection(pelvisRigidbody.linearVelocity);
        sensor.AddObservation(localVelocity);

        // Observations 5+: Joint states (2 values per joint: angle + angular velocity)
        // Gives ML-Agents feedback on current joint positions and movement
        foreach (var joint in joints)
        {
            var jointRigidbody = joint.GetComponent<Rigidbody>();
            
            // Calculate current joint angle relative to parent (simplified X-axis rotation)
            Quaternion localRotation = Quaternion.Inverse(joint.transform.parent.rotation) * joint.transform.rotation;
            float currentAngle = Mathf.DeltaAngle(0f, localRotation.eulerAngles.x) * Mathf.Deg2Rad;
            
            // Add joint angle and angular velocity
            sensor.AddObservation(currentAngle);
            sensor.AddObservation(jointRigidbody != null ? jointRigidbody.angularVelocity.x : 0f);
        }
        
        // Total observations: 1 (uprightness) + 3 (velocity) + 2 * joints.Count (joint states)
    }

    protected override void HandleActiveActions(ActionBuffers actions)
    {
        var continuousActions = actions.ContinuousActions;
        
        // Validate action buffer matches joint configuration
        if (continuousActions.Length != joints.Count)
        {
            TrainArenaDebugManager.Log($"🎭 Warning: {name} - Action/joint mismatch: {continuousActions.Length} actions, {joints.Count} joints", 
                TrainArenaDebugManager.DebugLogLevel.Important);
            return;
        }
        
        // Apply ML-Agents actions to joint controllers
        ApplyActionsToJoints(continuousActions);
        
        // Calculate and apply rewards for learning progress
        CalculateLocomotionRewards();
        
        // Check ragdoll-specific episode termination conditions
        CheckRagdollEpisodeTermination();
    }

    /// <summary>
    /// Apply continuous actions from ML-Agents to individual joint controllers
    /// </summary>
    private void ApplyActionsToJoints(Unity.MLAgents.Actuators.ActionSegment<float> actions)
    {
        for (int i = 0; i < joints.Count; i++)
        {
            // Clamp actions to valid range and apply to joint PD controller
            float clampedAction = Mathf.Clamp(actions[i], -1f, 1f);
            joints[i].SetTarget01(clampedAction);
        }
    }

    /// <summary>
    /// Calculate rewards for joint-based ragdoll locomotion learning
    /// Balances uprightness, forward movement, energy efficiency, and stability
    /// </summary>
    private void CalculateLocomotionRewards()
    {
        if (pelvis == null) return;
        
        var pelvisRigidbody = pelvis.GetComponent<Rigidbody>();
        float forwardVelocity = Vector3.Dot(pelvisRigidbody.linearVelocity, transform.forward);
        
        // Use configuration values (with fallbacks if inspector values are 0)
        float effectiveTargetSpeed = targetSpeed > 0 ? targetSpeed : RewardConfig.TargetSpeed;
        
        // Primary reward: Forward movement within target speed range
        float normalizedVelocity = Mathf.Clamp(forwardVelocity, -effectiveTargetSpeed, effectiveTargetSpeed) / effectiveTargetSpeed;
        AddReward(normalizedVelocity * RewardConfig.VelocityRewardCoeff);
        
        // Balance reward: Encourage staying upright
        AddReward((Uprightness - RewardConfig.UprightThreshold) * RewardConfig.UprightRewardCoeff);
        
        // Energy efficiency: Penalize excessive joint movements
        float energyUsage = CalculateEnergyUsage();
        AddReward(-energyUsage * RewardConfig.EnergyPenaltyCoeff);
        
        // Stability reward: Discourage chaotic spinning
        float angularVelocityMagnitude = pelvisRigidbody.angularVelocity.magnitude;
        AddReward(-angularVelocityMagnitude * RewardConfig.AngularVelocityPenaltyCoeff);
        
        // Performance logging (sample periodically to avoid log spam)
        if (Time.fixedTime % 5f < Time.fixedDeltaTime)
        {
            TrainArenaDebugManager.Log($"🎭 {name}: Uprightness={Uprightness:F2} | Velocity={forwardVelocity:F2}m/s | Height={pelvis.position.y:F2}m | Energy={energyUsage:F3}", 
                TrainArenaDebugManager.DebugLogLevel.Verbose);
        }
    }

    /// <summary>
    /// Calculate energy usage based on joint action magnitudes
    /// </summary>
    private float CalculateEnergyUsage()
    {
        // Energy is approximated as sum of squared joint actions
        // Encourages smooth, coordinated movements over jerky actions
        float totalEnergy = 0f;
        for (int i = 0; i < joints.Count; i++)
        {
            // Note: We don't have direct access to last actions here, so this is a placeholder
            // In practice, this would need to be calculated in ApplyActionsToJoints
            totalEnergy += 0.1f; // Placeholder - would need action caching
        }
        return totalEnergy;
    }

    /// <summary>
    /// Check ragdoll-specific episode termination conditions with learning-friendly grace period
    /// </summary>
    private void CheckRagdollEpisodeTermination()
    {
        if (pelvis == null) return;
        
        float episodeTime = Time.time - episodeStartTime;
        float height = pelvis.position.y;
        
        // Use configuration values (with fallbacks if inspector values are 0)
        float effectiveGracePeriod = episodeGracePeriod > 0 ? episodeGracePeriod : RewardConfig.EpisodeGracePeriod;
        float effectiveMaxDuration = maxEpisodeDuration > 0 ? maxEpisodeDuration : RewardConfig.MaxEpisodeDuration;
        float effectiveMinUprightness = minUprightness > 0 ? minUprightness : RewardConfig.MinUprightness;
        float effectiveMinHeight = minHeight != 0 ? minHeight : RewardConfig.MinHeight;
        
        // Grace period: Allow ragdoll to fall and learn during initial seconds
        if (episodeTime < effectiveGracePeriod)
        {
            return; // No termination during grace period
        }
        
        // Episode timeout: Prevent episodes from running indefinitely
        if (episodeTime > effectiveMaxDuration)
        {
            TrainArenaDebugManager.Log($"🎭 {name}: Episode timeout after {episodeTime:F1}s - Uprightness={Uprightness:F2}, Height={height:F2}m", 
                TrainArenaDebugManager.DebugLogLevel.Important);
            EndEpisode();
            return;
        }
        
        // Failure conditions: Only terminate if ragdoll is severely compromised
        bool severelyFallen = Uprightness < effectiveMinUprightness;
        bool belowGround = height < effectiveMinHeight;
        
        if (severelyFallen || belowGround)
        {
            string reason = severelyFallen ? $"severe fall (Uprightness={Uprightness:F2} < {effectiveMinUprightness})" : $"below ground (height={height:F2}m < {effectiveMinHeight}m)";
            TrainArenaDebugManager.Log($"🎭 {name}: Episode terminated after {episodeTime:F1}s - {reason}", 
                TrainArenaDebugManager.DebugLogLevel.Important);
            EndEpisode();
        }
        else
        {
            // Periodic status logging during training (every 5 seconds)
            if (episodeTime % 5.0f < Time.fixedDeltaTime && episodeTime > effectiveGracePeriod)
            {
                TrainArenaDebugManager.Log($"🎭 {name}: Episode progress - Time={episodeTime:F1}s, Uprightness={Uprightness:F2}, Height={height:F2}m", 
                    TrainArenaDebugManager.DebugLogLevel.Verbose);
            }
        }
    }

    protected override void HandleInactiveState()
    {
        // Disable PD control when agent is inactive to allow natural ragdoll physics
        // This is useful for testing natural behavior or when ML-Agents is disconnected
        foreach (var joint in joints)
        {
            joint.DisableControl();
        }
    }
    
    void FixedUpdate()
    {
        // Performance monitoring - track training progress periodically
        if (Time.fixedTime % 10f < Time.fixedDeltaTime)
        {
            TrainArenaDebugManager.Log($"🎭 {name}: Episodes={CompletedEpisodes} | Steps={StepCount} | Activity={AgentActivity}", 
                TrainArenaDebugManager.DebugLogLevel.Verbose);
        }
    }

    protected override void HandleActiveHeuristic(in ActionBuffers actionsOut)
    {
        // Generate heuristic actions for manual testing and baseline comparison
        var heuristicActions = actionsOut.ContinuousActions;
        
        // Validate heuristic action buffer
        if (heuristicActions.Length != joints.Count)
        {
            TrainArenaDebugManager.Log($"🎭 Warning: {name} - Heuristic buffer size mismatch: expected {joints.Count}, got {heuristicActions.Length}", 
                TrainArenaDebugManager.DebugLogLevel.Important);
            return;
        }
        
        // Generate coordinated locomotion patterns using centralized configuration
        // This creates natural movements that should help the ragdoll balance and eventually walk
        float time = Time.time;
        float baseFrequency = heuristicFrequencyOverride > 0f ? 
            heuristicFrequencyOverride : 
            RagdollHeuristicConfig.BaseFrequency;
        
        for (int i = 0; i < joints.Count && i < heuristicActions.Length; i++)
        {
            string jointName = joints[i].name;
            
            // Get joint-specific heuristic configuration from centralized config
            var (amplitude, frequencyMult, basePhase) = RagdollHeuristicConfig.GetJointHeuristicConfig(jointName, i);
            
            // Apply global amplitude multiplier (with config fallback)
            float effectiveAmplitudeMultiplier = heuristicAmplitudeMultiplier > 0 ? heuristicAmplitudeMultiplier : RewardConfig.HeuristicAmplitudeDefault;
            amplitude *= effectiveAmplitudeMultiplier;
            
            // Calculate coordinated movement pattern
            float frequency = baseFrequency * frequencyMult;
            float action = amplitude * Mathf.Sin(time * frequency + basePhase);
            
            heuristicActions[i] = Mathf.Clamp(action, -1f, 1f);
        }
        
        // Debug log occasionally to verify heuristic is running
        if (Time.fixedTime % RagdollHeuristicConfig.DebugLogInterval < Time.fixedDeltaTime)
        {
            TrainArenaDebugManager.Log($"🎭 {name}: Heuristic active - generating coordinated walking pattern with {joints.Count} joints", 
                TrainArenaDebugManager.DebugLogLevel.Verbose);
        }
    }
}