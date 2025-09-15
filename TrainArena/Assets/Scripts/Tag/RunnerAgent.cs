using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RunnerAgent : Agent
{
    public Transform tagger;
    public LayerMask wallMask;
    public float moveAccel = 10f;
    public float arenaRadius = 6f;
    public float rayLength = 8f;
    public RewardHUD rewardHUD;

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

        // randomize positions
        transform.position = spawnCenter + new Vector3(Random.Range(-arenaRadius, arenaRadius), 0.5f, Random.Range(-arenaRadius, arenaRadius));
        if (tagger != null)
            tagger.position = spawnCenter + new Vector3(Random.Range(-arenaRadius, arenaRadius), 0.5f, Random.Range(-arenaRadius, arenaRadius));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Local velocity of runner
        sensor.AddObservation(transform.InverseTransformDirection(rb.velocity)); // 3

        // Relative position/velocity of tagger
        if (tagger != null)
        {
            var tagRB = tagger.GetComponent<Rigidbody>();
            Vector3 toTagger = tagger.position - transform.position;
            sensor.AddObservation(transform.InverseTransformDirection(toTagger)); // 3
            sensor.AddObservation(transform.InverseTransformDirection(tagRB ? tagRB.velocity : Vector3.zero)); // 3
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(Vector3.zero);
        }

        // 8 wall rays (keep distance from boundary)
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
        float moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float moveZ = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        Vector3 localMove = new Vector3(moveX, 0f, moveZ);
        rb.AddForce(transform.TransformDirection(localMove) * moveAccel, ForceMode.Acceleration);

        // Rewards
        float dist = Vector3.Distance(transform.position, tagger.position);
        float keepAway = Mathf.Clamp01((dist - 1.5f) / (arenaRadius)); // scaled 0..1
        AddReward(0.002f * keepAway); // survive reward
        AddReward(-0.0005f * localMove.sqrMagnitude); // small energy cost
        if (rewardHUD) { rewardHUD.SetReward("Survival", 2f*0.002f*keepAway); rewardHUD.SetReward("Energy", -0.0005f * localMove.sqrMagnitude * 50f); }

        // End if caught
        if (dist < 1.1f)
        {
            AddReward(-1f);
            EndEpisode();
        }

        // Keep inside arena (soft penalty near edges)
        Vector3 flat = transform.position - spawnCenter;
        flat.y = 0f;
        float border = Mathf.Clamp01((flat.magnitude - (arenaRadius - 1f)) / 1f);
        if (border > 0f) AddReward(-0.002f * border);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var ca = actionsOut.ContinuousActions;
        ca[0] = Input.GetAxis("Horizontal");
        ca[1] = Input.GetAxis("Vertical");
    }
}