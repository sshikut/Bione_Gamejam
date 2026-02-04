using UnityEngine;
using System.Collections.Generic;

public class DeliveryZone : MonoBehaviour
{
    [Header("Link")]
    public Shelf linkedShelf; // 이 구역과 연결된 진열대 (에디터에서 할당)

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (linkedShelf == null) return;

        // 1. 플레이어 확인
        if (collision.CompareTag("Player"))
        {
            var player = collision.GetComponent<PlayerInteraction>();
            if (player == null) return;

            List<Cargo> holdingList = player.GetHoldingCargos();
            if (holdingList.Count == 0) return;

            // 2. 들고 있는 물건 중 진열대 속성에 맞는 것만 쏙쏙 빼서 넣기
            // 리스트를 순회하며 삭제해야 하므로 역순으로 돌거나 별도 리스트 사용
            for (int i = holdingList.Count - 1; i >= 0; i--)
            {
                Cargo cargo = holdingList[i];

                // 진열대에 넣기 시도 (속성 매칭 & 공간 확인 내부 수행)
                if (linkedShelf.TryAddStock(cargo.data))
                {
                    // 성공 시:
                    // A. 플레이어 손에서 제거 (OnItemSold 재활용하여 제거 처리)
                    player.OnItemSold(cargo);

                    // B. 월드상 화물(GridManager) 제거 (플레이어가 들고 있어서 이미 제거됐겠지만 확인사살)
                    GridManager.Instance.UnregisterCargo(cargo.CurrentGridPos);

                    Debug.Log($"[진열] {cargo.data.itemName} -> {linkedShelf.shelfCategory} 진열대");
                }
            }
        }
    }
}