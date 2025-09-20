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
    [Tooltip("Target forward velocity for reward calculation")]
    public float targetSpeed = 1.0f;
    
    [Tooltip("Weight for uprightness reward component")]
    public float uprightBonus = 0.5f;
    
    [Header("Heuristic Configuration")]
    [Tooltip("Override base frequency for heuristic movements (0 = use default from config)")]
    public float heuristicFrequencyOverride = 0f;
    
    [Tooltip("Global amplitude multiplier for all heuristic movements")]
    [Range(0.1f, 2.0f)]
    public float heuristicAmplitudeMultiplier = 1.0f;
    
    // Observation space constants (matching CubeAgent pattern)
    public const int UPRIGHTNESS_OBSERVATIONS = 1;      // Pelvis uprightness (dot product with world up)
    public const int VELOCITY_OBSERVATIONS = 3;         // Pelvis velocity (x, y, z)
    public const int JOINT_STATE_OBSERVATIONS_PER_JOINT = 2; // Angle and angular velocity per joint
    // Total observations = UPRIGHTNESS_OBSERVATIONS + VELOCITY_OBSERVATIONS + (joints.Count * JOINT_STATE_OBSERVATIONS_PER_JOINT)
    
    // Episode management
    [Header("Episode Management")]
    [Tooltip("Minimum time before episode can end (learning grace period)")]
    public float episodeGracePeriod = 8.0f;
    
    [Tooltip("Maximum episode duration before auto-reset")]
    public float maxEpisodeDuration = 30.0f;
    
    [Tooltip("Minimum uprightness to avoid termination")]
    public float minUprightness = 0.1f;  // Very lenient during learning
    
    [Tooltip("Minimum height before episode termination")]
    public float minHeight = -0.5f;  // Allow falling below ground briefly
    
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
        return UPRIGHTNESS_OBSERVATIONS + VELOCITY_OBSERVATIONS + (joints.Count * JOINT_STATE_OBSERVATIONS_PER_JOINT);
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

        // Verify each joint has required components
        for (int i = 0; i < joints.Count; i++)
        {
            var joint = joints[i];
            if (joint == null)
            {
                TrainArenaDebugManager.LogError($"❌ {name}: Joint {i} is null!");
                continue;
            }

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
            pelvis.position = startPosition + Vector3.up * 0.2f;
            pelvis.rotation = startRotation;
        }
        
        // Reset all rigidbody velocities to ensure clean episode start
        foreach (var rigidbody in GetComponentsInChildren<Rigidbody>())
        {
            rigidbody.linearVelocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }
        
        // Enhanced episode start logging with detailed diagnostics
        TrainArenaDebugManager.Log($"🎭 {name}: Episode {CompletedEpisodes} started - Grace={episodeGracePeriod:F1}s, Timeout={maxEpisodeDuration:F1}s", 
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
        
        // Primary reward: Forward movement within target speed range
        float normalizedVelocity = Mathf.Clamp(forwardVelocity, -targetSpeed, targetSpeed) / targetSpeed;
        AddReward(normalizedVelocity * 0.03f);
        
        // Balance reward: Encourage staying upright (threshold at 0.8 for stability)
        AddReward((Uprightness - 0.8f) * 0.02f);
        
        // Energy efficiency: Penalize excessive joint movements
        float energyUsage = CalculateEnergyUsage();
        AddReward(-energyUsage * 0.001f);
        
        // Stability reward: Discourage chaotic spinning
        float angularVelocityMagnitude = pelvisRigidbody.angularVelocity.magnitude;
        AddReward(-angularVelocityMagnitude * 0.001f);
        
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
        
        // Grace period: Allow ragdoll to fall and learn during initial seconds
        if (episodeTime < episodeGracePeriod)
        {
            return; // No termination during grace period
        }
        
        // Episode timeout: Prevent episodes from running indefinitely
        if (episodeTime > maxEpisodeDuration)
        {
            TrainArenaDebugManager.Log($"🎭 {name}: Episode timeout after {episodeTime:F1}s - Uprightness={Uprightness:F2}, Height={height:F2}m", 
                TrainArenaDebugManager.DebugLogLevel.Important);
            EndEpisode();
            return;
        }
        
        // Failure conditions: Only terminate if ragdoll is severely compromised
        bool severelyFallen = Uprightness < minUprightness;
        bool belowGround = height < minHeight;
        
        if (severelyFallen || belowGround)
        {
            string reason = severelyFallen ? $"severe fall (Uprightness={Uprightness:F2} < {minUprightness})" : $"below ground (height={height:F2}m < {minHeight}m)";
            TrainArenaDebugManager.Log($"🎭 {name}: Episode terminated after {episodeTime:F1}s - {reason}", 
                TrainArenaDebugManager.DebugLogLevel.Important);
            EndEpisode();
        }
        else
        {
            // Periodic status logging during training (every 5 seconds)
            if (episodeTime % 5.0f < Time.fixedDeltaTime && episodeTime > episodeGracePeriod)
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
            
            // Apply global amplitude multiplier from inspector
            amplitude *= heuristicAmplitudeMultiplier;
            
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