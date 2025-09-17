using UnityEngine;
using Unity.MLAgents;

/// <summary>
/// Simple scene controller for testing trained CubeAgent models.
/// Creates a single arena with one agent for easy observation and evaluation.
/// </summary>
public class TestSceneController : MonoBehaviour
{
    [Header("Test Arena Setup")]
    [SerializeField] private GameObject cubeAgentPrefab;
    [SerializeField] private GameObject goalPrefab;
    [SerializeField] private Material arenaMaterial;
    [SerializeField] private Material goalMaterial;
    
    [Header("Arena Dimensions")]
    [SerializeField] private float arenaSize = 20f;
    [SerializeField] private float arenaHeight = 1f;
    
    [Header("Test Controls")]
    [SerializeField] private KeyCode resetKey = KeyCode.R;
    [SerializeField] private KeyCode switchModeKey = KeyCode.M;
    
    private GameObject arena;
    private GameObject agent;
    private GameObject goal;
    private CubeAgent cubeAgent;
    
    void Start()
    {
        CreateTestArena();
        CreateTestAgent();
        CreateGoal();
        
        // Position camera for good viewing
        Camera.main.transform.position = new Vector3(0, 15, -15);
        Camera.main.transform.LookAt(Vector3.zero);
        
        Debug.Log("🧪 TEST SCENE READY!");
        Debug.Log($"🔧 Controls: {resetKey} = Reset Agent | {switchModeKey} = Switch Behavior Mode");
        Debug.Log("📋 Instructions: Load your trained model (.onnx) into the CubeAgent's BehaviorParameters!");
    }
    
    void Update()
    {
        // Reset agent
        if (Input.GetKeyDown(resetKey))
        {
            ResetAgent();
        }
        
        // Switch behavior mode
        if (Input.GetKeyDown(switchModeKey))
        {
            SwitchBehaviorMode();
        }
    }
    
    void CreateTestArena()
    {
        // Create arena floor
        arena = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arena.name = "TestArena";
        arena.transform.position = Vector3.zero;
        arena.transform.localScale = new Vector3(arenaSize, arenaHeight, arenaSize);
        
        // Setup arena material
        if (arenaMaterial != null)
        {
            arena.GetComponent<Renderer>().material = arenaMaterial;
        }
        else
        {
            // Default gray arena
            arena.GetComponent<Renderer>().material.color = Color.gray;
        }
        
        // Arena is static
        arena.isStatic = true;
    }
    
    void CreateTestAgent()
    {
        if (cubeAgentPrefab != null)
        {
            agent = Instantiate(cubeAgentPrefab);
        }
        else
        {
            // Create basic cube if no prefab provided
            agent = GameObject.CreatePrimitive(PrimitiveType.Cube);
            agent.AddComponent<Rigidbody>();
            agent.AddComponent<CubeAgent>();
        }
        
        agent.name = "TestCubeAgent";
        cubeAgent = agent.GetComponent<CubeAgent>();
        
        // Position agent randomly on arena
        ResetAgent();
    }
    
    void CreateGoal()
    {
        if (goalPrefab != null)
        {
            goal = Instantiate(goalPrefab);
        }
        else
        {
            // Create basic goal
            goal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            goal.GetComponent<Collider>().isTrigger = true;
            goal.transform.localScale = Vector3.one * 2f;
            
            if (goalMaterial != null)
            {
                goal.GetComponent<Renderer>().material = goalMaterial;
            }
            else
            {
                goal.GetComponent<Renderer>().material.color = Color.green;
            }
        }
        
        goal.name = "TestGoal";
        goal.tag = "Goal";
        
        // Position goal randomly
        RepositionGoal();
    }
    
    void ResetAgent()
    {
        if (cubeAgent == null) return;
        
        // Random position on arena (avoiding edges)
        float range = arenaSize * 0.4f; // Stay away from edges
        Vector3 randomPos = new Vector3(
            Random.Range(-range, range),
            arenaHeight + 2f, // Above arena surface
            Random.Range(-range, range)
        );
        
        agent.transform.position = randomPos;
        agent.transform.rotation = Quaternion.identity;
        
        // Reset physics
        Rigidbody rb = agent.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        // Reposition goal
        RepositionGoal();
        
        // Reset ML-Agents episode
        cubeAgent.EndEpisode();
        
        Debug.Log($"🔄 Agent reset! New position: {randomPos}");
    }
    
    void RepositionGoal()
    {
        if (goal == null) return;
        
        float range = arenaSize * 0.4f;
        Vector3 randomGoalPos = new Vector3(
            Random.Range(-range, range),
            arenaHeight + 1f,
            Random.Range(-range, range)
        );
        
        goal.transform.position = randomGoalPos;
    }
    
    void SwitchBehaviorMode()
    {
        if (cubeAgent == null) return;
        
        var behaviorParams = cubeAgent.GetComponent<Unity.MLAgents.Policies.BehaviorParameters>();
        if (behaviorParams == null) return;
        
        // Cycle through behavior types
        switch (behaviorParams.BehaviorType)
        {
            case Unity.MLAgents.Policies.BehaviorType.Default:
                behaviorParams.BehaviorType = Unity.MLAgents.Policies.BehaviorType.HeuristicOnly;
                Debug.Log("🎮 Switched to: Heuristic (WASD controls)");
                break;
            case Unity.MLAgents.Policies.BehaviorType.HeuristicOnly:
                behaviorParams.BehaviorType = Unity.MLAgents.Policies.BehaviorType.InferenceOnly;
                Debug.Log("🧠 Switched to: Inference (AI model)");
                break;
            case Unity.MLAgents.Policies.BehaviorType.InferenceOnly:
                behaviorParams.BehaviorType = Unity.MLAgents.Policies.BehaviorType.Default;
                Debug.Log("🔄 Switched to: Default (training mode)");
                break;
        }
    }
    
    void OnGUI()
    {
        // Simple status display
        GUI.BeginGroup(new Rect(10, 10, 300, 150));
        GUI.Box(new Rect(0, 0, 300, 150), "Test Scene Status");
        
        if (cubeAgent != null)
        {
            var behaviorParams = cubeAgent.GetComponent<Unity.MLAgents.Policies.BehaviorParameters>();
            string mode = behaviorParams?.BehaviorType.ToString() ?? "Unknown";
            string model = (behaviorParams?.Model != null) ? behaviorParams.Model.name : "No Model";
            
            GUI.Label(new Rect(10, 25, 280, 20), $"Mode: {mode}");
            GUI.Label(new Rect(10, 45, 280, 20), $"Model: {model}");
            GUI.Label(new Rect(10, 65, 280, 20), $"Position: {agent.transform.position}");
            GUI.Label(new Rect(10, 85, 280, 20), $"Goal Distance: {Vector3.Distance(agent.transform.position, goal.transform.position):F1}");
            GUI.Label(new Rect(10, 105, 280, 20), $"Controls: {resetKey}=Reset | {switchModeKey}=Switch Mode");
        }
        
        GUI.EndGroup();
    }
}