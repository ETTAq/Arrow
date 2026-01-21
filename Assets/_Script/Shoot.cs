using UnityEngine;

public class Shoot : MonoBehaviour
{
    [Header("───── 충전 관련 설정 ─────")]
    [SerializeField] private float maxChargeTime = 1.5f;
    [SerializeField] private float maxRadius = 5f;
    [SerializeField] private float ringWidth = 0.12f;

    [Header("───── 디버깅용 (선택) ─────")]
    [SerializeField] private bool showDebugText = false;

    [Header("───── Bow 스프라이트 설정 ─────")]
    [SerializeField] private Sprite normalBowSprite;       // 기본 Bow 스프라이트
    [SerializeField] private Sprite chargingBowSprite;     // 0~50% 충전
    [SerializeField] private Sprite halfChargedBowSprite;  // 50% 이상 ~ 99%
    [SerializeField] private Sprite fullyChargedBowSprite; // 100% 충전

    [Header("───── 프리팹 ─────")]
    public GameObject arrowPref;

    private LineRenderer chargeRing;
    private float currentChargeTime = 0f;
    private bool isCharging = false;

    private Camera mainCam;
    private Vector3 clickPosition;

    private void Awake()
    {
        mainCam = Camera.main;

        GameObject ringObj = new GameObject("ChargeRing");
        ringObj.transform.parent = transform;
        chargeRing = ringObj.AddComponent<LineRenderer>();
        SetupRing(chargeRing);
        chargeRing.enabled = false;
    }

    private void SetupRing(LineRenderer lr)
    {
        lr.positionCount = 96;
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

        if (Input.GetMouseButtonDown(0))
        {
            isCharging = true;
            currentChargeTime = 0f;
            chargeRing.enabled = true;
            clickPosition = mouseWorldPos;

            ChangeBowSprites(chargingBowSprite);
        }

        if (Input.GetMouseButton(0) && isCharging)
        {
            currentChargeTime += Time.deltaTime;
            currentChargeTime = Mathf.Clamp(currentChargeTime, 0f, maxChargeTime);

            float chargeRatio = currentChargeTime / maxChargeTime;
            float radius = chargeRatio * maxRadius;

            UpdateChargeRing(clickPosition, radius);

            // ───── 차징 비율에 따른 Bow 스프라이트 변경 ─────
            if (chargeRatio >= 1f)
            {
                ChangeBowSprites(fullyChargedBowSprite);
            }
            else if (chargeRatio >= 0.5f)
            {
                ChangeBowSprites(halfChargedBowSprite);
            }
            else
            {
                ChangeBowSprites(chargingBowSprite);
            }

            if (showDebugText)
                Debug.Log($"충전: {chargeRatio:P1} | 반지름: {radius:F2}");
        }

        if (Input.GetMouseButtonUp(0) && isCharging)
        {
            isCharging = false;
            chargeRing.enabled = false;

            float chargeRatio = currentChargeTime / maxChargeTime;
            float power = chargeRatio * 20f; // 튜닝값

            // Bow 태그 자식 위치마다 화살 생성
            SpriteRenderer[] childRenderers = GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in childRenderers)
            {
                if (sr.CompareTag("Bow"))
                {
                    GameObject arrowObj = Instantiate(arrowPref.gameObject, sr.transform.position, transform.rotation);

                    Arrow arrowScript = arrowObj.GetComponent<Arrow>();
                    if (arrowScript != null)
                    {
                        Vector2 dir = transform.right;

                        // 현재 활의 방향 각도
                        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                        // 위로 올릴 각도
                        float angleOffset = 5f;

                        // 최대 허용 각도
                        float maxAngle = 60f;

                        // 최종 각도 = 기본 각도 + 오프셋, 단 최대 각도 제한
                        float finalAngle = Mathf.Clamp(baseAngle + angleOffset, -maxAngle, maxAngle);

                        // 최종 방향 벡터 계산
                        float rad = finalAngle * Mathf.Deg2Rad;
                        Vector2 finalDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

                        arrowScript.Launch(finalDir.normalized, power);
                    }

                }
            }

            ChangeBowSprites(normalBowSprite);
        }


    }

    private Vector3 LookAtMouse()
    {
        if (isCharging)
            return clickPosition;

        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

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

    private void ChangeBowSprites(Sprite newSprite)
    {
        SpriteRenderer[] childRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in childRenderers)
        {
            if (sr.CompareTag("Bow"))
            {
                sr.sprite = newSprite;
            }
        }
    }
}
