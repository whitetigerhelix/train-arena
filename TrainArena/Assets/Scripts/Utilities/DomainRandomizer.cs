using UnityEngine;

public class DomainRandomizer : MonoBehaviour
{
    [Range(0f, 2f)] public float massScale = 1f;
    [Range(0f, 2f)] public float frictionScale = 1f;
    public Light sceneLight;
    public Color[] lightColors;

    Rigidbody[] rbs;
    PhysicMaterial[] physicsMats;

    void Awake()
    {
        rbs = GetComponentsInChildren<Rigidbody>();
        var colliders = GetComponentsInChildren<Collider>();
        physicsMats = new PhysicMaterial[colliders.Length];
        for (int i = 0; i < colliders.Length; i++)
        {
            physicsMats[i] = new PhysicMaterial("dynMat" + i);
            colliders[i].material = physicsMats[i];
        }
    }

    public void ApplyRandomization()
    {
        foreach (var rb in rbs)
        {
            rb.mass *= Random.Range(0.5f, 1.5f) * massScale;
        }
        foreach (var mat in physicsMats)
        {
            mat.dynamicFriction = Random.Range(0.2f, 1.0f) * frictionScale;
            mat.staticFriction = mat.dynamicFriction;
        }
        if (sceneLight && lightColors.Length > 0)
        {
            sceneLight.color = lightColors[Random.Range(0, lightColors.Length)];
            sceneLight.intensity = Random.Range(0.6f, 1.4f);
        }
    }
}