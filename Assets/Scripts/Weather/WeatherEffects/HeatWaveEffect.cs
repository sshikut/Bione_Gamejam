using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Weather/HeatWave")]
public class HeatWaveEffect : WeatherEffectSO
{
    public override float GetDecayMultiplier(CargoProperty cargo)
    {
        // 1. 실온은 영향 없음
        if (cargo.StorageType == StorageType.RoomTemp) return 0f;

        // 2. [온장 물품] 예외 처리
        if (cargo.StorageType == StorageType.Heated)
        {
            // 규칙: "온장이 냉동 옆에 있으면? 신선도 + 1" (회복)
            if (cargo.IsNearCold) return -1.0f;

            // 그 외(냉기 없는 온장)는 폭염의 직접적 타겟이 아니므로 기본 감소(1배) 유지
            return 1.0f;
        }

        // 3. [냉동 / 냉장 / 액상] 물품 로직

        // 중요 규칙: "열기/냉기 둘 다 받으면? = 폭염 효과만 (x5)"
        // (냉기의 보호 효과가 무효화됨)
        if (cargo.IsNearHeat && cargo.IsNearCold)
        {
            return 5.0f;
        }

        // 4. 냉기 보호 (위의 '둘 다 받음' 조건을 통과했으므로 여기서는 '냉기만' 있는 상태)
        // 규칙: "냉동 주변 액상/냉장 신선도 증감 없음"
        // (기획서 맥락상 냉동 물품도 냉기 옆에 있으면 보호받는 것이 타당하므로 포함)
        if (cargo.IsNearCold)
        {
            return 0f;
        }

        // 5. 기본 폭염 효과
        // 규칙: "기본으로 온장 제외 신선도 감소 x5"
        return 5.0f;
    }
}
