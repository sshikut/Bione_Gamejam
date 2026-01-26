using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cargo : MonoBehaviour
{
    public Vector2Int CurrentGridPos; // 현재 내가 있는 그리드 좌표

    private bool _isCarried = false;
    private Vector3 _targetPosition;
    private float _moveSpeed = 15f;

    private Collider2D _myCollider;
    private Vector3 _targetLocalPos;

    private void Awake()
    {
        _myCollider = GetComponent<Collider2D>();
    }

    // 초기화 시 그리드에 자신을 등록 (선택 사항)
    void Start()
    {
        // 게임 시작 시 이미 맵에 배치된 화물이라면 등록
        Vector2Int startPos = GridManager.Instance.WorldToGrid(transform.position);

        // 위치를 그리드 중앙으로 강제 스냅(Snap)
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
            // 플레이어 손(Local 0,0)으로 부드럽게 이동
            transform.localPosition = Vector3.Lerp(transform.localPosition, _targetLocalPos, Time.deltaTime * _moveSpeed);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, Time.deltaTime * _moveSpeed);
        }
        // 2. 바닥에 놓여 있을 때 (또는 놓여지는 중일 때)
        else
        {
            // ★ 목표 위치(_targetPosition)로 부드럽게 이동 (스르륵~)
            transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * _moveSpeed);
        }
    }

    // 외부(플레이어)에서 호출할 함수: 집기
    public void OnPickedUp(Transform playerTransform, Collider2D playerCollider)
    {
        _isCarried = true;
        transform.SetParent(playerTransform);

        Physics2D.IgnoreCollision(playerCollider, _myCollider, true);

        Vector3 offset = transform.position - playerTransform.position;
        _targetLocalPos = new Vector3(Mathf.Round(offset.x), Mathf.Round(offset.y), 0);
        // [Juice 효과 1] 집는 순간 살짝 작아졌다가 커지는 느낌 (스케일 연출)
        // (이건 Coroutine이나 Tween 라이브러리가 필요하지만 일단 생략)
    }

    public void OnDropped(Vector3 dropPosition, Vector2Int newGridPos, Collider2D playerCollider)
    {
        _isCarried = false;
        transform.SetParent(null); // 부모 해제

        // 놓을 때도 스냅하지 않고, 목표 위치만 정해두고 Update에서 이동시킬 수도 있음
        // 하지만 정확성을 위해 놓을 때는 스냅을 추천합니다. (윌못도 놓을 땐 꽤 빠릅니다)
        _targetPosition = dropPosition;

        // 회전값 초기화 (깔끔하게 정렬)
        transform.rotation = Quaternion.identity;
        CurrentGridPos = newGridPos;

        Physics2D.IgnoreCollision(playerCollider, _myCollider, false);
    }
}
