using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Snapping")]
    public float snapSpeed = 10f;
    public float snapThreshold = 0.01f;

    private Rigidbody2D _rb;
    private Vector2 _inputDir;
    private Vector2Int _currentGridPos; // 현재 캐릭터가 서 있는 그리드 좌표

    private PlayerStats _stats;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        // 중력 영향 안 받게 설정 (코드에서 강제 설정)
        _rb.gravityScale = 0;
        _rb.freezeRotation = true;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        _stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        // 입력 받기 (GetAxisRaw는 즉시 반응, GetAxis는 관성 있음)
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        _inputDir = new Vector2(x, y).normalized;

        // 디버깅: 현재 내가 어느 그리드 위에 있는지 로그 찍기
        if (GridManager.Instance != null)
        {
            Vector2Int gridPos = GridManager.Instance.WorldToGrid(transform.position);
            if (gridPos != _currentGridPos)
            {
                _currentGridPos = gridPos;
            }
        }
    }

    void FixedUpdate()
    {
        // 물리를 이용한 이동 (벽 비비기 가능)
        if (_inputDir != Vector2.zero)
        {
            float currentSpeed = _stats.CurrentMoveSpeed;

            _rb.MovePosition(_rb.position + _inputDir * currentSpeed * Time.fixedDeltaTime);
        }
        else
        {
            SnapToGrid();
        }

        // 바라보는 방향 회전 (선택 사항)
        /*
        if (_inputDir != Vector2.zero)
        {
            float angle = Mathf.Atan2(_inputDir.y, _inputDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        */
    }

    void SnapToGrid()
    {
        // 현재 내 위치 기준 가장 가까운 그리드 좌표 찾기
        Vector2Int closestGridPos = GridManager.Instance.WorldToGrid(transform.position);

        // 그 그리드의 정중앙 좌표(목표점) 가져오기
        Vector2 targetPos = GridManager.Instance.GridToWorldCenter(closestGridPos);

        // 현재 위치와 목표점 사이의 거리 계산
        float distance = Vector2.Distance(_rb.position, targetPos);

        // 아직 목표점에 도착하지 않았다면 부드럽게 이동 (Lerp)
        if (distance > snapThreshold)
        {
            // Lerp를 사용하여 부드럽게 감속하며 이동
            Vector2 smoothPos = Vector2.Lerp(_rb.position, targetPos, Time.fixedDeltaTime * snapSpeed);
            _rb.MovePosition(smoothPos);
        }
        // 목표점에 거의 도착했다면 확실하게 고정 (미세한 떨림 방지)
        else
        {
            // 이미 거의 중앙이라면 연산을 멈추거나 미세 조정만 수행
            // _rb.MovePosition(targetPos); // 완전 고정을 원하면 주석 해제 (딱딱할 수 있음)
        }
    }
}