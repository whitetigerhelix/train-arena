using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HeuristicTagger : MonoBehaviour
{
    public Transform target;
    public float speed = 3.0f;
    public float accel = 8f;
    Rigidbody rb;

    void Awake() { rb = GetComponent<Rigidbody>(); }

    void FixedUpdate()
    {
        if (!target) return;
        Vector3 dir = (target.position - transform.position).normalized;
        Vector3 desiredVel = new Vector3(dir.x, 0f, dir.z) * speed;
        rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, desiredVel, accel * Time.fixedDeltaTime);
    }
}