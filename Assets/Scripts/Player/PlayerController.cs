using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    private Rigidbody2D _rb;
    private Vector2 _inputDir;
    private Vector2Int _currentGridPos; // 현재 캐릭터가 서 있는 그리드 좌표

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        // 중력 영향 안 받게 설정 (코드에서 강제 설정)
        _rb.gravityScale = 0;
        _rb.freezeRotation = true;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
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
                Debug.Log($"현재 위치: {_currentGridPos}"); 
            }
        }
    }

    void FixedUpdate()
    {
        // 물리를 이용한 이동 (벽 비비기 가능)
        _rb.MovePosition(_rb.position + _inputDir * moveSpeed * Time.fixedDeltaTime);

        // 바라보는 방향 회전 (선택 사항)

        /*
        if (_inputDir != Vector2.zero)
        {
            float angle = Mathf.Atan2(_inputDir.y, _inputDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        */
    }
}