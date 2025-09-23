using UnityEngine;
using System.Collections;

/// <summary>
/// Simple eye blinking animation for visual polish - completely reusable
/// Automatically finds child objects with "Eye" in their name and animates them
/// </summary>
public class EyeBlinker : MonoBehaviour
{
    [Header("Blink Settings")]
    [SerializeField] private float minBlinkInterval = 2f;
    [SerializeField] private float maxBlinkInterval = 5f;
    [SerializeField] private float blinkDuration = 0.15f;
    [SerializeField] private AnimationCurve blinkCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private Transform[] eyeTransforms;
    private Vector3[] originalEyeScales;
    private Coroutine blinkCoroutine;

    void OnEnable()
    {
        FindEyes();
        StartBlinking();
    }

    private void OnDisable()
    {
        SetBlinkingEnabled(false);
    }

    void FindEyes()
    {
        // Find all child objects with "Eye" in their name
        var eyeList = new System.Collections.Generic.List<Transform>();
        var originalScales = new System.Collections.Generic.List<Vector3>();

        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child != transform && child.name.Contains("Eye"))
            {
                Debug.Log($"EyeBlinker on {gameObject.name}: Found eye object '{child.name}'");
                eyeList.Add(child);
                originalScales.Add(child.localScale);
            }
        }

        eyeTransforms = eyeList.ToArray();
        originalEyeScales = originalScales.ToArray();

        if (eyeTransforms.Length == 0)
        {
            Debug.LogWarning($"EyeBlinker on {gameObject.name}: No objects with 'Eye' in name found!");
        }
    }

    void StartBlinking()
    {
        if (eyeTransforms.Length > 0)
        {
            blinkCoroutine = StartCoroutine(BlinkLoop());
        }
    }

    IEnumerator BlinkLoop()
    {
        Debug.Log($"EyeBlinker on {gameObject.name}: Starting blink loop with {eyeTransforms.Length} eyes.");
        while (true)
        {
            // Wait for random interval between blinks
            float waitTime = Random.Range(minBlinkInterval, maxBlinkInterval);
            //Debug.Log($"EyeBlinker on {gameObject.name}: Waiting {waitTime:F2} seconds before next blink.");
            yield return new WaitForSeconds(waitTime);

            // Perform blink
            yield return StartCoroutine(PerformBlink());
        }
    }

    IEnumerator PerformBlink()
    {
        //Debug.Log($"EyeBlinker on {gameObject.name}: Performing blink.");

        float elapsedTime = 0f;

        while (elapsedTime < blinkDuration)
        {
            float progress = elapsedTime / blinkDuration;
            float curveValue = blinkCurve.Evaluate(progress);

            // Animate eye scale - squash Y to create blink effect
            for (int i = 0; i < eyeTransforms.Length; i++)
            {
                if (eyeTransforms[i] != null)
                {
                    Vector3 scale = originalEyeScales[i];
                    scale.y *= curveValue; // Squash Y-axis for blink
                    eyeTransforms[i].localScale = scale;
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure eyes return to original scale
        for (int i = 0; i < eyeTransforms.Length; i++)
        {
            if (eyeTransforms[i] != null)
            {
                eyeTransforms[i].localScale = originalEyeScales[i];
            }
        }
    }

    void OnDestroy()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
    }

    // Public method to trigger manual blink (for special events)
    public void TriggerBlink()
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(PerformBlink());
        }
    }

    // Method to temporarily stop/start blinking
    public void SetBlinkingEnabled(bool enabled)
    {
        Debug.Log($"EyeBlinker on {gameObject.name}: SetBlinkingEnabled({enabled}) called.");

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        if (enabled)
        {
            StartBlinking();
        }
    }
}