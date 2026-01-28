using UnityEngine;
using System.Collections.Generic; // 리스트 사용

public class DeliveryZone : MonoBehaviour
{
    [Header("Zone Settings")]
    [Tooltip("General: 진열대 포탈, Pickup: 카운터")]
    public CustomerType targetType;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 플레이어인지 확인
        if (collision.CompareTag("Player"))
        {
            // PlayerInteraction 스크립트 가져오기
            var player = collision.GetComponent<PlayerInteraction>();
            if (player == null) return;

            // 2. 플레이어가 들고 있는 리스트 가져오기
            List<Cargo> holdingList = player.GetHoldingCargos();

            // 빈손이면 무시
            if (holdingList.Count == 0) return;

            // 3. 현재 구역에 맞는 손님(Target Customer) 찾기
            Customer targetCustomer = null;

            if (targetType == CustomerType.Pickup)
            {
                // 픽업 구역 -> 픽업 손님 조회
                targetCustomer = CustomerManager.Instance.GetPickupCustomer();
            }
            else
            {
                // 진열대 구역 -> 일반 손님들 중 하나 찾기 (단순화)
                // 들고 있는 화물 중 하나라도 원하는 손님이 있는지 역으로 탐색
                Customer[] generals = FindObjectsOfType<Customer>();

                // (최적화를 위해 간단히 처리: 들고 있는 첫 번째 물건을 원하는 손님 찾기)
                // 실제로는 이중 반복문이 필요할 수 있지만, 우선순위 로직으로 처리
            }

            // ★ 리스트 순회: 들고 있는 물건 중에 손님이 원하는 게 있는지 검사
            // (Foreach로 돌리다가 찾으면 팔고 break)

            // 리스트 원본을 직접 수정하면 안 되므로 복사본이나 인덱스로 처리하지 않고,
            // 찾은 즉시 break 할 것이므로 foreach 가능
            foreach (var cargo in holdingList)
            {
                // 손님이 있고 + 그 손님이 원하는 물건이 이 화물이라면?
                // (일반 손님 로직도 여기서 같이 처리하기 위해 함수로 뺌)
                if (TrySellCargo(cargo, targetType))
                {
                    // 판매 성공 시 플레이어에게 알리고 종료 (한 번에 하나씩만 팜)
                    player.OnItemSold(cargo);
                    break;
                }
            }
        }
    }

    // 판매 시도 헬퍼 함수
    private bool TrySellCargo(Cargo cargo, CustomerType zoneType)
    {
        Customer targetCustomer = null;

        if (zoneType == CustomerType.Pickup)
        {
            targetCustomer = CustomerManager.Instance.GetPickupCustomer();
        }
        else // General
        {
            // 일반 손님들 중 이 화물을 원하는 사람이 있나?
            Customer[] generals = FindObjectsOfType<Customer>();
            foreach (var c in generals)
            {
                if (c.type == CustomerType.General && c.wantedItem == cargo.data)
                {
                    targetCustomer = c;
                    break;
                }
            }
        }

        // 손님을 찾았고, 물건 전달에 성공했다면 true
        if (targetCustomer != null && targetCustomer.ReceiveItem(cargo.data))
        {
            // 화물 장부(GridManager)에서도 지우기
            // (플레이어가 들고 있을 때 이미 장부에서 빠졌을 수도 있지만 안전하게 체크)
            GridManager.Instance.UnregisterCargo(cargo.CurrentGridPos);

            Debug.Log($"[{zoneType}] 판매 성공: {cargo.data.itemName}");
            return true;
        }

        return false;
    }
}