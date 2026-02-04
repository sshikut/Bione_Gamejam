using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("Base Stats (Default)")]
    public float baseMoveSpeed = 5.0f;     // 기본 이동 속도
    public int baseMaxCapacity = 3;       // 화물 최대 소지 개수
    public float baseInteractionRange = 1.5f;  // 화물을 들 수 있는 기본 거리
    public float baseWeightPenalty = 0.1f; // 물건 1개당 느려지는 비율 (10%)

    [Header("Modifiers (증강으로 더해지는 값)")]
    public float moveSpeedBonus = 0f;      // 추가 이동 속도
    public int capacityBonus = 0;          // 추가 소지량
    public float weightResistance = 0f;    // 무게 패널티 감소율 (0.0 ~ 1.0)

    [Header("Runtime Status")]
    public int currentCarryCount = 0;      // 현재 들고 있는 개수

    private void Awake()
    {
        Instance = this;
    }

    // 실제 이동 속도 계산 로직
    public int MaxCapacity => baseMaxCapacity + capacityBonus;

    // 2. 최종 이동 속도 (무게 패널티 반영)
    public float CurrentMoveSpeed
    {
        get
        {
            // A. 기본 속도 + 증강 보너스
            float speed = baseMoveSpeed + moveSpeedBonus;

            // B. 무게 패널티 계산
            // (1개당 10% 감소) - (저항력)
            // 예: 저항력이 0.5(50%)라면, 10% 느려질 게 5%만 느려짐
            float penaltyPerItem = Mathf.Max(0, baseWeightPenalty * (1.0f - weightResistance));
            float totalPenalty = currentCarryCount * penaltyPerItem;

            // 최대 90%까지만 느려지게 제한 (완전히 멈추면 안 되니까)
            totalPenalty = Mathf.Clamp(totalPenalty, 0f, 0.9f);

            // C. 최종 적용
            return speed * (1.0f - totalPenalty);
        }
    }
}