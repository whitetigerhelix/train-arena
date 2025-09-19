using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using TrainArena.Core;

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
    //TODO: We need to move more important shared logic into BaseTrainArenaAgent from CubeAgent, and leverage that here, then tune here appropriately for ragdoll locomotion training

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
    
    // Episode reset state
    private Vector3 startPosition;
    private Quaternion startRotation;
    
    // BaseTrainArenaAgent abstract property implementations
    public override Transform MainTransform => pelvis;
    public override Rigidbody MainRigidbody => pelvis?.GetComponent<Rigidbody>();
    public override string AgentTypeIcon => "🎭";

    public override int GetTotalObservationCount()
    {
        return 16;  //TODO: Don't hardcode - use constants like CubeAgent does, add a comment describing the observations so it's clear
    }

    public override void Initialize()
    {
        // Ensure pelvis reference is set
        if (pelvis == null && transform != null) 
            pelvis = transform;
        
        // Store initial pose for episode resets
        startPosition = pelvis.position;
        startRotation = pelvis.rotation;
        
        // Log initialization success
        TrainArenaDebugManager.Log($"🎭 {name}: Initialized ragdoll with {joints.Count} joints, Activity={AgentActivity}", 
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

    static int globalRagdollEpisodeCount = 0; // Track episodes across all ragdoll agents
    
    public override void OnEpisodeBegin()
    {
        // Reset ragdoll to initial pose with slight elevation to prevent ground clipping
        pelvis.position = startPosition + Vector3.up * 0.2f;
        pelvis.rotation = startRotation;
        
        // Reset all rigidbody velocities to ensure clean episode start
        foreach (var rigidbody in GetComponentsInChildren<Rigidbody>())
        {
            rigidbody.linearVelocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }
        
        // Global episode tracking for memory management
        globalRagdollEpisodeCount++;
        
        // Perform periodic garbage collection to prevent memory accumulation during training
        if (globalRagdollEpisodeCount % 50 == 0)
        {
            System.GC.Collect();
            TrainArenaDebugManager.Log($"🧹 Memory: Garbage collection after {globalRagdollEpisodeCount} episodes", 
                                     TrainArenaDebugManager.DebugLogLevel.Important);
        }
        
        // Episode start logging (verbose only to avoid spam during training)
        TrainArenaDebugManager.Log($"🎭 {name}: Episode {CompletedEpisodes} started - {joints.Count} joints, pelvis at Y={pelvis.position.y:F2}", 
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
        var pelvisRigidbody = pelvis.GetComponent<Rigidbody>();
        
        // Observation 1: Pelvis uprightness (1 value)
        // Range: [-1,1] where 1 = perfectly upright, 0 = horizontal, -1 = upside down
        float uprightness = Vector3.Dot(pelvis.up, Vector3.up);
        sensor.AddObservation(uprightness);
        
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
        
        // Check episode termination conditions
        CheckEpisodeTermination();
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
        var pelvisRigidbody = pelvis.GetComponent<Rigidbody>();
        float uprightness = Vector3.Dot(pelvis.up, Vector3.up);
        float forwardVelocity = Vector3.Dot(pelvisRigidbody.linearVelocity, transform.forward);
        
        // Primary reward: Forward movement within target speed range
        float normalizedVelocity = Mathf.Clamp(forwardVelocity, -targetSpeed, targetSpeed) / targetSpeed;
        AddReward(normalizedVelocity * 0.03f);
        
        // Balance reward: Encourage staying upright (threshold at 0.8 for stability)
        AddReward((uprightness - 0.8f) * 0.02f);
        
        // Energy efficiency: Penalize excessive joint movements
        float energyUsage = CalculateEnergyUsage();
        AddReward(-energyUsage * 0.001f);
        
        // Stability reward: Discourage chaotic spinning
        float angularVelocityMagnitude = pelvisRigidbody.angularVelocity.magnitude;
        AddReward(-angularVelocityMagnitude * 0.001f);
        
        // Performance logging (sample periodically to avoid log spam)
        if (Time.fixedTime % 5f < Time.fixedDeltaTime)
        {
            TrainArenaDebugManager.Log($"🎭 {name}: Upright={uprightness:F2} | Velocity={forwardVelocity:F2}m/s | Height={pelvis.position.y:F2}m | Energy={energyUsage:F3}", 
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
    /// Check if episode should terminate due to ragdoll falling or other conditions
    /// </summary>
    private void CheckEpisodeTermination()
    {
        float uprightness = Vector3.Dot(pelvis.up, Vector3.up);
        float height = pelvis.position.y;
        
        // Terminate if ragdoll falls over or clips through ground
        if (uprightness < 0.3f || height < 0.2f)
        {
            TrainArenaDebugManager.Log($"🎭 {name}: Episode terminated - Uprightness={uprightness:F2}, Height={height:F2}m", 
                TrainArenaDebugManager.DebugLogLevel.Verbose);
            EndEpisode();
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
        
        // Generate coordinated sinusoidal movements for walking-like behavior
        // This provides a baseline for comparing trained behavior against
        for (int i = 0; i < joints.Count && i < heuristicActions.Length; i++)
        {
            // Create phase-shifted sinusoidal patterns for different joints
            float amplitude = 0.6f;                    // Movement strength
            float baseFrequency = 1.0f;               // Base walking frequency
            float jointFrequency = baseFrequency + i * 0.2f; // Stagger joint timing
            float phaseOffset = i * Mathf.PI / 3f;    // Phase difference between joints
            
            // Generate coordinated joint movement
            heuristicActions[i] = amplitude * Mathf.Sin(Time.time * jointFrequency + phaseOffset);
        }
    }
}