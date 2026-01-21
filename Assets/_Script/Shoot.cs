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
    [SerializeField] private Sprite normalBowSprite;
    [SerializeField] private Sprite chargingBowSprite;
    [SerializeField] private Sprite halfChargedBowSprite;
    [SerializeField] private Sprite fullyChargedBowSprite;

    [Header("───── 프리팹 및 머티리얼 ─────")]
    [SerializeField] private GameObject arrowPref;
    [SerializeField] private Material ringMaterial; // 1번 개선: 캐싱된 머티리얼 사용

    private LineRenderer chargeRing;
    private float currentChargeTime = 0f;
    private bool isCharging = false;

    private Camera mainCam;
    private Vector3 clickPosition;

    // 2번 개선: Bow SpriteRenderer 캐싱
    private SpriteRenderer[] bowRenderers;

    private void Awake()
    {
        mainCam = Camera.main;

        GameObject ringObj = new GameObject("ChargeRing");
        ringObj.transform.parent = transform;
        chargeRing = ringObj.AddComponent<LineRenderer>();
        SetupRing(chargeRing);
        chargeRing.enabled = false;

        bowRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    private void SetupRing(LineRenderer lr)
    {
        lr.positionCount = 96;
        lr.startWidth = ringWidth;
        lr.endWidth = ringWidth;
        lr.useWorldSpace = true;
        lr.loop = true;
        lr.material = ringMaterial; // 1번 개선
        lr.sortingLayerName = "Default";
        lr.sortingOrder = 5;

        // 4번 개선: Gradient 활용
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 1f, 0.6f), 0f),
                new GradientColorKey(new Color(0f, 0f, 0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.3f, 0f),
                new GradientAlphaKey(0.95f, 1f)
            }
        );
        lr.colorGradient = gradient;
    }

    void Update()
    {
        Vector3 mouseWorldPos = LookAtMouse();

        if (Input.GetMouseButtonDown(0))
            StartCharging(mouseWorldPos);

        if (Input.GetMouseButton(0) && isCharging)
            HandleCharging();

        if (Input.GetMouseButtonUp(0) && isCharging)
            HandleRelease();
    }

    private void StartCharging(Vector3 mouseWorldPos)
    {
        isCharging = true;
        currentChargeTime = 0f;
        chargeRing.enabled = true;
        clickPosition = mouseWorldPos;

        ChangeBowSprites(chargingBowSprite);
    }

    private void HandleCharging()
    {
        currentChargeTime += Time.deltaTime;
        currentChargeTime = Mathf.Clamp(currentChargeTime, 0f, maxChargeTime);

        float chargeRatio = currentChargeTime / maxChargeTime;
        float radius = chargeRatio * maxRadius;

        UpdateChargeRing(clickPosition, radius);

        if (chargeRatio >= 1f)
            ChangeBowSprites(fullyChargedBowSprite);
        else if (chargeRatio >= 0.5f)
            ChangeBowSprites(halfChargedBowSprite);
        else
            ChangeBowSprites(chargingBowSprite);

        if (showDebugText)
            Debug.Log($"충전: {chargeRatio:P1} | 반지름: {radius:F2}");
    }

    private void HandleRelease()
    {
        isCharging = false;
        chargeRing.enabled = false;

        float chargeRatio = currentChargeTime / maxChargeTime;
        float power = chargeRatio * 20f;

        foreach (var sr in bowRenderers)
        {
            if (sr.CompareTag("Bow"))
            {
                GameObject arrowObj = Instantiate(arrowPref, sr.transform.position, transform.rotation);

                Arrow arrowScript = arrowObj.GetComponent<Arrow>();
                if (arrowScript != null)
                {
                    // 3번 개선: transform.rotation 직접 활용
                    Vector2 dir = transform.right;
                    float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                    float angleOffset = 5f + Random.Range(-1f, 1f);
                    if (baseAngle > 90f || baseAngle < -90f)
                        angleOffset = -angleOffset;

                    float finalAngle = baseAngle + angleOffset;
                    float rad = finalAngle * Mathf.Deg2Rad;
                    Vector2 finalDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

                    power *= Random.Range(0.95f, 1.05f);
                    arrowScript.Launch(finalDir.normalized, power);
                }
            }
        }

        ChangeBowSprites(normalBowSprite);
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
        foreach (var sr in bowRenderers)
        {
            if (sr.CompareTag("Bow"))
                sr.sprite = newSprite;
        }
    }
}
