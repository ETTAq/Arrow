using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

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

    [Header("───── 프리팹 ─────")]
    [SerializeField] private GameObject arrowPref;

    private LineRenderer chargeRing;
    private float currentChargeTime = 0f;
    private bool isCharging = false;

    private Camera mainCam;
    private Vector3 clickPosition;

    private SpriteRenderer[] bowRenderers;

    private void Awake()
    {
        mainCam = Camera.main;

        // 🔥 충전 링 오브젝트 생성 및 LineRenderer 추가
        GameObject ringObj = new GameObject("ChargeRing");
        ringObj.transform.parent = transform;
        chargeRing = ringObj.AddComponent<LineRenderer>();
        SetupRing(chargeRing);
        chargeRing.enabled = false;

        bowRenderers = GetComponentsInChildren<SpriteRenderer>();
    }
    private void OnEnable()
    {
        GlobalDelegate.Subscribe<BowAdded>(OnBowAdded);
        GlobalDelegate.Subscribe<ChargeSpeedUpgraded>(OnChargeSpeedUpgraded);
    }

    private void OnDisable()
    {
        GlobalDelegate.Unsubscribe<BowAdded>(OnBowAdded);
        GlobalDelegate.Unsubscribe<ChargeSpeedUpgraded>(OnChargeSpeedUpgraded);
    }

    private void OnChargeSpeedUpgraded(ChargeSpeedUpgraded upgraded)
    {
        maxChargeTime *= upgraded.factor; // 충전 시간 감소 → 속도 증가
    }

    private void OnBowAdded(BowAdded evt)
    {
        // 새 활을 기존 활들과 동일하게 동작하도록 처리
        bowRenderers = GetComponentsInChildren<SpriteRenderer>();

        // 현재 활 방향과 동일하게 회전
        evt.bowObj.transform.rotation = transform.rotation;
    }



    private void SetupRing(LineRenderer lr)
    {
        lr.positionCount = 96; // 원형 세그먼트 수
        lr.startWidth = ringWidth;
        lr.endWidth = ringWidth;
        lr.useWorldSpace = true;
        lr.loop = true;

        // 🔥 기본 Sprite Shader 사용
        lr.material = new Material(Shader.Find("Sprites/Default"));

        // 🔥 항상 위에 보이도록 레이어/순서 설정
        lr.sortingLayerName = "Default";
        lr.sortingOrder = 5;

        // 초기 색상은 투명
        lr.startColor = Color.clear;
        lr.endColor = Color.clear;
    }

    void Update()
    {
        Vector3 mouseWorldPos = LookAtMouse();

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

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
        float power = chargeRatio * 35f;

        // 활들을 순차적으로 발사하는 코루틴 시작
        StartCoroutine(FireArrowsWithExpandingDelay(power));

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

        // 🔥 충전 비율에 따라 색상 그라데이션
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
        foreach (var sr in bowRenderers)
        {
            if (sr.CompareTag("Bow"))
                sr.sprite = newSprite;
        }
    }

    [SerializeField] private float maxArrowDelay = 0.5f; // 활 발사 최대 딜레이 (초)

    private IEnumerator FireArrowsWithExpandingDelay(float power)
    {
        int bowIndex = 0;
        foreach (var sr in bowRenderers)
        {
            if (sr.CompareTag("Bow"))
            {
                if (bowIndex == 0)
                {
                    // 첫 활은 즉시 발사
                    FireSingleArrow(sr, power);
                }
                else
                {
                    // 활이 뒤로 갈수록 랜덤 딜레이 범위를 넓힘
                    float minDelay = 0.05f * bowIndex;
                    float maxDelay = 0.15f * bowIndex;

                    // 최대 딜레이 제한 적용
                    maxDelay = Mathf.Min(maxDelay, maxArrowDelay);
                    minDelay = Mathf.Min(minDelay, maxArrowDelay);

                    float delay = Random.Range(minDelay, maxDelay);
                    yield return new WaitForSeconds(delay);

                    FireSingleArrow(sr, power);
                }

                bowIndex++;
            }
        }
    }



    private void FireSingleArrow(SpriteRenderer sr, float power)
    {
        GameObject arrowObj = Instantiate(arrowPref, sr.transform.position, sr.transform.rotation);

        Arrow arrowScript = arrowObj.GetComponent<Arrow>();
        if (arrowScript != null)
        {
            Vector2 dir = sr.transform.right;
            float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            float angleOffset = 5f + Random.Range(-0.65f, 0.65f);
            if (baseAngle > 90f || baseAngle < -90f)
                angleOffset = -angleOffset;

            float finalAngle = baseAngle + angleOffset;
            float rad = finalAngle * Mathf.Deg2Rad;
            Vector2 finalDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            float finalPower = power * Random.Range(0.96f, 1.04f);
            arrowScript.Launch(finalDir.normalized, finalPower);
        }
    }



}
