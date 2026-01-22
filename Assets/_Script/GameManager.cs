using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("───── 버튼 ─────")]
    public Button getPointUpgrade;
    public Button autoGetPointUpgrade;
    public Button chargeSpeedUpgrade;
    public Button addBowUpgrade;

 
    public float getPoint = 10f;
    public float autoGetPoint = 0f;

    private float cost_getPointUpgrade = 30;
    private float cost_autoGetPointUpgrade = 200;
    private float cost_chargeSpeedUpgrade = 150;
    private float cost_addBowUpgrade = 100;

    private float factor_getPointUpgrade = 5;
    private float factor_autoGetPointUpgrade = 20;
    private float factor_chargeSpeedUpgrade = 10;
    private float factor_addBowUpgrade = 1.5f;

    // 버튼별 Cost 텍스트 캐싱
    private Text costText_getPoint;
    private Text costText_autoGetPoint;
    private Text costText_chargeSpeed;
    private Text costText_addBow;


    [Header("───── 활 추가 설정 ─────")]
    [SerializeField] private GameObject bowPrefab;     // 생성할 활 프리팹
    [SerializeField] private Collider2D areaCollider;  // 생성 범위를 지정할 Collider2D
    [SerializeField] private LayerMask bowLayer;       // 활들이 속한 레이어
    [SerializeField] private float checkRadius = 0.5f; // 겹침 검사 반경
    [SerializeField] private Transform bowParent; // 생성된 활을 넣을 부모 오브젝트


    public void AddBow()
    {
        if (bowPrefab == null || areaCollider == null)
        {
            Debug.LogWarning("BowPrefab 또는 AreaCollider가 할당되지 않았습니다.");
            return;
        }

        Bounds bounds = areaCollider.bounds;

        for (int i = 0; i < 20; i++)
        {
            Vector2 randomPos = new Vector2(
                UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                UnityEngine.Random.Range(bounds.min.y, bounds.max.y)
            );

            // 겹침 검사 완화: 완전히 겹치는 경우만 막기
            Collider2D[] overlaps = Physics2D.OverlapCircleAll(randomPos, checkRadius, bowLayer);
            bool canPlace = true;

            foreach (var col in overlaps)
            {
                float dist = Vector2.Distance(randomPos, col.transform.position);
                if (dist < checkRadius * 0.5f) // 중심이 너무 가까우면 완전 겹침으로 간주
                {
                    canPlace = false;
                    break;
                }
            }

            if (canPlace)
            {
                GameObject newBow = Instantiate(bowPrefab, randomPos, Quaternion.identity, bowParent);
                Debug.Log("활 추가 성공!");
                GlobalDelegate.Raise(new BowAdded(newBow)); // 이벤트 발생
                return;
            }

        }

        Debug.Log("활을 놓을 수 있는 위치를 찾지 못했습니다.");
    }



    private void OnEnable()
    {
        GlobalDelegate.Subscribe<HitTarget>(Hit);
    }

    private void OnDisable()
    {
        GlobalDelegate.Unsubscribe<HitTarget>(Hit);
    }

    private void Hit(HitTarget target)
    {
        Debug.Log("Hit Target! : GM");
        Money.Instance.ChangeValue(getPoint);
    }

    void Start()
    {
        // 버튼 하위에서 Cost 태그를 가진 오브젝트 자동 검색 (널 체크 포함)
        if (getPointUpgrade) costText_getPoint = getPointUpgrade.GetComponentInChildren<Text>(true);
        if (autoGetPointUpgrade) costText_autoGetPoint = autoGetPointUpgrade.GetComponentInChildren<Text>(true);
        if (chargeSpeedUpgrade) costText_chargeSpeed = chargeSpeedUpgrade.GetComponentInChildren<Text>(true);
        if (addBowUpgrade) costText_addBow = addBowUpgrade.GetComponentInChildren<Text>(true);

        // 버튼 클릭 이벤트 연결 (널 체크 포함)
        if (getPointUpgrade)
            getPointUpgrade.onClick.AddListener(() => TryUpgrade(ref cost_getPointUpgrade, factor_getPointUpgrade, costText_getPoint, () => getPoint += factor_getPointUpgrade));

        if (autoGetPointUpgrade)
            autoGetPointUpgrade.onClick.AddListener(() => TryUpgrade(ref cost_autoGetPointUpgrade, factor_autoGetPointUpgrade, costText_autoGetPoint, () => { /* 자동 점수 획득 로직 */ autoGetPoint += factor_autoGetPointUpgrade; }));

        if (chargeSpeedUpgrade)
            chargeSpeedUpgrade.onClick.AddListener(() => TryUpgrade(ref cost_chargeSpeedUpgrade, factor_chargeSpeedUpgrade, costText_chargeSpeed, () => { /* 차지 속도 증가 로직 */ GlobalDelegate.Raise(new ChargeSpeedUpgraded(0.9f)); }));

        if (addBowUpgrade)
            addBowUpgrade.onClick.AddListener(() => TryUpgrade(ref cost_addBowUpgrade, factor_addBowUpgrade, costText_addBow, () => { /* 활 추가 로직 */ AddBow(); }));

        // 초기 가격 표시
        RefreshCosts();

        StartCoroutine(AutoGetPointRoutine());
    }

    private IEnumerator AutoGetPointRoutine()
    {
        while (true)
        {
            if (autoGetPoint > 0 && Money.Instance != null) { Money.Instance.ChangeValue(autoGetPoint); }
            yield return new WaitForSeconds(1f); // 1초마다 자동 점수 획득 } }
        }
    }

    private void TryUpgrade(ref float cost, float factor, Text costText, Action onSuccess)
    {
        Debug.Log($"Attempting upgrade with cost: {cost}");
        if (Money.Instance == null)
        {
            Debug.LogError("Money.Instance is null!");
            return;
        }
        if (Money.Instance != null && Money.Instance.TrySpendMoney(cost))
        {
            // 구매 성공
            onSuccess?.Invoke();
            cost *= 1.5f; // 가격 증가
            //if (costText) costText.text = cost.ToString("0"); // UI 갱신
            if (costText) costText.text = FormatNumber(cost); // UI 갱신

        }
        else
        {
            Debug.Log("Not enough money or Money.Instance is null!");
        }
    }

    private void RefreshCosts()
    {
        //if (costText_getPoint) costText_getPoint.text = cost_getPointUpgrade.ToString("0");
        //if (costText_autoGetPoint) costText_autoGetPoint.text = cost_autoGetPointUpgrade.ToString("0");
        //if (costText_chargeSpeed) costText_chargeSpeed.text = cost_chargeSpeedUpgrade.ToString("0");
        //if (costText_addBow) costText_addBow.text = cost_addBowUpgrade.ToString("0");
        if (costText_getPoint) costText_getPoint.text = FormatNumber(cost_getPointUpgrade);
        if (costText_autoGetPoint) costText_autoGetPoint.text = FormatNumber(cost_autoGetPointUpgrade);
        if (costText_chargeSpeed) costText_chargeSpeed.text = FormatNumber(cost_chargeSpeedUpgrade);
        if (costText_addBow) costText_addBow.text = FormatNumber(cost_addBowUpgrade);

    }

    private string FormatNumber(float num)
    {
        
        num = Mathf.Floor(num);

        int suffixIndex = 0;
        while (num >= 1000f)
        {
            num /= 1000f;
            suffixIndex++;
        }

        string suffix = GetSuffix(suffixIndex);
  
        suffix = suffix.ToUpper();

        return num.ToString("0.#") + suffix;
    }

    /// <summary>
    /// 단위 문자열 생성 (aa, ab, ac …)
    /// </summary>
    private string GetSuffix(int index)
    {
        if (index == 0) return "";

        // 기본 단위: k, m, b, t
        string[] baseSuffixes = { "k", "m", "b", "t" };
        if (index <= baseSuffixes.Length)
            return baseSuffixes[index - 1];

        // 그 이상은 aa, ab, ac … 식으로 생성
        index -= baseSuffixes.Length;

        StringBuilder sb = new StringBuilder();
        while (index > 0)
        {
            index--; // 0-based
            sb.Insert(0, (char)('a' + (index % 26)));
            index /= 26;
        }

        return sb.ToString();
    }
}
