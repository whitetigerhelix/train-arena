using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using TrainArena.Core;

public class RagdollAgent : BaseTrainArenaAgent
{
    [Header("Joints (order = action ordering)")]
    public List<PDJointController> joints = new List<PDJointController>();
    public Transform pelvis;
    public float targetSpeed = 1.0f;
    public float uprightBonus = 0.5f;
    
    // AgentActivity is now inherited from BaseTrainArenaAgent
    
    Vector3 startPos;
    Quaternion startRot;
    
    // BaseTrainArenaAgent abstract property implementations
    public override Transform MainTransform => pelvis;
    public override Rigidbody MainRigidbody => pelvis?.GetComponent<Rigidbody>();
    public override string AgentTypeIcon => "🎭";

    public override void Initialize()
    {
        if (pelvis == null && transform != null) pelvis = transform;
        startPos = pelvis.position;
        startRot = pelvis.rotation;
        
        // Debug logging for initialization
        TrainArenaDebugManager.Log($"🎭 {name}: Initialized - AgentActivity={AgentActivity}, BehaviorType={BehaviorParameters?.BehaviorType}, Joints={joints.Count}", 
            TrainArenaDebugManager.DebugLogLevel.Important);
    }

    public override void OnEpisodeBegin()
    {
        // Reset pose & velocities
        pelvis.position = startPos + Vector3.up * 0.2f;
        pelvis.rotation = startRot;
        foreach (var rb in GetComponentsInChildren<Rigidbody>())
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        // Debug logging for ragdoll configuration
        TrainArenaDebugManager.Log($"🎭 {name} Episode Begin - Joints: {joints.Count}, Pelvis Y: {pelvis.position.y:F2}", TrainArenaDebugManager.DebugLogLevel.Verbose);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Pelvis orientation (up dir) and velocity
        sensor.AddObservation(Vector3.Dot(pelvis.up, Vector3.up)); // uprightness scalar
        var rbPelvis = pelvis.GetComponent<Rigidbody>();
        sensor.AddObservation(pelvis.InverseTransformDirection(rbPelvis.linearVelocity)); // 3

        // Each joint: current local angle + ang vel (very approximate w/ PDJointController assumptions)
        foreach (var j in joints)
        {
            var rb = j.GetComponent<Rigidbody>();
            Quaternion localRot = Quaternion.Inverse(j.transform.parent.rotation) * j.transform.rotation;
            float angle = Mathf.DeltaAngle(0f, localRot.eulerAngles.x) * Mathf.Deg2Rad;
            sensor.AddObservation(angle);
            sensor.AddObservation(rb ? rb.angularVelocity.x : 0f);
        }
    }

    protected override void HandleActiveActions(ActionBuffers actions)
    {
        // Debug logging to track action execution
        if (Time.fixedTime % 3f < Time.fixedDeltaTime) // Log every 3 seconds
        {
            TrainArenaDebugManager.Log($"🎭 {name}: HandleActiveActions called - received {actions.ContinuousActions.Length} actions", 
                TrainArenaDebugManager.DebugLogLevel.Verbose);
        }
        
        // Map actions [-1,1] to joint target angles
        var ca = actions.ContinuousActions;
        for (int i = 0; i < joints.Count && i < ca.Length; i++)
            joints[i].SetTarget01(Mathf.Clamp(ca[i], -1f, 1f));

        // Diagnostic logging for ragdoll behavior
        if (Time.fixedTime % 2f < Time.fixedDeltaTime) // Log every 2 seconds
        {
            var rbPelvis = pelvis.GetComponent<Rigidbody>();
            float uprightness = Vector3.Dot(pelvis.up, Vector3.up);
            TrainArenaDebugManager.Log($"🎭 {name}: Upright={uprightness:F2}, Vel={rbPelvis.linearVelocity.magnitude:F2}, Y={pelvis.position.y:F2}, Actions=[{string.Join(",", System.Array.ConvertAll(ca.Array, x => x.ToString("F1")))}]", 
                TrainArenaDebugManager.DebugLogLevel.Verbose);
            
            // Log joint forces for first joint as sample
            if (joints.Count > 0)
            {
                var joint0 = joints[0];
                TrainArenaDebugManager.Log($"🎭 {name}: Joint0 kp={joint0.kp}, kd={joint0.kd}, target range=[{joint0.minAngle:F0}°, {joint0.maxAngle:F0}°]", 
                    TrainArenaDebugManager.DebugLogLevel.Verbose);
            }
        }

        // Rewards
        float forward = Vector3.Dot(pelvis.GetComponent<Rigidbody>().linearVelocity, transform.forward);
        AddReward(Mathf.Clamp(forward, -targetSpeed, targetSpeed) / targetSpeed * 0.02f);
        AddReward((Vector3.Dot(pelvis.up, Vector3.up) - 0.8f) * 0.01f); // bonus if upright

        // Calculate energy penalty (sum of squares of actions)
        float energy = 0f;
        for (int i = 0; i < ca.Length; i++)
            energy += ca[i] * ca[i];
        AddReward(-0.001f * energy); // energy

        // Fail on fall
        if (Vector3.Dot(pelvis.up, Vector3.up) < 0.4f || pelvis.position.y < 0.2f)
            EndEpisode();
    }

    protected override void HandleInactiveState()
    {
        // When inactive, disable joint control to allow natural physics
        foreach (var joint in joints)
        {
            joint.DisableControl();
        }
    }

    protected override void HandleActiveHeuristic(in ActionBuffers actionsOut)
    {
        // Debug logging to track heuristic execution
        if (Time.fixedTime % 3f < Time.fixedDeltaTime) // Log every 3 seconds
        {
            TrainArenaDebugManager.Log($"🎭 {name}: HandleActiveHeuristic called - generating {joints.Count} actions", 
                TrainArenaDebugManager.DebugLogLevel.Verbose);
        }
        
        // More pronounced manual wiggle when active for better visibility
        var ca2 = actionsOut.ContinuousActions;
        for (int i = 0; i < joints.Count && i < ca2.Length; i++)
        {
            // Use larger amplitude and different frequencies for each joint
            float amplitude = 0.8f; // Increased from implicit 1.0f to make movement more pronounced
            float frequency = 1.5f + i * 0.3f; // Different frequency per joint
            ca2[i] = amplitude * Mathf.Sin(Time.time * frequency + i * 0.8f);
        }
    }
}