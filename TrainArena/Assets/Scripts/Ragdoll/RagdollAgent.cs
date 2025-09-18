using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

/// <summary>
/// Agent activity state - controls whether agents respond to actions
/// This is separate from ML-Agents BehaviorType and controls whether agents respond to actions
/// </summary>
public enum AgentActivity
{
    Active,     // Agent responds normally to all actions
    Inactive    // Agent ignores all actions and remains stationary (for demos)
}

public class RagdollAgent : Agent
{
    [Header("Joints (order = action ordering)")]
    public List<PDJointController> joints = new List<PDJointController>();
    public Transform pelvis;
    public float targetSpeed = 1.0f;
    public float uprightBonus = 0.5f;
    
    [Header("Agent Activity Control")]
    public AgentActivity agentActivity = AgentActivity.Active;  // Controls whether agent responds to actions

    Vector3 startPos;
    Quaternion startRot;

    public override void Initialize()
    {
        if (pelvis == null && transform != null) pelvis = transform;
        startPos = pelvis.position;
        startRot = pelvis.rotation;
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

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Check if agent is inactive - if so, don't respond to actions (for demos)
        if (agentActivity == AgentActivity.Inactive)
        {
            return; // Skip all actions but keep agent in scene
        }
        
        // Map actions [-1,1] to joint target angles
        var ca = actions.ContinuousActions;
        for (int i = 0; i < joints.Count && i < ca.Length; i++)
            joints[i].SetTarget01(Mathf.Clamp(ca[i], -1f, 1f));

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

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Check if agent is inactive - if so, don't provide heuristic actions
        if (agentActivity == AgentActivity.Inactive)
        {
            // Zero out all actions when inactive
            var ca = actionsOut.ContinuousActions;
            for (int i = 0; i < ca.Length; i++)
                ca[i] = 0f;
            return;
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