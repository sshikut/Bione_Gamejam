using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class Shelf : MonoBehaviour
{
    [Header("Settings")]
    public StorageType shelfCategory; // 이 진열대가 취급하는 속성 (예: Frozen)
    public int maxCapacity = 10;          // 최대 진열 개수

    [Header("UI")]
    public TextMeshProUGUI stockText;         // 남은 수량 표시 (3D Text 등)

    // 진열된 상품들의 가격 정보를 담아둡니다 (FIFO: 먼저 넣은게 먼저 팔림)
    private Queue<int> _stockedItemPrices = new Queue<int>();

    // 현재 재고 수량
    public int CurrentStock => _stockedItemPrices.Count;

    private void Start()
    {
        UpdateUI();
    }

    // [플레이어 -> 진열대] 물건 채우기
    public bool TryAddStock(ItemData item)
    {
        // 1. 카테고리 불일치 or 꽉 참 체크
        if (item.storageType != shelfCategory) return false;
        if (CurrentStock >= maxCapacity) return false;

        // 2. 재고 추가 (가격 정보 저장)
        _stockedItemPrices.Enqueue(item.basePrice); // 아이템의 가격을 저장
        UpdateUI();
        return true;
    }

    // [손님 -> 진열대] 물건 사기
    public int TryTakeStock()
    {
        if (CurrentStock <= 0) return 0; // 재고 없음

        // 3. 재고 차감 및 해당 물건 가격 반환
        int price = _stockedItemPrices.Dequeue();
        UpdateUI();
        return price; // 판매된 금액 리턴
    }

    private void UpdateUI()
    {
        if (stockText != null)
        {
            stockText.text = $"{shelfCategory}\n{CurrentStock}/{maxCapacity}";
        }
    }
}