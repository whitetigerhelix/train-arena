using UnityEngine;
using UnityEngine.InputSystem;
using Unity.MLAgents;
using TrainArena.Core;

/// <summary>
/// Debug information display for all TrainArena agents - shows agent state and observations
/// </summary>
public class AgentDebugInfo : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool showDebugInfo = true;
    public bool showObservations = false;
    
    private ITrainArenaAgent agent;
    private CubeAgent cubeAgent;
    private RagdollAgent ragdollAgent;
    private Rigidbody rb;
    
    void Start()
    {
        agent = GetComponent<ITrainArenaAgent>();
        cubeAgent = GetComponent<CubeAgent>();
        ragdollAgent = GetComponent<RagdollAgent>();
        rb = GetComponent<Rigidbody>();
        
        // For ragdolls, use the main rigidbody (pelvis)
        if (agent != null && agent.MainRigidbody != null)
        {
            rb = agent.MainRigidbody;
        }
    }
    
    void OnGUI()
    {
        if (!TrainArenaDebugManager.ShowAgentDebugInfo || agent == null || rb == null) return;

        // Use the agent's main transform for positioning
        Transform debugTransform = agent.MainTransform;
        Vector3 worldScreenPos = Camera.main.WorldToScreenPoint(debugTransform.position + Vector3.up * 2f);
        if (worldScreenPos.z > 0) // Only show if in front of camera
        {
            Vector2 screenPos = new Vector2(worldScreenPos.x, worldScreenPos.y);
            
            // Adjust rect size based on whether observations are shown
            float width = TrainArenaDebugManager.ShowObservations ? 220f : 180f;
            float height = TrainArenaDebugManager.ShowObservations ? 250f : 120f;
            Rect infoRect = new Rect(screenPos.x - width/2f, Screen.height - screenPos.y - height/2f, width, height);

            GUI.backgroundColor = new Color(0, 0, 0, 0.7f);
            GUI.Box(infoRect, "");
            
            GUI.color = Color.white;
            GUILayout.BeginArea(infoRect);
            
            // Show agent type icon and name
            GUILayout.Label($"{agent.AgentTypeIcon} Agent: {gameObject.name}");
            GUILayout.Label($"Type: {(cubeAgent != null ? "Cube" : ragdollAgent != null ? "Ragdoll" : "Unknown")}");
            GUILayout.Label($"Activity: {agent.AgentActivity}");
            GUILayout.Label($"Velocity: {rb.linearVelocity.magnitude:F1}");
            
            // Show cube-specific info
            if (cubeAgent != null && cubeAgent.goal != null)
            {
                float distance = Vector3.Distance(debugTransform.position, cubeAgent.goal.position);
                GUILayout.Label($"Goal Dist: {distance:F1}");
            }
            
            // Show ragdoll-specific info
            if (ragdollAgent != null)
            {
                float uprightness = Vector3.Dot(debugTransform.up, Vector3.up);
                GUILayout.Label($"Upright: {uprightness:F2}");
                GUILayout.Label($"Joints: {ragdollAgent.joints.Count}");
            }
            
            // Show observations if enabled
            if (TrainArenaDebugManager.ShowObservations)
            {
                GUILayout.Label("--- Observations ---");
                
                // Velocity observation (first 3 values)
                Vector3 localVel = debugTransform.InverseTransformDirection(rb.linearVelocity);
                GUILayout.Label($"Local Vel: ({localVel.x:F2},{localVel.y:F2},{localVel.z:F2})");
                
                // Cube-specific observations
                if (cubeAgent != null)
                {
                    // Goal direction observation (next 3 values)
                    if (cubeAgent.goal != null)
                    {
                        Vector3 toGoal = cubeAgent.goal.position - debugTransform.position;
                        Vector3 localGoal = debugTransform.InverseTransformDirection(toGoal);
                        GUILayout.Label($"Local Goal: ({localGoal.x:F2},{localGoal.y:F2},{localGoal.z:F2})");
                    }
                    
                    // Raycast distances (configurable count)
                    GUILayout.Label("Raycast distances:");
                    for (int i = 0; i < cubeAgent.raycastDirections; i++)
                    {
                        float angle = i * (360f / cubeAgent.raycastDirections);
                        Vector3 dir = Quaternion.Euler(0f, angle, 0f) * debugTransform.forward;
                        if (Physics.Raycast(debugTransform.position + Vector3.up * 0.2f, dir, out RaycastHit hit, cubeAgent.rayLength, cubeAgent.obstacleMask))
                        {
                            float normalizedDist = hit.distance / cubeAgent.rayLength;
                            GUILayout.Label($"  {angle:F0}°: {normalizedDist:F2}");
                        }
                        else
                        {
                            GUILayout.Label($"  {angle:F0}°: 1.00");
                        }
                    }
                }
                
                // Ragdoll-specific observations
                if (ragdollAgent != null)
                {
                    float uprightness = Vector3.Dot(debugTransform.up, Vector3.up);
                    GUILayout.Label($"Uprightness: {uprightness:F2}");
                    
                    GUILayout.Label("Joint angles (sample):");
                    for (int i = 0; i < Mathf.Min(3, ragdollAgent.joints.Count); i++)
                    {
                        var joint = ragdollAgent.joints[i];
                        if (joint != null)
                        {
                            var jointRb = joint.GetComponent<Rigidbody>();
                            if (jointRb != null)
                            {
                                GUILayout.Label($"  J{i}: vel={jointRb.angularVelocity.magnitude:F1}");
                            }
                        }
                    }
                }
            }
            
            // Show ML-Agents info using the polymorphic agent interface
            var mlAgent = (Agent)agent; // Both CubeAgent and RagdollAgent inherit from Agent
            GUILayout.Label($"Episode: {mlAgent.CompletedEpisodes}");
            GUILayout.Label($"Reward: {mlAgent.GetCumulativeReward():F2}");
            
            GUILayout.EndArea();
        }
    }
    
    void Update()
    {
        // Input handling is now managed by TrainArenaDebugManager
        // This component just responds to the global settings
    }
}