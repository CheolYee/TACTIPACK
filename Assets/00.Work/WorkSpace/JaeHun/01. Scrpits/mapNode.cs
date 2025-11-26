using System;
using System.Collections.Generic;
using System.Text;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Enemies;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Stages.Maps;
using _00.Work.WorkSpace.CheolYee._04.Scripts.UI;
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
        
        private TooltipTarget _tooltipTarget;

        private Tween _pulseTween; // DOTween 트윈 저장
        private Vector3 _originalScale; // 원래 크기 저장
        
        private const float PulseScaleMultiplier = 1.08f;
        private const float PulseDuration = 0.6f;

        public MapSo MapData => mapData;

        private void Awake()
        {
            _tooltipTarget = GetComponent<TooltipTarget>();
        }

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
            
            UpdateTooltip();
        }

        private void UpdateTooltip()
        {
            if (_tooltipTarget == null || mapData == null)
                return;

            string title = string.IsNullOrEmpty(mapData.mapName)
                ? "알 수 없는 맵"
                : mapData.mapName;

            string body = BuildTooltipBody(mapData);

            // TooltipTarget 안의 SetText 사용
            _tooltipTarget.SetText(title, body);
        }

        private string BuildTooltipBody(MapSo map)
        {
            var sb = new StringBuilder();

            // 전투 방인지 판별 (일반 + 랜덤)
            bool isCombatMap = map.mapType == MapType.Enemy ||
                               map.mapType == MapType.Random;
            
            if (!isCombatMap)
                return sb.ToString();

            var stage = map.stageData;
            if (stage == null || stage.enemies == null || stage.enemies.Count == 0)
                return sb.ToString();

            // 빈 줄 하나
            sb.AppendLine();

            // EnemyId 기준으로 묶기
            var groups = new Dictionary<int, (EnemyDefaultData data, int count)>();

            foreach (var entry in stage.enemies)
            {
                if (entry == null || entry.enemyData == null)
                    continue;

                var data = entry.enemyData;
                int id = data.EnemyId;

                if (!groups.TryGetValue(id, out var current))
                {
                    groups[id] = (data, 1);
                }
                else
                {
                    groups[id] = (current.data, current.count + 1);
                }
            }
            
            foreach (var kv in groups.Values)
            {
                var data = kv.data;
                int count = kv.count;

                string enemyName = string.IsNullOrEmpty(data.EnemyName)
                    ? $"Enemy {data.EnemyId}"
                    : data.EnemyName;

                sb.AppendLine($"{enemyName} x {count}");
            }

            return sb.ToString();
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
