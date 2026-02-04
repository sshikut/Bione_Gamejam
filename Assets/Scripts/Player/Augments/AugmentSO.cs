using UnityEngine;

public abstract class AugmentSO : ScriptableObject
{
    [Header("Info")]
    public string id;
    public string augmentName;
    [TextArea] public string description;
    public Sprite icon; // UI 표시용

    [Header("Tier")]
    public int tier = 1; // 1성, 2성, 3성...

    // ★ 장착했을 때 스탯을 변화시킴
    public virtual void OnEquip(PlayerStats stats) { }

    // ★ 해제했을 때 스탯을 원래대로 돌림
    public virtual void OnUnequip(PlayerStats stats) { }

    // (선택) 매 프레임 실행할 로직이 있다면 (예: 체력 재생)
    public virtual void OnUpdate(PlayerStats stats) { }
}