using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FanZone : MonoBehaviour
{
    [Tooltip("빨간색 로컬 X축 방향으로 바람이 붑니다. (음수로 하면 반대 방향)")]
    [SerializeField] private float windAcceleration = 70f;

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
            player.Body.AddForce(transform.right * windAcceleration, ForceMode.Acceleration);  // X축(빨강)으로 바람
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.right * 3f);  // 빨간 화살표 = 바람 방향
    }
}
