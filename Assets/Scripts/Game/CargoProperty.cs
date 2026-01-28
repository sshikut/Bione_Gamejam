using UnityEngine;

public class CargoProperty : CargoTrait
{
    [Header("Data Source")]
    [Tooltip("이 화물의 종류를 결정하는 데이터 파일")]
    public ItemData data;
    public Sprite defaultSpoiledIcon;
    public Sprite defaultWetIcon;
    public Sprite defaultHeatedIcon;
    public Sprite defaultFrozenBurstIcon;

    [Header("Runtime Status")]
    [Tooltip("현재 남은 신선도")]
    public float currentFreshness;

    [Header("State Info")]
    public CargoState currentState = CargoState.Normal;
    public int stenchStack = 0; // 악취 스택
    public int freezingStack = 0;
    private float _freezingTimer = 0f;

    private float _stenchTimer = 0f;

    [SerializeField] private bool _isNearHeat; // 열기 감지
    [SerializeField] private bool _isNearCold; // 냉기 감지

    // 신선도 감소량 (기획에 따라 Inspector에서 조절 혹은 ItemData로 이동 가능)
    [SerializeField] private float _decayAmount = 1.0f;

    public bool testBool = false;

    public StorageType StorageType
    {
        get
        {
            // 1. 데이터가 없으면 실온
            if (data == null) return StorageType.RoomTemp;

            // 2. 이미 망가진 상태(상함, 젖음, 가열 등)라면 -> 원래 속성 무시하고 '실온' 리턴
            // (이렇게 하면 더 이상 신선도 감소 로직이 돌지 않고, 주변에 속성 영향도 주지 않음)
            if (currentState != CargoState.Normal)
            {
                return StorageType.RoomTemp;
            }

            // 3. 정상 상태라면 -> 원래 데이터의 속성 반환
            return data.storageType;
        }
    }

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
        currentFreshness = 100f;

        // B. 디버깅 편의를 위해 오브젝트 이름 변경 (예: Cargo_Milk)
        gameObject.name = $"Cargo_{data.itemName}";

        // C. 외형 업데이트
        UpdateVisuals(currentState);
    }

    // --- [게임 로직] ---

    // GameTimeManager 혹은 Cargo.cs의 코루틴에서 주기적으로 호출
    public override void OnTick()
    {
        if (_cargo == null || data == null) return;
        // 0. 이미 상태가 변해서 기능을 상실했으면 로직 중단? 
        // (기획에 따라 Spoiled 상태여도 악취는 풍겨야 하므로 계속 돔)

        // 1. 주변 환경 스캔
        ScanSurroundings();

        // 2. 신선도 변화 계산
        CalculateFreshnessChange();
        CheckFreezingBurst();

        // 3. 상태 이상 효과 처리 (악취 전파 등)
        HandleStateEffects();
    }

    private void ScanSurroundings()
    {
        if (_cargo == null) return;

        _isNearHeat = false;
        _isNearCold = false;

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
                              new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1) }; // 8방향 열기 체크

        foreach (var dir in dirs)
        {
            Cargo neighbor = GridManager.Instance.GetCargoAt(_cargo.CurrentGridPos + dir);
            if (neighbor != null)
            {
                CargoProperty neighborProp = neighbor.GetComponent<CargoProperty>();
                if (neighborProp != null)
                {
                    // 열기는 8방향, 냉기는 4방향(또는 8방향) 등 기획에 따라 조절
                    if (CargoInteractionLogic.IsHeatSource(neighborProp) && (data.storageType != StorageType.Heated)) _isNearHeat = true;
                    if (CargoInteractionLogic.IsColdSource(neighborProp) && (data.storageType != StorageType.Frozen && data.storageType != StorageType.Heated)) _isNearCold = true;
                }
            }
        }
    }

    private void CalculateFreshnessChange()
    {
        if (IsInSafeZone())
        {
            return;
        }

        float changeAmount = 0f;

        Weather weather = GameTimeManager.Instance.currentWeather;

        // --- [감소 로직] ---
        if (StorageType != StorageType.RoomTemp)
        {
            float multiplier = 1.0f;

            // 1. 폭염 체크
            multiplier *= CargoInteractionLogic.GetWeatherDecayMultiplier(weather, StorageType, _isNearCold);

            // 2. 장마 체크 (식품 & 4면 밀착)
            if (weather == Weather.RainySeason && data.category == ItemCategory.Food)
            {
                if (CargoInteractionLogic.CheckRainySeasonPenalty(_cargo))
                {
                    multiplier *= 3.0f;
                }
            }

            // 3. 열기 효과 (주변에 온장이 있으면)
            if (_isNearHeat)
            {
                // 감소량 대폭 증가? 혹은 고정 수치 감소?
                // 예: 열기 있으면 2배 빨리 썩음
                multiplier += 1.0f;
            }

            changeAmount -= (_decayAmount * multiplier);
        }

        // --- [증가(회복/과냉각) 로직] ---
        // 한파 또는 냉동 주변
        bool isColdWave = (weather == Weather.ColdWave);

        if ((isColdWave || _isNearCold) && (StorageType == StorageType.Refrigerated || StorageType == StorageType.Liquid))
        {
            float recoveryMultiplier = 2.0f;

            changeAmount += (_decayAmount * recoveryMultiplier);
        }

        // --- [최종 적용] ---
        if (testBool) Debug.Log(changeAmount);
        ApplyFreshness(changeAmount);
    }

    private bool IsInSafeZone()
    {
        if (GridManager.Instance == null || _cargo == null) return false;

        Vector2Int pos = _cargo.CurrentGridPos;
        Vector2Int gridSize = GridManager.Instance.gridSize;

        // [삭제됨] X축(양쪽 끝) 체크 로직을 삭제했습니다.
        // bool isEdgeX = (pos.x == 0 || pos.x == gridSize.x - 1);
        // if (!isEdgeX) return false;

        // 2. Y축 체크: 중앙을 기준으로 세로 5칸 범위 안에 있는가?
        int centerY = gridSize.y / 2;
        int range = 2; // 5칸 (중앙 ±2)

        // 이제 X축 상관없이, 높이(Y)만 범위 내라면 무조건 안전지대입니다.
        if (pos.y >= centerY - range && pos.y <= centerY + range)
        {
            return true;
        }

        return false;
    }

    private void ApplyFreshness(float amount)
    {
        currentFreshness += amount;
        
        // 최대치 제한 (130 등)
        if (currentFreshness > data.maxFreshness) currentFreshness = data.maxFreshness;

        // 0 이하 도달 시 => [이벤트 트리거] 발동!
        if (currentFreshness <= 0)
        {
            currentFreshness = 0;
            if (currentState == CargoState.Normal) // 정상일 때만 트리거
            {
                Debug.Log("상해버림");
                TriggerZeroFreshnessEvent();
            }
        }
    }

    // ★ 신선도 0 도달 시 발생하는 특수 이벤트
    private void TriggerZeroFreshnessEvent()
    {
        // 1. [녹음] 냉동 + 열기 옆
        if (StorageType == StorageType.Frozen && _isNearHeat)
        {
            ChangeState(CargoState.Wet);
            // 주변에 '즉시' 물 뿌리기 (데미지)
            ExplodeWetDamage();
        }
        // 2. [상함] 냉장 + 열기 옆
        else if (StorageType == StorageType.Refrigerated && _isNearHeat)
        {
            // 즉시 상함 상태가 되진 않고, 악취를 풍기기 시작하는 상태(전구간)로 봐도 되고
            // 기획상으로는 "신선도 0 -> 악취 스택 시작 -> 5스택 -> 상함" 인가요?
            // "신선도가 0이 되면... 악취 스택을 축적" 이라고 하셨으니
            // 여기서는 일단 'Spoiled(상함)' 상태로 만들고, Spoiled 상태일 때 악취를 뿜게 합시다.
            ChangeState(CargoState.Spoiled);
        }
        // 3. [가열] 액상 + 열기 옆
        else if (StorageType == StorageType.Liquid && _isNearHeat)
        {
            ChangeState(CargoState.HeatedState); // 이제 나도 온장이다!
        }
        // 4. 일반적인 0 도달 (장마철 등)
        else
        {
            if (StorageType == StorageType.Frozen)
            {
                ChangeState(CargoState.Wet);
                ExplodeWetDamage();
            }
            else
            {
                ChangeState(CargoState.Spoiled); // 나머지는 그냥 상함
            }
        }
    }

    private void ChangeState(CargoState newState)
    {
        currentState = newState;
        Debug.Log($"{name} 상태 변경: {newState}");

        // 비주얼 변경 (색깔 등)
        UpdateVisuals(currentState);
    }

    // --- [상태별 지속 효과] ---
    private void HandleStateEffects()
    {
        // A. 내가 상했다면 -> 주변에 악취 뿌리기
        if (currentState == CargoState.Spoiled)
        {
            _stenchTimer += 1.0f; // OnTick이 1초라고 가정
            if (_stenchTimer >= 10.0f) // 10초마다
            {
                _stenchTimer = 0;
                SpreadStench();
            }
        }
    }

    private void CheckFreezingBurst()
    {
        if (currentFreshness > 100f)
        {
            _freezingTimer += 1.0f;

            if (_freezingTimer >= 10.0f)
            {
                _freezingTimer = 0f;
                freezingStack++;

                if (freezingStack >= 3)
                {
                    ChangeState(CargoState.FrozenBurst);
                }
            }
        }
        else
        {
            freezingStack = 0;
            _freezingTimer = 0f;
        }
    }

    // 주변에 악취 스택 쌓기
    private void SpreadStench()
    {
        // 상하좌우(또는 8방향) 이웃에게 스택 추가
        // ... (이웃 찾기 로직) ...
        // neighborProp.AddStenchStack(1);

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
                              new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1) };

        foreach (var dir in dirs)
        {
            Cargo neighbor = GridManager.Instance.GetCargoAt(_cargo.CurrentGridPos + dir);
            if (neighbor != null)
            {
                CargoProperty neighborProp = neighbor.GetComponent<CargoProperty>();
                if (neighborProp != null && neighborProp.StorageType != StorageType.RoomTemp)
                {
                    neighborProp.AddStenchStack(1);
                }
            }
        }
    }

    // 외부에서 악취 스택을 쌓을 때 호출
    public void AddStenchStack(int amount)
    {
        stenchStack += amount;
        if (stenchStack >= 5 && currentState == CargoState.Normal)
        {
            // 5스택 쌓이면 나도 상함!
            currentFreshness = 0;
            ChangeState(CargoState.Spoiled);
        }
    }

    // [녹음] 이벤트: 주변에 물 뿌리기 (데미지)
    private void ExplodeWetDamage()
    {
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
                              new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1) };

        foreach (var dir in dirs)
        {
            Cargo neighbor = GridManager.Instance.GetCargoAt(_cargo.CurrentGridPos + dir);
            if (neighbor != null)
            {
                CargoProperty neighborProp = neighbor.GetComponent<CargoProperty>();
                if (neighborProp != null)
                {
                    if (neighborProp.StorageType == StorageType.Refrigerated)
                    {
                        neighborProp.ApplyDamage(40f);
                    }
                    else if (neighborProp.StorageType != StorageType.RoomTemp)
                    {
                        neighborProp.ApplyDamage(20f);
                    }
                }
            }
        }
    }

    // --- [시각 효과] ---

    private void UpdateVisuals(CargoState cargo)
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

        switch (cargo)
        {
            case CargoState.Wet:
                if (defaultWetIcon != null)
                    sr.sprite = defaultWetIcon;
                break;

            case CargoState.HeatedState:
                if (defaultHeatedIcon != null)
                    sr.sprite = defaultHeatedIcon;
                break;

            case CargoState.Spoiled:
                if (defaultSpoiledIcon != null)
                    sr.sprite = defaultSpoiledIcon;
                break;

            case CargoState.FrozenBurst:
                if (defaultFrozenBurstIcon != null)
                {
                    sr.sprite = defaultFrozenBurstIcon;
                }
                break;
        }

    }

    public void ApplyDamage(float damageAmount)
    {
        // 피해는 음수로 처리해서 ApplyFreshness에 넘김
        ApplyFreshness(-damageAmount);
    }
}