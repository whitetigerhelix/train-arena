using UnityEngine;
using UnityEngine.InputSystem;
using Unity.MLAgents;

/// <summary>
/// Debug information display for CubeAgent - shows agent state and observations
/// </summary>
public class AgentDebugInfo : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool showDebugInfo = true;
    public bool showObservations = false;
    
    private CubeAgent cubeAgent;
    private Rigidbody rb;
    
    void Start()
    {
        cubeAgent = GetComponent<CubeAgent>();
        rb = GetComponent<Rigidbody>();
    }
    
    void OnGUI()
    {
        if (!TrainArenaDebugManager.ShowAgentDebugInfo || cubeAgent == null) return;

        // Only show for one agent to avoid UI clutter
        if (gameObject.name != "CubeAgent_Arena_0") return;

        Vector3 worldScreenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);
        if (worldScreenPos.z > 0) // Only show if in front of camera
        {
            Vector2 screenPos = new Vector2(worldScreenPos.x, worldScreenPos.y);
            Rect infoRect = new Rect(screenPos.x - 75f, Screen.height - screenPos.y - 60f, 150f, 80f);

            GUI.backgroundColor = new Color(0, 0, 0, 0.7f);
            GUI.Box(infoRect, "");
            
            GUI.color = Color.white;
            GUILayout.BeginArea(infoRect);
            
            GUILayout.Label($"Agent: {gameObject.name}");
            GUILayout.Label($"Velocity: {rb.linearVelocity.magnitude:F1}");
            
            if (cubeAgent.goal != null)
            {
                float distance = Vector3.Distance(transform.position, cubeAgent.goal.position);
                GUILayout.Label($"Goal Dist: {distance:F1}");
            }
            
            GUILayout.Label($"Episode: {cubeAgent.CompletedEpisodes}");
            GUILayout.Label($"Reward: {cubeAgent.GetCumulativeReward():F2}");
            
            GUILayout.EndArea();
        }
    }
    
    void Update()
    {
        // Input handling is now managed by TrainArenaDebugManager
        // This component just responds to the global settings
    }
}