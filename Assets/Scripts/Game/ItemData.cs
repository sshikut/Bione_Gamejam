using UnityEngine;

// 프로젝트 창에서 우클릭 -> Create -> Game -> ItemData 로 파일을 만들 수 있게 함
[CreateAssetMenu(fileName = "New Item", menuName = "Game/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("Category")]
    public ItemCategory category = ItemCategory.Food; // 기본은 식품

    [Header("Visual")]
    public string itemName;      // 아이템 이름 (예: 사과)
    public Sprite icon;          // 아이템 이미지 (예: 사과 그림)
    public Color displayColor = Color.white; // (이미지가 없다면) 색깔로 구분

    [Header("Attributes")]
    public StorageType storageType; // 보관 속성 (냉장, 냉동 등)
    public int maxFreshness = 100;  // 최대 신선도 (물건마다 다를 수 있음)
    public int basePrice = 100;

    private void OnValidate()
    {
        switch (storageType)
        {
            case StorageType.Refrigerated:
            case StorageType.Liquid:
                maxFreshness = 101;
                break;

            case StorageType.Frozen:
            case StorageType.RoomTemp:
            case StorageType.Heated:
                maxFreshness = 100; // 기본값
                break;
        }
    }
}