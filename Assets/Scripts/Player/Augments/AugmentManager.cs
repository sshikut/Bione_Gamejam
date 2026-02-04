using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

public class AugmentManager : MonoBehaviour
{
    public static AugmentManager Instance;

    [Header("Inventory")]
    public List<AugmentSO> acquiredAugments = new List<AugmentSO>();

    public List<AugmentSO> allAugments = new List<AugmentSO>();

    private void Awake()
    {
        Instance = this;
        LoadResources();
    }

    void LoadResources()
    {
        // ItemData 자동 로드 (Resources/ItemData 폴더)
        AugmentSO[] loadedData = Resources.LoadAll<AugmentSO>("AugmentData");
        allAugments = loadedData.ToList();
    }

    // 증강 획득
    public void AddAugment(AugmentSO newAugment)
    {
        // 1. 리스트에 추가
        acquiredAugments.Add(newAugment);

        // 2. 효과 적용 (OnEquip)
        if (PlayerStats.Instance != null)
        {
            newAugment.OnEquip(PlayerStats.Instance);
        }

        Debug.Log($"증강 획득! [{newAugment.augmentName}]");

        // (UI 갱신 코드 추가 가능)
    }

    // 증강 제거 (필요하다면)
    public void RemoveAugment(AugmentSO augmentToRemove)
    {
        if (acquiredAugments.Contains(augmentToRemove))
        {
            acquiredAugments.Remove(augmentToRemove);

            if (PlayerStats.Instance != null)
            {
                augmentToRemove.OnUnequip(PlayerStats.Instance);
            }
        }
    }
}