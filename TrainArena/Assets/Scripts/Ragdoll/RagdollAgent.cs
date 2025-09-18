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
        
        // CRITICAL DEBUG: Log everything about ragdoll state
        TrainArenaDebugManager.Log($"🎭 RAGDOLL INIT: {name} - Activity={AgentActivity}, Behavior={BehaviorParameters?.BehaviorType}, Joints={joints.Count}", 
            TrainArenaDebugManager.DebugLogLevel.Important);
        TrainArenaDebugManager.Log($"🎭 RAGDOLL INIT: Pelvis={pelvis?.name}, StartPos={startPos}, BehaviorParams={BehaviorParameters != null}", 
            TrainArenaDebugManager.DebugLogLevel.Important);
            
        // CRITICAL: Log ActionSpec configuration
        if (BehaviorParameters != null)
        {
            var actionSpec = BehaviorParameters.BrainParameters.ActionSpec;
            TrainArenaDebugManager.Log($"🎭 INIT ACTION SPEC: {name} - NumContinuous={actionSpec.NumContinuousActions}, NumDiscrete={actionSpec.NumDiscreteActions}, BehaviorName='{BehaviorParameters.BehaviorName}'", 
                TrainArenaDebugManager.DebugLogLevel.Important);
        }
        
        // Log joint details
        for (int i = 0; i < joints.Count; i++)
        {
            var joint = joints[i];
            TrainArenaDebugManager.Log($"🎭 Joint[{i}]: {joint?.name}, kp={joint?.kp}, kd={joint?.kd}, enabled={joint?.enabled}", 
                TrainArenaDebugManager.DebugLogLevel.Important);
        }
        
        // Start coroutine to check if we're getting action calls
        StartCoroutine(DiagnoseActionCalls());
    }
    
    System.Collections.IEnumerator DiagnoseActionCalls()
    {
        yield return new WaitForSeconds(3f); // Wait for scene to fully initialize
        
        TrainArenaDebugManager.Log($"🎭 DIAGNOSTIC: {name} - Checking if actions are being received...", 
            TrainArenaDebugManager.DebugLogLevel.Important);
        
        bool receivedActions = false;
        for (int i = 0; i < 10; i++) // Check for 10 seconds
        {
            yield return new WaitForSeconds(1f);
            // The action logging will show if we receive any calls
        }
        
        if (!receivedActions)
        {
            TrainArenaDebugManager.Log($"🎭 CRITICAL: {name} - NO ACTIONS RECEIVED IN 10 SECONDS! Agent may be inactive or misconfigured!", 
                TrainArenaDebugManager.DebugLogLevel.Important);
        }
    }

    static int globalRagdollEpisodeCount = 0; // Track episodes across all ragdoll agents
    
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
        
        // Episode tracking for memory management
        globalRagdollEpisodeCount++;
        
        // Periodic garbage collection to prevent memory buildup
        if (globalRagdollEpisodeCount % 50 == 0)
        {
            System.GC.Collect();
            TrainArenaDebugManager.Log($"🧹 Ragdoll GC: Performed garbage collection after {globalRagdollEpisodeCount} ragdoll episodes", 
                                     TrainArenaDebugManager.DebugLogLevel.Important);
        }
        
        // Debug logging for ragdoll configuration
        TrainArenaDebugManager.Log($"🎭 {name} Episode Begin - Joints: {joints.Count}, Pelvis Y: {pelvis.position.y:F2}", TrainArenaDebugManager.DebugLogLevel.Verbose);
        
        // CRITICAL DEBUG: Check ML-Agents registration
        var behaviorParams = GetComponent<Unity.MLAgents.Policies.BehaviorParameters>();
        TrainArenaDebugManager.Log($"🎭 CRITICAL DEBUG: {name} - BehaviorType={behaviorParams?.BehaviorType}, Model={behaviorParams?.Model != null}, TeamId={behaviorParams?.TeamId}, MaxStep={MaxStep}", 
            TrainArenaDebugManager.DebugLogLevel.Important);
        
        // Force request decision to ensure ML-Agents knows about this agent
        RequestDecision();
        TrainArenaDebugManager.Log($"🎭 DECISION REQUESTED: {name} - Manually requested decision after episode begin", 
            TrainArenaDebugManager.DebugLogLevel.Important);
            
        // CRITICAL: Check ActionSpec after episode begin
        if (BehaviorParameters != null)
        {
            var actionSpec = BehaviorParameters.BrainParameters.ActionSpec;
            TrainArenaDebugManager.Log($"🎭 POST-EPISODE ACTION SPEC: {name} - NumContinuous={actionSpec.NumContinuousActions}, NumDiscrete={actionSpec.NumDiscreteActions}", 
                TrainArenaDebugManager.DebugLogLevel.Important);
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

    protected override void HandleActiveActions(ActionBuffers actions)
    {
        // CRITICAL DEBUG: Always log action execution for debugging
        TrainArenaDebugManager.Log($"🎭 ACTIONS: {name} - Activity={AgentActivity}, ContinuousActions.Length={actions.ContinuousActions.Length}, DiscreteActions.Length={actions.DiscreteActions.Length}", 
            TrainArenaDebugManager.DebugLogLevel.Important);
        
        // CRITICAL: Check ActionSpec configuration
        var behaviorParams = GetComponent<Unity.MLAgents.Policies.BehaviorParameters>();
        if (behaviorParams != null)
        {
            var actionSpec = behaviorParams.BrainParameters.ActionSpec;
            TrainArenaDebugManager.Log($"🎭 ACTION SPEC: {name} - NumContinuous={actionSpec.NumContinuousActions}, NumDiscrete={actionSpec.NumDiscreteActions}, SumOfDiscreteBranchSizes={actionSpec.SumOfDiscreteBranchSizes}", 
                TrainArenaDebugManager.DebugLogLevel.Important);
        }
        
        // Map actions [-1,1] to joint target angles
        var ca = actions.ContinuousActions;
        if (ca.Length == 0)
        {
            TrainArenaDebugManager.Log($"🎭 CRITICAL: {name} - Received 0 continuous actions! ActionSpec may be misconfigured!", 
                TrainArenaDebugManager.DebugLogLevel.Important);
            return;
        }
        
        for (int i = 0; i < joints.Count && i < ca.Length; i++)
        {
            joints[i].SetTarget01(Mathf.Clamp(ca[i], -1f, 1f));
            TrainArenaDebugManager.Log($"🎭 Joint[{i}] action={ca[i]:F2} -> target angle", 
                TrainArenaDebugManager.DebugLogLevel.Important);
        }

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
    
    void FixedUpdate()
    {
        // CRITICAL DEBUG: Monitor agent stepping and force decision requests
        if (Time.fixedTime % 5f < Time.fixedDeltaTime) // Every 5 seconds
        {
            TrainArenaDebugManager.Log($"🎭 STEPPING: {name} - StepCount={StepCount}, MaxStep={MaxStep}, Activity={AgentActivity}, CompletedEpisodes={CompletedEpisodes}", 
                TrainArenaDebugManager.DebugLogLevel.Important);
                
            // Force decision request if we haven't been getting actions
            if (AgentActivity == AgentActivity.Active)
            {
                RequestDecision();
                TrainArenaDebugManager.Log($"🎭 FORCED DECISION: {name} - Manually requested decision in FixedUpdate", 
                    TrainArenaDebugManager.DebugLogLevel.Important);
            }
        }
    }

    protected override void HandleActiveHeuristic(in ActionBuffers actionsOut)
    {
        // CRITICAL DEBUG: Always log heuristic execution for debugging
        TrainArenaDebugManager.Log($"🎭 HEURISTIC: {name} - Activity={AgentActivity}, ContinuousActions.Length={actionsOut.ContinuousActions.Length}, DiscreteActions.Length={actionsOut.DiscreteActions.Length}", 
            TrainArenaDebugManager.DebugLogLevel.Important);
        
        // More pronounced manual wiggle when active for better visibility
        var ca2 = actionsOut.ContinuousActions;
        if (ca2.Length == 0)
        {
            TrainArenaDebugManager.Log($"🎭 CRITICAL: {name} - Heuristic received 0-length continuous actions buffer! ActionSpec misconfigured!", 
                TrainArenaDebugManager.DebugLogLevel.Important);
            return;
        }
        
        for (int i = 0; i < joints.Count && i < ca2.Length; i++)
        {
            // Use larger amplitude and different frequencies for each joint
            float amplitude = 0.8f; // Increased from implicit 1.0f to make movement more pronounced
            float frequency = 1.5f + i * 0.3f; // Different frequency per joint
            ca2[i] = amplitude * Mathf.Sin(Time.time * frequency + i * 0.8f);
            
            TrainArenaDebugManager.Log($"🎭 Heuristic Joint[{i}] = {ca2[i]:F2}", 
                TrainArenaDebugManager.DebugLogLevel.Important);
        }
    }
}