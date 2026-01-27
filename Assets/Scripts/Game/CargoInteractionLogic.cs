using UnityEngine;

public static class CargoInteractionLogic
{
    /*
     * 1. 온장 주변의 냉동/냉장/액상 물류의 신선도 감소 (열기 효과)
     * 2. 냉동 주변의 냉장/액상 물류의 신선도 증가 = 부정적인 효과
     * 
     * 일단 기본적인 영향 효과이고, 추가로 이벤트 발생 
     * 
     * [녹음] 냉동 주변에 온장이 있을 때 신선도가 0이 되면 냉동 주변의 물품을 [젖음] 상태로 만들어 30의 신선도 감소, 냉장은 2배인 60 감소
     * [상함] 냉장 주변에 온장이 있을 때 신선도가 0이 되면 냉장 주변의 물품에 [악취] 스택을 10초에 1씩 축적, [악취] 스택이 5 축적될 시 주변 물품도 [상함] 상태. [상함] 상태는 단순히 신선도가 0이 되는 것과 같음
     * [가열] 액상 주변에 온장이 있을 대 신선도가 0이 되면 해당 액상 물품이 [가열] 상태가 되어 해당 물품 또한 주변 8칸에 온장과 같은 열기 영향을 미침
     * 
     * 또한 이상 기후 이벤트가 있는데, 
     * 
     * [폭염] 냉기에 영향을 받지 않는 냉동/액상/냉장의 신선도 감소 속도 3배 증가
     * [장마] 음식 물류의 상하좌우 4면 중 최소 1면이 빈 공간이어야 함, 4면 밀착 시 밀착된 음식 물품의 신선도 감소 3배, 신선도 0 도달 시 [상함] 상태, 대각선 배치는 괜찮음
     * [한파] 액상/냉장 물품의 신선도가 빠르게 증가
     */


    // A. 주변에서 열기가 뿜어져 나오는지 확인 (온장 속성 OR 가열된 상태)
    public static bool IsHeatSource(CargoProperty prop)
    {
        if (prop == null) return false;
        // 원래 온장이거나, 액상이 끓어서(HeatedState) 온장 효과를 내거나
        return prop.StorageType == StorageType.Heated || prop.currentState == CargoState.HeatedState;
    }

    // B. 주변에서 냉기가 뿜어져 나오는지 확인
    public static bool IsColdSource(CargoProperty prop)
    {
        if (prop == null) return false;
        return prop.StorageType == StorageType.Frozen;
    }

    public static float GetWeatherDecayMultiplier(Weather weather, StorageType type, bool isProtectedByCold)
    {
        // 1. 폭염: 냉기 보호 못 받는 냉동/냉장/액상은 3배 빨리 썩음
        if (weather == Weather.HeatWave)
        {
            if (!isProtectedByCold && (type == StorageType.Frozen || type == StorageType.Refrigerated || type == StorageType.Liquid))
            {
                return 3.0f;
            }
        }

        // 2. 한파: (질문하신 대로) 냉장/액상의 신선도가 빠르게 증가(회복/과냉각)
        // 이건 감소 배율이 아니라 회복 로직에서 처리해야 하므로 여기선 패스하거나 음수 반환 가능

        return 1.0f; // 기본 배율
    }

    public static bool CheckRainySeasonPenalty(Cargo myCargo)
    {
        // 상하좌우 4면 검사
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        int blockedSides = 0;

        foreach (var dir in dirs)
        {
            if (GridManager.Instance.IsOccupied(myCargo.CurrentGridPos + dir))
            {
                blockedSides++;
            }
        }

        // 4면이 다 막혀있으면 패널티 대상
        return blockedSides == 4;
    }
}