using UnityEngine;

public class Shoot : MonoBehaviour
{
    [Header("───── 충전 관련 설정 ─────")]
    [SerializeField] private float maxChargeTime = 1.5f;   // 100%까지 걸리는 시간 (초)
    [SerializeField] private float maxRadius = 5f;         // 최대 반지름
    [SerializeField] private float ringWidth = 0.12f;      // 링 두께

    [Header("───── 디버깅용 (선택) ─────")]
    [SerializeField] private bool showDebugText = false;

    private LineRenderer chargeRing;
    private float currentChargeTime = 0f;
    private bool isCharging = false;

    private Camera mainCam;

    private void Awake()
    {
        mainCam = Camera.main;

        // 단일 링 LineRenderer 생성
        GameObject ringObj = new GameObject("ChargeRing");
        ringObj.transform.parent = transform;
        chargeRing = ringObj.AddComponent<LineRenderer>();
        SetupRing(chargeRing);
        chargeRing.enabled = false;
    }

    private void SetupRing(LineRenderer lr)
    {
        lr.positionCount = 96; // 부드러운 원형
        lr.startWidth = ringWidth;
        lr.endWidth = ringWidth;
        lr.useWorldSpace = true;
        lr.loop = true;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.sortingLayerName = "Default";
        lr.sortingOrder = 5;
        lr.startColor = Color.clear;
        lr.endColor = Color.clear;
    }

    void Update()
    {
        // 마우스 위치 → 월드 좌표 변환
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        // 항상 마우스 방향 바라보기
        Vector3 direction = mouseWorldPos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // ───────────────────── 충전 로직 ─────────────────────
        if (Input.GetMouseButtonDown(0))
        {
            isCharging = true;
            currentChargeTime = 0f;
            chargeRing.enabled = true;
        }

        if (Input.GetMouseButton(0) && isCharging)
        {
            currentChargeTime += Time.deltaTime;
            currentChargeTime = Mathf.Clamp(currentChargeTime, 0f, maxChargeTime);

            float chargeRatio = currentChargeTime / maxChargeTime;
            float radius = chargeRatio * maxRadius;

            UpdateChargeRing(mouseWorldPos, radius);

            if (showDebugText)
                Debug.Log($"충전: {chargeRatio:P1} | 반지름: {radius:F2}");
        }

        if (Input.GetMouseButtonUp(0) && isCharging)
        {
            isCharging = false;
            chargeRing.enabled = false;

            float chargeRatio = currentChargeTime / maxChargeTime;
            Debug.Log($"발사 완료! 충전량: {chargeRatio:P1}");
        }
    }

    private void UpdateChargeRing(Vector3 center, float radius)
    {
        chargeRing.enabled = true;

        // 색상: 충전 비율에 따라 변화
        float ringRatio = radius / maxRadius;
        Color innerColor = new Color(1f, 1f, 0.6f, 0.3f);
        Color outerColor = new Color(0f, 0f, 0f, 0.95f);
        Color ringColor = Color.Lerp(innerColor, outerColor, ringRatio);

        chargeRing.startColor = ringColor;
        chargeRing.endColor = ringColor;

        // 링 두께를 반지름에 따라 조절
        // 예: 최소 0.02, 최대 0.12
        float dynamicWidth = Mathf.Lerp(0.02f, ringWidth, ringRatio);
        chargeRing.startWidth = dynamicWidth;
        chargeRing.endWidth = dynamicWidth;

        DrawSingleCircle(chargeRing, center, radius);
    }


    private void DrawSingleCircle(LineRenderer lr, Vector3 center, float radius)
    {
        int segments = lr.positionCount;
        float angleStep = (Mathf.PI * 2f) / segments;

        for (int j = 0; j < segments; j++)
        {
            float angle = j * angleStep;
            Vector3 point = center + new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f
            );
            lr.SetPosition(j, point);
        }
    }
}
