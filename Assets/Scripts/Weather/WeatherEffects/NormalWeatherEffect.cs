using UnityEngine;

[CreateAssetMenu(menuName = "Weather/Normal")]
public class NormalWeatherEffect : WeatherEffectSO
{
    public override float GetDecayMultiplier(CargoProperty cargo)
    {
        if (cargo.StorageType == StorageType.RoomTemp) return 0f;
        if (cargo.IsNearCold && !cargo.IsNearHeat) return -1.0f;
        if ((cargo.IsNearHeat && !cargo.IsNearCold) && cargo.StorageType != StorageType.Heated) return 2.0f;
        if (cargo.IsNearHeat && cargo.IsNearCold) return 1f;

        return 1f;
    }
}
