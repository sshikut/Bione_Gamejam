using UnityEngine;

public abstract class WeatherEffectSO : ScriptableObject
{
    [Header("UI Info")]
    public string weatherName;       // 화면에 띄울 이름 (예: "폭염")
    public Weather type;         // Enum (코드 식별용)
    public Sprite weatherIcon;       // UI 아이콘 (해, 비구름, 눈송이 등)
    public string description;       // 설명 (예: "냉동 식품이 빨리 녹습니다.")

    // 로직 함수
    public abstract float GetDecayMultiplier(CargoProperty cargo);
}
