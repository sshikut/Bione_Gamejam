using UnityEngine;

public class PlayerPush : MonoBehaviour
{
    [Header("Settings")]
    public float pushCooldown = 0.25f; // 너무 빨리 밀리지 않게 딜레이 (0.2~0.3초 추천)

    private float _lastPushTime;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Cargo hitCargo = collision.gameObject.GetComponent<Cargo>();
        if (hitCargo != null)
        {
            // "지금 막 밀었다"고 기록 -> 이제 0.25초가 지나야 밀림
            _lastPushTime = Time.time;
        }
    }

    // 플레이어가 무언가와 계속 부딪히고 있을 때 발생
    private void OnCollisionStay2D(Collision2D collision)
    {
        // 1. 쿨타임 체크 (연속으로 다다다 밀리는 것 방지)
        if (Time.time < _lastPushTime + pushCooldown) return;

        // 2. 부딪힌 게 '화물(Cargo)'인지 확인
        Cargo hitCargo = collision.gameObject.GetComponent<Cargo>();
        if (hitCargo != null)
        {
            TryPushCargo(hitCargo);
        }
    }

    public void TryPushCargo(Cargo cargo)
    {
        if (Time.time < _lastPushTime + pushCooldown) return;

        // ★ [핵심 변경] 위치 차이가 아니라, 플레이어의 "입력 방향"을 사용합니다.
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        // 입력이 없으면(가만히 있는데 부딪히면) 밀지 않음
        if (x == 0 && y == 0) return;

        if (x != 0 && y != 0) // 대각선 체크
        {
            return;
        }
        // 대각선 입력 방지: 더 강하게 누른 쪽만 선택
        Vector2Int pushDir = Vector2Int.zero;
        if (Mathf.Abs(x) > Mathf.Abs(y))
            pushDir = new Vector2Int((x > 0) ? 1 : -1, 0); // 좌우 이동 중
        else
            pushDir = new Vector2Int(0, (y > 0) ? 1 : -1); // 상하 이동 중

        // --- 이하 로직은 동일 ---

        Vector2Int currentCargoPos = GridManager.Instance.WorldToGrid(cargo.transform.position);
        Vector2Int targetPos = currentCargoPos + pushDir;

        if (GridManager.Instance.IsValidGridPosition(targetPos) &&
            !GridManager.Instance.IsOccupied(targetPos))
        {
            GridManager.Instance.UnregisterCargo(currentCargoPos);
            GridManager.Instance.RegisterCargo(targetPos, cargo);

            cargo.MoveByPush(targetPos);

            _lastPushTime = Time.time;
        }
    }

    public void ResetPushTimer()
    {
        _lastPushTime = Time.time;
    }
}