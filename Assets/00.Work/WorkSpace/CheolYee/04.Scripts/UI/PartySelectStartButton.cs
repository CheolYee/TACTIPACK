using System.Collections.Generic;
using _00.Work.Resource.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Events;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players;
using _00.Work.WorkSpace.Hyeonjin0209._01.Scripts.System.Character;
using UnityEngine;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.UI
{
    public class PartySelectStartButton : MonoBehaviour
    {
        [Header("Scene")]
        [SerializeField] private int battleSceneBuildIndex = 1; // 전투 씬 빌드 인덱스

        [Header("UI")]
        [SerializeField] private Button startButton;

        private void Awake()
        {
            if (startButton == null)
                startButton = GetComponent<Button>();

            if (startButton != null)
                startButton.onClick.AddListener(OnClickStart);
        }

        private void OnDestroy()
        {
            if (startButton != null)
                startButton.onClick.RemoveListener(OnClickStart);
        }

        private void OnClickStart()
        {
            var selectMgr = CharacterSelectManager.Instance;

            List<PlayerDefaultData> party = selectMgr.GetSelectedPartyData();
            if (party == null || party.Count == 0)
            {
                Bus<MessageEvent>.Raise(new MessageEvent("선택된 캐릭터가 없습니다. 최소 1명은 선택해야 합니다."));
                return;
            }

            // 1) 파티 데이터를 컨테이너에 저장
            var container = PartyDataContainer.Instance;
            if (container != null)
            {
                container.SetParty(party);
            }

            // 2) 씬 전환 (FadeManager 있으면 사용, 없으면 그냥 LoadScene)
            var fade = FadeManager.Instance;
            if (fade != null)
            {
                fade.FadeToSceneAsync(battleSceneBuildIndex);
            }
        }
    }
}
