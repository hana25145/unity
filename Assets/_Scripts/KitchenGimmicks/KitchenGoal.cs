using UnityEngine;

[RequireComponent(typeof(Collider))]
public class KitchenGoal : MonoBehaviour
{
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
        if (other.GetComponentInParent<PlayerBall>() != null)
            KitchenGameManager.Instance?.TryCompleteCourse();
    }
}
