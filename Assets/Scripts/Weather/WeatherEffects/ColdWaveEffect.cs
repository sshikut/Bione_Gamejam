using UnityEngine;

[CreateAssetMenu(menuName = "Weather/ColdWave")]
public class ColdWaveEffect : WeatherEffectSO
{
    public override float GetDecayMultiplier(CargoProperty cargo)
    {
        return 1.0f;
    }
}
