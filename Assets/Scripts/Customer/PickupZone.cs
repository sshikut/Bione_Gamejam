using UnityEngine;
using System.Collections.Generic; 

public class PickupZone : MonoBehaviour
{
    [Header("Zone Settings")]
    public CustomerType targetType;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            var player = collision.GetComponent<PlayerInteraction>();
            if (player == null) return;

            List<Cargo> holdingList = player.GetHoldingCargos();

            if (holdingList.Count == 0) return;

            Customer targetCustomer = null;

            if (targetType == CustomerType.Pickup)
            {
                targetCustomer = CustomerManager.Instance.GetPickupCustomer();
            }

            foreach (var cargo in holdingList)
            {
                if (TrySellCargo(cargo, targetType))
                {
                    player.OnItemSold(cargo);
                    break;
                }
            }
        }
    }

    private bool TrySellCargo(Cargo cargo, CustomerType zoneType)
    {
        Customer targetCustomer = null;

        if (zoneType == CustomerType.Pickup)
        {
            targetCustomer = CustomerManager.Instance.GetPickupCustomer();
        }

        if (targetCustomer != null && targetCustomer.ReceivePickupItem(cargo.data))
        {

            GridManager.Instance.UnregisterCargo(cargo.CurrentGridPos);

            return true;
        }

        return false;
    }
}