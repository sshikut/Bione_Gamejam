using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Customer : MonoBehaviour
{
    [Header("Identity (Runtime)")]
    public CustomerType type;      // 데이터에서 받아온 타입
    public string customerName;

    [Header("Order Info")]
    public ItemData wantedItem;    // 원하는 물건
    public int wantedCount;        // 원하는 수량
    public int currentReceivedCount = 0;

    [Header("Settings")]
    public int basePrice = 100;    // 물건 개당 기본 가격
    public float stopDistance = 0.1f; // 이동 멈춤 거리

    [Header("Components")]
    public SpriteRenderer bodyRenderer; // 몸통 스프라이트

    [Header("UI Refs")]
    public Image itemIcon;         // 말풍선 아이콘
    public TextMeshProUGUI countText; // 수량 텍스트
    public Slider patienceSlider;  // 인내심 게이지

    // 내부 변수
    private float _timer;          // 남은 인내심 시간
    private float _maxPatience;    // 최대 인내심 (슬라이더용)
    private float _moveSpeed;      // 이동 속도
    private bool _isActive = false;

    private Transform _targetPoint;       // 현재 이동 목표
    private Transform[] _roamPoints;      // 일반 손님 배회 경로
    private Transform _finalDestination;  // 최종 목적지 (카운터 or 포탈)
    private bool _isMoving = false;

    // ★ 초기화: 매니저가 데이터를 꽂아줄 때 호출
    // (CustomerProfile -> CustomerData로 변경됨)
    public void Initialize(ItemData item, int count,
                           Transform destination, Transform[] waypoints,
                           CustomerData data)
    {
        // 1. 데이터 적용
        if (data == null)
        {
            Debug.LogError("Customer: CustomerData가 없습니다!");
            return;
        }

        type = data.type;
        customerName = data.customerName;
        gameObject.name = $"Customer_{type}_{customerName}";

        // 외형 및 능력치 적용
        if (bodyRenderer != null) bodyRenderer.sprite = data.bodySprite;

        // 기본값(3.0f, 60f)에 데이터 계수를 곱함
        _moveSpeed = 3.0f * data.speedMultiplier;
        _maxPatience = 60f * data.patienceMultiplier;
        _timer = _maxPatience;

        // 2. 주문 정보 설정
        wantedItem = item;
        wantedCount = count;
        _finalDestination = destination;
        _roamPoints = waypoints;
        _isActive = true;

        // 3. UI 갱신
        if (itemIcon != null) itemIcon.sprite = item.icon;
        if (patienceSlider != null) patienceSlider.maxValue = _maxPatience;
        UpdateCountUI();

        // 픽업 손님 강조 (텍스트 색상 변경 등)
        if (type == CustomerType.Pickup && countText != null)
            countText.color = Color.red;

        // 4. 행동 시작
        if (type == CustomerType.Pickup)
        {
            // 픽업: 목적지(카운터)로 직행
            MoveTo(_finalDestination);
        }
        else
        {
            // 일반: 편의점 구경(배회) 시작
            StartCoroutine(GeneralCustomerRoutine());
        }
    }

    private void Update()
    {
        // 1. 이동 로직
        if (_isMoving && _targetPoint != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPoint.position, _moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, _targetPoint.position) <= stopDistance)
            {
                _isMoving = false;
            }
        }

        // 2. 인내심 로직 (활성화 상태일 때만)
        if (!_isActive) return;

        _timer -= Time.deltaTime;
        if (patienceSlider != null) patienceSlider.value = _timer;

        // 시간 초과
        if (_timer <= 0)
        {
            HandleFail();
        }
    }

    public void MoveTo(Transform target)
    {
        _targetPoint = target;
        _isMoving = true;

        // 스프라이트 좌우 반전 (바라보는 방향)
        if (target.position.x < transform.position.x)
            transform.localScale = new Vector3(-1, 1, 1);
        else
            transform.localScale = new Vector3(1, 1, 1);
    }

    IEnumerator GeneralCustomerRoutine()
    {
        if (_roamPoints != null && _roamPoints.Length > 0)
        {
            int visitCount = Random.Range(1, 3);
            for (int i = 0; i < visitCount; i++)
            {
                Transform randomPoint = _roamPoints[Random.Range(0, _roamPoints.Length)];
                MoveTo(randomPoint);

                yield return new WaitUntil(() => !_isMoving);
                yield return new WaitForSeconds(Random.Range(1.0f, 2.0f));
            }
        }

        MoveTo(_finalDestination);
    }

    public bool ReceiveItem(ItemData item)
    {
        if (item != wantedItem) return false;

        currentReceivedCount++;
        UpdateCountUI();

        if (currentReceivedCount >= wantedCount)
        {
            HandleSuccess();
        }
        return true;
    }

    void HandleSuccess()
    {
        _isActive = false;
        int reward = basePrice * wantedCount;

        if (type == CustomerType.Pickup)
        {
            reward *= 2;
            Debug.Log("픽업 배송 성공! (2배 보상)");
        }

        MoneyManager.Instance.AddBion(reward);
        Leave();
    }

    void HandleFail()
    {
        _isActive = false;

        if (type == CustomerType.Pickup)
        {
            int penalty = basePrice * wantedCount;
            Debug.Log($"픽업 시간 초과! 위약금: -{penalty}");
            MoneyManager.Instance.ApplyPenalty(penalty);
        }
        else
        {
            Debug.Log("일반 손님이 떠났습니다. (패널티 없음)");
        }

        Leave();
    }

    void UpdateCountUI()
    {
        if (countText != null) countText.text = $"x{wantedCount - currentReceivedCount}";
    }

    void Leave()
    {
        if (CustomerManager.Instance != null)
            CustomerManager.Instance.OnCustomerLeft(this);

        Destroy(gameObject);
    }
}