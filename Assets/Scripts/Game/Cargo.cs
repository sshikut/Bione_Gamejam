using UnityEngine;

public class Cargo : MonoBehaviour
{
    [Header("Components")]
    public CargoProperty property;

    [Header("Status")]
    public Vector2Int CurrentGridPos; // 현재 내가 있는 그리드 좌표
    public bool isHeld = false;

    [Header("Settings")]
    [SerializeField] private float _moveSpeed = 15f;

    // 내부 상태 변수
    private bool _isCarried = false;
    private Vector3 _targetPosition;    // 바닥에 있을 때 목표 월드 좌표
    private Vector3 _targetLocalPos;    // 들려 있을 때 목표 로컬 좌표 (오프셋)

    // 컴포넌트 캐싱
    private Collider2D _myCollider;
    private PlayerPush _ownerPushScript; // 나를 들고 있는 플레이어의 밀기 스크립트

    public bool IsCarried => _isCarried; // 외부에서 확인용 프로퍼티

    private void Awake()
    {
        _myCollider = GetComponent<Collider2D>();
        if (property == null) property = GetComponent<CargoProperty>();
    }

    void Start()
    {
        // 게임 시작 시 초기화 및 장부 등록
        Vector2Int startPos = GridManager.Instance.WorldToGrid(transform.position);

        // 위치를 그리드 중앙으로 강제 스냅
        _targetPosition = GridManager.Instance.GridToWorldCenter(startPos);
        transform.position = _targetPosition;

        // 매니저에 등록
        GridManager.Instance.RegisterCargo(startPos, this);
    }

    void Update()
    {
        // 1. 플레이어가 들고 있을 때
        if (_isCarried)
        {
            // 플레이어 기준 상대 좌표(_targetLocalPos)로 부드럽게 이동
            transform.localPosition = Vector3.Lerp(transform.localPosition, _targetLocalPos, Time.deltaTime * _moveSpeed);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, Time.deltaTime * _moveSpeed);
        }
        // 2. 바닥에 놓여 있을 때 (또는 밀리는 중일 때)
        else
        {
            // 월드 목표 좌표(_targetPosition)로 부드럽게 이동
            transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * _moveSpeed);
        }
    }

    public ItemData data
    {
        get
        {
            // 내 오브젝트에 붙어있는 CargoProperty를 찾아서 data를 리턴
            CargoProperty property = GetComponent<CargoProperty>();
            if (property != null)
            {
                return property.data;
            }
            return null; // 데이터가 없으면 null
        }
    }

    // --- [상호작용 함수들] ---

    // 플레이어가 이 화물을 집었을 때 호출
    public void OnPickedUp(Transform playerTransform, Collider2D playerCollider)
    {
        _isCarried = true;
        transform.SetParent(playerTransform);

        // 1. 플레이어와 충돌 무시 (튕겨나감 방지)
        Physics2D.IgnoreCollision(playerCollider, _myCollider, true);

        // 2. 현재 위치 유지 (플레이어 기준 오프셋 계산 및 정수 스냅)
        Vector3 offset = transform.position - playerTransform.position;
        _targetLocalPos = new Vector3(Mathf.Round(offset.x), Mathf.Round(offset.y), 0);

        // 3. ★ 중요: 플레이어의 밀기 기능(PlayerPush)을 빌려옴 (대리 밀기용)
        _ownerPushScript = playerTransform.GetComponent<PlayerPush>();

        isHeld = true;
    }

    // 플레이어가 이 화물을 내려놓았을 때 호출
    public void OnDropped(Vector3 dropPosition, Vector2Int newGridPos, Collider2D playerCollider)
    {
        _isCarried = false;
        transform.SetParent(null);

        // 1. 목표 위치 설정
        _targetPosition = dropPosition;
        CurrentGridPos = newGridPos;

        // 2. 회전 초기화
        transform.rotation = Quaternion.identity;

        // 3. 충돌 무시 해제
        Physics2D.IgnoreCollision(playerCollider, _myCollider, false);

        // 4. ★ 중요: 주인님 정보 초기화
        _ownerPushScript = null;

        isHeld = false;
    }

    // 외부(PlayerPush)에서 이 화물을 밀 때 호출
    public bool MoveByPush(Vector2Int newGridPos)
    {
        // 들려있는 상태면 밀 수 없음
        if (_isCarried) return false;

        CurrentGridPos = newGridPos;

        // 목표 위치 갱신 -> Update에서 Lerp로 이동
        _targetPosition = GridManager.Instance.GridToWorldCenter(newGridPos);

        return true;
    }

    // --- [물리 충돌 처리 (대리 밀기)] ---

    // 1. 처음 닿았을 때: 즉시 밀림 방지 (관성 쿨타임 리셋)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 내가 들려있고 && 주인이 있고 && 부딪힌게 바닥에 있는 화물이면
        if (_isCarried && _ownerPushScript != null)
        {
            Cargo hitCargo = collision.gameObject.GetComponent<Cargo>();
            if (hitCargo != null && !hitCargo._isCarried)
            {
                // 주인님의 쿨타임을 리셋시켜서 0.25초 대기하게 만듦
                _ownerPushScript.ResetPushTimer();
            }
        }
    }

    // 2. 계속 닿고 있을 때: 실제로 밀기 시도
    private void OnCollisionStay2D(Collision2D collision)
    {
        // 주인이 없으면 실행 안 함
        if (!_isCarried || _ownerPushScript == null) return;

        Cargo hitCargo = collision.gameObject.GetComponent<Cargo>();

        // 바닥에 있는 화물과 부딪혔다면
        if (hitCargo != null && !hitCargo._isCarried)
        {
            // ★ 주인님의 힘을 빌려 민다 (플레이어의 입력 방향대로 밀림)
            _ownerPushScript.TryPushCargo(hitCargo);
        }
    }
}