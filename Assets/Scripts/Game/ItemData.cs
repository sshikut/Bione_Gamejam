using UnityEngine;

public enum StorageType
{
    RoomTemp,     // 실온 (기본, 일반 물건)
    Refrigerated, // 냉장
    Frozen,       // 냉동
    Heated,       // 온장
    Liquid        // 액상
}

// 프로젝트 창에서 우클릭 -> Create -> Game -> ItemData 로 파일을 만들 수 있게 함
[CreateAssetMenu(fileName = "New Item", menuName = "Game/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("Visual")]
    public string itemName;      // 아이템 이름 (예: 사과)
    public Sprite icon;          // 아이템 이미지 (예: 사과 그림)
    public Color displayColor = Color.white; // (이미지가 없다면) 색깔로 구분

    [Header("Attributes")]
    public StorageType storageType; // 보관 속성 (냉장, 냉동 등)
    public int maxFreshness = 100;  // 최대 신선도 (물건마다 다를 수 있음)
}