using UnityEngine;
using System;

public class GameTimeManager : MonoBehaviour
{
    public static GameTimeManager Instance;

    [Header("Game Settings")]
    public int maxDays = 14;
    public float tickInterval = 1.0f;
    public AugmentSelector augmentUI;

    [Header("Phase Durations (Seconds)")]
    // 기획서에 따른 시간 설정
    public float morningDuration = 210f; // 3분 30초
    public float dayServiceDuration = 120f; // 2분
    public float midDeliveryDuration = 150f; // 2분 30초
    public float nightServiceDuration = 120f; // 2분
    public float nightShiftDuration = 210f; // 3분 30초

    [Header("Current Status")]
    public int currentDay = 1;
    public float currentPhaseTimer; // 현재 단계의 남은 시간
    public GameState currentState = GameState.Preparation;
    public bool isNightMode = false;

    // --- [이벤트] ---
    // 상태가 바뀔 때마다 알림 (UI 갱신, 스포너 작동 등에 사용)
    public event Action<GameState> OnStateChanged;
    public event Action OnNightThemeStart; // 밤 테마 전환용
    public event Action OnMorningThemeStart; // 아침 테마 복귀용
    public event Action<int> OnDayEnded; // 정산용
    public event Action OnGameClear;
    public event Action OnTickEvent;
    public event Action<int> OnDayChanged; // 날씨 변화용

    private float _tickTimer;
    private bool _hasSwitchedToNight = false;

    // ★ 현재 게임이 진행 중인지 확인하는 프로퍼티 (5단계 중 하나라면 true)
    public bool IsPlaying
    {
        get
        {
            return currentState == GameState.MorningShift ||
                   currentState == GameState.DayService ||
                   currentState == GameState.MidDelivery ||
                   currentState == GameState.NightService ||
                   currentState == GameState.NightShift;
        }
    }

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    private void Start()
    {
        currentDay = 1;
        ChangeState(GameState.Preparation);
    }

    private void Update()
    {
        // 진행 중인 상태가 아니면 타이머 멈춤
        if (!IsPlaying) return;

        float deltaTime = Time.deltaTime;

        // 1. 단계별 타이머 감소
        currentPhaseTimer -= deltaTime;

        // 2. 틱 이벤트 (신선도 등)
        _tickTimer += deltaTime;
        if (_tickTimer >= tickInterval)
        {
            _tickTimer = 0;
            OnTickEvent?.Invoke();
        }

        // 3. ★ 중간 배송(MidDelivery) 중 밤 테마 전환 체크
        if (currentState == GameState.MidDelivery && !_hasSwitchedToNight)
        {
            // 시간이 절반 이하로 떨어지면 밤 모드 발동
            if (currentPhaseTimer <= (midDeliveryDuration / 2f))
            {
                TriggerNightTheme();
            }
        }

        // 4. 시간이 다 되면 다음 단계로
        if (currentPhaseTimer <= 0)
        {
            NextPhase();
        }
    }

    // --- [로직 제어] ---

    // 게임 시작 (UI 버튼)
    public void StartGame()
    {
        currentDay = 1;
        isNightMode = false;
        ChangeState(GameState.MorningShift); // 첫 단계 시작
        OnDayChanged?.Invoke(currentDay);
    }

    // 단계 전환 로직 (자동 호출)
    private void NextPhase()
    {
        switch (currentState)
        {
            case GameState.MorningShift:
                ChangeState(GameState.DayService);
                break;
            case GameState.DayService:
                ChangeState(GameState.MidDelivery);
                break;
            case GameState.MidDelivery:
                ChangeState(GameState.NightService);
                break;
            case GameState.NightService:
                ChangeState(GameState.NightShift);
                break;
            case GameState.NightShift:
                FinishDay(); // 하루 끝
                break;
        }
    }

    // 상태 변경 및 시간 설정 (핵심 함수)
    private void ChangeState(GameState newState)
    {
        currentState = newState;

        // 각 상태별 시간 세팅
        switch (newState)
        {
            case GameState.MorningShift: currentPhaseTimer = morningDuration; break;
            case GameState.DayService: currentPhaseTimer = dayServiceDuration; break;
            case GameState.MidDelivery:
                currentPhaseTimer = midDeliveryDuration;
                _hasSwitchedToNight = false; // 플래그 초기화
                break;
            case GameState.NightService: currentPhaseTimer = nightServiceDuration; break;
            case GameState.NightShift: currentPhaseTimer = nightShiftDuration; break;
        }

        Debug.Log($"상태 변경: {newState} (Day {currentDay})");
        OnStateChanged?.Invoke(newState); // 구독자들에게 알림
    }

    // 하루 종료 처리
    private void FinishDay()
    {
        ChangeState(GameState.DayEnded);
        OnDayEnded?.Invoke(currentDay); // 정산창 띄우기

        augmentUI.ShowSelectionUI();
    }

    // 다음 날 시작 (정산창 Next 버튼)
    public void ProceedToNextDay()
    {
        if (currentDay < maxDays)
        {
            currentDay++;

            // 아침 테마로 복귀
            isNightMode = false;
            OnMorningThemeStart?.Invoke();

            // 바로 다음 날 아침 시작
            ChangeState(GameState.MorningShift);

            Debug.Log($"{currentDay}일차 시작 알림 발송!");
            OnDayChanged?.Invoke(currentDay);
        }
        else
        {
            ChangeState(GameState.GameClear);
            OnGameClear?.Invoke();
        }
    }

    private void TriggerNightTheme()
    {
        _hasSwitchedToNight = true;
        isNightMode = true;
        Debug.Log("테마 변경: 밤");
        OnNightThemeStart?.Invoke();
    }

    // UI용 시간 포맷터
    public string GetFormattedTime()
    {
        if (!IsPlaying) return "00:00";
        int m = Mathf.FloorToInt(currentPhaseTimer / 60F);
        int s = Mathf.FloorToInt(currentPhaseTimer % 60F);
        return string.Format("{0:00}:{1:00}", m, s);
    }

    public void SkipPhase() // Test
    {
        // 게임 진행 중이 아니면 무시 (준비 화면이나 정산 창에서는 작동 X)
        if (!IsPlaying) return;

        Debug.Log("[Console] 단계를 강제로 건너뜁니다!");

        // private이었던 NextPhase를 내부에서 호출
        NextPhase();
    }

    public void TriggerGameOver(GameOverReason reason)
    {
        if (currentState == GameState.GameOver) return; // 이미 끝났으면 패스

        Debug.Log($"게임 오버 발생 사유: {reason}");

        // 상태 변경
        ChangeState(GameState.GameOver);

        // (선택) 화물 소각으로 인한 게임 오버라면 시간을 멈추거나 특수 연출 가능
        if (reason == GameOverReason.CargoBurned)
        {
            Time.timeScale = 0; // 게임 일시 정지 (긴박감!)
        }
    }
}