using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("Settings")]
    public float cellSize = 1.0f; // 그리드 한 칸의 크기
    public Vector2Int gridSize = new Vector2Int(10, 10); // 맵 크기

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 월드 좌표 -> 그리드 좌표 (배열 인덱스용)
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / cellSize);
        int y = Mathf.FloorToInt(worldPos.y / cellSize);
        return new Vector2Int(x, y);
    }

    // 그리드 좌표 -> 월드 중심 좌표 (아이템 배치용)
    public Vector3 GridToWorldCenter(Vector2Int gridPos)
    {
        float x = gridPos.x * cellSize + (cellSize * 0.5f);
        float y = gridPos.y * cellSize + (cellSize * 0.5f);
        return new Vector3(x, y, 0);
    }

    // 에디터에서 그리드를 눈으로 확인하기 위한 기즈모
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        for (int x = 0; x <= gridSize.x; x++)
        {
            Gizmos.DrawLine(new Vector3(x * cellSize, 0, 0), new Vector3(x * cellSize, gridSize.y * cellSize, 0));
        }
        for (int y = 0; y <= gridSize.y; y++)
        {
            Gizmos.DrawLine(new Vector3(0, y * cellSize, 0), new Vector3(gridSize.x * cellSize, y * cellSize, 0));
        }
    }
}
