using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Weather/HeatWave")]
public class HeatWaveEffect : WeatherEffectSO
{
    public override float GetDecayMultiplier(CargoProperty cargo)
    {
        // 1. 실온 보관 아이템은 날씨 영향 없음
        if (cargo.StorageType == StorageType.RoomTemp) return 0f;

        // 3. 온장 물품이 아니거나 냉기 없으면 3배 가속
        float multiplier = 1.0f;
        if (!cargo.IsNearCold && cargo.StorageType != StorageType.Heated) multiplier = 3.0f;

        return multiplier;
    }
}
