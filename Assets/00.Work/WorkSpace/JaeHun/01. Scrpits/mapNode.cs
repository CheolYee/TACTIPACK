using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MapNode : MonoBehaviour
{
    [SerializeField] private Button mapButton;
    [SerializeField] private MapSO mapData;

    private Tween pulseTween; // DOTween 트윈 저장
    private Vector3 originalScale; // 원래 크기 저장

    private void Start()
    {
        if (transform.localScale.x < 0.5f)
            transform.localScale = Vector3.one;

        originalScale = transform.localScale;
        mapButton.onClick.AddListener(OnClickMap); // 버튼 이벤트에 자동 등록
        UpdateVisual();
    }

    private void OnDisable()
    {
        pulseTween?.Kill(); // 비활성화 시 꺼주기
    }

    public void UpdateVisual()
    {
        mapButton.interactable = !mapData.isLook;  // 만약 isLook가 true면 애도 true, false면 애도 false

        bool shouldPulse = !mapData.isLook && HasNextMapOpened();

        if (shouldPulse)
            StartPulse();
        else
            StopPulse();
    }

    private bool HasNextMapOpened()
    {
        // 다음 맵 중 하나라도 isLook이 true면 true 반환
        if (mapData.nextMap == null || mapData.nextMap.Length == 0)
            return false;

        foreach (var next in mapData.nextMap)
        {
            if (next != null && next.isLook)
                return true;
        }
        return false;
    }

    private void StartPulse()
    {
        // 이미 트윈이 실행 중이면 중복 방지
        if (pulseTween != null && pulseTween.IsActive()) return;

        // 커졌다 작아졌다 반복 (1배 → 1.15배 → 1배)
        pulseTween = transform.DOScale(originalScale * 1.15f, 0.7f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void StopPulse()
    {
        pulseTween?.Kill();
        transform.localScale = originalScale;
    }

    private void OnClickMap()
    {
        if (mapData.isLook) return; // 잠겨있으면 클릭 못하게 만들기

        MapManager.Instance.OnMapClicked(mapData);
    }
}
