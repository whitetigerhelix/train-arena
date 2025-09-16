using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class CubeAgent : Agent
{
    [Header("Scene Refs")]
    public Transform goal;
    public LayerMask obstacleMask;
    public float moveAccel = 10f;
    public float rayLength = 10f;
    
    [Header("Observation Space Configuration")]
    public int raycastDirections = 8;
    
    [Header("Episode Management")]
    public int maxEpisodeSteps = 100;               // Ultra-short episodes to prevent Unity hanging
    public float episodeTimeLimit = 10f;            // Ultra-short time limit (10 seconds)
    
    // Episode tracking
    private int episodeStepCount;
    private float episodeStartTime;
    private int totalEpisodesCompleted;
    
    // Memory management
    private static int globalEpisodeCount = 0;
    
    // Observation space constants
    public const int VELOCITY_OBSERVATIONS = 3;     // Local velocity (x, y, z)
    public const int GOAL_OBSERVATIONS = 3;         // Local goal direction (x, y, z)
    // raycast observations = raycastDirections (configurable)
    
    // Timing and threshold constants
    private const float PHYSICS_DEBUG_INTERVAL = 10f;       // Physics debugging every 10 seconds
    private const float AGENT_LOG_INTERVAL = 5f;            // Agent status logging every 5 seconds
    private const float RANDOM_ACTION_CHANGE_TIME = 2f;     // Change random actions every 2 seconds
    private const float MOVEMENT_THRESHOLD = 0.1f;          // Minimum velocity to consider "moving"
    private const float HIGH_VELOCITY_THRESHOLD = 2f;       // High velocity worth logging
    private const float FORCE_DISPLAY_THRESHOLD = 0.1f;     // Minimum force to display in gizmos

    Rigidbody rb;
    float prevDist;
    Vector3 lastAppliedForce;
    
    // Physics debugging
    private Vector3 totalForceThisFrame;
    private int forceApplicationCount;
    private float lastPhysicsDebugTime;
    
    // For testing when ML-Agents isn't connected
    private float randomActionTimer;
    private Vector2 currentRandomAction;
    
    /// <summary>
    /// Calculate total observation count based on configuration
    /// </summary>
    public int GetTotalObservationCount()
    {
        return VELOCITY_OBSERVATIONS + GOAL_OBSERVATIONS + raycastDirections;
    }

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 20f;
        
        // Debug Rigidbody setup (only at verbose level - not needed during normal operation)
        TrainArenaDebugManager.Log($"🔧 {gameObject.name} INITIALIZED: Mass={rb.mass:F1} | Drag={rb.linearDamping:F1} | AngDrag={rb.angularDamping:F1} | isKinematic={rb.isKinematic} | Constraints={rb.constraints}", 
                                 TrainArenaDebugManager.DebugLogLevel.Verbose);
        
        // Check for potential physics blockers (verbose level only)
        var colliders = GetComponents<Collider>();
        foreach (var col in colliders)
        {
            TrainArenaDebugManager.Log($"🔧 {gameObject.name} Collider: {col.GetType().Name} | isTrigger={col.isTrigger} | enabled={col.enabled}", 
                                     TrainArenaDebugManager.DebugLogLevel.Verbose);
        }
        
        // Initialize random action for testing
        randomActionTimer = 0f;
        GenerateRandomAction();
    }
    
    void FixedUpdate()
    {
        // Request decisions for ML-Agents system to work
        RequestDecision();
    }
    
    void LateUpdate()
    {
        // Physics debugging only for truly stuck agents (much less frequent)
        if (Time.time - lastPhysicsDebugTime > (PHYSICS_DEBUG_INTERVAL * 3f)) // Every 30 seconds
        {
            // Only log if agent is really stuck with consistent force application
            if (forceApplicationCount > 10 && rb.linearVelocity.magnitude < MOVEMENT_THRESHOLD)
            {
                Vector3 avgForce = totalForceThisFrame / forceApplicationCount;
                TrainArenaDebugManager.Log($"⚠️ {gameObject.name} PERSISTENTLY STUCK: AvgForce={avgForce.magnitude:F1} | Vel={rb.linearVelocity.magnitude:F3} | Actions=({forceApplicationCount})", 
                                         TrainArenaDebugManager.DebugLogLevel.Warnings);
            }
            
            // Reset counters
            totalForceThisFrame = Vector3.zero;
            forceApplicationCount = 0;
            lastPhysicsDebugTime = Time.time;
        }
    }

    public override void OnEpisodeBegin()
    {
        // Reset episode tracking
        episodeStepCount = 0;
        episodeStartTime = Time.time;
        totalEpisodesCompleted++;
        globalEpisodeCount++;
        
        // Periodic garbage collection to prevent memory buildup
        if (globalEpisodeCount % 50 == 0)
        {
            System.GC.Collect();
            TrainArenaDebugManager.Log($"🧹 Performed garbage collection after {globalEpisodeCount} episodes", 
                                     TrainArenaDebugManager.DebugLogLevel.Important);
        }
        
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

        // Configurable raycast observations
        for (int i = 0; i < raycastDirections; i++)
        {
            float angle = i * (360f / raycastDirections);
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
        
        // Verbose debug: Log action details (only when verbose level enabled)
        if ((Mathf.Abs(moveX) > 0.01f || Mathf.Abs(moveZ) > 0.01f) && Time.fixedTime % (AGENT_LOG_INTERVAL * 4f) < Time.fixedDeltaTime)
        {
            TrainArenaDebugManager.Log($"🎮 {gameObject.name} ACTION: ({moveX:F3},{moveZ:F3}) → Force={worldForce.magnitude:F1}", 
                                     TrainArenaDebugManager.DebugLogLevel.Verbose);
        }
        
        rb.AddForce(worldForce, ForceMode.Acceleration);
        
        // Store for debug visualization
        lastAppliedForce = worldForce;
        
        // Track physics debugging
        totalForceThisFrame += worldForce;
        forceApplicationCount++;
        
        // Minimal agent status logging (very infrequent to reduce Unity stress)
        if (Time.fixedTime % (AGENT_LOG_INTERVAL * 12f) < Time.fixedDeltaTime) // Every 60 seconds
        {
            var behaviorParams = GetComponent<Unity.MLAgents.Policies.BehaviorParameters>();
            string behaviorType = behaviorParams ? behaviorParams.BehaviorType.ToString() : "Unknown";
            
            Vector3 velocity = rb.linearVelocity;
            string status = velocity.magnitude > MOVEMENT_THRESHOLD ? "MOVING" : "STUCK";
            
            // Show episode progress and status
            float goalDistance = goal ? Vector3.Distance(transform.position, goal.position) : 0f;
            float episodeTime = Time.time - episodeStartTime;
            
            // Only log if something noteworthy (stuck agents, training mode, or close to goal)
            if (status == "STUCK" || behaviorType != "HeuristicOnly" || goalDistance < 3f)
            {
                TrainArenaDebugManager.Log($"🤖 {gameObject.name}: {status} | {behaviorType} | Vel={velocity.magnitude:F1} | Goal={goalDistance:F1} | Step={episodeStepCount}/{maxEpisodeSteps} | Time={episodeTime:F0}s", 
                                         TrainArenaDebugManager.DebugLogLevel.Important);
            }
        }

        // Increment episode step counter
        episodeStepCount++;
        
        // Simplified penalties to reduce computation
        AddReward(-0.005f); // Single time penalty per step

        // Goal reaching check
        if (goal != null)
        {
            float d = Vector3.Distance(transform.position, goal.position);
            AddReward(prevDist - d); // progress
            prevDist = d;

            if (d < 1.5f) // Even easier goal to ensure regular completion
            {
                AddReward(+3.0f); // Higher reward for reaching goal
                TrainArenaDebugManager.Log($"🎯 {gameObject.name} REACHED GOAL! Distance={d:F2} | Steps={episodeStepCount} | Time={Time.time - episodeStartTime:F1}s | Episodes={totalEpisodesCompleted}", 
                                         TrainArenaDebugManager.DebugLogLevel.Important);
                EndEpisode();
                return;
            }
        }

        // Fall detection
        if (transform.position.y < -1f)
        {
            AddReward(-0.5f); // Penalty for falling
            TrainArenaDebugManager.Log($"💥 {gameObject.name} FELL! Steps={episodeStepCount} | Time={Time.time - episodeStartTime:F1}s", 
                                     TrainArenaDebugManager.DebugLogLevel.Important);
            EndEpisode();
            return;
        }
        
        // Aggressive episode limits to prevent Unity hanging
        if (episodeStepCount >= maxEpisodeSteps)
        {
            AddReward(-0.5f); // Penalty for timeout
            TrainArenaDebugManager.Log($"⏰ {gameObject.name} STEP TIMEOUT! Steps={episodeStepCount} | Episodes={totalEpisodesCompleted}", 
                                     TrainArenaDebugManager.DebugLogLevel.Important);
            EndEpisode();
            return;
        }
        
        if (Time.time - episodeStartTime > episodeTimeLimit)
        {
            AddReward(-0.5f); // Penalty for timeout
            TrainArenaDebugManager.Log($"⏰ {gameObject.name} TIME TIMEOUT! Time={Time.time - episodeStartTime:F1}s | Episodes={totalEpisodesCompleted}", 
                                     TrainArenaDebugManager.DebugLogLevel.Important);
            EndEpisode();
            return;
        }
        
        // Emergency reset if agent is stuck in one place for too long
        if (episodeStepCount > 50 && rb.linearVelocity.magnitude < 0.05f)
        {
            AddReward(-0.3f); // Penalty for being stuck
            TrainArenaDebugManager.Log($"🚫 {gameObject.name} STUCK RESET! Vel={rb.linearVelocity.magnitude:F3} | Steps={episodeStepCount}", 
                                     TrainArenaDebugManager.DebugLogLevel.Important);
            EndEpisode();
            return;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var ca = actionsOut.ContinuousActions;
        
        // Use new Input System for Unity 6.2 compatibility
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard != null)
        {
            ca[0] = 0f;
            ca[1] = 0f;
            
            if (keyboard.aKey.isPressed) ca[0] -= 1f; // Left
            if (keyboard.dKey.isPressed) ca[0] += 1f; // Right
            if (keyboard.sKey.isPressed) ca[1] -= 1f; // Back
            if (keyboard.wKey.isPressed) ca[1] += 1f; // Forward
            
            // Debug log when manual input is detected (verbose only)
            if ((ca[0] != 0f || ca[1] != 0f) && Time.fixedTime % 2f < Time.fixedDeltaTime)
            {
                TrainArenaDebugManager.Log($"Manual input: ({ca[0]:F2}, {ca[1]:F2})", TrainArenaDebugManager.DebugLogLevel.Verbose);
            }
        }
        else
        {
            // Fallback to old Input system if new one isn't available
            ca[0] = Input.GetAxis("Horizontal");
            ca[1] = Input.GetAxis("Vertical");
            
            if (ca[0] != 0f || ca[1] != 0f)
            {
                TrainArenaDebugManager.Log($"Legacy input: ({ca[0]:F2}, {ca[1]:F2})", TrainArenaDebugManager.DebugLogLevel.Verbose);
            }
        }
        
        // If no manual input and not connected to ML-Agents, use random actions for testing
        if (ca[0] == 0f && ca[1] == 0f)
        {
            randomActionTimer += Time.deltaTime;
            if (randomActionTimer > RANDOM_ACTION_CHANGE_TIME)
            {
                GenerateRandomAction();
                randomActionTimer = 0f;
                TrainArenaDebugManager.Log($"🎲 {gameObject.name} Generated new random action: ({currentRandomAction.x:F2}, {currentRandomAction.y:F2})", 
                                         TrainArenaDebugManager.DebugLogLevel.Verbose);
            }
            
            ca[0] = currentRandomAction.x;
            ca[1] = currentRandomAction.y;
        }
    }
    
    private void GenerateRandomAction()
    {
        // Generate random movement direction
        currentRandomAction = new Vector2(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        );
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Obstacle"))
            AddReward(-0.1f);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        // Show raycast visualization if global toggle is ON
        if (TrainArenaDebugManager.ShowRaycastVisualization)
        {
            DrawRaycastVisualization();
        }
        
        // Show velocity vectors if enabled
        if (TrainArenaDebugManager.ShowVelocityDisplay && rb != null)
        {
            Gizmos.color = Color.blue;
            Vector3 velocity = rb.linearVelocity;
            if (velocity.magnitude > MOVEMENT_THRESHOLD)
            {
                Gizmos.DrawLine(transform.position, transform.position + velocity);
                Gizmos.DrawSphere(transform.position + velocity, 0.1f);
            }
        }
        
        // Show force vectors if observations enabled
        if (TrainArenaDebugManager.ShowObservations && lastAppliedForce.magnitude > FORCE_DISPLAY_THRESHOLD)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + lastAppliedForce * 0.5f);
            Gizmos.DrawSphere(transform.position + lastAppliedForce * 0.5f, 0.1f);
        }
        
        // Show arena bounds if enabled
        if (TrainArenaDebugManager.ShowArenaBounds)
        {
            Gizmos.color = Color.white;
            var arena = transform.parent;
            if (arena != null)
            {
                Vector3 center = arena.position;
                Gizmos.DrawWireCube(center, new Vector3(14f, 0.1f, 14f));
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        
        // Show raycast visualization when selected (even if global toggle is OFF)
        if (!TrainArenaDebugManager.ShowRaycastVisualization)
        {
            DrawRaycastVisualization();
        }
        
        // Highlight selected agent's arena bounds in bright yellow
        Gizmos.color = Color.yellow;
        var arena = transform.parent;
        if (arena != null)
        {
            Vector3 center = arena.position;
            Gizmos.DrawWireCube(center, new Vector3(14f, 0.1f, 14f));
        }
    }
    
    private void DrawRaycastVisualization()
    {
        Vector3 rayStart = transform.position + Vector3.up * 0.2f;
        
        for (int i = 0; i < raycastDirections; i++)
        {
            float angle = i * (360f / raycastDirections);
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