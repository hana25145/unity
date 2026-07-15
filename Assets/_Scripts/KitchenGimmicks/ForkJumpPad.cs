using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ForkJumpPad : MonoBehaviour
{
    [SerializeField] private float upwardSpeed = 9f;
    [SerializeField] private float forwardSpeed = 5f;
    [SerializeField] private float reuseDelay = 0.35f;

    private readonly Dictionary<PlayerBall, float> lastLaunchTimes = new();

    public void Configure(float upSpeed, float forwardLaunchSpeed, float delay = 0.35f)
    {
        upwardSpeed = Mathf.Max(0f, upSpeed);
        forwardSpeed = Mathf.Max(0f, forwardLaunchSpeed);
        reuseDelay = Mathf.Max(0f, delay);
    }

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerBall player = other.GetComponentInParent<PlayerBall>();
        if (player == null)
            return;

        if (lastLaunchTimes.TryGetValue(player, out float lastTime) &&
            Time.time - lastTime < reuseDelay)
            return;

        Rigidbody body = player.Body;
        Vector3 velocity = body.linearVelocity;
        velocity.y = Mathf.Max(0f, velocity.y);
        body.linearVelocity = velocity;
        body.AddForce(
            Vector3.up * upwardSpeed + transform.forward * forwardSpeed,
            ForceMode.VelocityChange
        );

        lastLaunchTimes[player] = Time.time;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 direction = Vector3.up * upwardSpeed + transform.forward * forwardSpeed;
        Gizmos.DrawRay(transform.position, direction * 0.25f);
    }
}
