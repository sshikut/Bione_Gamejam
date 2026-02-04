using UnityEngine;

[CreateAssetMenu(menuName = "Augment/Stat Bonus")]
public class StatAugment : AugmentSO
{
    [Header("Bonus Values")]
    public float speedAdd = 0f;      // 속도 증가량
    public int capacityAdd = 0;      // 용량 증가량
    public float resistanceAdd = 0f; // 저항력 증가량 (0.1 = 10%)

    public override void OnEquip(PlayerStats stats)
    {
        stats.moveSpeedBonus += speedAdd;
        stats.capacityBonus += capacityAdd;
        stats.weightResistance += resistanceAdd;

        Debug.Log($"[증강 장착] {augmentName}: 스탯 적용됨");
    }

    public override void OnUnequip(PlayerStats stats)
    {
        stats.moveSpeedBonus -= speedAdd;
        stats.capacityBonus -= capacityAdd;
        stats.weightResistance -= resistanceAdd;
    }
}