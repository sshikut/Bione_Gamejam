using UnityEngine;

public enum StorageType
{
    RoomTemp,     // 실온 (기본, 일반 물건)
    Refrigerated, // 냉장
    Frozen,       // 냉동
    Heated,       // 온장
    Liquid        // 액상
}

public enum ItemCategory
{
    General, // 일반 (날씨 영향 X)
    Food     // 식품 (장마철 밀집 패널티 등 적용)
}

public enum CargoState
{
    Normal,     // 정상
    Wet,        // 젖음 (냉동이 녹아서 터짐)
    Spoiled,    // 상함 (악취 풍김)
    HeatedState // 가열됨 (액상이 끓어서 열기 발산)
}

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

    private void OnValidate()
    {
        switch (storageType)
        {
            case StorageType.Refrigerated:
            case StorageType.Liquid:
            case StorageType.Frozen:
                maxFreshness = 101;
                break;

            case StorageType.RoomTemp:
            case StorageType.Heated:
                maxFreshness = 100; // 기본값
                break;
        }
    }
}