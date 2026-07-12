using UnityEngine;

public class KitchenCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new(0f, 11f, -13f);
    [SerializeField] private float smoothTime = 0.18f;

    private Vector3 velocity;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            target.position + offset,
            ref velocity,
            smoothTime
        );
        transform.LookAt(target.position + Vector3.up * 0.5f);
    }
}
