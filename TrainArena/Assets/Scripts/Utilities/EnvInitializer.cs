using UnityEngine;

public class EnvInitializer : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject cubeAgentPrefab;
    public GameObject goalPrefab;
    public GameObject obstaclePrefab;

    [Header("Layout")]
    public int envCountX = 4;
    public int envCountZ = 4;
    public float arenaSize = 20f; // spacing between arena centers (was 14f - too close!)
    public int obstaclesPerArena = 6;

    void Start()
    {
        if (cubeAgentPrefab == null || goalPrefab == null) { Debug.LogError("Assign prefabs in EnvInitializer"); return; }

        for (int x = 0; x < envCountX; x++)
        {
            for (int z = 0; z < envCountZ; z++)
            {
                Vector3 center = transform.position + new Vector3(x * arenaSize, 0f, z * arenaSize);
                SpawnArena(center);
                Debug.Log($"Spawned Arena {x},{z} at position {center} (EnvManager at {transform.position})");
            }
        }
    }

    void SpawnArena(Vector3 center)
    {
        // Create arena container to group all objects for this specific arena
        var arenaIndex = transform.childCount;
        var arenaContainer = new GameObject($"Arena_{arenaIndex}");
        arenaContainer.transform.parent = transform;
        arenaContainer.transform.position = center;
        
        Debug.Log($"=== SPAWNING ARENA {arenaIndex} ===");
        Debug.Log($"Arena Center: {center}");
        Debug.Log($"Arena Bounds: X[{center.x-7f} to {center.x+7f}], Z[{center.z-7f} to {center.z+7f}]");

        // Ground - make it big enough for the 12x12 agent spawn area
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.transform.position = center;
        ground.transform.localScale = Vector3.one * 1.4f; // 10x10 plane -> 14x14 world (matches arenaSize)
        ground.name = "Ground";
        ground.transform.parent = arenaContainer.transform;

        // Agent - spawn at arena center, properly positioned ON TOP of ground
        var agentPosition = center + Vector3.up * 1.0f; // 1.0f = half cube height above ground
        var agentGO = Instantiate(cubeAgentPrefab, agentPosition, Quaternion.identity, arenaContainer.transform);
        agentGO.name = $"CubeAgent_Arena_{transform.childCount}";
        var agent = agentGO.GetComponent<CubeAgent>();
        
        Debug.Log($"Spawned Agent at {agentPosition} in Arena container {arenaContainer.name}");

        // Goal - place in a distributed pattern within THIS arena
        // Use the arenaIndex we calculated at the start, not childCount which is now off by 1
        var goalAngle = (arenaIndex * 73f) % 360f; // Spread goals around using prime number offset
        var goalDistance = Random.Range(2f, 4f);
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
            Debug.Log($"Arena {arenaIndex}: Agent at {agentPosition} -> Goal at {goalPosition} (distance: {Vector3.Distance(agentPosition, goalPosition):F2})");
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
                    offset = new Vector3(Random.Range(-4f, 4f), 1.0f, Random.Range(-4f, 4f)); // Fixed height to 1.0f
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
                    Debug.Log($"Arena {arenaIndex}: Placed Obstacle {i} at {obsPosition}");
                }
                else
                {
                    Debug.LogWarning($"Arena {arenaIndex}: Failed to place Obstacle {i} after {attempts} attempts");
                }
            }
            Debug.Log($"Arena {arenaIndex}: Successfully placed {successfulObstacles}/{obstaclesPerArena} obstacles");
        }
        
        // Validation: Ensure all objects are within arena bounds
        ValidateArenaObjects(arenaContainer, center, arenaIndex);
        
        Debug.Log($"=== ARENA {arenaIndex} COMPLETE ===\n");
    }
    
    void ValidateArenaObjects(GameObject arenaContainer, Vector3 center, int arenaIndex)
    {
        float arenaBounds = 7f; // Half of 14x14 arena
        bool allValid = true;
        
        foreach (Transform child in arenaContainer.transform)
        {
            Vector3 pos = child.position;
            Vector3 localPos = pos - center;
            
            if (Mathf.Abs(localPos.x) > arenaBounds || Mathf.Abs(localPos.z) > arenaBounds)
            {
                Debug.LogError($"Arena {arenaIndex}: {child.name} is OUTSIDE arena bounds at {pos}! Local offset: {localPos}");
                allValid = false;
            }
        }
        
        if (allValid)
        {
            Debug.Log($"Arena {arenaIndex}: All objects within bounds ✓");
        }
    }
}