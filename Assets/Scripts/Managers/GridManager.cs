using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("Settings")]
    public float cellSize = 1.0f;
    public Vector2Int gridSize = new Vector2Int(10, 10); // 예: 10x10

    private Dictionary<Vector2Int, Cargo> _gridContents = new Dictionary<Vector2Int, Cargo>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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
        if (!_gridContents.ContainsKey(gridPos))
        {
            _gridContents.Add(gridPos, cargo);
            cargo.CurrentGridPos = gridPos;
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
}