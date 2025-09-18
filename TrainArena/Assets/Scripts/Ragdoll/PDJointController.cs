using UnityEngine;

public class PDJointController : MonoBehaviour
{
    public ConfigurableJoint joint;
    [Header("PD Gains")]
    public float kp = 50f;  // Reduced from 200f for more natural movement
    public float kd = 3f;   // Reduced from 10f for less damping
    [Header("Limits (deg)")]
    public float minAngle = -45f;
    public float maxAngle = 45f;

    float targetAngle; // radians
    bool controlEnabled = true; // Control if PD controller is active

    void Reset()
    {
        joint = GetComponent<ConfigurableJoint>();
    }

    public void SetTarget01(float t01)
    {
        // map [-1,1] to [min,max] degrees
        float deg = Mathf.Lerp(minAngle, maxAngle, (t01 + 1f) * 0.5f);
        targetAngle = deg * Mathf.Deg2Rad;
        controlEnabled = true; // Enable control when setting target
        
        // CRITICAL DEBUG: Log target setting
        if (Time.fixedTime % 2f < Time.fixedDeltaTime)
        {
            TrainArenaDebugManager.Log($"🔧 PDJoint {name}: t01={t01:F2} -> deg={deg:F1}° -> rad={targetAngle:F3}", 
                TrainArenaDebugManager.DebugLogLevel.Important);
        }
    }
    
    public void DisableControl()
    {
        // Completely disable PD control for natural physics
        controlEnabled = false;
    }

    void FixedUpdate()
    {
        if (joint == null || !controlEnabled) return;

        // Assume hinge around joint's local X for simplicity
        // Compute current angle using joint's local rotation
        Quaternion localRot = Quaternion.Inverse(transform.parent.rotation) * transform.rotation;
        float current = Mathf.DeltaAngle(0f, localRot.eulerAngles.x) * Mathf.Deg2Rad;

        float error = Mathf.DeltaAngle(localRot.eulerAngles.x, targetAngle * Mathf.Rad2Deg) * Mathf.Deg2Rad;
        float torque = kp * error - kd * (GetAngularVelocity().x);

        // Diagnostic logging for joint behavior (sample first joint occasionally)
        if (name.Contains("0") && Time.fixedTime % 3f < Time.fixedDeltaTime) // Log joint "0" every 3 seconds
        {
            TrainArenaDebugManager.Log($"🔧 Joint {name}: enabled={controlEnabled}, target={targetAngle * Mathf.Rad2Deg:F1}°, current={current * Mathf.Rad2Deg:F1}°, error={error * Mathf.Rad2Deg:F1}°, torque={torque:F1}", 
                TrainArenaDebugManager.DebugLogLevel.Verbose);
        }

        GetComponent<Rigidbody>().AddTorque(transform.right * torque, ForceMode.Acceleration);
    }

    Vector3 GetAngularVelocity()
    {
        var rb = GetComponent<Rigidbody>();
        return rb ? rb.angularVelocity : Vector3.zero;
    }
}