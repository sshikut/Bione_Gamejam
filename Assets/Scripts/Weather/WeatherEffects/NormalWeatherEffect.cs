using UnityEngine;

[CreateAssetMenu(menuName = "Weather/Normal")]
public class NormalWeatherEffect : WeatherEffectSO
{
    public override float GetDecayMultiplier(CargoProperty cargo)
    {
        if (cargo.StorageType == StorageType.RoomTemp) return 0f;

        return 1f;
    }
}
