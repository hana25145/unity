using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SoapZone : MonoBehaviour
{
    [Range(0.05f, 1f)] [SerializeField] private float steeringMultiplier = 0.3f;
    [Range(0.1f, 1f)] [SerializeField] private float maxSpeedMultiplier = 1f;
    [SerializeField] private float slipperyDamping = 0f;

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
            player.AddSurfaceEffect(this, steeringMultiplier, maxSpeedMultiplier, slipperyDamping);
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerBall player = other.GetComponentInParent<PlayerBall>();
        if (player != null)
            player.RemoveSurfaceEffect(this);
    }
}
