using UnityEngine;

public class EnvInitializer : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject cubeAgentPrefab;
    public GameObject goalPrefab;
    public GameObject obstaclePrefab;

    [Header("Layout Configuration")]
    [SerializeField] private EnvPreset preset = EnvPreset.Training;
    [Space]
    
    [Header("Manual Configuration (when preset = Custom)")]
    [SerializeField] private int envCountX = 4;
    [SerializeField] private int envCountZ = 4;
    [SerializeField] private int obstaclesPerArena = 6;
    
    [Header("Arena Configuration")]
    [SerializeField] private ArenaHelper arenaHelper = new ArenaHelper();
    
    // Expose ArenaHelper for CubeAgent access
    public ArenaHelper ArenaHelper => arenaHelper;
    
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
                arenaHelper.ArenaSize = 20f;
                obstaclesPerArena = 3; // Fewer obstacles for testing
                TrainArenaDebugManager.Log("📋 Applied SingleArena preset: 1x1 grid for testing", TrainArenaDebugManager.DebugLogLevel.Important);
                break;
                
            case EnvPreset.Training:
                envCountX = 2;
                envCountZ = 2;
                arenaHelper.ArenaSize = 20f;
                obstaclesPerArena = 2; // Minimal obstacles for maximum performance
                TrainArenaDebugManager.Log("📋 Applied Training preset: 2x2 grid for maximum performance", TrainArenaDebugManager.DebugLogLevel.Important);
                break;
                
            case EnvPreset.LargeTraining:
                envCountX = 6;
                envCountZ = 6;
                arenaHelper.ArenaSize = 20f;
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
        if (cubeAgentPrefab == null || goalPrefab == null) { TrainArenaDebugManager.LogError("Assign prefabs in EnvInitializer"); return; }

        // Apply preset configuration
        ApplyPreset();
        
        TrainArenaDebugManager.Log($"🏗️ Creating {envCountX}x{envCountZ} arena grid (Total: {envCountX * envCountZ} arenas)", TrainArenaDebugManager.DebugLogLevel.Important);

        for (int x = 0; x < envCountX; x++)
        {
            for (int z = 0; z < envCountZ; z++)
            {
                Vector3 center = transform.position + new Vector3(x * arenaHelper.ArenaSize, 0f, z * arenaHelper.ArenaSize);
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
        // Create arena container to group all objects for this specific arena
        var arenaIndex = transform.childCount;
        var arenaContainer = new GameObject($"Arena_{arenaIndex}");
        arenaContainer.transform.parent = transform;
        arenaContainer.transform.position = center;
        
        // Verbose logging for arena setup
        TrainArenaDebugManager.Log($"Spawning Arena {arenaIndex} at {center} with radius {arenaHelper.ArenaRadius}", TrainArenaDebugManager.DebugLogLevel.Verbose);
        TrainArenaDebugManager.Log($"Arena Bounds: X[{center.x - arenaHelper.ArenaRadius} to {center.x + arenaHelper.ArenaRadius}], Z[{center.z - arenaHelper.ArenaRadius} to {center.z + arenaHelper.ArenaRadius}]", TrainArenaDebugManager.DebugLogLevel.Verbose);
        TrainArenaDebugManager.Log(arenaHelper.GetDebugInfo(), TrainArenaDebugManager.DebugLogLevel.Verbose);

        // Ground - use PrimitiveBuilder for consistent materials and ArenaHelper for scale
        var ground = PrimitiveBuilder.CreateGround("Ground");
        ground.transform.position = center;
        ground.transform.localScale = arenaHelper.GetGroundScale();
        ground.transform.parent = arenaContainer.transform;

        // Agent - use ArenaHelper for initial positioning (will be overridden in OnEpisodeBegin)
        var agentPosition = center + Vector3.up * 0.5f; // Initial position at center
        var agentGO = Instantiate(cubeAgentPrefab, agentPosition, Quaternion.identity, arenaContainer.transform);
        agentGO.name = $"CubeAgent_Arena_{arenaIndex}";
        var agent = agentGO.GetComponent<CubeAgent>();
        
        TrainArenaDebugManager.Log($"Spawned Agent at {agentPosition} in {arenaContainer.name}", TrainArenaDebugManager.DebugLogLevel.Verbose);

        // Goal - use ArenaHelper for distributed placement pattern
        var goalPosition = arenaHelper.GetDistributedGoalPosition(center, arenaIndex);
        var goalGO = Instantiate(goalPrefab, goalPosition, Quaternion.identity, arenaContainer.transform);
        goalGO.name = $"Goal_Arena_{arenaIndex}";
        StyleGoal(goalGO.GetComponent<Renderer>());
        if (agent != null) 
        {
            agent.goal = goalGO.transform;
            TrainArenaDebugManager.Log($"Arena {arenaIndex}: Agent -> Goal distance: {Vector3.Distance(agentPosition, goalPosition):F2}", TrainArenaDebugManager.DebugLogLevel.Important);
        }

        // Obstacles - use ArenaHelper for smart placement avoiding agent and goal
        if (obstaclePrefab != null)
        {
            int successfulObstacles = 0;
            var existingObstacles = new System.Collections.Generic.List<Vector3>();
            
            for (int i = 0; i < obstaclesPerArena; i++)
            {
                var obstaclePosition = arenaHelper.GetValidObstaclePosition(center, agentPosition, goalPosition, existingObstacles);
                
                // Check if position is valid (ArenaHelper will warn if not)
                if (arenaHelper.IsWithinSafeZone(obstaclePosition, center))
                {
                    var obs = Instantiate(obstaclePrefab, obstaclePosition, Quaternion.identity, arenaContainer.transform);
                    obs.name = $"Obstacle_{i}_Arena_{arenaIndex}";
                    obs.tag = "Obstacle";
                    existingObstacles.Add(obstaclePosition);
                    successfulObstacles++;
                    TrainArenaDebugManager.Log($"Arena {arenaIndex}: Placed Obstacle {i} at {obstaclePosition}", TrainArenaDebugManager.DebugLogLevel.Verbose);
                }
                else
                {
                    TrainArenaDebugManager.LogWarning($"Arena {arenaIndex}: Failed to place Obstacle {i} - position outside safe zone");
                }
            }
            TrainArenaDebugManager.Log($"Arena {arenaIndex}: Placed {successfulObstacles}/{obstaclesPerArena} obstacles", TrainArenaDebugManager.DebugLogLevel.Important);
        }
        
        // Validation: Use ArenaHelper for enhanced validation
        arenaHelper.ValidateArenaObjects(arenaContainer.transform, center, arenaIndex, true);
        
        TrainArenaDebugManager.Log($"Arena {arenaIndex} setup complete", TrainArenaDebugManager.DebugLogLevel.Important);
    }

    void StyleGoal(Renderer r)
    {
        // Use PrimitiveBuilder for consistent goal materials
        var goalMaterial = PrimitiveBuilder.CreateURPMaterial(smoothness: 0.3f, metallic: 0.0f);
        var gold = new Color(1f, 0.83f, 0.29f);
        goalMaterial.color = gold;
        goalMaterial.name = "GoalMaterial";
        
        // Add emission for better visibility
        if (goalMaterial.HasProperty("_EmissionColor"))
        {
            goalMaterial.SetColor("_EmissionColor", gold * 2f);
            goalMaterial.EnableKeyword("_EMISSION");
        }
        
        r.material = goalMaterial;
    }
}