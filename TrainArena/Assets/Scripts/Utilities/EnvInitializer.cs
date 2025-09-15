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
        // Ground
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.transform.position = center;
        ground.transform.localScale = Vector3.one * 0.5f; // 10x10 plane -> 5x5 world by default
        ground.name = "Ground";

        // Agent
        var agentGO = Instantiate(cubeAgentPrefab, center + Vector3.up * 0.5f, Quaternion.identity, transform);
        agentGO.name = "CubeAgent";
        var agent = agentGO.GetComponent<CubeAgent>();

        // Goal
        var goalGO = Instantiate(goalPrefab, center + new Vector3(2f, 0.5f, 2f), Quaternion.identity, transform);
        goalGO.name = "Goal";
        if (agent != null) agent.goal = goalGO.transform;

        // Obstacles
        if (obstaclePrefab != null)
        {
            for (int i = 0; i < obstaclesPerArena; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-5f, 5f), 0.5f, Random.Range(-5f, 5f));
                var obs = Instantiate(obstaclePrefab, center + offset, Quaternion.identity, transform);
                obs.tag = "Obstacle";
            }
        }
    }
}