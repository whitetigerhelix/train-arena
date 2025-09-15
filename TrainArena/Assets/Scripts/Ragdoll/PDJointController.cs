using UnityEngine;

public class PDJointController : MonoBehaviour
{
    public ConfigurableJoint joint;
    [Header("PD Gains")]
    public float kp = 200f;
    public float kd = 10f;
    [Header("Limits (deg)")]
    public float minAngle = -45f;
    public float maxAngle = 45f;

    float targetAngle; // radians

    void Reset()
    {
        joint = GetComponent<ConfigurableJoint>();
    }

    public void SetTarget01(float t01)
    {
        // map [-1,1] to [min,max] degrees
        float deg = Mathf.Lerp(minAngle, maxAngle, (t01 + 1f) * 0.5f);
        targetAngle = deg * Mathf.Deg2Rad;
    }

    void FixedUpdate()
    {
        if (joint == null) return;

        // Assume hinge around joint's local X for simplicity
        // Compute current angle using joint's local rotation
        Quaternion localRot = Quaternion.Inverse(transform.parent.rotation) * transform.rotation;
        float current = Mathf.DeltaAngle(0f, localRot.eulerAngles.x) * Mathf.Deg2Rad;

        float error = Mathf.DeltaAngle(localRot.eulerAngles.x, targetAngle * Mathf.Rad2Deg) * Mathf.Deg2Rad;
        float torque = kp * error - kd * (GetAngularVelocity().x);

        GetComponent<Rigidbody>().AddTorque(transform.right * torque, ForceMode.Acceleration);
    }

    Vector3 GetAngularVelocity()
    {
        var rb = GetComponent<Rigidbody>();
        return rb ? rb.angularVelocity : Vector3.zero;
    }
}