using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FanZone : MonoBehaviour
{
    [Tooltip("파란색 로컬 Z축 방향으로 바람이 붑니다.")]
    [SerializeField] private float windAcceleration = 16f;
    [SerializeField] private Vector3 worldWindDirection;

    private Vector3 WindDirection => worldWindDirection.sqrMagnitude > 0.001f
        ? worldWindDirection.normalized
        : transform.forward;

    public void Configure(Vector3 direction, float acceleration)
    {
        worldWindDirection = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector3.zero;
        windAcceleration = Mathf.Max(0f, acceleration);
    }

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerStay(Collider other)
    {
        PlayerBall player = other.GetComponentInParent<PlayerBall>();
        if (player != null)
            player.Body.AddForce(WindDirection * windAcceleration, ForceMode.Acceleration);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, WindDirection * 3f);
    }
}
