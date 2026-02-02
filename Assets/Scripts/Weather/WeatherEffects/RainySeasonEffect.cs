using UnityEngine;

[CreateAssetMenu(menuName = "Weather/RainySeason")]
public class RainySeasonEffect : WeatherEffectSO
{
    public override float GetDecayMultiplier(CargoProperty cargo)
    {
        return 1f;
    }
}
