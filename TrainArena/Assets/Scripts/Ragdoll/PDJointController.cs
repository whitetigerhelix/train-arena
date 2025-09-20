using UnityEngine;
using TrainArena.Configuration;

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
    public float kp = 200f;  // Proportional gain - increased for stronger joint response
    public float kd = 20f;   // Derivative gain - increased for better stability
    
    [Header("Joint Angle Limits (degrees)")]
    public float minAngle = -90f;   // Minimum joint angle in degrees - increased range
    public float maxAngle = 90f;    // Maximum joint angle in degrees - increased range

    private float targetAngle;      // Current target angle in radians
    private bool controlEnabled = true; // Whether PD control is active

    void Reset()
    {
        joint = GetComponent<ConfigurableJoint>();
    }

    void Start()
    {
        // Apply centralized configuration if available
        ApplyCentralizedConfig();
    }

    /// <summary>
    /// Apply centralized joint configuration based on joint name
    /// </summary>
    public void ApplyCentralizedConfig()
    {
        if (joint == null) return;

        // Get configuration from centralized system
        var (minAngleConfig, maxAngleConfig, kpConfig, kdConfig) = RagdollJointNames.GetJointControllerConfig(name);
        
        // Only apply if we have valid configuration (non-zero values)
        if (minAngleConfig != 0f || maxAngleConfig != 0f)
        {
            minAngle = minAngleConfig;
            maxAngle = maxAngleConfig;
        }
        
        if (kpConfig > 0f)
        {
            kp = kpConfig;
        }
        
        if (kdConfig > 0f)
        {
            kd = kdConfig;
        }

        TrainArenaDebugManager.Log($"🔧 {name}: Applied centralized config - angles:[{minAngle:F1}°, {maxAngle:F1}°], gains:[kp:{kp}, kd:{kd}]", 
            TrainArenaDebugManager.DebugLogLevel.Verbose);
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
            TrainArenaDebugManager.Log($"🔧 {name}: Target={targetDegrees:F1}°, Action={t01:F2}, kp={kp}, kd={kd}", 
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

        var rigidbody = GetComponent<Rigidbody>();
        if (rigidbody == null) return;

        // Get the joint's primary axis in world space
        Vector3 jointAxis = transform.TransformDirection(joint.axis);
        
        // Calculate current joint angle relative to rest position
        Quaternion currentRotation = transform.localRotation;
        float currentAngle = 0f;
        
        // Extract angle around the joint's primary axis
        if (joint.angularXMotion == ConfigurableJointMotion.Limited)
        {
            currentAngle = currentRotation.eulerAngles.x;
            if (currentAngle > 180f) currentAngle -= 360f;
            currentAngle *= Mathf.Deg2Rad;
        }
        else if (joint.angularZMotion == ConfigurableJointMotion.Limited)
        {
            currentAngle = currentRotation.eulerAngles.z;
            if (currentAngle > 180f) currentAngle -= 360f;
            currentAngle *= Mathf.Deg2Rad;
        }

        // PD Controller calculation: P (proportional to error) + D (derivative damping)
        float angleError = targetAngle - currentAngle;
        
        // Get angular velocity projected onto the joint axis
        Vector3 angularVelocity = rigidbody.angularVelocity;
        float axisAngularVelocity = Vector3.Dot(angularVelocity, jointAxis);
        
        // Calculate control torque with improved scaling
        float controlTorque = kp * angleError - kd * axisAngularVelocity;
        
        // Apply torque along the joint axis with mass scaling for consistent behavior
        Vector3 torqueVector = jointAxis * controlTorque * rigidbody.mass;
        rigidbody.AddTorque(torqueVector, ForceMode.Force);

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