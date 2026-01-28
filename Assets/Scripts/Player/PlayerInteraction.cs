using UnityEngine;
using System.Collections.Generic; // 리스트 사용을 위해 필수

public class PlayerInteraction : MonoBehaviour
{
    [Header("Settings")]
    public float interactionRange = 1.5f; // 인접 판정 거리 (그리드 1칸이 1.0이므로 1.5면 넉넉함)

    private Collider2D _playerCollider;

    // ★ [핵심 변경] 여러 개를 들기 위해 리스트로 변경
    private List<Cargo> _holdingCargos = new List<Cargo>();

    void Start()
    {
        _playerCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        // 1. 마우스 왼쪽 클릭: 물건 집기 / 놓기 (토글)
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }

        // 2. 스페이스바: 들고 있는 모든 물건 내려놓기 (일괄 하차)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DropAllCargos();
        }
    }

    public List<Cargo> GetHoldingCargos()
    {
        return _holdingCargos;
    }

    public void OnItemSold(Cargo soldCargo)
    {
        if (_holdingCargos.Contains(soldCargo))
        {
            // 1. 리스트에서 제거 (장부 지우기)
            _holdingCargos.Remove(soldCargo);

            // 2. 실제 오브젝트 파괴
            Destroy(soldCargo.gameObject);

            Debug.Log("플레이어: 물건 판매 완료! (리스트에서 제거됨)");
        }
    }

    // 마우스 클릭 처리
    void HandleMouseClick()
    {
        // 마우스 화면 좌표 -> 월드 좌표 변환
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int clickedGridPos = GridManager.Instance.WorldToGrid(mousePos);

        // 클릭한 곳에 화물이 있는가?
        Cargo clickedCargo = GridManager.Instance.GetCargoAt(clickedGridPos);

        if (clickedCargo != null)
        {
            // A. 이미 내가 들고 있는 화물을 클릭했다면? -> 놓기 (선택적 해제)
            if (_holdingCargos.Contains(clickedCargo))
            {
                DropSpecificCargo(clickedCargo);
            }
            // B. 바닥에 있는 화물을 클릭했다면? -> 집기 시도
            else
            {
                TryPickUp(clickedCargo);
            }
        }
    }

    // 집기 로직
    void TryPickUp(Cargo cargo)
    {
        // 1. 거리가 가까운지 확인 (인접 체크)
        // 플레이어 중심과 화물 중심 사이의 거리 계산
        float distance = Vector3.Distance(transform.position, cargo.transform.position);

        if (distance <= interactionRange)
        {
            // 장부에서 제거
            GridManager.Instance.UnregisterCargo(cargo.CurrentGridPos);

            // 리스트에 추가
            _holdingCargos.Add(cargo);

            // 화물에게 "나한테 붙어!" 명령 (이전 코드 활용)
            // Cargo 스크립트가 offset을 계산해서 알아서 플레이어 주변에 붙음
            cargo.OnPickedUp(transform, _playerCollider);
        }
        else
        {
            Debug.Log("너무 멉니다! 가까이 가서 클릭하세요.");
        }
    }

    // 특정 화물 하나만 놓기
    void DropSpecificCargo(Cargo cargo)
    {
        // 현재 화물 위치의 그리드 좌표
        Vector2Int currentGridPos = GridManager.Instance.WorldToGrid(cargo.transform.position);

        // 내려놓기 가능 조건 검사:
        // 1. 그리드 범위 내인가? (IsValidGridPosition)
        // 2. 이미 다른 화물이 있지 않은가? (!IsOccupied)
        // 3. ★ [추가] 금지 구역(컨베이어 벨트)이 아닌가? (IsDropAllowed)
        if (GridManager.Instance.IsValidGridPosition(currentGridPos) &&
            !GridManager.Instance.IsOccupied(currentGridPos) &&
            GridManager.Instance.IsDropAllowed(currentGridPos))  // <--- 이 조건 추가!
        {
            // --- [성공 로직] ---
            // 리스트에서 제거
            _holdingCargos.Remove(cargo);

            // 내려놓기 (위치 스냅)
            Vector3 snapPos = GridManager.Instance.GridToWorldCenter(currentGridPos);
            cargo.OnDropped(snapPos, currentGridPos, _playerCollider);

            // 장부 등록
            GridManager.Instance.RegisterCargo(currentGridPos, cargo);
        }
        else
        {
            // --- [실패 로직] ---
            // 여기에 걸리면 리스트에서 제거되지 않으므로 계속 들고 있게 됨
            Debug.Log("이곳에는 내려놓을 수 없습니다. (금지 구역 혹은 이미 있음)");
        }
    }

    // 모두 내려놓기 (스페이스바)
    void DropAllCargos()
    {
        // 리스트를 역순으로 순회하거나, 별도 리스트 복사해서 처리 (삭제 시 오류 방지)
        // 여기서는 간단하게 처리
        for (int i = _holdingCargos.Count - 1; i >= 0; i--)
        {
            DropSpecificCargo(_holdingCargos[i]);
        }
    }

    // 디버깅: 인접 범위 그리기
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}