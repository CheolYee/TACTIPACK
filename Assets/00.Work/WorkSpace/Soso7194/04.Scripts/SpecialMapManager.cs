using System;
using _00.Work.Scripts.Managers;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _00.Work.WorkSpace.Soso7194._04.Scripts
{
    public class SpecialMapManager : MonoSingleton<SpecialMapManager>
    {
        [SerializeField] private CanvasGroup chest;
        [SerializeField] private CanvasGroup rest;
        [SerializeField] private CanvasGroup shop;

        private void Update()
        {
            if (Keyboard.current.aKey.wasPressedThisFrame)
            {
                EnableChest();
            }

            if (Keyboard.current.sKey.wasPressedThisFrame)
            {
                EnableRest();
            }

            if (Keyboard.current.dKey.wasPressedThisFrame)
            {
                EnableShop();
            }
        }

        public void EnableChest()
        {
            chest.gameObject.SetActive(true);
            chest.alpha = 0f;
            chest.DOFade(1f, 0.5f);
        }
        
        public void EnableRest()
        {
            rest.gameObject.SetActive(true);
            rest.alpha = 0f;
            rest.DOFade(1f, 0.5f);
        }
        
        public void EnableShop()
        {
            shop.gameObject.SetActive(true);
        }
    }
}