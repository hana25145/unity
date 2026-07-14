using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FanZone : MonoBehaviour
{
    [Tooltip("파란색 로컬 Z축 방향으로 바람이 붑니다.")]
    [SerializeField] private float windAcceleration = 16f;

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
            player.Body.AddForce(transform.forward * windAcceleration, ForceMode.Acceleration);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, transform.forward * 3f);
    }
}
