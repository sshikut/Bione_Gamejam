using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    public static WeatherManager Instance;

    [Header("Current Status")]
    public WeatherEffectSO currentWeatherEffect;
    public event Action<WeatherEffectSO> OnWeatherChanged;

    [Header("Weather Schedule")]
    private Dictionary<Weather, WeatherEffectSO> _weatherDatabase = new Dictionary<Weather, WeatherEffectSO>();

    private void Awake()
    {
        Instance = this;

        LoadWeatherDataAuto();
    }

    private void Start()
    {
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnDayChanged += HandleDayChange;
        }
    }
    private void OnDestroy()
    {
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnDayChanged -= HandleDayChange;
        }
    }
    void LoadWeatherDataAuto()
    {
        WeatherEffectSO[] loadedEffects = Resources.LoadAll<WeatherEffectSO>("WeatherData");

        // 2. 딕셔너리에 등록 (Key: Enum타입, Value: 파일)
        foreach (var effect in loadedEffects)
        {
            if (!_weatherDatabase.ContainsKey(effect.type))
            {
                _weatherDatabase.Add(effect.type, effect);
            }
        }

        Debug.Log($"날씨 데이터 {loadedEffects.Length}개 로드 완료!");
    }

    private void HandleDayChange(int newDay)
    {
        Weather targetType = Weather.Normal;

        // (스케줄표는 나중에 별도 데이터로 뺄 수도 있지만, 일단 여기 둬도 무방)
        switch (newDay)
        {
            case 2: targetType = Weather.HeatWave; break;
            case 4: targetType = Weather.RainySeason; break;
            case 6: targetType = Weather.ColdWave; break;
            default: targetType = Weather.Normal; break;
        }

        // 2. ★ 딕셔너리에서 해당 타입의 파일 꺼내오기 (if문 필요 없음!)
        if (_weatherDatabase.TryGetValue(targetType, out WeatherEffectSO foundEffect))
        {
            ChangeWeather(foundEffect);
        }
        else
        {
            Debug.LogError($"{targetType} 타입의 날씨 파일이 Resources 폴더에 없습니다!");
            // 비상시 기본값
            ChangeWeather(_weatherDatabase[Weather.Normal]);
        }
    }

    public void ChangeWeather(WeatherEffectSO newWeatherEffect)
    {
        currentWeatherEffect = newWeatherEffect;
        Debug.Log($"[WeatherManager] 날씨 적용 완료: {currentWeatherEffect.weatherName}");
        OnWeatherChanged?.Invoke(currentWeatherEffect);
    }
}
