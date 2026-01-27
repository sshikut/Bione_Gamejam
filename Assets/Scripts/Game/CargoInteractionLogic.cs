using UnityEngine;

public static class CargoInteractionLogic
{
    public static float CalculateInfluence(StorageType myType, StorageType neighborType)
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
        float influence = 0f;

        switch (myType)
        {
            // 1. 내가 냉동/냉장/액상일 때 온장이 주변에 있음
            case StorageType.Frozen:
            case StorageType.Refrigerated:
            case StorageType.Liquid:
            
                if (neighborType == StorageType.Heated)
                {
                    influence -= 5.0f;
                }
                break;
        }

        return influence;
    }
}