using UnityEngine;

// 공을 따라다니는 카메라 (+ 시작 인트로: 장바구니 안에서 잠깐 머물다 위로 올라감)
// 사용법:
// 1) Main Camera에 붙이고 target에 공(Player)을 드래그
// 2) Main Camera를 "장바구니 안을 들여다보는" 위치/각도에 놓고 시작 (여기가 인트로 시작 장면)
// 3) offset으로 최종 따라가는 시점 조절 (재생 중 조절 추천)
public class CameraFollow : MonoBehaviour
{
    [Header("따라갈 대상")]
    public Transform target;                              // 공(Player)

    [Header("최종 따라가는 시점")]
    public Vector3 offset = new Vector3(0f, 9f, -11f);    // 공 기준 뒤·위
    public float followSmooth = 4f;
    public bool lookAtTarget = true;
    public float lookSmooth = 6f;

    [Header("인트로 (장바구니에서 시작)")]
    [Tooltip("시작 후 이 시간 동안 처음 위치(장바구니 안)에 머묾")]
    public float introHold = 1.2f;
    [Tooltip("머문 뒤 위로 천천히 올라오는 구간 길이(초)")]
    public float introRise = 2.5f;
    [Tooltip("올라올 때 속도(작을수록 천천히)")]
    public float riseSmooth = 1.5f;

    private float timer;

    void Start() { timer = 0f; }

    void LateUpdate()
    {
        if (target == null) return;
        timer += Time.deltaTime;

        // 인트로 단계별로 따라가는 강도를 조절
        float smooth;
        if (timer < introHold) smooth = 0f;                 // ① 장바구니 안에서 멈춰 시작 장면 보여주기
        else if (timer < introHold + introRise) smooth = riseSmooth;  // ② 천천히 위로 올라감
        else smooth = followSmooth;                         // ③ 평소처럼 따라가기

        Vector3 desired = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desired, smooth * Time.deltaTime);

        if (lookAtTarget)   // 인트로 내내 공을 바라봄
        {
            Quaternion want = Quaternion.LookRotation(target.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, want, lookSmooth * Time.deltaTime);
        }
    }
}
