using UnityEngine;

public class KitchenSpinner : MonoBehaviour
{
    [SerializeField] private Vector3 localAxis = Vector3.forward;
    [SerializeField] private float degreesPerSecond = 260f;

    private void Update()
    {
        transform.Rotate(localAxis, degreesPerSecond * Time.deltaTime, Space.Self);
    }
}
