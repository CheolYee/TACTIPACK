using System;
using _00.Work.Resource.Scripts.Managers;
using _00.Work.WorkSpace.JaeHun._01._Scrpits;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.Soso7194._04.Scripts.UI
{
    public class ItemClick : MonoBehaviour
    {
        [SerializeField] private CanvasGroup panel;
        [SerializeField] private CanvasGroup itemPanel;
        
        private Animator _animator;

        private readonly int _isCilck =  Animator.StringToHash("IsClick");
        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
        }

        private void OnEnable()
        {
            panel.interactable = true;
            panel.blocksRaycasts = true;
            itemPanel.gameObject.SetActive(false);
        }

        public void SetIsClick(bool isClick)
        {
            _animator.SetBool(_isCilck, isClick);
        }
        
        public void OnClick()
        {
            SoundManager.Instance.PlaySfx(SfxId.Chest);
            SetIsClick(true);
        }

        public void OnExit()
        {
            if (panel == null)
                return;

            SetIsClick(false);

            // 중복 클릭 방지
            panel.interactable = false;
            panel.blocksRaycasts = false;

            FadeManager.Instance.FadeIn(() =>
            {
                panel.gameObject.SetActive(false);
                
                var mapMgr = MapManager.Instance;
                if (mapMgr != null)
                {
                    mapMgr.CompleteCurrentMap();
                    mapMgr.ShowCurrentChapterRoot();
                }
                
                FadeManager.Instance.FadeOut();
            });
        }
    }
}