using UnityEngine;

public class PlayerBall : MonoBehaviour
{
    public float speed = 20f;        // 최고 속도 (이제 수치가 정직하게 작동합니다)
    public float acceleration = 50f; // 가속도 (얼마나 빠르게 최고 속도에 도달할지)
    public float deceleration = 30f; // 감속도 (키를 떼었을 때 얼마나 미끄러지며 멈출지)

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        // 1. 입력에 따른 '목표 속도' 방향과 크기를 계산합니다.
        Vector3 moveDirection = new Vector3(moveHorizontal, 0.0f, moveVertical).normalized;
        Vector3 targetVelocity = moveDirection * speed;

        // 2. 현재 공의 X, Z축 속도만 따로 가져옵니다. (Y축 중력 제외)
        Vector3 currentHorizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // 3. 키를 누르고 있을 때와 뗐을 때의 가감속 비율을 결정합니다.
        float activeRate = (moveDirection.magnitude > 0) ? acceleration : deceleration;

        // 4. MoveTowards를 통해 현재 속도에서 목표 속도까지 매 프레임 부드럽게 변화시킵니다.
        Vector3 newVelocity = Vector3.MoveTowards(
            currentHorizontalVelocity, 
            targetVelocity, 
            activeRate * Time.fixedDeltaTime
        );

        // 5. 계산된 속도를 Rigidbody에 적용합니다. (Y축 중력은 그대로 유지)
        rb.linearVelocity = new Vector3(newVelocity.x, rb.linearVelocity.y, newVelocity.z);
    }
}
