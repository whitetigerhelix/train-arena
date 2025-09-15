using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TaggerAgentTrainable : Agent
{
    public Transform runner;
    public float moveAccel = 12f;
    public float catchDistance = 1.1f;
    public float rayLength = 8f;
    public LayerMask wallMask;

    Rigidbody rb;
    Vector3 spawnCenter;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        spawnCenter = transform.position;
    }

    public override void OnEpisodeBegin()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // positions randomized by arena spawner typically
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.InverseTransformDirection(rb.velocity)); // 3

        if (runner != null)
        {
            var rrb = runner.GetComponent<Rigidbody>();
            Vector3 toRunner = runner.position - transform.position;
            sensor.AddObservation(transform.InverseTransformDirection(toRunner)); // 3
            sensor.AddObservation(transform.InverseTransformDirection(rrb ? rrb.velocity : Vector3.zero)); // 3
        }
        else { sensor.AddObservation(Vector3.zero); sensor.AddObservation(Vector3.zero); }

        // Wall rays (8)
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * transform.forward;
            if (Physics.Raycast(transform.position + Vector3.up*0.2f, dir, out var hit, rayLength, wallMask))
                sensor.AddObservation(hit.distance / rayLength);
            else sensor.AddObservation(1f);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float mx = Mathf.Clamp(actions.ContinuousActions[0], -1, 1);
        float mz = Mathf.Clamp(actions.ContinuousActions[1], -1, 1);
        Vector3 localMove = new Vector3(mx, 0, mz);
        rb.AddForce(transform.TransformDirection(localMove) * moveAccel, ForceMode.Acceleration);

        // shaped: encourage proximity, reward catch, penalize energy
        float d = Vector3.Distance(transform.position, runner.position);
        AddReward(-0.0005f * localMove.sqrMagnitude);
        AddReward(Mathf.Clamp01((2.5f - d) / 2.5f) * 0.002f); // closer = better

        if (d < catchDistance)
        {
            AddReward(+1.0f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var ca = actionsOut.ContinuousActions;
        ca[0] = Input.GetAxis("Horizontal");
        ca[1] = Input.GetAxis("Vertical");
    }
}