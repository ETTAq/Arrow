using UnityEngine;
using System.Collections;

public class Arrow : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isStuck = false;

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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isStuck) return;

        if (other.CompareTag("Target") || other.CompareTag("Ground"))
        {
            isStuck = true;

            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;

            // 트리거 충돌 시점에서 화살 끝이 닿도록 보정
            // 여기서는 단순히 현재 위치 유지하거나, Collider의 ClosestPoint를 활용
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            Vector3 arrowTip = transform.position; // Pivot을 화살 끝으로 옮겼으므로 Transform 자체가 끝 위치
            Vector3 offset = hitPoint - arrowTip;
            transform.position += offset;

            // 착탄 직후 회전값 저장
            Quaternion impactRotation = transform.rotation;

            StartCoroutine(RotateShakeAndDisappear(impactRotation));
        }
    }

    private IEnumerator RotateShakeAndDisappear(Quaternion impactRotation)
    {
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float strength = Mathf.Lerp(15f, 0f, elapsed / duration);
            float shakeAngle = Mathf.Sin(elapsed * 40f) * strength;

            // 착탄 직후 회전값을 기준으로 흔들림
            transform.rotation = impactRotation * Quaternion.Euler(0, 0, shakeAngle);

            yield return null;
        }

        // 최종적으로 착탄 직후 각도로 복귀
        transform.rotation = impactRotation;

        yield return new WaitForSeconds(5f);
        gameObject.SetActive(false);
    }
}
