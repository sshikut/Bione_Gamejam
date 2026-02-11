using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal; // 2D 라이트 사용 시 필수

public class LightingManager : MonoBehaviour
{
    [Header("Global Light")]
    public Light2D globalLight; // 전체 화면을 비추는 Global Light 2D
    public float dayIntensity = 1.0f;   // 낮 밝기
    public float nightIntensity = 0.05f; // 밤 밝기
    public float fadeDuration = 5.0f;   // 어두워지는 시간 (5초)

    [Header("Night Lights")]
    [Tooltip("밤에 켜질 플레이어 조명과 편의점 조명들을 여기에 넣으세요")]
    public GameObject[] nightLightObjects; // 껐다 켰다 할 조명들
    public float lightsOnDelay = 1.0f; // 어두워진 뒤 조명 켜질 때까지 대기 시간

    private void Start()
    {
        // 1. 게임 시작 시 초기 상태 설정
        if (globalLight != null) globalLight.intensity = dayIntensity;
        SetNightLights(false); // 밤 조명 끄기

        // 2. GameTimeManager의 이벤트 구독
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnNightThemeStart += HandleNightStart;
            GameTimeManager.Instance.OnMorningThemeStart += HandleMorningStart;
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제 (메모리 누수 방지)
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnNightThemeStart -= HandleNightStart;
            GameTimeManager.Instance.OnMorningThemeStart -= HandleMorningStart;
        }
    }

    // --- [이벤트 핸들러] ---

    private void HandleNightStart()
    {
        StopAllCoroutines(); // 진행 중인 페이드가 있다면 중지
        StartCoroutine(TransitionToNight());
    }

    private void HandleMorningStart()
    {
        StopAllCoroutines();
        StartCoroutine(TransitionToDay());
    }

    // --- [코루틴: 밤으로 전환] ---
    private IEnumerator TransitionToNight()
    {
        Debug.Log("밤이 되었습니다. 조명을 어둡게 합니다.");

        // 1. 5초 동안 글로벌 라이트 어둡게 (Fade Out)
        float currentIntensity = globalLight.intensity;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            // Lerp를 사용하여 부드럽게 값 변경
            globalLight.intensity = Mathf.Lerp(currentIntensity, nightIntensity, timer / fadeDuration);
            yield return null;
        }
        globalLight.intensity = nightIntensity; // 확실하게 값 고정

        // 2. 1초 대기
        yield return new WaitForSeconds(lightsOnDelay);

        // 3. 플레이어/편의점 조명 켜기
        SetNightLights(true);
        Debug.Log("조명이 켜졌습니다.");
    }

    // --- [코루틴: 아침으로 전환] ---
    private IEnumerator TransitionToDay()
    {
        Debug.Log("아침이 밝아옵니다.");

        // 1. 밤 조명 즉시 끄기 (혹은 천천히 끄고 싶으면 순서 변경 가능)
        SetNightLights(false);

        // 2. 5초 동안 글로벌 라이트 밝게 (Fade In)
        float currentIntensity = globalLight.intensity;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            globalLight.intensity = Mathf.Lerp(currentIntensity, dayIntensity, timer / fadeDuration);
            yield return null;
        }
        globalLight.intensity = dayIntensity;
    }

    // 조명들 켜고 끄는 헬퍼 함수
    private void SetNightLights(bool isOn)
    {
        if (nightLightObjects == null) return;

        foreach (var lightObj in nightLightObjects)
        {
            if (lightObj != null) lightObj.SetActive(isOn);
        }
    }
}