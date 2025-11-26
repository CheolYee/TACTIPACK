using System;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.AnimatorSystem;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Agents
{
    public class AgentRenderer : MonoBehaviour, IAgentComponent, IAgentRenderer, IAnimationTrigger
    {
        [field: SerializeField] public bool LookToTheLeft { get; set; } //처음 시작 시 왼쪽을 보는가 아닌가
        [SerializeField] private int attackSortingBoost = 50;
        public event Action OnAnimationEnd; //애니메이션 종료 이벤트
        public event Action OnAnimationFire; //타격 시점 이벤트
        public float FacingDirection { get; private set; } = 1f; //보는 방향 (기본 우측)
        public Sprite CurrentSprite => _spriteRenderer.sprite; //현재 스프라이트 가져오기 (잔상이나 다른 효과)
        
        private Agent _agent; //Agent 캐싱
        private Animator _animator; //애니메이터
        private SpriteRenderer _spriteRenderer; //렌더러
        
        private int _baseSortingOrder;
        private bool _highlighting;
        
        public void Initialize(Agent agent) //초기화
        {
            _agent = agent;
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            
            _baseSortingOrder = _spriteRenderer.sortingOrder;
            
            if (LookToTheLeft) Flip();
        }
        public void InitRenderer(RuntimeAnimatorController characterDataAnimatorController, Vector2 characterOffset)
        {
            _animator.runtimeAnimatorController = characterDataAnimatorController;
            transform.position = (Vector2)_agent.transform.position + characterOffset;
        }
        
        public void SetAttackSortingHighlight(bool enable)
        {
            if (_spriteRenderer == null) return;
            if (enable == _highlighting) return;

            _highlighting = enable;

            if (enable)
            {
                _spriteRenderer.sortingOrder = _baseSortingOrder + attackSortingBoost;
            }
            else
            {
                _spriteRenderer.sortingOrder = _baseSortingOrder;
            }
        }

        //파라미터 오버로딩
        public void SetParam(AnimParamSo param, bool value) => _animator.SetBool(param.HashValue, value);
        public void SetParam(AnimParamSo param, int value) => _animator.SetInteger(param.HashValue, value);
        public void SetParam(AnimParamSo param, float value) => _animator.SetFloat(param.HashValue, value);
        public void SetParam(AnimParamSo param) => _animator.SetTrigger(param.HashValue);

        public void Flip() //좌우 반전
        {
            FacingDirection *= -1;
            float yRotation = FacingDirection > 0 ? 0 : 180;
            transform.rotation = Quaternion.Euler(0, yRotation, 0);
        }
        
        private void AnimationFireTrigger()
        {
            OnAnimationFire?.Invoke();
            //애니메이션이 시작했음을 알려주는 메서드
        }

        private void AnimationEndTrigger() => OnAnimationEnd?.Invoke(); //애니메이션이 끝났음을 알려주는 메서드 (애니메이션 트리거에서 실행)

    }
}