using UnityEngine;

public class CurriculumController : MonoBehaviour
{
    [Range(0, 5)] public int difficulty = 0;
    public float arenaSizeBase = 6f;
    public int obstaclesBase = 2;
    public float taggerSpeedBase = 2.0f;

    // Exposed so UI can call Apply()
    public System.Action<int> OnDifficultyChanged;

    public void Apply(int level)
    {
        difficulty = Mathf.Clamp(level, 0, 5);
        OnDifficultyChanged?.Invoke(difficulty);
    }

    // Utility helpers for other systems to read "current" values
    public float CurrentArenaSize => arenaSizeBase + difficulty * 1.2f;
    public int CurrentObstacles => obstaclesBase + difficulty * 2;
    public float CurrentTaggerSpeed => taggerSpeedBase + difficulty * 0.5f;
}