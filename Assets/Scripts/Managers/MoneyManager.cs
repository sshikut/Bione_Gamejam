using UnityEngine;
using System;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance;

    [Header("Settings")]
    public int startingBion = 0; // 초기 자금

    [Header("Runtime Status")]
    public int currentBion;       // 현재 보유 비온
    public int todayEarnedBion;   // 오늘 번 비온 (정산용)
    public int todayPenaltyBion;  // 오늘 잃은 비온 (벌금 등)

    public event Action<int> OnBionChanged;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    private void Start()
    {
        currentBion = startingBion;
        OnBionChanged?.Invoke(currentBion);

        if (GameTimeManager.Instance != null)
        {
            // GameTimeManager.Instance.OnDayStart += HandleDayStart;
        }
    }

    public void AddBion(int amount)
    {
        if (amount <= 0) return;

        currentBion += amount;
        todayEarnedBion += amount;

        Debug.Log($"수입: +{amount} 비온 (현재: {currentBion} B)");
        OnBionChanged?.Invoke(currentBion);
    }

    public bool SubtractBion(int amount)
    {
        if (amount <= 0) return false;

        if (currentBion < amount)
        {
            Debug.Log($"잔액 부족! (필요: {amount} B / 보유: {currentBion} B)");
            return false;
        }

        currentBion -= amount;

        Debug.Log($"지출: -{amount} 비온 (현재: {currentBion} B)");
        OnBionChanged?.Invoke(currentBion);
        return true;
    }

    public void ApplyPenalty(int amount)
    {
        currentBion -= amount;
        todayPenaltyBion += amount;
        Debug.Log($"벌금: -{amount} B");
        OnBionChanged?.Invoke(currentBion);
    }

    private void HandleDayStart(int day)
    {
        todayEarnedBion = 0;
        todayPenaltyBion = 0;
    }
}