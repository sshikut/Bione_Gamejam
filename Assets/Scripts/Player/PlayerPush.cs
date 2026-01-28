using UnityEngine;

public class PlayerPush : MonoBehaviour
{
    [Header("Settings")]
    public float pushCooldown = 0.25f;

    private float _lastPushTime;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Cargo hitCargo = collision.gameObject.GetComponent<Cargo>();
        if (hitCargo != null)
        {
            _lastPushTime = Time.time;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (Time.time < _lastPushTime + pushCooldown) return;

        Cargo hitCargo = collision.gameObject.GetComponent<Cargo>();
        if (hitCargo != null)
        {
            TryPushCargo(hitCargo);
        }
    }

    public void TryPushCargo(Cargo cargo)
    {
        if (Time.time < _lastPushTime + pushCooldown) return;

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        if (x == 0 && y == 0) return;

        if (x != 0 && y != 0) return; // 대각선 무시

        Vector2Int pushDir = Vector2Int.zero;
        if (Mathf.Abs(x) > Mathf.Abs(y))
            pushDir = new Vector2Int((x > 0) ? 1 : -1, 0);
        else
            pushDir = new Vector2Int(0, (y > 0) ? 1 : -1);

        // --- 위치 계산 ---
        Vector2Int currentCargoPos = GridManager.Instance.WorldToGrid(cargo.transform.position);
        Vector2Int targetPos = currentCargoPos + pushDir;

        // [핵심 변경] 이동 조건에 'IsDropAllowed' 추가!
        // 1. 맵 안쪽인가? (IsValid)
        // 2. 빈 땅인가? (!IsOccupied)
        // 3. 금지 구역이 아닌가? (IsDropAllowed)
        bool isMoveable = GridManager.Instance.IsValidGridPosition(targetPos) &&
                          !GridManager.Instance.IsOccupied(targetPos) &&
                          GridManager.Instance.IsDropAllowed(targetPos); // <--- 여기가 핵심

        if (isMoveable)
        {
            // 성공 로직
            GridManager.Instance.UnregisterCargo(currentCargoPos);
            GridManager.Instance.RegisterCargo(targetPos, cargo);

            cargo.MoveByPush(targetPos);

            _lastPushTime = Time.time;
        }
        else
        {
            // 실패 로직 (디버깅용)
            // 막힌 이유가 금지 구역 때문인지 확인 가능
            if (!GridManager.Instance.IsDropAllowed(targetPos))
            {
                Debug.Log("그쪽(컨베이어/금지구역)으로는 밀 수 없습니다!");
            }
        }
    }

    public void ResetPushTimer()
    {
        _lastPushTime = Time.time;
    }
}