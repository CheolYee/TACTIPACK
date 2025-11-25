using System.Collections.Generic;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _00.Work.WorkSpace.Soso7194._04.Scripts.UI
{
    public class ResultUI : MonoBehaviour
    {
        [Header("Slot UI")]
        [SerializeField] private Image[] playerImages;
        [SerializeField] private TextMeshProUGUI[] playerNameText;
        [SerializeField] private TextMeshProUGUI[] playerHpText;

        [Header("Heal Settings")]
        [Tooltip("최대 체력 비례 최소 회복 비율 (0.25 = 25%)")]
        [SerializeField] private float minHealRatio = 0.25f;
        
        [Tooltip("최대 체력 비례 최대 회복 비율 (0.5 = 50%)")]
        [SerializeField] private float maxHealRatio = 0.5f;

        private void OnEnable()
        {
            RefreshResult();
        }

        private void RefreshResult()
        {
            var bsm = BattleSkillManager.Instance;
            if (bsm == null)
            {
                Debug.LogWarning("[ResultUI] BattleSkillManager 인스턴스를 찾을 수 없습니다.");
                HideAllSlots();
                return;
            }

            List<Player> players = bsm.GetPlayers();
            if (players == null || players.Count == 0)
            {
                Debug.LogWarning("[ResultUI] 파티에 플레이어가 없습니다.");
                HideAllSlots();
                return;
            }

            int slotCount = Mathf.Min(
                playerImages?.Length ?? 0,
                playerNameText?.Length ?? 0,
                playerHpText?.Length ?? 0
            );

            for (int i = 0; i < slotCount; i++)
            {
                if (i >= players.Count || players[i] == null)
                {
                    SetSlotActive(i, false);
                    continue;
                }

                Player player = players[i];
                AgentHealth health = player.Health;

                if (health == null)
                {
                    SetSlotActive(i, false);
                    continue;
                }

                SetSlotActive(i, true);

                float prevHp = health.CurrentHealth;
                float maxHp = health.MaxHealth;

                // 비례 회복량 랜덤 (25% ~ 50%)
                float ratio = Random.Range(minHealRatio, maxHealRatio);
                float healAmount = maxHp * ratio;

                // 실제 Heal 적용
                health.Heal(healAmount);

                float afterHp = health.CurrentHealth;
                float healed = Mathf.Max(0f, afterHp - prevHp);

                // 아이콘 세팅
                if (playerImages != null && i < playerImages.Length && playerImages[i] != null)
                {
                    Sprite icon = null;
                    if (player.CharacterData != null)
                        icon = player.CharacterData.CharacterBoxIcon;

                    playerImages[i].sprite = icon;
                    playerImages[i].enabled = icon != null; // 아이콘 없으면 안 보이게
                }

                // 이름 텍스트
                if (playerNameText != null && i < playerNameText.Length && playerNameText[i] != null)
                {
                    string nameText = player.CharacterData != null
                        ? player.CharacterData.CharacterName
                        : player.name;

                    playerNameText[i].text = nameText;
                }

                // HP 텍스트: "이전HP -> 이후HP (+회복량)"
                if (playerHpText != null && i < playerHpText.Length && playerHpText[i] != null)
                {
                    int prevInt = Mathf.RoundToInt(prevHp);
                    int afterInt = Mathf.RoundToInt(afterHp);
                    int healInt = Mathf.RoundToInt(healed);

                    playerHpText[i].text = $"{prevInt} → {afterInt} (+{healInt})";
                }
            }

            // 남는 슬롯 비활성화
            for (int i = players.Count; i < slotCount; i++)
            {
                SetSlotActive(i, false);
            }
        }

        private void HideAllSlots()
        {
            int slotCount = Mathf.Min(
                playerImages?.Length ?? 0,
                playerNameText?.Length ?? 0,
                playerHpText?.Length ?? 0
            );

            for (int i = 0; i < slotCount; i++)
            {
                SetSlotActive(i, false);
            }
        }

        private void SetSlotActive(int index, bool active)
        {
            // 슬롯 루트가 따로 없으면, 이미지의 부모를 슬롯 루트라고 가정
            if (playerImages != null && index >= 0 && index < playerImages.Length && playerImages[index] != null)
            {
                var root = playerImages[index].transform.parent;
                if (root != null)
                    root.gameObject.SetActive(active);
            }
        }
    }
}
