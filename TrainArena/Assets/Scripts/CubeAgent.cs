using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;
using TrainArena.Core;

[RequireComponent(typeof(Rigidbody))]
public class CubeAgent : BaseTrainArenaAgent
{
    [Header("Scene Refs")]
    public Transform goal;
    public LayerMask obstacleMask;
    public float moveAccel = 50f;                   // Increased force for better movement
    public float rayLength = 10f;
    
    [Header("Observation Space Configuration")]
    public int raycastDirections = 8;
    
    [Header("Episode Management")]
    public int maxEpisodeSteps = 500;               // Balanced episode length: visible navigation + performance
    public float episodeTimeLimit = 30f;            // 30 seconds: enough time to navigate, better performance
    
    // AgentActivity is now inherited from BaseTrainArenaAgent
    
    // Episode tracking
    private int episodeStepCount;
    private float episodeStartTime;
    private int totalEpisodesCompleted;
    private int totalActionsReceived;
    
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
    private const float MOVEMENT_THRESHOLD = 0.02f;         // Lower threshold since physics seem sluggish
    private const float HIGH_VELOCITY_THRESHOLD = 2f;       // High velocity worth logging
    private const float FORCE_DISPLAY_THRESHOLD = 0.1f;     // Minimum force to display in gizmos

    Rigidbody rb;
    float prevDist;
    Vector3 lastAppliedForce;
    
    // Arena integration
    private ArenaHelper arenaHelper;
    private Vector3 arenaCenter;
    
    // Physics debugging
    private Vector3 totalForceThisFrame;
    private int forceApplicationCount;
    private float lastPhysicsDebugTime;
    
    // Velocity tracking for debugging
    private Vector3 lastFrameVelocity;
    private Vector3 lastAppliedForceForTracking;
    private bool trackingVelocity = false;
    
    // For testing when ML-Agents isn't connected
    private float randomActionTimer;
    private Vector2 currentRandomAction;
    
    // Add these fields to your class
    private List<Collision> activeCollisions = new List<Collision>();
    private List<ContactPoint> currentContacts = new List<ContactPoint>();
    
    // BaseTrainArenaAgent abstract property implementations
    public override Transform MainTransform => transform;
    public override Rigidbody MainRigidbody => rb;
    public override string AgentTypeIcon => "🧊";
    
    public override float GetObservationRange()
    {
        return rayLength; // Use raycast length as observation range
    }
    
    /// <summary>
    /// Calculate total observation count based on configuration
    /// </summary>
    public int GetTotalObservationCount()
    {
        return VELOCITY_OBSERVATIONS + GOAL_OBSERVATIONS + raycastDirections;
    }

    public override void Initialize()
    {
        TrainArenaDebugManager.Log($"{gameObject.name} iniitializing", context: transform);
        
        rb = GetComponent<Rigidbody>();

        // Get ArenaHelper from parent EnvInitializer
        EnvInitializer envManager = FindFirstObjectByType<EnvInitializer>(); // Should only be one
        if (envManager != null)
        {
            arenaHelper = envManager.ArenaHelper;
            arenaCenter = transform.parent ? transform.parent.position : Vector3.zero;
            TrainArenaDebugManager.Log($"🔗 {gameObject.name} connected to ArenaHelper: {arenaHelper.GetDebugInfo()}", 
                                     TrainArenaDebugManager.DebugLogLevel.Important);
        }
        else
        {
            TrainArenaDebugManager.LogError($"❌ {gameObject.name} could not find EnvInitializer parent! Arena positioning will not work correctly.");
        }
        
        // Ensure optimal physics settings for movement
        SetPhysics(rb);
        
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
        // Track velocity changes for debugging inference issues
        var behaviorParams = GetComponent<Unity.MLAgents.Policies.BehaviorParameters>();
        bool isInference = behaviorParams?.BehaviorType == Unity.MLAgents.Policies.BehaviorType.InferenceOnly;
        
        if (trackingVelocity && isInference && totalActionsReceived <= 25)
        {
            Vector3 currentVelocity = rb.linearVelocity;
            Vector3 velocityChange = currentVelocity - lastFrameVelocity;
            
            TrainArenaDebugManager.Log($"📊 VELOCITY TRACKING: " +
                                     $"LastVel={lastFrameVelocity.magnitude:F3} | CurrentVel={currentVelocity.magnitude:F3} | " +
                                     $"Change={velocityChange.magnitude:F3} | LastForce={lastAppliedForceForTracking.magnitude:F1}", 
                                     TrainArenaDebugManager.DebugLogLevel.Important);
            trackingVelocity = false; // Only track for one frame after force application
        }
        
        // Store current velocity for next frame comparison
        lastFrameVelocity = rb.linearVelocity;
        
        // Note: ML-Agents decisions are now handled by DecisionRequester component
        // (consistent with RagdollAgent - no manual RequestDecision calls needed)
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
        totalActionsReceived = 0; // Reset action counter for new episode
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
        
        // CRITICAL: Verify physics settings are correct (especially for inference mode)
        var behaviorParams = GetComponent<Unity.MLAgents.Policies.BehaviorParameters>();
        bool isInference = behaviorParams?.BehaviorType == Unity.MLAgents.Policies.BehaviorType.InferenceOnly;
        
        if (isInference)
        {
            // Force correct physics settings for inference
            SetPhysics(rb);
            
            TrainArenaDebugManager.Log($"🔧 INFERENCE PHYSICS CHECK: Mass={rb.mass} | Drag={rb.linearDamping} | Kinematic={rb.isKinematic} | Constraints={rb.constraints} | MoveAccel={moveAccel} | TimeScale={Time.timeScale} | FixedDeltaTime={Time.fixedDeltaTime}", 
                                     TrainArenaDebugManager.DebugLogLevel.Important);
        }

        // Use ArenaHelper for consistent position generation (removes all hardcoding)
        if (arenaHelper != null)
        {
            // Generate new agent position using ArenaHelper
            Vector3 newAgentPos = arenaHelper.GetRandomAgentPosition(arenaCenter);
            transform.position = newAgentPos;
            transform.rotation = Quaternion.Euler(0f, Random.Range(0, 360f), 0f);

            // Generate new goal position using ArenaHelper (ensures minimum distance from agent)
            if (goal != null)
            {
                Vector3 newGoalPos = arenaHelper.GetRandomGoalPosition(arenaCenter, newAgentPos);
                goal.position = newGoalPos;
                
                // Enhanced debug logging with distance validation
                float distance = Vector3.Distance(newAgentPos, newGoalPos);
                bool withinBounds = arenaHelper.IsWithinArenaBounds(newAgentPos, arenaCenter) && 
                                   arenaHelper.IsWithinArenaBounds(newGoalPos, arenaCenter);
                TrainArenaDebugManager.Log($"Episode Reset: Agent {gameObject.name} → {newAgentPos}, Goal → {newGoalPos} | Distance: {distance:F2} | InBounds: {withinBounds}", 
                                         TrainArenaDebugManager.DebugLogLevel.Verbose);
            }
        }
        else
        {
            // Fallback to old method if ArenaHelper not available
            TrainArenaDebugManager.LogWarning($"⚠️ {gameObject.name}: ArenaHelper not available, using fallback positioning");
            var arena = transform.parent;
            Vector3 center = arena ? arena.position : Vector3.zero;
            transform.position = center + new Vector3(Random.Range(-8f, 8f), 0.5f, Random.Range(-8f, 8f));
            transform.rotation = Quaternion.Euler(0f, Random.Range(0, 360f), 0f);
            
            if (goal != null)
            {
                goal.position = center + new Vector3(Random.Range(-8f, 8f), 1.0f, Random.Range(-8f, 8f));
            }
        }

        prevDist = goal ? Vector3.Distance(transform.position, goal.position) : 0f;
    }

    void SetPhysics(Rigidbody rb)
    {
        if (rb != null)
        {
            rb.mass = 1f;                     // Standard mass
            rb.linearDamping = 0.5f;          // Low drag for responsive movement  
            rb.angularDamping = 5f;           // Higher angular drag for stability
            rb.isKinematic = false;           // Must be false for forces to work
            rb.useGravity = true;             // Keep gravity for realistic physics
            rb.maxAngularVelocity = 20f;
            rb.constraints = RigidbodyConstraints.None;
        }
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

    protected override void HandleActiveActions(ActionBuffers actions)
    {
        float moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float moveZ = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        
        // Count non-zero actions to detect if agent is receiving proper actions
        if (Mathf.Abs(moveX) > 0.01f || Mathf.Abs(moveZ) > 0.01f)
        {
            totalActionsReceived++;
        }
        
        // Debug inference mode - always log first few actions to verify model is working
        // Get behavior parameters once and use for all debugging
        var behaviorParams = GetComponent<Unity.MLAgents.Policies.BehaviorParameters>();
        bool isInference = behaviorParams?.BehaviorType == Unity.MLAgents.Policies.BehaviorType.InferenceOnly;
        string behaviorType = behaviorParams ? behaviorParams.BehaviorType.ToString() : "Unknown";
        string modelName = behaviorParams?.Model?.name ?? "NO_MODEL";
        bool hasModel = behaviorParams?.Model != null;
        
        if (isInference && totalActionsReceived <= 10)
        {
            TrainArenaDebugManager.Log($"🧠 INFERENCE ACTION #{totalActionsReceived}: Raw=({actions.ContinuousActions[0]:F4}, {actions.ContinuousActions[1]:F4}) | Clamped=({moveX:F4}, {moveZ:F4})", 
                                     TrainArenaDebugManager.DebugLogLevel.Important);
        }

        Vector3 localMove = new Vector3(moveX, 0f, moveZ);
        Vector3 worldForce = transform.TransformDirection(localMove) * moveAccel;
        
        // Enhanced debugging for inference mode - always log actions during inference
        if (isInference || (Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveZ) > 0.1f) && Time.fixedTime % 5f < Time.fixedDeltaTime)
        {
            string mode = isInference ? "INFERENCE" : "TRAINING";
            TrainArenaDebugManager.Log($"🎮 {gameObject.name} [{mode}] MODEL:{modelName} ACTION: ({moveX:F3},{moveZ:F3}) → Force={worldForce.magnitude:F1} | Vel={rb.linearVelocity.magnitude:F2}", 
                                     TrainArenaDebugManager.DebugLogLevel.Important);
        }
        
        // CRITICAL: Apply force before debugging so we can see the immediate effect
        Vector3 velocityBeforeForce = rb.linearVelocity;
        rb.AddForce(worldForce, ForceMode.Acceleration);
        
        // Enable velocity tracking for next frame
        if (isInference && totalActionsReceived <= 25)
        {
            trackingVelocity = true;
            lastAppliedForceForTracking = worldForce;
        }
        
        // Enhanced physics debugging for inference issues
        if (isInference && totalActionsReceived <= 20)
        {
            // Check if agent is grounded or colliding with something
            bool isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);
            int contactCount = GetContactCount(); // Use the proper collision callback system
            
            TrainArenaDebugManager.Log($"🔬 PHYSICS DEBUG #{totalActionsReceived}: " +
                                     $"Mass={rb.mass:F1} | Drag={rb.linearDamping:F1} | Kinematic={rb.isKinematic} | " +
                                     $"Constraints={rb.constraints} | Grounded={isGrounded} | Contacts={contactCount} | " +
                                     $"Position={transform.position} | " +
                                     $"VelBefore={velocityBeforeForce.magnitude:F3} | Force={worldForce.magnitude:F1}", 
                                     TrainArenaDebugManager.DebugLogLevel.Important);
        }
        
        // Store for debug visualization
        lastAppliedForce = worldForce;
        
        // Track physics debugging
        totalForceThisFrame += worldForce;
        forceApplicationCount++;
        
        // More frequent status logging for debugging movement issues
        if (Time.fixedTime % 15f < Time.fixedDeltaTime) // Every 15 seconds
        {
            Vector3 velocity = rb.linearVelocity;
            string status = velocity.magnitude > MOVEMENT_THRESHOLD ? "MOVING" : "STUCK";
            
            // Show episode progress and status
            float goalDistance = goal ? Vector3.Distance(transform.position, goal.position) : 0f;
            float episodeTime = Time.time - episodeStartTime;
            
            // Enhanced logging with model info - especially important for inference debugging
            TrainArenaDebugManager.Log($"🤖 {gameObject.name}: {status} | {behaviorType} | Model:{modelName}({hasModel}) | Vel={velocity.magnitude:F2} | Goal={goalDistance:F1} | Actions={totalActionsReceived} | Step={episodeStepCount}/{maxEpisodeSteps}", 
                                     TrainArenaDebugManager.DebugLogLevel.Important);
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
        
        // Removed emergency stuck reset - let agents have time to navigate properly
    }

    protected override void HandleActiveHeuristic(in ActionBuffers actionsOut)
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
    
    protected override void HandleInactiveState()
    {
        // Cube agent doesn't need special handling when inactive
        // Physics will naturally handle the cube without applied forces
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!activeCollisions.Contains(collision))
        {
            activeCollisions.Add(collision);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        // Update existing collision
        for (int i = 0; i < activeCollisions.Count; i++)
        {
            if (activeCollisions[i].gameObject == collision.gameObject)
            {
                activeCollisions[i] = collision;
                break;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        activeCollisions.RemoveAll(c => c.gameObject == collision.gameObject);
    }

    // Method to get current contact count
    private int GetContactCount()
    {
        currentContacts.Clear();
        foreach (var collision in activeCollisions)
        {
            currentContacts.AddRange(collision.contacts);
        }
        return currentContacts.Count;
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
        
        // Show arena bounds if enabled (using ArenaHelper dimensions)
        if (TrainArenaDebugManager.ShowArenaBounds && arenaHelper != null)
        {
            Gizmos.color = Color.white;
            Vector3 arenaSize = new Vector3(arenaHelper.GroundRadius * 2f, 0.1f, arenaHelper.GroundRadius * 2f);
            Gizmos.DrawWireCube(arenaCenter, arenaSize);
            
            // Also show safe zone bounds
            Gizmos.color = Color.yellow;
            Vector3 safeZoneSize = new Vector3(arenaHelper.SafeZoneRadius * 2f, 0.1f, arenaHelper.SafeZoneRadius * 2f);
            Gizmos.DrawWireCube(arenaCenter, safeZoneSize);
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
        
        // Highlight selected agent's arena bounds (using ArenaHelper dimensions)
        if (arenaHelper != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 arenaSize = new Vector3(arenaHelper.GroundRadius * 2f, 0.1f, arenaHelper.GroundRadius * 2f);
            Gizmos.DrawWireCube(arenaCenter, arenaSize);
            
            // Show safe zone in bright green
            Gizmos.color = Color.green;
            Vector3 safeZoneSize = new Vector3(arenaHelper.SafeZoneRadius * 2f, 0.1f, arenaHelper.SafeZoneRadius * 2f);
            Gizmos.DrawWireCube(arenaCenter, safeZoneSize);
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