using UnityEngine;
using UnityEngine.UI; // 버튼 제어용
using TMPro;
using System.Collections.Generic; // TextMeshPro 사용

public class GameUIManager : MonoBehaviour
{
    [Header("UI References (Text)")]
    public TextMeshProUGUI dayText;       // "Day 1"
    public TextMeshProUGUI timeText;      // "02:59"
    public TextMeshProUGUI phaseText;     // "아침 교대", "낮 영업 중" 등 상태 표시
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI weatherText;

    [Header("UI References (Panels)")]
    public GameObject resultPanel;        // 정산 창 (하루 종료 시)
    public GameObject gameClearPanel;     // 엔딩 창 (14일 완료 시)

    [Header("Cargo Status UI")]
    public TextMeshProUGUI frozenBurstCountText;      // 동파
    public TextMeshProUGUI spoiledCountText;    // 상함
    public TextMeshProUGUI heatedStateCountText; // 가열
    public TextMeshProUGUI wetCountText;     // 녹음 

    [Header("Shelf Status UI")]
    public TextMeshProUGUI commonCountText;
    public TextMeshProUGUI heatedCountText;
    public TextMeshProUGUI frozenCountText;
    public TextMeshProUGUI refridgeratedCountText;
    public TextMeshProUGUI liquidCountText;

    public Shelf commonShelf;
    public Shelf heatedShelf;
    public Shelf frozenShelf;
    public Shelf refridgeratedShelf;
    public Shelf liquidShelf;

    private void Start()
    {
        // 1. 매니저가 존재하는지 확인 후 이벤트 구독
        if (GameTimeManager.Instance != null)
        {
            // 상태가 바뀔 때마다 (아침 -> 낮 -> 배송...) 호출
            GameTimeManager.Instance.OnStateChanged += HandleStateChange;

            // 하루가 끝나서 정산할 때 호출
            GameTimeManager.Instance.OnDayEnded += HandleDayEnded;

            // 게임 클리어 시 호출
            GameTimeManager.Instance.OnGameClear += HandleGameClear;

            GameTimeManager.Instance.OnTickEvent += UpdateCounts;

            GameTimeManager.Instance.OnTickEvent += UpdateShelfCounts;

            // 시작하자마자 현재 상태에 맞춰 UI 한 번 갱신
            HandleStateChange(GameTimeManager.Instance.currentState);
            UpdateDayText(GameTimeManager.Instance.currentDay);
        }

        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnBionChanged += UpdateMoneyUI;

            // 초기값 표시
            UpdateMoneyUI(MoneyManager.Instance.currentBion);
        }

        // 날씨 UI
        if (WeatherManager.Instance != null)
        {
            WeatherManager.Instance.OnWeatherChanged += UpdateWeatherUI;
        }

        // 패널 초기화 (꺼두기)
        if (resultPanel != null) resultPanel.SetActive(false);
        if (gameClearPanel != null) gameClearPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        // ★ 중요: 오브젝트가 사라질 때 구독 해제 (메모리 누수 및 에러 방지)
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnStateChanged -= HandleStateChange;
            GameTimeManager.Instance.OnDayEnded -= HandleDayEnded;
            GameTimeManager.Instance.OnGameClear -= HandleGameClear;
            GameTimeManager.Instance.OnTickEvent -= UpdateCounts;
            GameTimeManager.Instance.OnTickEvent -= UpdateShelfCounts;
        }
    }

    private void Update()
    {
        // 게임이 진행 중일 때만 시간 텍스트 갱신
        // (GameTimeManager에 새로 만든 IsPlaying 프로퍼티 활용)
        if (GameTimeManager.Instance != null && GameTimeManager.Instance.IsPlaying)
        {
            timeText.text = GameTimeManager.Instance.GetFormattedTime();
        }
    }

    // --- [이벤트 핸들러] ---

    // 1. 상태(GamePhase)가 바뀔 때 UI 갱신
    private void HandleStateChange(GameState state)
    {
        // 상태별 한글 텍스트 표시
        phaseText.text = GetKoreanStateName(state);

        // (선택 사항) 밤/낮에 따라 텍스트 색상 변경 연출
        if (state == GameState.NightService || state == GameState.NightShift)
        {
            phaseText.color = new Color(1f, 0.8f, 0.4f); // 노란빛 (밤)
        }
        else
        {
            phaseText.color = Color.white; // 흰색 (낮)
        }

        // 날짜 텍스트도 같이 갱신 (혹시 날짜가 바뀌었을 수 있으므로)
        UpdateDayText(GameTimeManager.Instance.currentDay);
    }

    // 2. 하루 종료 시 (정산 창)
    private void HandleDayEnded(int day)
    {
        timeText.text = "00:00"; // 시간 0으로 고정

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            // 여기에 "오늘의 수익: 000원" 같은 텍스트를 갱신하는 코드를 넣으면 됩니다.
        }
    }

    // 3. 게임 클리어 시
    private void HandleGameClear()
    {
        if (gameClearPanel != null)
        {
            gameClearPanel.SetActive(true);
        }
    }

    // --- [버튼 연결 함수] ---

    // 정산 창의 [다음 날로] 버튼에 연결
    public void OnClickNextDay()
    {
        // 정산 창 닫기
        if (resultPanel != null) resultPanel.SetActive(false);

        // 매니저에게 다음 날 시작 요청
        GameTimeManager.Instance.ProceedToNextDay();
    }

    // (선택) 시작 화면의 [게임 시작] 버튼
    public void OnClickStartGame()
    {
        GameTimeManager.Instance.StartGame();
    }

    public void OnClickSkipPhase()
    {
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.SkipPhase();
        }
    }

    // --- [헬퍼 함수] ---

    void UpdateDayText(int day)
    {
        dayText.text = $"Day {day}";
    }

    private void UpdateMoneyUI(int amount)
    {
        if (moneyText != null)
        {
            moneyText.text = $"{amount:N0} <color=#00FFFF>B</color>";
        }
    }

    // Enum을 한글로 예쁘게 변환해주는 함수
    string GetKoreanStateName(GameState state)
    {
        switch (state)
        {
            case GameState.Preparation: return "영업 준비 중";
            case GameState.MorningShift: return "아침 교대 (하역/정리)";
            case GameState.DayService: return "낮 영업 중 (손님)";
            case GameState.MidDelivery: return "중간 배송 시간";
            case GameState.NightService: return "밤 영업 중 (손님)";
            case GameState.NightShift: return "마감 교대 (정리)";
            case GameState.DayEnded: return "영업 종료";
            case GameState.GameClear: return "모든 계약 완료!";
            case GameState.GameOver: return "파산...";
            default: return "";
        }
    }

    // 날씨
    private void UpdateWeatherUI(WeatherEffectSO newWeather)
    {
        if (newWeather == null) return;

        if (weatherText != null) weatherText.text = newWeather.weatherName;
    }

    public void UpdateCounts()
    {
        int frozenBurst = 0;
        int spoiled = 0;
        int heatedState = 0;
        int wet = 0;

        // GridManager가 모든 화물 리스트를 관리한다고 가정
        // 만약 GridManager에 리스트가 없다면, FindObjectsOfType으로 찾거나 리스트를 만들어야 함
        List<Cargo> allCargos = GridManager.Instance.GetAllCargos();

        foreach (var cargo in allCargos)
        {
            if (cargo.property == null) continue;

            switch (cargo.property.currentState)
            {
                case CargoState.FrozenBurst: frozenBurst++; break;
                case CargoState.Spoiled: spoiled++; break;
                case CargoState.HeatedState: heatedState++; break;
                case CargoState.Wet: wet++; break;
            }
        }

        // UI 적용
        if (frozenBurstCountText != null) frozenBurstCountText.text = $"{frozenBurst}";
        if (spoiledCountText != null) spoiledCountText.text = $"{spoiled}";
        if (heatedStateCountText != null) heatedStateCountText.text = $"{heatedState}";
        if (wetCountText != null) wetCountText.text = $"{wet}";
    }

    public void UpdateShelfCounts()
    {
        commonCountText.text = commonShelf.CurrentStock.ToString();
        heatedCountText.text = heatedShelf.CurrentStock.ToString();
        frozenCountText.text = frozenShelf.CurrentStock.ToString();
        refridgeratedCountText.text = refridgeratedShelf.CurrentStock.ToString();
        liquidCountText.text = liquidShelf.CurrentStock.ToString();
    }
}