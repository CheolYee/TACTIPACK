using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.ItemTypes.ActiveItems;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Enemies;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Managers;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Agents
{
    [RequireComponent(typeof(Collider2D))]
    public class AgentWorldTooltip : MonoBehaviour
    {
        [SerializeField] private Agent agent;
        [SerializeField] private float showDelay = 0.5f;

        private AgentTooltipStats _stats;
        private Enemy _enemy;
        private Player _player;

        private bool _hover;
        private float _hoverTimer;
        private bool _shown;

        private void Awake()
        {
            if (agent == null)
                agent = GetComponentInParent<Agent>();

            if (agent != null)
            {
                _stats = agent.GetCompo<AgentTooltipStats>();
                _enemy = agent as Enemy;
                _player = agent as Player;
            }
        }

        private void OnMouseEnter()
        {
            _hover = true;
            _hoverTimer = 0f;
            _shown = false;
        }

        private void OnMouseExit()
        {
            _hover = false;
            _hoverTimer = 0f;
            _shown = false;
            ToolTipManager.Instance?.Hide();
        }

        private void Update()
        {
            if (!_hover || agent == null) return;

            _hoverTimer += Time.unscaledDeltaTime;

            //딜레이 뒤에 최초 1번 Show 호출
            if (!_shown && _hoverTimer >= showDelay)
            {
                _shown = true;
                ShowOrUpdateTooltip(forceRefresh: true);
            }

            //보여진 상태에서는 마우스 따라 위치만 갱신
            if (_shown)
            {
                ShowOrUpdateTooltip(forceRefresh: false);
            }
        }

        private void ShowOrUpdateTooltip(bool forceRefresh)
        {
            var tm = ToolTipManager.Instance;
            if (tm == null) return;

            Vector2 screenPos = Input.mousePosition;

            if (forceRefresh)
            {
                BuildTooltipText(out string title, out string body);
                tm.Show(title, body, screenPos);
            }
            else
            {
                tm.UpdatePosition(screenPos);
            }
        }

        private void BuildTooltipText(out string title, out string body)
        {
            // ===== 제목 (이름) =====
            if (_player != null && _player.CharacterData != null)
            {
                title = _player.CharacterData.CharacterName;
            }
            else if (_enemy != null && _enemy.EnemyData != null)
            {
                title = _enemy.EnemyData.EnemyName;
            }
            else
            {
                title = agent.name;
            }

            // ===== HP & 크리 =====
            string hpLine = "체력 : ?";
            string critLine = "크리티컬 확률 : ?";

            if (_stats != null)
            {
                float hpNow = _stats.CurrentHp;
                float hpMax = _stats.MaxHp;

                if (hpMax > 0f)
                    hpLine = $"체력 : {Mathf.CeilToInt(hpNow)} / {Mathf.CeilToInt(hpMax)}";

                float crit01 = _stats.GetFinalCritChance01();
                float critPercent = crit01 * 100f;
                critLine = $"크리티컬 확률 : {critPercent:F1}%";
            }

            body = $"{hpLine}\n{critLine}";

            //적이면 이번 턴 스킬 표시
            if (_enemy != null)
            {
                AttackItemSo skill = _enemy.PlannedSkill; // 밑에서 추가할 프로퍼티
                if (skill != null)
                {
                    // SO 자산 이름을 기본으로 사용 (표시용 이름 프로퍼티 있으면 거기로 교체)
                    string skillName = skill.itemName;
                    string desc = skill.description;

                    body += $"\n\n이번 턴 스킬 : {skillName}";
                    body += $"\n{desc}";
                }
            }
        }
    }
}