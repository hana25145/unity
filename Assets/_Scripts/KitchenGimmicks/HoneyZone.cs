using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HoneyZone : MonoBehaviour
{
    [Header("Honey slowdown")]
    [Range(0.05f, 1f)] [SerializeField] private float accelerationMultiplier = 0.65f;
    [Range(0.05f, 1f)] [SerializeField] private float maxSpeedMultiplier = 0.7f;
    [SerializeField] private float honeyLinearDamping = 1.25f;

    public void Configure(float acceleration, float maxSpeed, float linearDamping)
    {
        accelerationMultiplier = Mathf.Clamp(acceleration, 0.05f, 1f);
        maxSpeedMultiplier = Mathf.Clamp(maxSpeed, 0.05f, 1f);
        honeyLinearDamping = Mathf.Max(0f, linearDamping);
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
        if (player != null)
            player.AddSurfaceEffect(this, accelerationMultiplier, maxSpeedMultiplier, honeyLinearDamping);
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerBall player = other.GetComponentInParent<PlayerBall>();
        if (player != null)
            player.RemoveSurfaceEffect(this);
    }
}
