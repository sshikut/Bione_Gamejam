using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("Settings")]
    public float cellSize = 1.0f;
    public Vector2Int gridSize = new Vector2Int(10, 10); // 예: 10x10

    private Dictionary<Vector2Int, Cargo> _gridContents = new Dictionary<Vector2Int, Cargo>();

    private HashSet<Vector2Int> _noDropZones = new HashSet<Vector2Int>();

    private HashSet<Vector2Int> _dangerZones = new HashSet<Vector2Int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        SetConveyorBeltZone();
        SetDangerZone();
    }

    // ★ [핵심] 그리드의 "물리적 왼쪽 아래 좌표"를 계산하는 함수
    // GridManager(중심) 위치에서 너비/2, 높이/2 만큼 왼쪽 아래로 이동한 지점
    private Vector3 GetGridBottomLeft()
    {
        float width = gridSize.x * cellSize;
        float height = gridSize.y * cellSize;

        // transform.position을 기준으로 빼줍니다.
        return transform.position - new Vector3(width * 0.5f, height * 0.5f, 0);
    }

    // 월드 좌표 -> 그리드 좌표 (0,0 ~ 9,9)
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        // 1. 월드 좌표에서 시작점(왼쪽 아래) 좌표를 뺍니다.
        //    이렇게 하면 시작점이 (0,0)인 로컬 좌표계가 됩니다.
        Vector3 localPos = worldPos - GetGridBottomLeft();

        int x = Mathf.FloorToInt(localPos.x / cellSize);
        int y = Mathf.FloorToInt(localPos.y / cellSize);
        return new Vector2Int(x, y);
    }

    // 그리드 좌표 -> 월드 중심 좌표
    public Vector3 GridToWorldCenter(Vector2Int gridPos)
    {
        // 1. 시작점(왼쪽 아래)에서 출발
        Vector3 worldPos = GetGridBottomLeft();

        // 2. 그리드 칸만큼 이동
        worldPos.x += (gridPos.x * cellSize) + (cellSize * 0.5f);
        worldPos.y += (gridPos.y * cellSize) + (cellSize * 0.5f);

        return worldPos;
    }

    // 유효성 검사 (기존 유지)
    public bool IsValidGridPosition(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < gridSize.x &&
               gridPos.y >= 0 && gridPos.y < gridSize.y;
    }

    // 장부 관련 함수들 (기존 유지)
    public bool IsOccupied(Vector2Int gridPos) => _gridContents.ContainsKey(gridPos);

    public Cargo GetCargoAt(Vector2Int gridPos) => _gridContents.TryGetValue(gridPos, out Cargo c) ? c : null;

    public void RegisterCargo(Vector2Int gridPos, Cargo cargo)
    {
        // 1. 딕셔너리에 화물 등록 (기존 로직 유지)
        if (!_gridContents.ContainsKey(gridPos))
        {
            _gridContents.Add(gridPos, cargo);
            cargo.CurrentGridPos = gridPos;
        }
        else
        {
            // (안전장치) 만약 이미 키가 있다면 덮어씌워서 위치 갱신 확실하게 함
            _gridContents[gridPos] = cargo;
            cargo.CurrentGridPos = gridPos;
        }

        // 2. [추가된 핵심 로직] 등록된 위치가 '위험 구역'인지 체크 -> 게임 오버!
        if (_dangerZones.Contains(gridPos))
        {
            Debug.Log($"화물({cargo.name})이 위험 구역 {gridPos}에 진입! GAME OVER");

            if (GameTimeManager.Instance != null)
            {
                // 화물 소각 사유로 게임 오버 트리거
                GameTimeManager.Instance.TriggerGameOver(GameOverReason.CargoBurned);
            }
        }
    }

    public void UnregisterCargo(Vector2Int gridPos) { if (_gridContents.ContainsKey(gridPos)) _gridContents.Remove(gridPos); }

    // 기즈모 그리기 (중앙 정렬에 맞춰 수정됨)
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        Vector3 startPos = GetGridBottomLeft();

        // 세로선
        for (int x = 0; x <= gridSize.x; x++)
        {
            Vector3 bottom = startPos + new Vector3(x * cellSize, 0, 0);
            Vector3 top = bottom + new Vector3(0, gridSize.y * cellSize, 0);
            Gizmos.DrawLine(bottom, top);
        }
        // 가로선
        for (int y = 0; y <= gridSize.y; y++)
        {
            Vector3 left = startPos + new Vector3(0, y * cellSize, 0);
            Vector3 right = left + new Vector3(gridSize.x * cellSize, 0, 0);
            Gizmos.DrawLine(left, right);
        }

        // (선택) 그리드 매니저 위치(중앙) 표시
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }

    private void SetConveyorBeltZone()
    {
        // 맵의 정중앙 Y 좌표 구하기
        int centerY = gridSize.y / 2;

        // 중앙 기준 위아래 2칸씩 (총 5칸: -2, -1, 0, +1, +2)
        int range = 2;

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = centerY - range; y <= centerY + range; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                // 금지 구역 리스트에 추가
                _noDropZones.Add(pos);
            }
        }

        Debug.Log($"배치 금지 구역 설정 완료: {_noDropZones.Count}칸");
    }

    public bool IsDropAllowed(Vector2Int pos)
    {
        // 금지 구역 리스트에 포함되어 있다면 false (놓기 금지)
        if (_noDropZones.Contains(pos))
        {
            return false;
        }

        return true; // 포함 안 되어 있으면 true (놓기 가능)
    }

    private void SetDangerZone()
    {
        int centerX = gridSize.x / 2; // 가로 중앙
        int centerY = gridSize.y / 2; // 세로 중앙
        int range = 2; // 중앙 기준 위아래 2칸 (총 5칸)

        // 불은 맵의 정중앙 세로줄에만 있음
        for (int y = centerY - range; y <= centerY + range; y++)
        {
            Vector2Int pos = new Vector2Int(centerX, y);
            _dangerZones.Add(pos);
        }
    }
}