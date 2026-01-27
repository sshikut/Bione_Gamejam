using UnityEngine;

public class CargoProperty : CargoTrait
{
    [Header("Data Source")]
    [Tooltip("이 화물의 종류를 결정하는 데이터 파일")]
    public ItemData data;

    [Header("Runtime Status")]
    [Tooltip("현재 남은 신선도")]
    public float currentFreshness;

    // 신선도 감소량 (기획에 따라 Inspector에서 조절 혹은 ItemData로 이동 가능)
    [SerializeField] private float _decayAmount = 1.0f;

    // --- [프로퍼티] ---

    // 데이터가 없으면 기본값(실온) 반환 (Null 에러 방지)
    public StorageType StorageType => data != null ? data.storageType : StorageType.RoomTemp;

    // 신선도가 0 이하인지 확인
    public bool IsSpoiled => currentFreshness <= 0;


    // --- [초기화 로직] ---

    // 1. Cargo.Start()에서 호출됨 (Trait 기본 초기화)
    public override void Initialize(Cargo cargo)
    {
        base.Initialize(cargo);

        // 만약 에디터에서 미리 배치해둔 화물이라서 data가 이미 Inspector에 들어있다면 바로 적용
        if (data != null)
        {
            ApplyData();
        }
    }

    // 2. Spawner가 생성 직후 데이터를 꽂아줄 때 호출
    public void SetData(ItemData newData)
    {
        data = newData;
        ApplyData();
    }

    // 실제 데이터를 적용하여 상태와 외형을 바꾸는 내부 함수
    private void ApplyData()
    {
        if (data == null) return;

        // A. 수치 초기화
        currentFreshness = data.maxFreshness;

        // B. 디버깅 편의를 위해 오브젝트 이름 변경 (예: Cargo_Milk)
        gameObject.name = $"Cargo_{data.itemName}";

        // C. 외형 업데이트
        UpdateVisuals();
    }

    // --- [게임 로직] ---

    // GameTimeManager 혹은 Cargo.cs의 코루틴에서 주기적으로 호출
    public override void OnTick()
    {
        if (data == null) return;
        if (IsSpoiled) return; // 이미 썩었으면 계산 중단

        // ★ 규칙: 실온(RoomTemp)을 제외한 나머지 속성들은 신선도가 감소함
        if (StorageType != StorageType.RoomTemp)
        {
            currentFreshness -= _decayAmount;

            if (currentFreshness <= 0)
            {
                currentFreshness = 0;
                OnSpoiled(); // 썩었을 때 처리
            }
        }

        // (추후 추가) 주변 화물과의 상호작용(디메리트) 체크
        // CheckSurroundings();
    }

    // --- [시각 효과] ---

    private void UpdateVisuals()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        // 데이터에 아이콘(Sprite)이 있으면 그걸 쓰고, 없으면 색깔만 바꿈
        if (data.icon != null)
        {
            sr.sprite = data.icon;
            sr.color = Color.white; // 스프라이트 본연의 색 유지
        }
        else
        {
            sr.sprite = null; // 스프라이트 제거 (네모 박스)
            sr.color = data.displayColor; // 지정된 색상 적용
        }
    }

    // 신선도가 0이 되었을 때 호출
    private void OnSpoiled()
    {
        Debug.Log($"{gameObject.name} 화물이 상했습니다!");

        // 예: 색을 검게 칠하거나, 파리 날리는 이펙트 추가
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = Color.gray;
    }
}