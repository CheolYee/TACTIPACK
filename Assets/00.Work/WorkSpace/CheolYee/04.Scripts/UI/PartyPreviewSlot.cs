using _00.Work.WorkSpace.CheolYee._04.Scripts.Creatures.Players;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.UI
{
    public class PartyPreviewSlot : MonoBehaviour
    {
        private static readonly int Idle = Animator.StringToHash("IDLE");
        [SerializeField] private Animator animator;

        private PlayerDefaultData _currentData;
        private Vector2 _startPosition;

        private void Awake()
        {
            _startPosition = transform.position;
            
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }

        /// <summary>
        /// 이 슬롯에 해당 캐릭터의 애니메이터를 세팅하고 보여줌
        /// </summary>
        public void Show(PlayerDefaultData data)
        {
            _currentData = data;

            if (data == null)
            {
                Hide();
                return;
            }

            gameObject.SetActive(true);

            if (animator != null && data.AnimatorController != null)
            {
                animator.runtimeAnimatorController = _currentData.AnimatorController;
                animator.SetBool(Idle, true);
                transform.position = _startPosition + _currentData.CharacterOffset;
            }
        }

        /// <summary>
        /// 이 슬롯 비우기
        /// </summary>
        public void Hide()
        {
            _currentData = null;
            gameObject.SetActive(false);
        }
    }
}