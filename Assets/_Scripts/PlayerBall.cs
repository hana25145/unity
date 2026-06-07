using UnityEngine;

public class PlayerBall : MonoBehaviour
{
    // 현실 물리 모드에서는 토크(회전시키는 힘)를 주므로 값이 커야 시원하게 움직입니다.
    public float rollPower = 30f; 

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        // 이동 입력 방향 계산
        Vector3 moveDirection = new Vector3(moveHorizontal, 0.0f, moveVertical).normalized;

        if (moveDirection.magnitude > 0)
        {
            // [핵심] 공이 앞으로 구르려면, 진행 방향의 '오른쪽 축'을 기준으로 회전해야 합니다.
            // 외적(Cross Product)을 이용해 굴러가야 할 회전축을 정확히 계산합니다.
            Vector3 rotationAxis = Vector3.Cross(Vector3.up, moveDirection);

            // 공에 회전력(Torque)을 가해 진짜 물리적으로 구르게 만듭니다.
            rb.AddTorque(rotationAxis * rollPower, ForceMode.Acceleration);
        }

        if (transform.position.y < -10f)
        {
            transform.position = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}