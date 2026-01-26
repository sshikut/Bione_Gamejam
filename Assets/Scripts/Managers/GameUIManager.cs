using UnityEngine;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI dayText;   // 예: "Day 1"
    public TextMeshProUGUI timeText;  // 예: "02:59"
    public GameObject resultPanel;    // 하루 끝나면 뜰 패널 (Button 포함)

    private void Start()
    {
        // 이벤트 구독 (옵저버 패턴)
        GameTimeManager.Instance.OnDayStart += HandleDayStart;
        GameTimeManager.Instance.OnDayEnd += HandleDayEnd;

        resultPanel.SetActive(false); // 시작 땐 끔
    }

    private void Update()
    {
        // 매 프레임 남은 시간 갱신
        if (GameTimeManager.Instance.currentState == GameState.Playing)
        {
            timeText.text = GameTimeManager.Instance.GetFormattedTime();
        }
    }

    // 하루 시작되면 할 일
    void HandleDayStart(int day)
    {
        dayText.text = $"Day {day}";
        resultPanel.SetActive(false);
    }

    // 하루 끝나면 할 일
    void HandleDayEnd()
    {
        timeText.text = "00:00";
        resultPanel.SetActive(true); // 정산 창 띄우기
    }

    // (버튼에 연결) 정산 창의 [다음 날로] 버튼
    public void OnClickNextDay()
    {
        GameTimeManager.Instance.ProceedToNextDay();
    }

    // (버튼에 연결) 시작 화면의 [게임 시작] 버튼
    public void OnClickStartGame()
    {
        GameTimeManager.Instance.StartDay();
    }
}