using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AugmentCardUI : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;             // 아이콘
    public TextMeshProUGUI nameText;    // 이름
    public TextMeshProUGUI descText;    // 설명
    public TextMeshProUGUI tierText;    // 등급 (1성, 2성...)
    public Image borderImage;           // 테두리 (등급별 색상 변경용)

    private AugmentSO _myAugment;       // 내가 담고 있는 데이터
    private AugmentSelector _selector;  // 나를 관리하는 부모

    // 초기화 함수 (데이터 세팅)
    public void Setup(AugmentSO data, AugmentSelector selector)
    {
        _myAugment = data;
        _selector = selector;

        // 1. 텍스트 & 아이콘 적용
        nameText.text = data.augmentName;
        descText.text = data.description;
        iconImage.sprite = data.icon;

        // 2. 등급 표시 (티어)
        tierText.text = GetTierText(data.tier);

        // 3. 등급별 테두리 색상 변경 (예시)
        if (borderImage != null)
        {
            switch (data.tier)
            {
                case 1: borderImage.color = Color.gray; break;   // 실버
                case 2: borderImage.color = Color.yellow; break; // 골드
                case 3: borderImage.color = Color.cyan; break;   // 프리즘
            }
        }

        // 4. 버튼 리스너 연결
        Button btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners(); // 재사용 시 기존 연결 제거
        btn.onClick.AddListener(OnCardClicked);
    }

    // 클릭 시 실행
    private void OnCardClicked()
    {
        // 부모(매니저)에게 "나 선택됐어!"라고 알림
        _selector.SelectAugment(_myAugment);
    }

    private string GetTierText(int tier)
    {
        switch (tier)
        {
            case 1: return "1성 [일반]";
            case 2: return "2성 [희귀]";
            case 3: return "3성 [전설]";
            default: return "";
        }
    }
}