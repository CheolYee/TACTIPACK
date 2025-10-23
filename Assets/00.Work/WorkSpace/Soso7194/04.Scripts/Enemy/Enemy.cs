using System;
using _00.Work.WorkSpace.Soso7194._04.Scripts.SO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace _00.Work.WorkSpace.Soso7194._04.Scripts.Enemy
{
    public class Enemy : MonoBehaviour, IEnemy
    {
        [SerializeField] private float hpBarLerpSpeed = 3f;
        public int CurrentHP { get; set; }
        public CharacterDataSO Data { get; set; }
        
        private RectTransform _hpBarRect;
        private Image _hpFillImage;
        private Camera _mainCamera;
        
        private int _maxHp;
        private int _attack;
        private EnemiesSpawn _spawner;
        
        private float _targetFillAmount;

        private void Start()
        {
            _mainCamera = Camera.main;
        }
        
        private void Update()
        {
            if (_hpBarRect != null)
            {
                if (_hpFillImage != null)
                {
                    _hpFillImage.fillAmount = Mathf.Lerp(_hpFillImage.fillAmount, _targetFillAmount, Time.deltaTime * hpBarLerpSpeed);
                }
                
                UpdateHpBarPosition();
            }
        }

        public void Setup(EnemySO data, EnemiesSpawn spawner)
        {
            _maxHp = data.maxHP;
            _attack = data.attack;
            _spawner = spawner;
            CurrentHP = _maxHp;
            _targetFillAmount = 1f;
            gameObject.name = data.name;
        }

        public void SetHpBar(RectTransform hpBarRect)
        {
            _hpBarRect = hpBarRect;
            
            Transform hpTransform = hpBarRect.Find("HP");
            if (hpTransform != null)
            {
                _hpFillImage = hpTransform.GetComponent<Image>();
                if (_hpFillImage != null)
                {
                    _hpFillImage.fillAmount = 1f;
                }
            }
        }

        private void UpdateHpBarPosition()
        {
            if (_hpBarRect == null || _mainCamera == null) return;
            
            Vector3 screenPos = _mainCamera.WorldToScreenPoint(transform.position);

            _hpBarRect.position = screenPos + new Vector3(0, 150, 0);
        }

        public void TakeDamage(int damage)
        {
            CurrentHP -= damage;
            _targetFillAmount = CurrentHP / (float)_maxHp;
            
            Debug.Log(gameObject.name + " 대미지 입음! " + damage);
            if (CurrentHP <= 0)
            {
                Die();
            }
        }

        public void Attack()
        {
            
        }

        private void Die()
        {
            _spawner.RemoveEnemy(gameObject);

            if (_hpBarRect != null)
            {
                Destroy(_hpBarRect.gameObject);
            }
            
            Destroy(gameObject, 0.1f);
        }
    }
}
