using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.Soso7194._04.Scripts.UI
{
    public class ItemClick : MonoBehaviour
    {
        [SerializeField] private CanvasGroup panel;
        
        private Animator _animator;

        private readonly int _isCilck =  Animator.StringToHash("IsClick");
        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
        }

        public void SetIsClick(bool isClick)
        {
            _animator.SetBool(_isCilck, isClick);
        }
        
        public void OnClick()
        {
            SetIsClick(true);
        }

        public void OnExit()
        {
            SetIsClick(false);
            panel.DOFade(0f, 0.5f).onComplete += () =>
            {
                panel.gameObject.SetActive(false);
            };
        }
    }
}