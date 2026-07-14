using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WaterZone : MonoBehaviour
{
    [SerializeField] private float submergeDelay = 1.25f;
    private readonly Dictionary<PlayerBall, float> submergedTimes = new();

    public void Configure(float delay)
    {
        submergeDelay = Mathf.Max(0f, delay);
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
        if (player == null)
            return;

        submergedTimes[player] = 0f;
        if (submergeDelay > .2f)
            KitchenGameManager.Instance?.ShowMessage("GET OUT OF THE WATER!", submergeDelay);
    }

    private void OnTriggerStay(Collider other)
    {
        PlayerBall player = other.GetComponentInParent<PlayerBall>();
        if (player == null)
            return;

        float elapsed = submergedTimes.TryGetValue(player, out float current) ? current : 0f;
        elapsed += Time.fixedDeltaTime;
        if (elapsed < submergeDelay)
        {
            submergedTimes[player] = elapsed;
            return;
        }

        submergedTimes.Remove(player);
        player.Respawn();
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerBall player = other.GetComponentInParent<PlayerBall>();
        if (player != null)
            submergedTimes.Remove(player);
    }

    private void OnDisable()
    {
        submergedTimes.Clear();
    }
}
