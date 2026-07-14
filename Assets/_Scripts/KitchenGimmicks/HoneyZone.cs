using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HoneyZone : MonoBehaviour
{
    [Header("Honey slowdown")]
    [Range(0.05f, 1f)] [SerializeField] private float accelerationMultiplier = 0.2f;
    [Range(0.05f, 1f)] [SerializeField] private float maxSpeedMultiplier = 0.3f;
    [SerializeField] private float honeyLinearDamping = 5f;

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
