using UnityEngine;
using Random = UnityEngine.Random;

public class DomainRandomizer : MonoBehaviour
{
    [Header("Toggles")]
    public bool randomizeMass = true;
    public bool randomizeFriction = true;
    public bool randomizeLighting = true;
    public bool randomizeGravity = false;

    [Header("Ranges")]
    public Vector2 massScaleRange = new Vector2(0.7f, 1.3f);
    public Vector2 frictionRange = new Vector2(0.2f, 1.0f);
    public Vector2 lightIntensityRange = new Vector2(0.6f, 1.4f);
    public Vector2 gravityScaleRange = new Vector2(0.8f, 1.2f);

    public void ApplyOnce()
    {
        if (randomizeMass)
        {
            foreach (var rb in GetComponentsInChildren<Rigidbody>())
            {
                float scale = Random.Range(massScaleRange.x, massScaleRange.y);
                rb.mass = Mathf.Max(0.1f, rb.mass * scale);
            }
        }

        if (randomizeFriction)
        {
            foreach (var col in GetComponentsInChildren<Collider>())
            {
                var mat = new PhysicsMaterial("DomRand");
                float f = Random.Range(frictionRange.x, frictionRange.y);
                mat.dynamicFriction = mat.staticFriction = f;
                col.material = mat;
            }
        }

        if (randomizeLighting)
        {
            var light = FindFirstObjectByType<Light>();
            if (light) light.intensity = Random.Range(lightIntensityRange.x, lightIntensityRange.y);
        }

        if (randomizeGravity)
        {
            Physics.gravity = new Vector3(0, -9.81f * Random.Range(gravityScaleRange.x, gravityScaleRange.y), 0);
        }
    }
}