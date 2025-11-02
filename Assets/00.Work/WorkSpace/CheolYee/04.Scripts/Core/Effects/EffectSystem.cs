using System.Collections;
using _00.Work.Resource.Scripts.Managers;
using _00.Work.Scripts.Managers;
using _00.Work.Scripts.SO;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Effects
{
    public class EffectSystem : MonoBehaviour, IPoolable
    {
        [SerializeField] private string itemName;
        [SerializeField] private float duration;
        
        private ParticleSystem _particleSystem;
        private WaitForSeconds _waitForSeconds;
        public string ItemName => itemName;
        public GameObject GameObject => gameObject;

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            _waitForSeconds = new WaitForSeconds(duration);
        }

        public void SetPosAndPlay(Vector3 pos)
        {
            transform.position = pos;
            _particleSystem.Play();
            StartCoroutine(DelayAndGoToPool());
        }

        private IEnumerator DelayAndGoToPool()
        {
            yield return _waitForSeconds;
            PoolManager.Instance.Push(this);
        }

        public void ResetItem()
        {
            _particleSystem.Stop(); //멈추기
            _particleSystem.Simulate(0); //0초로 되감기 (처음으로 가기)
        }
    }
}