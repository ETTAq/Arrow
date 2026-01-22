using UnityEngine;
using System.Collections;

public class Arrow : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isStuck = false;

    [Header("───── 물리 설정 ─────")]
    [SerializeField] private float dragAscending = 0.8f; // 상승 중 damping
    [SerializeField] private float dragDescending = 0.2f; // 하강 중 damping
    [SerializeField] private float extraForwardForce = 0.3f; // 진행 방향 최대 힘
    [SerializeField] private float extraDownForce = 0.5f;    // 아래 방향 최대 힘
    [SerializeField] private float forceDuration = 2f;       // 힘이 줄어드는 시간 (초)

    private float flightTime = 0f; // 비행 시간 추적

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 direction, float power)
    {
        isStuck = false;
        gameObject.SetActive(true);

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.simulated = true;

        rb.AddForce(direction * power, ForceMode2D.Impulse);

        // 발사 각도에 따라 초기 damping 설정
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rb.linearDamping = (angle > 90f) ? dragAscending : dragDescending;

        flightTime = 0f; // 비행 시간 초기화
    }

    private void FixedUpdate()
    {
        if (!isStuck && rb != null)
        {
            flightTime += Time.fixedDeltaTime;
            Vector2 vel = rb.linearVelocity;

            if (vel.y > 0)
            {
                // 상승 중 → damping 크게
                rb.linearDamping = dragAscending;
            }
            else
            {
                // 하강 중 → damping 줄이고 추가 힘 적용
                rb.linearDamping = dragDescending;

                if (vel.sqrMagnitude > 0.01f)
                {
                    // 보간된 힘 크기 계산 (시간이 지날수록 줄어듦)
                    float forwardForce = Mathf.Lerp(extraForwardForce, 0f, flightTime / forceDuration);
                    float downForce = Mathf.Lerp(extraDownForce, 0f, flightTime / forceDuration);

                    // 진행 방향으로 힘
                    Vector2 dir = vel.normalized;
                    rb.AddForce(dir * forwardForce, ForceMode2D.Force);

                    // 아래 방향으로 힘
                    rb.AddForce(Vector2.down * downForce, ForceMode2D.Force);
                }
            }
        }
    }

    private void Update()
    {
        // 이동 중일 때 진행 방향에 맞춰 회전
        if (!isStuck && rb != null && rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            Vector2 vel = rb.linearVelocity;
            float angle = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    [SerializeField] private float minShakeAngle = 5f;   // 최소 흔들림 각도
    [SerializeField] private float maxShakeAngle = 20f;  // 최대 흔들림 각도
    [SerializeField] private float maxImpactSpeed = 20f; // 속도 기준 상한
    [SerializeField] private float shakeMultiplier = 1f; // 흔들림 배율

    private float impactSpeed; // 착탄 속도 저장

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isStuck) return;

        if (other.CompareTag("Target") || other.CompareTag("Ground"))
        {
            isStuck = true;

            if (other.CompareTag("Target"))
            {
                // 타겟에 맞았을 때 점수 획득 로직
                GlobalDelegate.Raise(new HitTarget());
                Debug.Log("Hit Target!");
            }

            // 착탄 직전 속도 크기 저장
            impactSpeed = rb.linearVelocity.magnitude;

            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;

            Vector3 hitPoint = other.ClosestPoint(transform.position);
            Vector3 arrowTip = transform.position;
            Vector3 offset = hitPoint - arrowTip;
            transform.position += offset;

            Quaternion impactRotation = transform.rotation;
            StartCoroutine(RotateShakeAndDisappear(impactRotation));
        }
    }

    private IEnumerator RotateShakeAndDisappear(Quaternion impactRotation)
    {
        float duration = 0.3f;
        float elapsed = 0f;

        // 속도에 비례한 초기 흔들림 강도
        float baseStrength = Mathf.Clamp(impactSpeed, 0f, maxImpactSpeed) * shakeMultiplier;

        // 속도에 비례한 최대 흔들림 각도
        float maxAngle = Mathf.Lerp(minShakeAngle, maxShakeAngle, impactSpeed / maxImpactSpeed);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float strength = Mathf.Lerp(baseStrength, 0f, elapsed / duration);

            // 흔들림 각도는 속도 기반 maxAngle을 사용
            float shakeAngle = Mathf.Sin(elapsed * 40f) * strength * (maxAngle / maxImpactSpeed);

            transform.rotation = impactRotation * Quaternion.Euler(0, 0, shakeAngle);
            yield return null;
        }

        transform.rotation = impactRotation;

        yield return new WaitForSeconds(5f);
        gameObject.SetActive(false);
    }


}
