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
    private Vector3 clickPosition; // 클릭한 위치 저장

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
        Vector3 mouseWorldPos = LookAtMouse();

        // ───────────────────── 충전 로직 ─────────────────────
        if (Input.GetMouseButtonDown(0))
        {
            isCharging = true;
            currentChargeTime = 0f;
            chargeRing.enabled = true;

            // 클릭한 위치를 저장
            clickPosition = mouseWorldPos;

            // 커서 고정 및 숨김
            //Cursor.lockState = CursorLockMode.Locked;
            //Cursor.visible = false;
        }

        if (Input.GetMouseButton(0) && isCharging)
        {
            currentChargeTime += Time.deltaTime;
            currentChargeTime = Mathf.Clamp(currentChargeTime, 0f, maxChargeTime);

            float chargeRatio = currentChargeTime / maxChargeTime;
            float radius = chargeRatio * maxRadius;

            // 클릭 위치에 고정된 링 업데이트
            UpdateChargeRing(clickPosition, radius);

            if (showDebugText)
                Debug.Log($"충전: {chargeRatio:P1} | 반지름: {radius:F2}");
        }

        if (Input.GetMouseButtonUp(0) && isCharging)
        {
            isCharging = false;
            chargeRing.enabled = false;

            float chargeRatio = currentChargeTime / maxChargeTime;
            Debug.Log($"발사 완료! 충전량: {chargeRatio:P1}");

            // 커서 원래대로 복원
            //Cursor.lockState = CursorLockMode.None;
            //Cursor.visible = true;
        }
    }

    private Vector3 LookAtMouse()
    {
        // 충전 중일 때는 새로운 마우스 위치를 반환하지 않음
        if (isCharging)
            return clickPosition;

        // 마우스 위치 → 월드 좌표 변환
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        // 마우스 방향 바라보기
        Vector3 direction = mouseWorldPos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        return mouseWorldPos;
    }

    private void UpdateChargeRing(Vector3 center, float radius)
    {
        chargeRing.enabled = true;

        float ringRatio = radius / maxRadius;
        Color innerColor = new Color(1f, 1f, 0.6f, 0.3f);
        Color outerColor = new Color(0f, 0f, 0f, 0.95f);
        Color ringColor = Color.Lerp(innerColor, outerColor, ringRatio);

        chargeRing.startColor = ringColor;
        chargeRing.endColor = ringColor;

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
