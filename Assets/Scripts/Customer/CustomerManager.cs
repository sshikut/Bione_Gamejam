using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance;

    [Header("Assets")]
    public GameObject customerPrefab; // 손님 프리팹 (껍데기)
    public List<ItemData> menu;           // 주문 가능한 아이템 목록

    [Header("Customer Data (ScriptableObjects)")]
    public CustomerData[] generalDataList; // 일반 손님 데이터들 (Type: General)
    public CustomerData[] pickupDataList;  // 픽업 손님 데이터들 (Type: Pickup)

    [Header("Spawn Locations")]
    public Transform generalSpawnPoint;   // 일반 등장 (문)
    public Transform generalWaitPoint;    // 일반 목적지 (진열대 포탈 앞)
    public Transform pickupSpawnPoint;    // 픽업 등장 (문)
    public Transform pickupCounterPoint;  // 픽업 목적지 (카운터 앞)
    public Transform[] storeWaypoints;    // 편의점 내부 산책로

    [Header("Pickup System Logic")]
    public float pickupCheckInterval = 20f; // 체크 주기
    public int pickupQuotaToday;            // 오늘 목표 할당량
    public int pickupSpawnedCount;          // 현재 스폰된 수
    public int currentPickupChance = 20;    // 현재 확률 (20% -> 40%...)

    // 내부 변수
    private float _pickupTimer;
    private float _generalTimer;
    private Customer _currentPickupCustomer; // 현재 카운터 차지 중인 손님

    private void Awake()
    {
        Instance = this;
        LoadItemDataAuto();
    }

    private void Start()
    {
        if (GameTimeManager.Instance != null)
        {
            // GameTimeManager.Instance.OnDayStart += HandleDayStart;
        }
    }

    void LoadItemDataAuto()
    {
        // 1. Resources/Items 폴더 안의 모든 ItemData를 불러옴
        // ("Items"는 Resources 폴더 안의 하위 폴더 이름입니다. 본인 폴더명에 맞게 수정하세요)
        ItemData[] loadedData = Resources.LoadAll<ItemData>("ItemData");

        // 2. 리스트에 덮어쓰기
        menu = loadedData.ToList();

        Debug.Log($"[System] 메뉴에 {menu.Count}개 자동 로드 완료!");
    }

    private void HandleDayStart(int day)
    {
        pickupQuotaToday = Random.Range(3, 7);
        pickupSpawnedCount = 0;
        currentPickupChance = 20;
        _pickupTimer = 0f;

        Debug.Log($"Day {day} 픽업 예약: {pickupQuotaToday}명");
    }

    private void Update()
    {
        if (GameTimeManager.Instance == null) return;
        GameState state = GameTimeManager.Instance.currentState;

        // 1. 일반 손님 스폰 (서비스 시간: 낮/밤)
        if (state == GameState.DayService || state == GameState.NightService)
        {
            HandleGeneralSpawn();
        }

        // 2. 픽업 손님 스폰 (서비스 시간 + 중간 배송 시간)
        bool isOperatingTime = (state == GameState.DayService ||
                                state == GameState.MidDelivery ||
                                state == GameState.NightService);

        if (isOperatingTime && pickupSpawnedCount < pickupQuotaToday && _currentPickupCustomer == null)
        {
            HandlePickupSpawnLogic();
        }
    }

    // --- 일반 손님 스폰 로직 ---
    void HandleGeneralSpawn()
    {
        _generalTimer += Time.deltaTime;
        if (_generalTimer >= 8.0f) // 8초마다 1명
        {
            _generalTimer = 0;
            SpawnCustomer(CustomerType.General);
        }
    }

    // --- 픽업 손님 확률 로직 ---
    void HandlePickupSpawnLogic()
    {
        _pickupTimer += Time.deltaTime;

        if (_pickupTimer >= pickupCheckInterval) // 20초 경과
        {
            _pickupTimer = 0f;

            int roll = Random.Range(0, 100);
            Debug.Log($"픽업 확률 체크: {roll} < {currentPickupChance}%");

            if (roll < currentPickupChance)
            {
                SpawnCustomer(CustomerType.Pickup);
                currentPickupChance = 20; // 확률 초기화
            }
            else
            {
                currentPickupChance += 20;
                if (currentPickupChance > 100) currentPickupChance = 100;
            }
        }
    }

    // --- 통합 스폰 함수 ---
    void SpawnCustomer(CustomerType type)
    {
        // 1. 타입에 맞는 데이터 목록 가져오기
        CustomerData[] targetList = (type == CustomerType.Pickup) ? pickupDataList : generalDataList;

        if (targetList == null || targetList.Length == 0) return;

        // 2. 랜덤 데이터 & 위치 선정
        CustomerData data = targetList[Random.Range(0, targetList.Length)];

        Transform spawnPos = (type == CustomerType.Pickup) ? pickupSpawnPoint : generalSpawnPoint;
        Transform destPos = (type == CustomerType.Pickup) ? pickupCounterPoint : generalWaitPoint;

        // 3. 생성
        GameObject obj = Instantiate(customerPrefab, spawnPos.position, Quaternion.identity);
        Customer customer = obj.GetComponent<Customer>();

        // 4. 메뉴 선정
        ItemData item = menu[Random.Range(0, menu.Count)];
        int count = Random.Range(1, 4);

        // 5. 초기화 (데이터 주입)
        customer.Initialize(item, count, destPos, storeWaypoints, data);

        // 6. 픽업 손님이면 매니저에 등록
        if (type == CustomerType.Pickup)
        {
            _currentPickupCustomer = customer;
            pickupSpawnedCount++;
        }
    }

    public void OnCustomerLeft(Customer customer)
    {
        if (customer == _currentPickupCustomer)
        {
            _currentPickupCustomer = null;
        }
    }

    public Customer GetPickupCustomer()
    {
        return _currentPickupCustomer;
    }
}