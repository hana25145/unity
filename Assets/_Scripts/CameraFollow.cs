using UnityEngine;


public class CameraFollow : MonoBehaviour
{
    [Header("따라갈 대상")]
    public Transform target;                              

    [Header("카메라 위치")]
    public Vector3 offset = new Vector3(0f, 9f, -11f); 
    public float followSmooth = 6f;                     
    public bool lookAtTarget = true;                    
    public float lookSmooth = 8f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desired, followSmooth * Time.deltaTime);
        
        if (lookAtTarget)
        {
            Quaternion want = Quaternion.LookRotation(target.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, want, lookSmooth * Time.deltaTime);
        }
    }
}
