using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class CargoSpawner : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("생성할 기본 화물 프리팹 (껍데기)")]
    public GameObject baseCargoPrefab;

    [Tooltip("생성 가능한 화물 데이터 목록")]
    public List<ItemData> spawnableItems;

    [Tooltip("스폰 주기 (초)")]
    public float spawnInterval = 5.0f;

    [Tooltip("한 번에 생성할 세로 줄 수 (예: 5칸)")]
    public int spawnHeight = 5;

    private float _timer;
    private bool _isSpawning = false; // 현재 스폰 중인가?

    private void Awake()
    {
        LoadItemDataAuto();
    }

    private void Start()
    {
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnStateChanged += HandleStateChange;

            // 시작하자마자 현재 상태 체크 (혹시 중간에 켜졌을 경우 대비)
            HandleStateChange(GameTimeManager.Instance.currentState);
        }
    }

    private void OnDestroy()
    {
        // 오브젝트가 사라질 때 구독 해제 (메모리 누수 방지)
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnStateChanged -= HandleStateChange;
        }
    }

    void LoadItemDataAuto()
    {
        // 1. Resources/Items 폴더 안의 모든 ItemData를 불러옴
        // ("Items"는 Resources 폴더 안의 하위 폴더 이름입니다. 본인 폴더명에 맞게 수정하세요)
        ItemData[] loadedData = Resources.LoadAll<ItemData>("ItemData");

        // 2. 리스트에 덮어쓰기
        spawnableItems = loadedData.ToList();

        Debug.Log($"[System] 아이템 데이터 {spawnableItems.Count}개 자동 로드 완료!");
    }

    private void HandleStateChange(GameState state)
    {
        // ★ 기획: 아침 교대(MorningShift) 또는 중간 배송(MidDelivery) 때만 화물 생성
        if (state == GameState.MorningShift || state == GameState.MidDelivery)
        {
            _isSpawning = true;
            _timer = 0; // 상태가 바뀌면 타이머 초기화 (선택 사항)
            // Debug.Log("화물 스폰 시작!");
        }
        else
        {
            _isSpawning = false;
            // Debug.Log("화물 스폰 중지.");
        }
    }

    private void Update()
    {
        // 스폰 활성화 상태일 때만 타이머 작동
        if (_isSpawning)
        {
            _timer += Time.deltaTime;

            if (_timer >= spawnInterval)
            {
                SpawnWave();
                _timer = 0;
            }
        }
    }

    // 좌우 양쪽에서 물류 동시 생성
    void SpawnWave()
    {
        if (GridManager.Instance == null) return;

        int gridYCenter = GridManager.Instance.gridSize.y / 2;
        int halfHeight = spawnHeight / 2;

        // 중앙을 기준으로 위아래로 펼쳐서 범위 설정
        int startY = gridYCenter - halfHeight;
        int endY = startY + spawnHeight;

        for (int y = startY; y < endY; y++)
        {
            // 1. 왼쪽 벽 -> 오른쪽으로 밀며 생성
            // 시작점: (0, y), 미는 방향: (1, 0)
            PushAndSpawn(new Vector2Int(0, y), Vector2Int.right);

            // 2. 오른쪽 벽 -> 왼쪽으로 밀며 생성
            // 시작점: (MaxX, y), 미는 방향: (-1, 0)
            int maxX = GridManager.Instance.gridSize.x - 1;
            PushAndSpawn(new Vector2Int(maxX, y), Vector2Int.left);
        }
    }

    // ★ 핵심 로직: 빈 공간 찾기 -> 밀기 -> 생성
    void PushAndSpawn(Vector2Int spawnPos, Vector2Int pushDir)
    {
        // 1. [탐색] 해당 라인에서 가장 가까운 빈 칸 찾기
        int limit = GridManager.Instance.gridSize.x;
        Vector2Int emptyPos = new Vector2Int(-1, -1);
        bool foundEmpty = false;

        for (int i = 0; i < limit; i++)
        {
            Vector2Int checkPos = spawnPos + (pushDir * i);

            // 맵 밖으로 나가면 중단
            if (!GridManager.Instance.IsValidGridPosition(checkPos)) break;

            // 빈 칸 발견!
            if (!GridManager.Instance.IsOccupied(checkPos))
            {
                emptyPos = checkPos;
                foundEmpty = true;
                break;
            }
        }

        // 빈 칸이 아예 없으면 생성 포기 (라인이 꽉 참)
        if (!foundEmpty) return;

        // 2. [밀기] 입구가 차 있다면, 빈 칸까지 물건들을 한 칸씩 뒤로 밀기
        if (emptyPos != spawnPos)
        {
            // 빈 칸 바로 앞 화물부터 시작해서 입구 쪽으로 역순 탐색
            Vector2Int currentCheckPos = emptyPos - pushDir;

            int safetyCount = 0; // 무한 루프 방지용 안전장치

            while (safetyCount < 100)
            {
                safetyCount++;

                // 화물이 있으면 한 칸 뒤(빈 곳)로 이동시킴
                if (GridManager.Instance.IsOccupied(currentCheckPos))
                {
                    Cargo cargo = GridManager.Instance.GetCargoAt(currentCheckPos);
                    Vector2Int nextPos = currentCheckPos + pushDir;

                    // 장부 업데이트
                    GridManager.Instance.UnregisterCargo(currentCheckPos);
                    GridManager.Instance.RegisterCargo(nextPos, cargo);

                    // 실제 이동 명령
                    cargo.MoveByPush(nextPos);
                }

                // 입구(SpawnPos)에 있는 화물까지 다 옮겼으면 종료
                if (currentCheckPos == spawnPos) break;

                // 입구 쪽으로 한 칸 이동
                currentCheckPos -= pushDir;
            }
        }

        // 3. [생성] 입구가 확보되었으므로 새 화물 생성
        if (spawnableItems.Count > 0 && baseCargoPrefab != null)
        {
            // 랜덤 데이터 뽑기
            ItemData randomItem = spawnableItems[Random.Range(0, spawnableItems.Count)];

            // 프리팹 생성 및 위치 잡기
            GameObject newObj = Instantiate(baseCargoPrefab);
            newObj.transform.position = GridManager.Instance.GridToWorldCenter(spawnPos);

            // 데이터 주입 ("너는 사과야!")
            CargoProperty property = newObj.GetComponent<CargoProperty>();
            if (property != null)
            {
                property.SetData(randomItem);
            }
        }
    }
}