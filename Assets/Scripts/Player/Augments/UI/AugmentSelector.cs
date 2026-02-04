using UnityEngine;
using System.Collections.Generic;
using System.Linq; // 리스트 섞기용

public class AugmentSelector : MonoBehaviour
{
    [Header("UI References")]
    public GameObject selectionPanel;    // 전체 패널 (켰다 껐다 할 거)
    public AugmentCardUI[] cardSlots;    // 3개의 카드 UI 슬롯
    public GameObject gameUI;

    [Header("Settings")]
    public bool testMode = false;        // 테스트용 (게임 시작 시 바로 띄우기)

    private List<AugmentSO> _allAugments; // 전체 증강 풀

    private void Start()
    {
        // 1. Resources 폴더에서 모든 증강 데이터 로드
        // (폴더 경로: Resources/Augments)
        _allAugments = Resources.LoadAll<AugmentSO>("AugmentData").ToList();

        // UI 일단 꺼두기
        selectionPanel.SetActive(false);

        if (testMode) ShowSelectionUI();
    }

    // 외부(GameTimeManager)에서 이 함수를 호출해서 창을 띄움
    public void ShowSelectionUI()
    {
        if (_allAugments.Count == 0)
        {
            Debug.LogError("로드된 증강이 없습니다! Resources/Augments 폴더를 확인하세요.");
            return;
        }
        gameUI.SetActive(false);
        selectionPanel.SetActive(true);
        Time.timeScale = 0; // 게임 일시정지 (선택하는 동안 멈춤)

        // 2. 랜덤으로 3개 뽑기 (중복 없이)
        // 리스트를 셔플하고 앞에서 3개 가져오기
        List<AugmentSO> randomPicks = GetRandomAugments(3);

        // 3. UI 슬롯에 데이터 채워넣기
        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (i < randomPicks.Count)
            {
                cardSlots[i].gameObject.SetActive(true);
                // 카드에게 데이터와 나 자신(Selector)을 넘겨줌
                cardSlots[i].Setup(randomPicks[i], this);
            }
            else
            {
                cardSlots[i].gameObject.SetActive(false); // 데이터 부족 시 숨김
            }
        }
    }

    // 카드에서 호출하는 함수
    public void SelectAugment(AugmentSO chosenAugment)
    {
        // 1. 매니저에 증강 등록
        AugmentManager.Instance.AddAugment(chosenAugment);

        // 2. UI 닫기 & 게임 재개
        CloseSelectionUI();
    }

    private void CloseSelectionUI()
    {
        selectionPanel.SetActive(false);
        gameUI.SetActive(true);
        Time.timeScale = 1; // 시간 다시 흐름

        // 3. 다음 날로 진행 (GameTimeManager)
        // (기획에 따라 여기서 바로 아침을 시작할지, 정산창으로 갈지 결정)
        // 예: GameTimeManager.Instance.StartNextDay(); 

        Debug.Log("증강 선택 완료, 창 닫음.");
    }

    // 중복 없는 랜덤 뽑기 헬퍼 함수
    private List<AugmentSO> GetRandomAugments(int count)
    {
        // 이미 획득한 증강은 제외할지 여부는 기획에 따라 결정 (여기선 포함)
        // Fisher-Yates Shuffle 알고리즘 등을 써도 되지만, Linq로 간단하게 처리
        return _allAugments.OrderBy(x => Random.value).Take(count).ToList();
    }
}