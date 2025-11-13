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
        
        public Sprite characterSprite { get; private set; }
        public int maxHP { get; set; }
        public int attackPower { get; private set; }
        public int criticalHitChance { get; private set; }

        private RectTransform _hpBarRect;
        private Image _hpFillImage;
        private Camera _mainCamera;

        private int _attack;
        private int _skillTurn;
        private int _skillDamage;
        private EnemiesSpawn _spawner;
        
        private float _targetFillAmount;

        private void Start()
        {
            //월드 좌표계를 스크린 좌표계으로 변경하기 위해 미리 써두기
            _mainCamera = Camera.main;
        }
        
        private void Update()
        {
            // 데미지를 입었을때 HP바 변동
            if (_hpBarRect != null)
            {
                if (_hpFillImage != null)
                {
                    _hpFillImage.fillAmount = Mathf.Lerp(_hpFillImage.fillAmount, _targetFillAmount, Time.deltaTime * hpBarLerpSpeed);
                }
                
                UpdateHpBarPosition();
            }
        }

        // 처음 세팅 적용하기
        public void Setup(EnemySO data, EnemiesSpawn spawner)
        {
            maxHP = data.maxHP;
            _attack = data.attack;
            _spawner = spawner;
            CurrentHP = maxHP;
            _targetFillAmount = 1f;
            gameObject.name = data.name;
        }

        // HP바 처음 세팅
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

        // HP바 위치 설정
        private void UpdateHpBarPosition()
        {
            if (_hpBarRect == null || _mainCamera == null) return;
            
            // 월드 좌표계를 스크린 좌표계로 변경
            Vector3 screenPos = _mainCamera.WorldToScreenPoint(transform.position);

            // 에너미 HP바 위치 설정
            _hpBarRect.position = screenPos + new Vector3(20, -150, 0);
        }
        
        public void TakeDamage(int damage)
        {
            try
            {
                CurrentHP -= damage;
                _targetFillAmount = CurrentHP / (float) maxHP;
                
                Debug.Log(gameObject.name + " 대미지 입음! " + damage);
                if (CurrentHP <= 0)
                {
                    Die();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[Enemy] {gameObject.name}의 TakeDamage 중 오류 발생: {e.Message}");
            }
        }

        public void Attack()
        {
            
        }

        // 죽었을때
        private void Die()
        {
            try
            {
                //스폰한 적 리스트에서 제거
                if (_spawner != null)
                {
                    _spawner.RemoveEnemy(gameObject);
                }
                else
                {
                    Debug.LogWarning($"[Enemy] {gameObject.name}의 _spawner가 null입니다.");
                }

                if (_hpBarRect != null)
                {
                    Destroy(_hpBarRect.gameObject);
                }
                
                Destroy(gameObject, 0.1f);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Enemy] {gameObject?.name}의 Die 중 오류 발생: {e.Message}");
                // 그래도 오브젝트는 파괴 시도
                Destroy(gameObject, 0.1f);
            }
        }
    }
}
