using UnityEngine;

public class WaterZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerBall player = other.GetComponent<PlayerBall>();
        if (player != null)
        {
            player.Respawn();
        }
    }
}