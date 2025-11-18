using _00.Work.WorkSpace.CheolYee._04.Scripts.Stages.Maps;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.JaeHun._01._Scrpits
{
    public class MapNode : MonoBehaviour
    {
        [SerializeField] private Button mapButton;
        [SerializeField] private MapSo mapData;

        private Tween _pulseTween; // DOTween 트윈 저장
        private Vector3 _originalScale; // 원래 크기 저장
        
        private const float PulseScaleMultiplier = 1.08f;
        private const float PulseDuration = 0.6f;

        public MapSo MapData => mapData;

        private void Start()
        {
            if (transform.localScale.x < 0.5f)
                transform.localScale = Vector3.one;

            _originalScale = transform.localScale;
            mapButton.onClick.AddListener(OnClickMap); // 버튼 이벤트에 자동 등록
            RefreshVisual();
        }

        private void OnDisable()
        {
            StopPulse();
        }
        public void RefreshVisual()
        {
            if (MapManager.Instance == null || mapData == null) return;

            var state = MapManager.Instance.GetNodeState(mapData);

            //활성 상태면 켜주기
            mapButton.interactable = state == MapNodeState.Available;

            switch (state)
            {
                case MapNodeState.Available:
                    StartPulse();
                    break;
                case MapNodeState.Locked:
                case MapNodeState.Cleared:
                    StopPulse();
                    break;
            }
        }

        private void StartPulse()
        {
            if (_pulseTween != null)
            {
                _pulseTween.Kill();
                _pulseTween = null;
            }

            transform.localScale = _originalScale;

            _pulseTween = transform
                .DOScale(_originalScale * PulseScaleMultiplier, PulseDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        private void StopPulse()
        {
            if (_pulseTween != null)
            {
                _pulseTween.Kill();
                _pulseTween = null;
            }

            transform.localScale = _originalScale;
        }

        private void OnClickMap()
        {
            if (MapManager.Instance == null) return;
            if (!MapManager.Instance.CanEnter(mapData)) return;
            
            MapManager.Instance.OnMapClickedFromUi(mapData);
        }

    }
}
