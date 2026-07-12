using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HoneyZone : MonoBehaviour
{
    [Range(0.05f, 1f)] [SerializeField] private float accelerationMultiplier = 0.4f;
    [Range(0.05f, 1f)] [SerializeField] private float maxSpeedMultiplier = 0.45f;
    [SerializeField] private float stickyDamping = 3f;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerBall player = other.GetComponentInParent<PlayerBall>();
        if (player != null)
            player.AddSurfaceEffect(this, accelerationMultiplier, maxSpeedMultiplier, stickyDamping);
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerBall player = other.GetComponentInParent<PlayerBall>();
        if (player != null)
            player.RemoveSurfaceEffect(this);
    }
}
