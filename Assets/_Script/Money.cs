using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;

public class Money : MonoBehaviour
{
    [Header("Target Text")]
    public Text targetText;

    [Header("Options")]
    public bool useInteger = true;       // 소수점 제거 여부
    public bool useSuffix = true;        // 단위 변환 여부
    public bool autoUpdate = true;       // 값 변경 시 자동 반영 여부
    public bool useUpperCaseSuffix = false; // 단위를 대문자로 표기할지 여부

    [Header("Value")]
    [SerializeField] private float value = 0f;

    [Header("Animation")]
    public AnimationCurve easeCurve = AnimationCurve.Linear(0, 0, 1, 1);

    private Coroutine animationCoroutine;

    private void Update()
    {
        if (autoUpdate)
            UpdateText();
    }

    public void SetValue(float newValue)
    {
        value = newValue;
        if (autoUpdate) UpdateText();
    }

    public void AddValue(float delta)
    {
        value += delta;
        if (autoUpdate) UpdateText();
    }

    public void AnimateToValue(float targetValue, float duration = 1f)
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(AnimateValueRoutine(targetValue, duration));
    }

    private IEnumerator AnimateValueRoutine(float targetValue, float duration)
    {
        float startValue = value;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float easedT = easeCurve.Evaluate(t);
            value = Mathf.Lerp(startValue, targetValue, easedT);

            UpdateText();
            yield return null;
        }

        value = targetValue;
        UpdateText();
        animationCoroutine = null;
    }

    public void UpdateText()
    {
        if (targetText == null) return;

        string formatted = FormatNumber(value);
        targetText.text = formatted;
    }

    private string FormatNumber(float num)
    {
        if (useInteger)
            num = Mathf.Floor(num);

        if (useSuffix)
        {
            int suffixIndex = 0;
            while (num >= 1000f)
            {
                num /= 1000f;
                suffixIndex++;
            }

            string suffix = GetSuffix(suffixIndex);
            if (useUpperCaseSuffix)
                suffix = suffix.ToUpper();

            return num.ToString("0.#") + suffix;
        }

        return num.ToString(useInteger ? "0" : "0.##");
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
