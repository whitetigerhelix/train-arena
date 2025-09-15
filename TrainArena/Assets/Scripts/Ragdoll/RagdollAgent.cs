using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class RagdollAgent : Agent
{
    [Header("Joints (order = action ordering)")]
    public List<PDJointController> joints = new List<PDJointController>();
    public Transform pelvis;
    public float targetSpeed = 1.0f;
    public float uprightBonus = 0.5f;

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
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Pelvis orientation (up dir) and velocity
        sensor.AddObservation(Vector3.Dot(pelvis.up, Vector3.up)); // uprightness scalar
        var rbPelvis = pelvis.GetComponent<Rigidbody>();
        sensor.AddObservation(pelvis.InverseTransformDirection(rbPelvis.velocity)); // 3

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
        // Map actions [-1,1] to joint target angles
        var ca = actions.ContinuousActions;
        for (int i = 0; i < joints.Count && i < ca.Length; i++)
            joints[i].SetTarget01(Mathf.Clamp(ca[i], -1f, 1f));

        // Rewards
        float forward = Vector3.Dot(pelvis.GetComponent<Rigidbody>().velocity, transform.forward);
        AddReward(Mathf.Clamp(forward, -targetSpeed, targetSpeed) / targetSpeed * 0.02f);
        AddReward((Vector3.Dot(pelvis.up, Vector3.up) - 0.8f) * 0.01f); // bonus if upright
        AddReward(-0.001f * ca.SqrMagnitude()); // energy

        // Fail on fall
        if (Vector3.Dot(pelvis.up, Vector3.up) < 0.4f || pelvis.position.y < 0.2f)
            EndEpisode();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // simple manual wiggle
        var ca = actionsOut.ContinuousActions;
        for (int i = 0; i < joints.Count && i < ca.Length; i++)
            ca[i] = Mathf.Sin(Time.time * 2f + i * 0.5f);
    }
}