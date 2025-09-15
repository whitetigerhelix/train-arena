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
        rb.velocity = Vector3.MoveTowards(rb.velocity, desiredVel, accel * Time.fixedDeltaTime);
    }
}