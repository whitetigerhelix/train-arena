using UnityEngine;

public class EnvInitializer : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject cubeAgentPrefab;
    public GameObject goalPrefab;
    public GameObject obstaclePrefab;

    [Header("Layout Configuration")]
    [SerializeField] private EnvPreset preset = EnvPreset.Training;
    [SerializeField, Range(0.1f, 1f)] private float groundPercentage = 0.7f; // Percentage of arena size for ground plane (0.0 to 1.0)
    public float groundRadius => (arenaSize * groundPercentage) / 2f;
    [Space]
    
    [Header("Manual Configuration (when preset = Custom)")]
    [SerializeField] private int envCountX = 4;
    [SerializeField] private int envCountZ = 4;
    [SerializeField] private float arenaSize = 20f; // spacing between arena centers
    public float arenaRadius => arenaSize / 2f; // Full width/depth of arena
    [SerializeField] private int obstaclesPerArena = 6;
    
    public enum EnvPreset
    {
        SingleArena,  // 1x1 for testing
        Training,     // 4x4 for training 
        LargeTraining, // 6x6 for intensive training
        Custom        // Use manual settings above
    }

    void ApplyPreset()
    {
        switch (preset)
        {
            case EnvPreset.SingleArena:
                envCountX = 1;
                envCountZ = 1;
                arenaSize = 20f;
                obstaclesPerArena = 3; // Fewer obstacles for testing
                TrainArenaDebugManager.Log("📋 Applied SingleArena preset: 1x1 grid for testing", TrainArenaDebugManager.DebugLogLevel.Important);
                break;
                
            case EnvPreset.Training:
                envCountX = 2;
                envCountZ = 2;
                arenaSize = 20f;
                obstaclesPerArena = 2; // Minimal obstacles for maximum performance
                TrainArenaDebugManager.Log("📋 Applied Training preset: 2x2 grid for maximum performance", TrainArenaDebugManager.DebugLogLevel.Important);
                break;
                
            case EnvPreset.LargeTraining:
                envCountX = 6;
                envCountZ = 6;
                arenaSize = 20f;
                obstaclesPerArena = 8;
                TrainArenaDebugManager.Log("📋 Applied LargeTraining preset: 6x6 grid for intensive training", TrainArenaDebugManager.DebugLogLevel.Important);
                break;
                
            case EnvPreset.Custom:
                TrainArenaDebugManager.Log($"📋 Using Custom settings: {envCountX}x{envCountZ} grid", TrainArenaDebugManager.DebugLogLevel.Important);
                break;
        }
    }

    void Start()
    {
        if (cubeAgentPrefab == null || goalPrefab == null) { Debug.LogError("Assign prefabs in EnvInitializer"); return; }

        // Apply preset configuration
        ApplyPreset();
        
        TrainArenaDebugManager.Log($"🏗️ Creating {envCountX}x{envCountZ} arena grid (Total: {envCountX * envCountZ} arenas)", TrainArenaDebugManager.DebugLogLevel.Important);

        for (int x = 0; x < envCountX; x++)
        {
            for (int z = 0; z < envCountZ; z++)
            {
                Vector3 center = transform.position + new Vector3(x * arenaSize, 0f, z * arenaSize);
                SpawnArena(center);
            }
        }
        
        // Clean up prefab instances after spawning all arenas
        CleanupPrefabs();
    }
    
    void CleanupPrefabs()
    {
        TrainArenaDebugManager.Log("Cleaning up prefab instances from scene hierarchy", TrainArenaDebugManager.DebugLogLevel.Important);
        
        if (cubeAgentPrefab != null)
        {
            cubeAgentPrefab.SetActive(false);
            TrainArenaDebugManager.Log("Disabled cubeAgentPrefab", TrainArenaDebugManager.DebugLogLevel.Verbose);
        }
        
        if (goalPrefab != null)
        {
            goalPrefab.SetActive(false);
            TrainArenaDebugManager.Log("Disabled goalPrefab", TrainArenaDebugManager.DebugLogLevel.Verbose);
        }
        
        if (obstaclePrefab != null)
        {
            obstaclePrefab.SetActive(false);
            TrainArenaDebugManager.Log("Disabled obstaclePrefab", TrainArenaDebugManager.DebugLogLevel.Verbose);
        }
    }

    void SpawnArena(Vector3 center)
    {
        float arenaRadius = arenaSize / 2f;

        // Create arena container to group all objects for this specific arena
        var arenaIndex = transform.childCount;
        var arenaContainer = new GameObject($"Arena_{arenaIndex}");
        arenaContainer.transform.parent = transform;
        arenaContainer.transform.position = center;
        
        // Verbose logging for arena setup
        TrainArenaDebugManager.Log($"Spawning Arena {arenaIndex} at {center} with radius {arenaRadius}", TrainArenaDebugManager.DebugLogLevel.Verbose);
        TrainArenaDebugManager.Log($"Arena Bounds: X[{center.x - arenaRadius} to {center.x + arenaRadius}], Z[{center.z - arenaRadius} to {center.z + arenaRadius}]", TrainArenaDebugManager.DebugLogLevel.Verbose);

        // Ground - make it big enough for the agent spawn area
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.transform.position = center;
        ground.transform.localScale = Vector3.one * ((groundRadius * 2f) / 10f); // 10x10 plane scaled to match ground
        ground.name = "Ground";
        ground.transform.parent = arenaContainer.transform;

        // Agent - spawn at arena center, properly positioned ON TOP of ground
        var agentPosition = center + Vector3.up * 0.5f; // Half cube height above ground
        var agentGO = Instantiate(cubeAgentPrefab, agentPosition, Quaternion.identity, arenaContainer.transform);
        agentGO.name = $"CubeAgent_Arena_{transform.childCount}";
        var agent = agentGO.GetComponent<CubeAgent>();
        
        TrainArenaDebugManager.Log($"Spawned Agent at {agentPosition} in {arenaContainer.name}", TrainArenaDebugManager.DebugLogLevel.Verbose);

        // Goal - place in a distributed pattern within THIS arena
        // Use the arenaIndex we calculated at the start, not childCount which is now off by 1
        var goalAngle = (arenaIndex * 73f) % 360f; // Spread goals around using prime number offset
        var goalDistance = Random.Range(groundRadius * 0.2f, groundRadius * 0.8f); // Avoid edges and center
        var goalOffset = new Vector3(
            Mathf.Cos(goalAngle * Mathf.Deg2Rad) * goalDistance,
            1.0f, // Goal height above ground (same as agent)
            Mathf.Sin(goalAngle * Mathf.Deg2Rad) * goalDistance
        );
        
        var goalPosition = center + goalOffset;
        var goalGO = Instantiate(goalPrefab, goalPosition, Quaternion.identity, arenaContainer.transform);
        goalGO.name = $"Goal_Arena_{transform.childCount}";
        if (agent != null) 
        {
            agent.goal = goalGO.transform;
            TrainArenaDebugManager.Log($"Arena {arenaIndex}: Agent -> Goal distance: {Vector3.Distance(agentPosition, goalPosition):F2}", TrainArenaDebugManager.DebugLogLevel.Important);
        }

        // Obstacles - distribute around the arena, avoiding goal area, parented to arena container
        if (obstaclePrefab != null)
        {
            int successfulObstacles = 0;
            for (int i = 0; i < obstaclesPerArena; i++)
            {
                Vector3 offset;
                int attempts = 0;
                bool validPosition = false;
                
                do
                {
                    //TODO: Hardcoding 0.8f in a lot of places - should be a variable
                    offset = new Vector3(Random.Range(-groundRadius, groundRadius) * 0.8f, 1.0f, Random.Range(-groundRadius, groundRadius) * 0.8f); // Fixed height to 1.0f
                    attempts++;
                    
                    // Check distance from goal and agent spawn point
                    float distanceFromGoal = Vector3.Distance(offset, goalOffset);
                    float distanceFromAgent = Vector3.Distance(offset, Vector3.up * 1.0f); // Agent at center + up
                    
                    validPosition = distanceFromGoal > 1.5f && distanceFromAgent > 1.5f;
                    
                } while (!validPosition && attempts < 20); // Increased attempts
                
                if (validPosition)
                {
                    var obsPosition = center + offset;
                    var obs = Instantiate(obstaclePrefab, obsPosition, Quaternion.identity, arenaContainer.transform);
                    obs.name = $"Obstacle_{i}_Arena_{transform.childCount}";
                    obs.tag = "Obstacle";
                    successfulObstacles++;
                    TrainArenaDebugManager.Log($"Arena {arenaIndex}: Placed Obstacle {i} at {obsPosition}", TrainArenaDebugManager.DebugLogLevel.Verbose);
                }
                else
                {
                    TrainArenaDebugManager.LogWarning($"Arena {arenaIndex}: Failed to place Obstacle {i} after {attempts} attempts");
                }
            }
            TrainArenaDebugManager.Log($"Arena {arenaIndex}: Placed {successfulObstacles}/{obstaclesPerArena} obstacles", TrainArenaDebugManager.DebugLogLevel.Important);
        }
        
        // Validation: Ensure all objects are within arena bounds
        ValidateArenaObjects(arenaContainer, center, arenaIndex);
        
        TrainArenaDebugManager.Log($"Arena {arenaIndex} setup complete", TrainArenaDebugManager.DebugLogLevel.Important);
    }
    
    void ValidateArenaObjects(GameObject arenaContainer, Vector3 center, int arenaIndex)
    {
        bool allValid = true;
        
        foreach (Transform child in arenaContainer.transform)
        {
            Vector3 pos = child.position;
            Vector3 localPos = pos - center;

            if (Mathf.Abs(localPos.x) > groundRadius || Mathf.Abs(localPos.z) > groundRadius)
            {
                TrainArenaDebugManager.LogError($"Arena {arenaIndex}: {child.name} is OUTSIDE arena bounds at {pos}! Local offset: {localPos}");
                allValid = false;
            }
        }
        
        if (allValid)
        {
            TrainArenaDebugManager.Log($"Arena {arenaIndex}: All objects within bounds ✓", TrainArenaDebugManager.DebugLogLevel.Important);
        }
    }
}