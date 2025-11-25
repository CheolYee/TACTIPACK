using System;
using _00.Work.Resource.Scripts.Managers;
using _00.Work.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Save;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Managers
{
    public class MoneyManager : MonoSingleton<MoneyManager>
    {
        [Header("Default Coin (no save)")]
        [SerializeField] private int startingCoin;

        public int Coin { get; private set; }

        public event Action<int> OnCoinChanged;

        protected override void Awake()
        {
            base.Awake();
            LoadFromSave();
        }

        private void LoadFromSave()
        {
            var saveMgr = SaveManager.Instance;
            if (saveMgr != null && saveMgr.HasShopSave())
            {
                var shopSave = saveMgr.LoadShop();
                if (shopSave != null)
                {
                    SetCoinInternal(shopSave.coin);
                    return;
                }
            }

            // 세이브 없으면 기본값
            SetCoinInternal(startingCoin);
        }

        private void SetCoinInternal(int value)
        {
            value = Mathf.Clamp(value, 0, int.MaxValue);
            Coin = value;
            OnCoinChanged?.Invoke(Coin);
        }

        public void SetCoin(int value)
        {
            if (value < 0) value = 0;
            if (value == Coin) return;
            SetCoinInternal(value);
        }

        public void AddCoin(int amount)
        {
            if (amount <= 0) return;
            SetCoinInternal(Coin + amount);
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0) return true;
            if (Coin < amount) return false;
            
            SoundManager.Instance.PlaySfx(SfxId.MoneySpend);

            SetCoinInternal(Coin - amount);
            return true;
        }

        // 세이브/로드용 헬퍼
        public ShopSaveData BuildSaveData()
        {
            return new ShopSaveData
            {
                coin = Coin
            };
        }

        public void ApplySaveData(ShopSaveData data)
        {
            if (data == null) return;
            SetCoinInternal(data.coin);
        }

        public void Save()
        {
            var saveMgr = SaveManager.Instance;
            if (saveMgr == null) return;

            saveMgr.SaveShop(BuildSaveData());
        }
    }
}