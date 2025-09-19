using UnityEngine;

/// <summary>
/// Multi-arena environment generator for ML-Agents training
/// 
/// Features:
/// - Configurable grid layouts (1x1 to 6x6 arenas)
/// - Preset configurations for different training scenarios
/// - Dynamic obstacle generation and goal placement
/// - Support for both cube and ragdoll agent types
/// - Arena spacing and size management through ArenaHelper
/// 
/// Presets:
/// - SingleArena: 1x1 for testing and debugging
/// - Training: 2x2 optimized for performance and learning speed
/// - LargeTraining: 6x6 for extensive training and robustness
/// - Custom: Manual configuration of all parameters
/// </summary>
public class EnvInitializer : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject agentPrefab;
    public GameObject goalPrefab;
    public GameObject obstaclePrefab;

    [Header("Layout Configuration")]
    [SerializeField] private EnvPreset preset = EnvPreset.Training;
    public EnvPreset Preset
    {
        get => preset;
        set
        {
            preset = value;
            ApplyPreset();
        }
    }
    [Space]

    [Header("Manual Configuration (when preset = Custom)")]
    [SerializeField] private int envCountX = 4;
    [SerializeField] private int envCountZ = 4;
    [SerializeField] private int obstaclesPerArena = 6;

    public int EnvCountX
    {
        get => envCountX;
        set => envCountX = Mathf.Max(1, value); // Ensure at least 1
    }
    public int EnvCountZ
    {
        get => envCountZ;
        set => envCountZ = Mathf.Max(1, value); // Ensure at least 1
    }
    public int ObstaclesPerArena
    {
        get => obstaclesPerArena;
        set => obstaclesPerArena = Mathf.Max(0, value); // Ensure non-negative
    }

    [Header("Arena Configuration")]
    [SerializeField] private ArenaHelper arenaHelper = new ArenaHelper();
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
                obstaclesPerArena = 5; // Fewer obstacles for testing
                TrainArenaDebugManager.Log("📋 Applied SingleArena preset: 1x1 grid for testing", TrainArenaDebugManager.DebugLogLevel.Important);
                break;
                
            case EnvPreset.Training:
                envCountX = 2;
                envCountZ = 2;
                arenaHelper.ArenaSize = 20f;
                obstaclesPerArena = 6; // Minimal obstacles for maximum performance
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
        if (agentPrefab == null || goalPrefab == null) { TrainArenaDebugManager.LogError("Assign prefabs in EnvInitializer"); return; }

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
        
        if (agentPrefab != null)
        {
            agentPrefab.SetActive(false);
            TrainArenaDebugManager.Log("Disabled agentPrefab", TrainArenaDebugManager.DebugLogLevel.Verbose);
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
        agentPosition.y += arenaHelper.AgentSpawnHeight;
        var agentGO = Instantiate(agentPrefab, agentPosition, Quaternion.identity, arenaContainer.transform);
        
        // Detect agent type and name appropriately
        var cubeAgent = agentGO.GetComponent<CubeAgent>();
        var ragdollAgent = agentGO.GetComponentInChildren<RagdollAgent>();
        
        if (ragdollAgent != null)
        {
            agentGO.name = $"RagdollAgent_Arena_{arenaIndex}";
        }
        else if (cubeAgent != null)
        {
            agentGO.name = $"CubeAgent_Arena_{arenaIndex}";
        }
        else
        {
            agentGO.name = $"Agent_Arena_{arenaIndex}"; // Fallback
        }
        
        var agent = cubeAgent; // For goal assignment (cubes need goal reference)
        
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