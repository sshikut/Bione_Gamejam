using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // 리스트 검색 기능(FirstOrDefault 등) 사용을 위해 필수

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance;

    [Header("Assets & Data")]
    public GameObject customerPrefab;      // 손님 프리팹
    public List<ItemData> menu;            // 전체 아이템 목록 (자동 로드됨)
    public CustomerData[] generalDataList; // 일반 손님 프로필 (외형/속도)
    public CustomerData[] pickupDataList;  // 픽업 손님 프로필

    [Header("Spawn Locations")]
    public Transform generalSpawnPoint;    // 일반 손님 등장 위치 (문)
    public Transform pickupSpawnPoint;     // 픽업 손님 등장 위치 (문)
    public Transform pickupCounterPoint;   // 픽업 손님 목적지 (카운터)
    public Transform[] storeWaypoints;     // 편의점 내부 산책로

    [Header("Shelves Management")]
    [Tooltip("씬에 있는 진열대들이 게임 시작 시 자동으로 등록됩니다.")]
    public List<Shelf> allShelves = new List<Shelf>();

    [Header("Pickup Logic Settings")]
    public float pickupCheckInterval = 20f; // 픽업 스폰 체크 주기
    public int currentPickupChance = 20;    // 픽업 등장 확률 (%)

    // 런타임 상태 변수
    private int _pickupQuotaToday;          // 오늘 와야 할 픽업 손님 수
    private int _pickupSpawnedCount;        // 현재까지 온 픽업 손님 수
    private float _pickupTimer;
    private float _generalTimer;

    // 현재 카운터에 있는 픽업 손님 (한 번에 한 명만 받기 위함)
    private Customer _currentPickupCustomer;

    private void Awake()
    {
        Instance = this;
        LoadResources();
    }

    private void Start()
    {
        // 씬에 배치된 모든 Shelf 컴포넌트를 찾아서 리스트에 등록
        // (에디터에서 일일이 넣는 수고를 덜어줌)
        allShelves = FindObjectsOfType<Shelf>().ToList();
        Debug.Log($"[Manager] 진열대 {allShelves.Count}개 감지 완료.");

        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnDayChanged += HandleDayStart;
        }
    }

    private void OnDestroy()
    {
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnDayChanged -= HandleDayStart;
        }
    }

    // --- [초기화 및 리소스 로드] ---
    void LoadResources()
    {
        // ItemData 자동 로드 (Resources/ItemData 폴더)
        ItemData[] loadedData = Resources.LoadAll<ItemData>("ItemData");
        menu = loadedData.ToList();
    }

    // 하루 시작 시 초기화 (GameTimeManager 이벤트 연동)
    private void HandleDayStart(int day)
    {
        // 날짜가 지날수록 픽업 손님이 많아지거나 랜덤하게 설정
        _pickupQuotaToday = Random.Range(3, 6 + day);
        _pickupSpawnedCount = 0;
        currentPickupChance = 20;
        _pickupTimer = 0f;
        _generalTimer = 0f;

        Debug.Log($"Day {day} 시작! 금일 픽업 예약: {_pickupQuotaToday}명");
    }

    // --- [메인 루프] ---
    private void Update()
    {
        if (GameTimeManager.Instance == null || !GameTimeManager.Instance.IsPlaying) return;

        GameState state = GameTimeManager.Instance.currentState;

        // 1. 일반 손님 스폰 (낮/밤 영업시간)
        if (state == GameState.DayService || state == GameState.NightService)
        {
            HandleGeneralSpawnLogic();
        }

        // 2. 픽업 손님 스폰 (영업시간 + 중간 배송 시간)
        bool isPickupTime = (state == GameState.DayService ||
                             state == GameState.MidDelivery ||
                             state == GameState.NightService);

        // 쿼터가 남아있고, 카운터가 비어있을 때만 시도
        if (isPickupTime && _pickupSpawnedCount < _pickupQuotaToday && _currentPickupCustomer == null)
        {
            HandlePickupSpawnLogic();
        }
    }

    // --- [일반 손님 (General) 스폰 로직] ---
    void HandleGeneralSpawnLogic()
    {
        _generalTimer += Time.deltaTime;

        // 8초마다 스폰 시도 (게임 난이도에 따라 조절 가능)
        if (_generalTimer >= 8.0f)
        {
            _generalTimer = 0;
            SpawnGeneralCustomer();
        }
    }

    void SpawnGeneralCustomer()
    {
        if (generalDataList.Length == 0) return;

        // 1. 랜덤 속성 결정 (ItemStorageType Enum 개수에 맞춰 랜덤)
        // (예: 0=RoomTemp, 1=Cold, 2=Frozen, 3=Liquid, 4=Heated 등)
        // System.Enum.GetValues를 쓰면 더 안전하지만, 성능상 간단히 처리
        StorageType randomCategory = (StorageType)Random.Range(0, 5);

        // 2. 해당 속성을 파는 진열대가 있는지 확인
        // (Linq: allShelves 리스트에서 조건에 맞는 첫 번째 요소를 찾음)
        Shelf targetShelf = allShelves.FirstOrDefault(s => s.shelfCategory == randomCategory);

        // 진열대가 없으면 손님을 보내지 않음 (혹은 다른 속성으로 재시도 로직 추가 가능)
        if (targetShelf == null)
        {
            // Debug.LogWarning($"[{randomCategory}] 속성을 파는 진열대가 없어 스폰 취소됨.");
            return;
        }

        // 3. 데이터 선정 및 생성
        CustomerData data = generalDataList[Random.Range(0, generalDataList.Length)];
        GameObject obj = Instantiate(customerPrefab, generalSpawnPoint.position, Quaternion.identity);
        Customer customer = obj.GetComponent<Customer>();

        // 4. 초기화 (카테고리와 목표 진열대 전달)
        customer.InitializeGeneral(randomCategory, targetShelf, storeWaypoints, data);
    }

    // --- [픽업 손님 (Pickup) 스폰 로직] ---
    void HandlePickupSpawnLogic()
    {
        _pickupTimer += Time.deltaTime;

        if (_pickupTimer >= pickupCheckInterval)
        {
            _pickupTimer = 0f;

            // 확률 굴리기
            int roll = Random.Range(0, 100);
            if (roll < currentPickupChance)
            {
                SpawnPickupCustomer();
                currentPickupChance = 20; // 스폰 성공 시 확률 초기화
            }
            else
            {
                currentPickupChance += 20; // 실패 시 확률 증가 (천장 시스템)
                if (currentPickupChance > 100) currentPickupChance = 100;
            }
        }
    }

    void SpawnPickupCustomer()
    {
        if (pickupDataList.Length == 0) return;

        // 1. 랜덤 아이템 및 수량 결정
        ItemData randomItem = menu[Random.Range(0, menu.Count)];
        int count = Random.Range(1, 4); // 1~3개 요구
        CustomerData data = pickupDataList[Random.Range(0, pickupDataList.Length)];

        // 2. 생성
        GameObject obj = Instantiate(customerPrefab, pickupSpawnPoint.position, Quaternion.identity);
        Customer customer = obj.GetComponent<Customer>();

        // 3. 초기화 (특정 아이템과 카운터 위치 전달)
        customer.InitializePickup(randomItem, count, pickupCounterPoint, data);

        // 4. 매니저에 등록 (자리 차지)
        _currentPickupCustomer = customer;
        _pickupSpawnedCount++;

        Debug.Log($"픽업 손님 등장! 요구: {randomItem.itemName} x{count}");
    }

    // --- [외부 접근 및 이벤트 처리] ---

    // 손님이 퇴장할 때(성공/실패 무관) Customer 스크립트가 호출
    public void OnCustomerLeft(Customer customer)
    {
        // 떠난 손님이 현재 픽업 손님이었다면 자리 비움 처리
        if (customer == _currentPickupCustomer)
        {
            _currentPickupCustomer = null;
        }
    }

    // DeliveryZone(카운터)에서 현재 응대해야 할 픽업 손님 정보를 가져갈 때 사용
    public Customer GetPickupCustomer()
    {
        return _currentPickupCustomer;
    }
}