using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Weather
{
    Normal,     // 평소
    HeatWave,   // 폭염
    RainySeason,// 장마
    ColdWave    // 한파
}

public enum GameState
{
    Preparation, // 시작 전 대기 (버튼 누르면 시작)
    // Playing,     // 게임 진행 중 (타이머 작동)

    MorningShift,   // 아침 교대 (3:30) - 물류 정리
    DayService,     // 낮 서비스 (2:00) - 손님 등장
    MidDelivery,    // 중간 배송 (2:30) - 2차 물류 & 밤 전환
    NightService,   // 밤 서비스 (2:00) - 손님 재등장
    NightShift,     // 저녁 교대 (3:30) - 마무리 정리

    DayEnded,    // 하루 종료 (정산 창)
    GameClear,   // 14일 완주
    GameOver     // 중간 파산 등
}


public enum GameOverReason
{
    Bankruptcy, // 파산
    CargoBurned // 화물 소각 (데드존)
}

public enum CustomerType
{
    General,
    Pickup
}

public enum StorageType
{
    RoomTemp,     // 실온 (기본, 일반 물건)
    Refrigerated, // 냉장
    Frozen,       // 냉동
    Heated,       // 온장
    Liquid        // 액상
}

public enum ItemCategory
{
    General, // 일반 (날씨 영향 X)
    Food     // 식품 (장마철 밀집 패널티 등 적용)
}

public enum CargoState
{
    Normal,     // 정상
    Wet,        // 젖음 (냉동이 녹아서 터짐)
    Spoiled,    // 상함 (악취 풍김)
    HeatedState, // 가열됨 (액상이 끓어서 열기 발산)
    FrozenBurst
}

public class Enums : MonoBehaviour
{

}
