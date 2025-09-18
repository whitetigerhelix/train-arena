using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Centralized arena management system that handles all position calculations,
/// boundary checks, and object placement logic for training environments.
/// Replaces hardcoded values throughout EnvInitializer and CubeAgent.
/// </summary>
[System.Serializable]
public class ArenaHelper
{
    [Header("Arena Dimensions")]
    [SerializeField] private float arenaSize = 20f;
    [SerializeField, Range(0.1f, 1f)] private float groundPercentage = 0.7f; // Percentage of arena size for ground plane (0.0 to 1.0)
    [SerializeField, Range(0.1f, 1f)] private float safeZonePercentage = 0.8f; // Keep objects/goal within this percentage of groundRadius to avoid edges

    [Header("Height Settings")]
    [SerializeField] private float agentSpawnHeight = 0.5f; // Half cube height above ground
    public float AgentSpawnHeight
    {
        get => agentSpawnHeight;
        set => agentSpawnHeight = value;
    }
    [SerializeField] private float goalHeight = 1.0f;       // Goal height above ground
    public float GoalHeight => goalHeight;
    [SerializeField] private float obstacleHeight = 1.0f;   // Obstacle height above ground
    public float ObstacleHeight => obstacleHeight;
    
    [Header("Placement Constraints")]
    [SerializeField] private float minGoalDistance = 1.5f;     // Minimum distance between agent and goal
    [SerializeField] private float minObstacleDistance = 1.5f; // Minimum clearance around obstacles
    [SerializeField] private int maxPlacementAttempts = 20;    // Maximum attempts for valid placement
    
    // Properties - match exact calculations from EnvInitializer
    public float ArenaRadius => arenaSize / 2f;
    public float GroundRadius => (arenaSize * groundPercentage) / 2f;
    public float SafeZoneRadius => GroundRadius * safeZonePercentage;
    
    // Expose settings for external configuration
    public float ArenaSize 
    { 
        get => arenaSize; 
        set => arenaSize = value; 
    }
    
    public float GroundPercentage 
    { 
        get => groundPercentage; 
        set => groundPercentage = Mathf.Clamp01(value); 
    }
    
    public float SafeZonePercentage 
    { 
        get => safeZonePercentage; 
        set => safeZonePercentage = Mathf.Clamp01(value); 
    }
    
    /// <summary>
    /// Generate a random agent spawn position within the safe zone of an arena.
    /// Preserves exact behavior from CubeAgent.OnEpisodeBegin()
    /// </summary>
    public Vector3 GetRandomAgentPosition(Vector3 arenaCenter)
    {
        float xOffset = Random.Range(-SafeZoneRadius, SafeZoneRadius);
        float zOffset = Random.Range(-SafeZoneRadius, SafeZoneRadius);
        return arenaCenter + new Vector3(xOffset, agentSpawnHeight, zOffset);
    }
    
    /// <summary>
    /// Generate a random goal position within the safe zone, ensuring minimum distance from agent.
    /// Preserves exact behavior from CubeAgent.OnEpisodeBegin() and EnvInitializer.SpawnArena()
    /// </summary>
    public Vector3 GetRandomGoalPosition(Vector3 arenaCenter, Vector3 agentPosition)
    {
        return GetRandomGoalPosition(arenaCenter, agentPosition, minGoalDistance);
    }
    
    /// <summary>
    /// Generate a random goal position with custom minimum distance from agent.
    /// </summary>
    public Vector3 GetRandomGoalPosition(Vector3 arenaCenter, Vector3 agentPosition, float customMinDistance)
    {
        Vector3 goalPosition;
        int attempts = 0;
        
        do 
        {
            float xOffset = Random.Range(-SafeZoneRadius, SafeZoneRadius);
            float zOffset = Random.Range(-SafeZoneRadius, SafeZoneRadius);
            goalPosition = arenaCenter + new Vector3(xOffset, goalHeight, zOffset);
            attempts++;
            
        } while (Vector3.Distance(goalPosition, agentPosition) < customMinDistance && attempts < maxPlacementAttempts);
        
        if (attempts >= maxPlacementAttempts)
        {
            TrainArenaDebugManager.LogWarning($"ArenaHelper: Could not place goal with minimum distance {customMinDistance} after {maxPlacementAttempts} attempts");
        }
        
        return goalPosition;
    }
    
    /// <summary>
    /// Generate a distributed goal position using arena index for consistent spread.
    /// Preserves exact behavior from EnvInitializer.SpawnArena() goal placement.
    /// </summary>
    public Vector3 GetDistributedGoalPosition(Vector3 arenaCenter, int arenaIndex)
    {
        // Use same algorithm as EnvInitializer: prime number offset for distribution
        var goalAngle = (arenaIndex * 73f) % 360f;
        var goalDistance = Random.Range(GroundRadius * 0.2f, GroundRadius * 0.8f);
        var goalOffset = new Vector3(
            Mathf.Cos(goalAngle * Mathf.Deg2Rad) * goalDistance,
            goalHeight,
            Mathf.Sin(goalAngle * Mathf.Deg2Rad) * goalDistance
        );
        
        return arenaCenter + goalOffset;
    }
    
    /// <summary>
    /// Generate a valid obstacle position avoiding agent, goal, and other obstacles.
    /// Preserves exact behavior from EnvInitializer.SpawnArena() obstacle placement.
    /// </summary>
    public Vector3 GetValidObstaclePosition(Vector3 arenaCenter, Vector3 agentPosition, Vector3 goalPosition, List<Vector3> existingObstacles = null)
    {
        Vector3 obstaclePosition;
        int attempts = 0;
        bool validPosition = false;
        
        do
        {
            // Use same range calculation as EnvInitializer
            float xOffset = Random.Range(-GroundRadius, GroundRadius) * safeZonePercentage;
            float zOffset = Random.Range(-GroundRadius, GroundRadius) * safeZonePercentage;
            obstaclePosition = arenaCenter + new Vector3(xOffset, obstacleHeight / 2f, zOffset);
            
            // Check distances (preserving exact logic from EnvInitializer)
            float distanceFromGoal = Vector3.Distance(obstaclePosition, goalPosition);
            float distanceFromAgent = Vector3.Distance(obstaclePosition, agentPosition);
            
            validPosition = distanceFromGoal > minObstacleDistance && distanceFromAgent > minObstacleDistance;
            
            // Check distance from existing obstacles if provided
            if (validPosition && existingObstacles != null)
            {
                foreach (var existingPos in existingObstacles)
                {
                    if (Vector3.Distance(obstaclePosition, existingPos) < minObstacleDistance)
                    {
                        validPosition = false;
                        break;
                    }
                }
            }
            
            attempts++;
            
        } while (!validPosition && attempts < maxPlacementAttempts);
        
        if (!validPosition)
        {
            TrainArenaDebugManager.LogWarning($"ArenaHelper: Could not place obstacle after {maxPlacementAttempts} attempts");
        }
        
        return obstaclePosition;
    }
    
    /// <summary>
    /// Check if a position is within the arena bounds.
    /// Used for validation in both EnvInitializer and CubeAgent.
    /// </summary>
    public bool IsWithinArenaBounds(Vector3 position, Vector3 arenaCenter)
    {
        Vector3 localPos = position - arenaCenter;
        return Mathf.Abs(localPos.x) <= GroundRadius && Mathf.Abs(localPos.z) <= GroundRadius;
    }
    
    /// <summary>
    /// Check if a position is within the safe zone (where objects can be placed).
    /// </summary>
    public bool IsWithinSafeZone(Vector3 position, Vector3 arenaCenter)
    {
        Vector3 localPos = position - arenaCenter;
        return Mathf.Abs(localPos.x) <= SafeZoneRadius && Mathf.Abs(localPos.z) <= SafeZoneRadius;
    }
    
    /// <summary>
    /// Get ground scale for Unity plane primitive (10x10 default).
    /// Preserves exact calculation from EnvInitializer.
    /// </summary>
    public Vector3 GetGroundScale()
    {
        return Vector3.one * ((GroundRadius * 2f) / 10f);
    }
    
    /// <summary>
    /// Validate all objects in an arena are within bounds.
    /// Enhanced version of EnvInitializer.ValidateArenaObjects()
    /// </summary>
    public bool ValidateArenaObjects(Transform arenaContainer, Vector3 arenaCenter, int arenaIndex, bool logResults = true)
    {
        bool allValid = true;
        int checkedObjects = 0;
        
        foreach (Transform child in arenaContainer)
        {
            Vector3 pos = child.position;
            
            if (!IsWithinArenaBounds(pos, arenaCenter))
            {
                if (logResults)
                {
                    TrainArenaDebugManager.LogError($"Arena {arenaIndex}: {child.name} is OUTSIDE arena bounds at {pos}! " +
                                 $"Local offset: {pos - arenaCenter}");
                }
                allValid = false;
            }
            checkedObjects++;
        }
        
        if (allValid && logResults)
        {
            TrainArenaDebugManager.Log($"Arena {arenaIndex}: All {checkedObjects} objects within bounds ✓");
        }
        
        return allValid;
    }
    
    /// <summary>
    /// Get debug information about arena configuration.
    /// Useful for troubleshooting and logging.
    /// </summary>
    public string GetDebugInfo()
    {
        return $"ArenaHelper Config: Size={arenaSize} | GroundRadius={GroundRadius:F1} | SafeZoneRadius={SafeZoneRadius:F1} | " +
               $"AgentHeight={agentSpawnHeight} | GoalHeight={goalHeight} | MinDistances=Agent-Goal:{minGoalDistance}, Obstacle:{minObstacleDistance}";
    }
}