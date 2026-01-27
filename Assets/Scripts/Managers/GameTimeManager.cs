using UnityEngine;
using System;

public enum GameState
{
    Preparation, // 시작 전 대기 (버튼 누르면 시작)
    Playing,     // 게임 진행 중 (타이머 작동)
    DayEnded,    // 하루 종료 (정산 창)
    GameClear,   // 14일 완주
    GameOver     // 중간 파산 등
}

public enum Weather
{
    Normal,     // 평소
    HeatWave,   // 폭염
    RainySeason,// 장마
    ColdWave    // 한파
}

public class GameTimeManager : MonoBehaviour
{
    public static GameTimeManager Instance;

    [Header("Environment")]
    public Weather currentWeather = Weather.Normal;

    [Header("Settings")]
    public int maxDays = 14;          // 총 14일
    public float minutesPerDay = 3f;  // 하루 = 3분 (조절 가능)
    public float tickInterval = 1.0f;

    [Header("Current Status (Read Only)")]
    public int currentDay = 1;
    public float currentDayTimeLeft;  // 남은 시간 (초)
    public GameState currentState = GameState.Preparation;

    // ★ 외부 시스템(스포너, UI)이 구독할 이벤트들
    public event Action<int> OnDayStart; // 인자: 시작된 날짜
    public event Action OnDayEnd;
    public event Action OnGameClear;

    public event Action OnTickEvent;

    private float _timer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 첫날 초기화
        currentDay = 1;
        currentState = GameState.Preparation;

        // (테스트용) 게임 시작하자마자 바로 Day 1 시작하고 싶으면 주석 해제
        // StartDay(); 
    }

    private void Update()
    {
        // 게임 진행 중일 때만 시간 흐름
        if (currentState == GameState.Playing)
        {
            currentDayTimeLeft -= Time.deltaTime;

            if (currentDayTimeLeft <= 0)
            {
                EndDay();
            }

            // 신선도 감소 로직, tickInterval 마다 감소
            _timer += Time.deltaTime;

            if (_timer >= tickInterval)
            {
                _timer = 0;
                OnTickEvent?.Invoke();
            }
        }
    }

    // --- [공용 함수들] ---

    // 1. 하루 시작 (UI 버튼 등에 연결)
    public void StartDay()
    {
        if (currentState == GameState.Playing) return; // 이미 진행 중이면 무시

        currentDayTimeLeft = minutesPerDay * 60f; // 분 -> 초 변환
        currentState = GameState.Playing;

        Debug.Log($"=== Day {currentDay} Start ===");

        // 구독자들에게 알림 (스포너야 일해라!, UI야 시간 띄워라!)
        OnDayStart?.Invoke(currentDay);
    }

    // 2. 하루 종료 (시간 다 됨)
    private void EndDay()
    {
        currentState = GameState.DayEnded;
        currentDayTimeLeft = 0;

        Debug.Log($"=== Day {currentDay} Ended ===");
        OnDayEnd?.Invoke(); // 정산 UI 띄우기 등

        // (선택) 여기서 바로 다음 날로 넘길지, 정산 창에서 '다음 날' 버튼을 누르게 할지 결정
        // 일단은 자동 진행이 아니라, 정산 창에서 NextDay()를 호출한다고 가정합니다.
    }

    // 3. 다음 날로 넘어가기 (정산 창의 'Next' 버튼)
    public void ProceedToNextDay()
    {
        if (currentDay < maxDays)
        {
            currentDay++;
            currentState = GameState.Preparation;

            // 바로 시작할지, 준비 단계에서 멈출지 결정 (여기선 바로 시작 예시)
            StartDay();
        }
        else
        {
            // 14일 끝!
            currentState = GameState.GameClear;
            Debug.Log("Game Clear! 14일 생존 성공!");
            OnGameClear?.Invoke();
        }
    }

    // UI 표시용 포맷팅 함수 (남은 시간 02:30 처럼 표시)
    public string GetFormattedTime()
    {
        if (currentState != GameState.Playing) return "00:00";

        int minutes = Mathf.FloorToInt(currentDayTimeLeft / 60F);
        int seconds = Mathf.FloorToInt(currentDayTimeLeft % 60F);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}