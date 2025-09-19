using UnityEngine;

/// <summary>
/// PD (Proportional-Derivative) Joint Controller for ragdoll locomotion
/// Controls individual ConfigurableJoint components with ML-Agents integration
/// Maps normalized actions [-1,1] to joint angle targets within specified limits
/// </summary>
public class PDJointController : MonoBehaviour
{
    [Header("Joint Configuration")]
    public ConfigurableJoint joint;
    
    [Header("PD Controller Gains")]
    public float kp = 80f;  // Proportional gain - controls joint response strength
    public float kd = 8f;   // Derivative gain - provides damping and smoothness
    
    [Header("Joint Angle Limits (degrees)")]
    public float minAngle = -45f;   // Minimum joint angle in degrees
    public float maxAngle = 45f;    // Maximum joint angle in degrees

    private float targetAngle;      // Current target angle in radians
    private bool controlEnabled = true; // Whether PD control is active

    void Reset()
    {
        joint = GetComponent<ConfigurableJoint>();
    }

    /// <summary>
    /// Set target joint angle from ML-Agents action
    /// </summary>
    /// <param name="t01">Normalized action value from [-1,1]</param>
    public void SetTarget01(float t01)
    {
        // Map ML-Agents action [-1,1] to joint angle range [minAngle, maxAngle] in degrees
        float targetDegrees = Mathf.Lerp(minAngle, maxAngle, (t01 + 1f) * 0.5f);
        targetAngle = targetDegrees * Mathf.Deg2Rad;
        controlEnabled = true; // Enable PD control when target is set
        
        // Debug logging for joint behavior analysis (sample occasionally to avoid spam)
        if (Time.fixedTime % 5f < Time.fixedDeltaTime)
        {
            TrainArenaDebugManager.Log($"🔧 {name}: Target={targetDegrees:F1}°, Action={t01:F2}", 
                TrainArenaDebugManager.DebugLogLevel.Verbose);
        }
    }
    
    /// <summary>
    /// Disable PD control to allow natural ragdoll physics
    /// Used when agent is inactive or for testing natural physics behavior
    /// </summary>
    public void DisableControl()
    {
        controlEnabled = false;
    }

    void FixedUpdate()
    {
        if (joint == null || !controlEnabled) return;

        // Calculate current joint angle relative to parent (simplified X-axis rotation)
        Quaternion localRotation = Quaternion.Inverse(transform.parent.rotation) * transform.rotation;
        float currentAngle = Mathf.DeltaAngle(0f, localRotation.eulerAngles.x) * Mathf.Deg2Rad;

        // PD Controller calculation: P (proportional to error) + D (derivative damping)
        float angleError = Mathf.DeltaAngle(localRotation.eulerAngles.x, targetAngle * Mathf.Rad2Deg) * Mathf.Deg2Rad;
        float angularVelocity = GetAngularVelocity().x;
        float controlTorque = kp * angleError - kd * angularVelocity;

        // Apply calculated torque to reach target angle
        GetComponent<Rigidbody>().AddTorque(transform.right * controlTorque, ForceMode.Acceleration);

        // Detailed joint diagnostics (sample occasionally to prevent log spam)
        if (name.Contains("0") && Time.fixedTime % 3f < Time.fixedDeltaTime)
        {
            TrainArenaDebugManager.Log($"🔧 {name}: Target={targetAngle * Mathf.Rad2Deg:F1}° | Current={currentAngle * Mathf.Rad2Deg:F1}° | Error={angleError * Mathf.Rad2Deg:F1}° | Torque={controlTorque:F1}", 
                TrainArenaDebugManager.DebugLogLevel.Verbose);
        }
    }

    /// <summary>
    /// Get angular velocity of the joint's rigidbody for derivative control
    /// </summary>
    private Vector3 GetAngularVelocity()
    {
        var rigidbody = GetComponent<Rigidbody>();
        return rigidbody != null ? rigidbody.angularVelocity : Vector3.zero;
    }
}