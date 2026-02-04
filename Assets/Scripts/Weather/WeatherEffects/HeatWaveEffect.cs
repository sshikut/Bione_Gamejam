using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Weather/HeatWave")]
public class HeatWaveEffect : WeatherEffectSO
{
    public override float GetDecayMultiplier(CargoProperty cargo)
    {
        if (cargo.StorageType == StorageType.RoomTemp) return 0f;
        if (cargo.StorageType == StorageType.Heated && !cargo.IsNearCold) return 1f;
        if (cargo.StorageType == StorageType.Heated && cargo.IsNearCold) return -1f;
        if (cargo.IsNearCold && (cargo.StorageType == StorageType.Liquid || cargo.StorageType == StorageType.Refrigerated))
        {
            return 0f;
        }

        return 3.0f;
    }
}
