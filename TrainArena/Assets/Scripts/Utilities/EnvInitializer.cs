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
    public float arenaSize = 14f; // spacing between envs
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
            }
        }
    }

    void SpawnArena(Vector3 center)
    {
        // Ground - make it big enough for the 12x12 agent spawn area
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.transform.position = center;
        ground.transform.localScale = Vector3.one * 1.4f; // 10x10 plane -> 14x14 world (matches arenaSize)
        ground.name = "Ground";

        // Agent - spawn closer to center, within reasonable bounds
        var agentGO = Instantiate(cubeAgentPrefab, center + Vector3.up * 0.5f, Quaternion.identity, transform);
        agentGO.name = "CubeAgent";
        var agent = agentGO.GetComponent<CubeAgent>();

        // Goal - place within smaller radius so it's always on ground
        var goalOffset = new Vector3(Random.Range(-4f, 4f), 0.5f, Random.Range(-4f, 4f));
        var goalGO = Instantiate(goalPrefab, center + goalOffset, Quaternion.identity, transform);
        goalGO.name = "Goal";
        if (agent != null) agent.goal = goalGO.transform;

        // Obstacles - keep within ground bounds
        if (obstaclePrefab != null)
        {
            for (int i = 0; i < obstaclesPerArena; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-4f, 4f), 0.5f, Random.Range(-4f, 4f));
                var obs = Instantiate(obstaclePrefab, center + offset, Quaternion.identity, transform);
                obs.tag = "Obstacle";
            }
        }
    }
}