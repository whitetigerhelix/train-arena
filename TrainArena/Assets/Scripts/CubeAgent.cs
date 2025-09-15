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
        float radius = 4f; // Reduced to match ground size (14x14 ground = 7 radius, use 4 for safety margin)
        
        // Position agent ON TOP of ground (Y = 1.0f, not 0.5f)
        Vector3 newAgentPos = center + new Vector3(Random.Range(-radius, radius), 1.0f, Random.Range(-radius, radius));
        transform.position = newAgentPos;
        transform.rotation = Quaternion.Euler(0f, Random.Range(0, 360f), 0f);

        if (goal != null)
        {
            Vector3 newGoalPos = center + new Vector3(Random.Range(-radius, radius), 1.0f, Random.Range(-radius, radius));
            goal.position = newGoalPos;
            
            // Debug logging for episode resets (verbose level only)
            TrainArenaDebugManager.Log($"Episode Reset: Agent {gameObject.name} moved to {newAgentPos}, Goal to {newGoalPos}", 
                                     TrainArenaDebugManager.DebugLogLevel.Verbose);
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

    private void OnDrawGizmos()
    {
        // Show raycast visualization if global toggle is ON, during play mode
        if (Application.isPlaying && TrainArenaDebugManager.ShowRaycastVisualization)
        {
            DrawRaycastVisualization();
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Show raycast visualization when selected (even if global toggle is OFF)
        if (Application.isPlaying && !TrainArenaDebugManager.ShowRaycastVisualization)
        {
            DrawRaycastVisualization();
        }
        
        // Additional info when selected
        Gizmos.color = Color.white;
        
        // Draw arena bounds
        var arena = transform.parent;
        if (arena != null)
        {
            Vector3 center = arena.position;
            Gizmos.DrawWireCube(center, new Vector3(14f, 0.1f, 14f)); // Arena boundaries
        }
        
        // Draw movement force vector
        if (Application.isPlaying && rb != null)
        {
            Gizmos.color = Color.blue;
            Vector3 velocity = rb.linearVelocity;
            Gizmos.DrawLine(transform.position, transform.position + velocity);
        }
    }
    
    private void DrawRaycastVisualization()
    {
        Vector3 rayStart = transform.position + Vector3.up * 0.2f;
        
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f;
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * transform.forward;
            
            // Perform the same raycast as in CollectObservations
            if (Physics.Raycast(rayStart, dir, out RaycastHit hit, rayLength, obstacleMask))
            {
                // Red line to hit point, then green line for remaining distance
                Gizmos.color = Color.red;
                Gizmos.DrawLine(rayStart, hit.point);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(hit.point, rayStart + dir * rayLength);
                
                // Draw small sphere at hit point
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(hit.point, 0.1f);
            }
            else
            {
                // Cyan line for clear path
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(rayStart, rayStart + dir * rayLength);
            }
        }
        
        // Draw goal direction vector
        if (goal != null)
        {
            Gizmos.color = Color.magenta;
            Vector3 toGoal = (goal.position - transform.position).normalized * 2f;
            Gizmos.DrawLine(transform.position, transform.position + toGoal);
            Gizmos.DrawSphere(transform.position + toGoal, 0.15f);
        }
    }
}