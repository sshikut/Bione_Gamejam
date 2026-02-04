using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Customer : MonoBehaviour
{
    [Header("Identity")]
    public CustomerType type;           // 손님 타입 (General / Pickup)
    public string customerName;

    [Header("General Customer Info")]
    public StorageType wantedCategory; // 일반 손님이 원하는 속성 (냉동, 실온 등)
    private Shelf _targetShelf;            // 목표 진열대

    [Header("Pickup Customer Info")]
    public ItemData wantedItem;         // 픽업 손님이 원하는 특정 아이템
    public int wantedCount;             // 픽업 수량
    public int currentReceivedCount = 0;

    [Header("Visual & UI")]
    public SpriteRenderer bodyRenderer;
    public Image itemIcon;              // 말풍선 아이콘 (카테고리 아이콘 or 아이템 아이콘)
    public TextMeshProUGUI countText;   // 수량 텍스트
    public Slider patienceSlider;       // 인내심 게이지

    // 내부 변수
    private float _moveSpeed;
    private float _maxPatience;
    private float _timer;
    private bool _isActive = false;
    private bool _isMoving = false;

    private Transform _targetPoint;      // 현재 이동 목표점
    private Transform[] _roamPoints;     // 산책 경로
    private Transform _finalDestination; // 최종 목적지 (픽업용 카운터 등)

    // ==================================================================================
    // 1. 초기화 (Initialize)
    // ==================================================================================

    // A. 일반 손님용 초기화 (진열대 쇼핑)
    public void InitializeGeneral(StorageType category, Shelf shelf,
                                  Transform[] waypoints, CustomerData data)
    {
        ApplyCommonData(data, CustomerType.General);

        wantedCategory = category;
        _targetShelf = shelf;
        _roamPoints = waypoints;

        // UI 설정 (카테고리 아이콘 설정 필요 - 여기서는 임시 처리)
        // if (itemIcon != null) itemIcon.sprite = GetCategoryIcon(category);
        if (countText != null) countText.text = ""; // 일반 손님은 수량 표시 안 함 (혹은 1개)

        // 행동 시작
        StartCoroutine(GeneralShoppingRoutine());
    }

    // B. 픽업 손님용 초기화 (카운터 수령)
    public void InitializePickup(ItemData item, int count,
                                 Transform destination, CustomerData data)
    {
        ApplyCommonData(data, CustomerType.Pickup);

        wantedItem = item;
        wantedCount = count;
        _finalDestination = destination;

        // UI 설정
        if (itemIcon != null) itemIcon.sprite = item.icon;
        UpdatePickupCountUI();
        if (countText != null) countText.color = Color.red; // 픽업 강조

        // 행동 시작 (카운터로 직행)
        MoveTo(_finalDestination);
    }

    // 공통 데이터 적용 헬퍼
    private void ApplyCommonData(CustomerData data, CustomerType setType)
    {
        type = setType;
        customerName = data.customerName;
        gameObject.name = $"Customer_{type}_{customerName}";

        if (bodyRenderer != null && data.bodySprite != null)
            bodyRenderer.sprite = data.bodySprite;

        // 능력치 적용
        _moveSpeed = 3.0f * data.speedMultiplier;
        _maxPatience = 60f * data.patienceMultiplier;
        _timer = _maxPatience;

        if (patienceSlider != null) patienceSlider.maxValue = _maxPatience;

        _isActive = true;
    }

    // ==================================================================================
    // 2. 메인 루프 (Update)
    // ==================================================================================

    private void Update()
    {
        // 이동 로직
        if (_isMoving && _targetPoint != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPoint.position, _moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, _targetPoint.position) <= 0.1f)
            {
                _isMoving = false;
            }
        }

        // 인내심 로직
        if (_isActive)
        {
            _timer -= Time.deltaTime;
            if (patienceSlider != null) patienceSlider.value = _timer;

            if (_timer <= 0)
            {
                HandleFail("Time Out");
            }
        }
    }

    // ==================================================================================
    // 3. 행동 패턴 (AI Routines)
    // ==================================================================================

    // [일반 손님] 산책 -> 진열대 -> 구매 -> 퇴장
    IEnumerator GeneralShoppingRoutine()
    {
        // 1. 산책 (1~2군데 들름)
        if (_roamPoints != null && _roamPoints.Length > 0)
        {
            int visitCount = Random.Range(1, 3);
            for (int i = 0; i < visitCount; i++)
            {
                Transform randomPoint = _roamPoints[Random.Range(0, _roamPoints.Length)];
                MoveTo(randomPoint);
                yield return new WaitUntil(() => !_isMoving);
                yield return new WaitForSeconds(Random.Range(0.5f, 1.5f)); // 아이쇼핑
            }
        }

        // 2. 목표 진열대로 이동
        if (_targetShelf != null)
        {
            MoveTo(_targetShelf.transform);
            yield return new WaitUntil(() => !_isMoving);

            // 도착 후 잠시 고민하는 연출
            yield return new WaitForSeconds(0.5f);

            // 3. 구매 시도
            TryBuyFromShelf();
        }
        else
        {
            HandleFail("No Shelf Found");
        }
    }

    // [일반 손님] 진열대 구매 로직
    private void TryBuyFromShelf()
    {
        if (_targetShelf == null) return;

        // 진열대에서 물건 하나 꺼내기 (가격 반환)
        int price = _targetShelf.TryTakeStock();

        if (price > 0)
        {
            // 구매 성공
            MoneyManager.Instance.AddBion(price);

            // (옵션) 말풍선에 "스마일" 아이콘 띄우기 등의 연출 추가 가능
            Debug.Log($"{customerName} 구매 성공! (+{price} Bion)");

            HandleSuccess();
        }
        else
        {
            // 재고 없음 -> 실망하고 퇴장
            Debug.Log($"{customerName} 구매 실패 (재고 없음)");
            HandleFail("Out of Stock");
        }
    }

    // [픽업 손님] 플레이어가 직접 건네주는 아이템 받기
    public bool ReceivePickupItem(ItemData item)
    {
        if (type != CustomerType.Pickup) return false; // 일반 손님은 직접 안 받음
        if (item != wantedItem) return false;          // 원하는 물건 아님

        currentReceivedCount++;
        UpdatePickupCountUI();

        // 목표 수량 달성
        if (currentReceivedCount >= wantedCount)
        {
            int reward = item.basePrice * wantedCount * 2; // 픽업은 2배 보상 (예시)
            MoneyManager.Instance.AddBion(reward);
            Debug.Log($"픽업 완료! (+{reward} Bion)");

            HandleSuccess();
        }
        return true;
    }

    // ==================================================================================
    // 4. 공통 기능 (Movement, Finish)
    // ==================================================================================

    public void MoveTo(Transform target)
    {
        _targetPoint = target;
        _isMoving = true;

        // 바라보는 방향 전환 (Flip)
        if (target.position.x < transform.position.x)
            transform.localScale = new Vector3(-1, 1, 1);
        else
            transform.localScale = new Vector3(1, 1, 1);
    }

    void HandleSuccess()
    {
        _isActive = false;
        // 퇴장 연출 (기쁨)
        Leave();
    }

    void HandleFail(string reason)
    {
        _isActive = false;

        // 픽업 손님은 실패 시 위약금
        if (type == CustomerType.Pickup)
        {
            int penalty = (wantedItem != null) ? wantedItem.basePrice * wantedCount : 100;
            MoneyManager.Instance.ApplyPenalty(penalty);
            Debug.Log($"픽업 실패 ({reason}) - 위약금 {penalty}");
        }

        Leave();
    }

    void Leave()
    {
        // 매니저 목록에서 제거 알림
        if (CustomerManager.Instance != null)
            CustomerManager.Instance.OnCustomerLeft(this);

        Destroy(gameObject); // 혹은 풀링 시스템 사용 시 ReturnToPool
    }

    void UpdatePickupCountUI()
    {
        if (countText != null)
            countText.text = $"{currentReceivedCount}/{wantedCount}";
    }
}