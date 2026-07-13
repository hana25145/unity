using UnityEngine;

public class HoneyZone : MonoBehaviour
{
    [Header("꿀 바닥 디버프 설정")]
    [SerializeField] private float accelerationMultiplier = 0.2f;
    [SerializeField] private float maxSpeedMultiplier = 0.3f;
    [SerializeField] private float honeyLinearDamping = 5.0f;

    private void OnTriggerEnter(Collider other)
    {
        PlayerBall player = other.GetComponent<PlayerBall>();
        if (player != null)
        {
            player.AddSurfaceEffect(this, accelerationMultiplier, maxSpeedMultiplier, honeyLinearDamping);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerBall player = other.GetComponent<PlayerBall>();
        if (player != null)
        {
            player.RemoveSurfaceEffect(this);
        }
    }
}