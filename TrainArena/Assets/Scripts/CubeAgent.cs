using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CubeAgent : Agent
{
    [Header("Scene Refs")]
    public Transform goal;
    public LayerMask obstacleMask;
    public float moveAccel = 10f;
    public float rayLength = 10f;

    Rigidbody rb;
    float prevDist;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 20f;
    }

    public override void OnEpisodeBegin()
    {
        // Reset physics
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Randomize start & goal within arena bounds (assumes parent positions origin of arena)
        var arena = transform.parent;
        Vector3 center = arena ? arena.position : Vector3.zero;
        float radius = 6f;
        transform.position = center + new Vector3(Random.Range(-radius, radius), 0.5f, Random.Range(-radius, radius));
        transform.rotation = Quaternion.Euler(0f, Random.Range(0, 360f), 0f);

        if (goal != null)
        {
            goal.position = center + new Vector3(Random.Range(-radius, radius), 0.5f, Random.Range(-radius, radius));
        }

        prevDist = goal ? Vector3.Distance(transform.position, goal.position) : 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Local velocity
        sensor.AddObservation(transform.InverseTransformDirection(rb.linearVelocity)); // 3

        // Vector to goal in local space
        if (goal != null)
        {
            Vector3 toGoal = goal.position - transform.position;
            sensor.AddObservation(transform.InverseTransformDirection(toGoal)); // 3
        }
        else
        {
            sensor.AddObservation(Vector3.zero); // keep obs size consistent
        }

        // 8 planar raycasts (N,E,S,W + diagonals)
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f;
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * transform.forward;
            if (Physics.Raycast(transform.position + Vector3.up * 0.2f, dir, out RaycastHit hit, rayLength, obstacleMask))
                sensor.AddObservation(hit.distance / rayLength);
            else
                sensor.AddObservation(1f);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float moveZ = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

        Vector3 localMove = new Vector3(moveX, 0f, moveZ);
        Vector3 worldForce = transform.TransformDirection(localMove) * moveAccel;
        rb.AddForce(worldForce, ForceMode.Acceleration);

        // Time penalty + tiny energy penalty
        AddReward(-0.001f);
        AddReward(-0.0005f * localMove.sqrMagnitude);

        if (goal != null)
        {
            float d = Vector3.Distance(transform.position, goal.position);
            AddReward(prevDist - d); // progress
            prevDist = d;

            if (d < 0.6f)
            {
                AddReward(+1.0f);
                EndEpisode();
            }
        }

        if (transform.position.y < -1f)
            EndEpisode();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var ca = actionsOut.ContinuousActions;
        ca[0] = Input.GetAxis("Horizontal");
        ca[1] = Input.GetAxis("Vertical");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Obstacle"))
            AddReward(-0.1f);
    }

    private void OnDrawGizmosSelected()
    {
        // visualize rays
        Gizmos.color = Color.cyan;
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f;
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * transform.forward;
            Gizmos.DrawLine(transform.position + Vector3.up * 0.2f, transform.position + Vector3.up * 0.2f + dir * rayLength);
        }
    }
}