using UnityEngine;

[CreateAssetMenu(menuName = "Weather/ColdWave")]
public class ColdWaveEffect : WeatherEffectSO
{
    public override float GetDecayMultiplier(CargoProperty cargo)
    {
        if (cargo.StorageType == StorageType.RoomTemp) return 0f;

        if (!cargo.IsNearHeat && 
            (cargo.StorageType == StorageType.Liquid 
            || cargo.StorageType == StorageType.Refrigerated)) return -1f;

        if (cargo.IsNearCold && !cargo.IsNearHeat) return -1.0f;
        if ((cargo.IsNearHeat && !cargo.IsNearCold) && cargo.StorageType != StorageType.Heated) return 2.0f;
        if (cargo.IsNearHeat && cargo.IsNearCold) return 1f;

        return 1f;
    }
}
