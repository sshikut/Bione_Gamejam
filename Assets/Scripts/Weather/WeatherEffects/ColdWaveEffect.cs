using UnityEngine;

[CreateAssetMenu(menuName = "Weather/ColdWave")]
public class ColdWaveEffect : WeatherEffectSO
{
    public override float GetDecayMultiplier(CargoProperty cargo)
    {
        if (cargo.StorageType == StorageType.Liquid || cargo.StorageType == StorageType.Refrigerated)
        {
            if (cargo.IsNearHeat) return 1.0f;

            return -1.0f;
        }

        if (cargo.IsNearCold && !cargo.IsNearHeat) return -1.0f;

        return 0f;
    }
}
