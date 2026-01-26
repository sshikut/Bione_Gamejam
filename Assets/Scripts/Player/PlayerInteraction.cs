using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Settings")]
    public Transform holdPoint; // 물건을 들었을 때 위치할 부모 (플레이어 자식)

    private Collider2D _playerCollider;
    private Cargo _holdingCargo = null; // 현재 들고 있는 화물
    private Vector2 _lastMoveDir = Vector2.right; // 마지막으로 바라본 방향

    private void Start()
    {
        _playerCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        // 1. 방향 체크 (마지막 이동 방향 기억)
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        if (x != 0 || y != 0)
        {
            _lastMoveDir = new Vector2(x, y).normalized;
        }

        // 2. 상호작용 키 (스페이스바)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_holdingCargo == null)
            {
                TryPickUp();
            }
            else
            {
                TryDrop();
            }
        }
    }

    // 물건 집기 시도
    void TryPickUp()
    {
        // 내 위치 + 바라보는 방향 1칸 앞
        Vector2Int myGridPos = GridManager.Instance.WorldToGrid(transform.position);
        Vector2Int targetPos = myGridPos + Vector2Int.RoundToInt(_lastMoveDir);

        Cargo targetCargo = GridManager.Instance.GetCargoAt(targetPos);

        if (targetCargo != null)
        {
            GridManager.Instance.UnregisterCargo(targetPos);

            _holdingCargo = targetCargo;
            targetCargo.OnPickedUp(transform, _playerCollider);
        }
        else
        {
            Debug.Log("앞에 물건이 없습니다.");
        }
    }

    // 물건 놓기 시도
    void TryDrop()
    {
        // 내 위치 + 바라보는 방향 1칸 앞
        Vector2Int boxGridPos = GridManager.Instance.WorldToGrid(_holdingCargo.transform.position);

        if (GridManager.Instance.IsValidGridPosition(boxGridPos) &&
            !GridManager.Instance.IsOccupied(boxGridPos))
        {
            Vector3 snapPos = GridManager.Instance.GridToWorldCenter(boxGridPos);

            _holdingCargo.OnDropped(snapPos, boxGridPos, _playerCollider);

            GridManager.Instance.RegisterCargo(boxGridPos, _holdingCargo);

            _holdingCargo = null;
        }
        else
        {
            Debug.Log($"거기({boxGridPos})에는 놓을 수 없습니다.");
        }
    }

    // 디버깅: 바라보는 방향 표시
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)_lastMoveDir);
    }
}