using UnityEngine;

public class Shoot : MonoBehaviour
{
    void Update()
    {
        // 마우스 위치를 월드 좌표로 변환
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Z축은 필요 없으므로 0으로 고정
        mousePosition.z = 0f;

        // 방향 벡터 계산
        Vector3 direction = mousePosition - transform.position;

        // 각도 계산 (라디안 → 도)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 오브젝트 회전 적용 (Z축 기준)
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }
}
