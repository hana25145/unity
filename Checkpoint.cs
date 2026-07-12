using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerBall player = other.GetComponentInParent<PlayerBall>();
        if (player == null)
            return;

        Transform point = respawnPoint != null ? respawnPoint : transform;
        player.SetCheckpoint(point.position, point.rotation);
    }
}
