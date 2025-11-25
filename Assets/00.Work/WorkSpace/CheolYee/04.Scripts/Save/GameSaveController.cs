using _00.Work.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.UI;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items.UI.SideItem;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Managers;
using _00.Work.WorkSpace.JaeHun._01._Scrpits;
using _00.Work.WorkSpace.Soso7194._04.Scripts.UI;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Save
{
    public class GameSaveController : MonoSingleton<GameSaveController>
    {
        [SerializeField] private bool dontDestroyOnLoad = true;

        protected override void Awake()
        {
            base.Awake();
            if (Instance == this && dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        /// <summary>
        /// 파티 + 맵 진행도 + 그리드 인벤 + 사이드 인벤을 한 번에 저장
        /// </summary>
        public void SaveAll()
        {
            var saveMgr = SaveManager.Instance;
            if (saveMgr == null)
            {
                Debug.LogError("[GameSaveController] SaveManager 가 없습니다.");
                return;
            }

            // 1) 파티
            var partyMgr = PartyManager.Instance;
            if (partyMgr != null)
            {
                // PartyManager 안에서 SaveManager.SaveParty 호출
                partyMgr.SaveCurrentParty();
            }

            // 2) 맵 진행도
            var mapMgr = MapManager.Instance;
            if (mapMgr != null)
            {
                // MapManager 안에서 SaveManager.SaveMapProgress 호출
                mapMgr.SaveCurrentMapProgress();
            }

            // 3) 그리드 인벤토리
            var gridCtrl = GridInventoryUIController.Instance;
            if (gridCtrl != null && gridCtrl.grid != null)
            {
                GridInventorySaveData gridData = gridCtrl.grid.CaptureSaveData();
                saveMgr.SaveGridInventory(gridData);
            }

            // 4) 사이드 인벤토리
            var sideMgr = SideInventoryManager.Instance;
            if (sideMgr != null)
            {
                SideInventorySaveData sideData = sideMgr.CaptureSaveData();
                saveMgr.SaveSideInventory(sideData);
            }
            
            // 5) 상점 골드
            MoneyManager money = MoneyManager.Instance;
            if (money != null)
            {
                var shopData = new ShopSaveData
                {
                    coin = money.Coin
                };
                saveMgr.SaveShop(shopData);
            }

            Debug.Log("[GameSaveController] SaveAll 완료");
        }

        /// <summary>
        /// 현재 씬에 그리드 인벤 + 사이드 인벤토리만 로드해서 적용
        /// 파티/맵은 PartyManager/MapManager의 Start에서 이미 SaveManager로부터 로드하므로 여기서는 건드리지 않음.
        /// </summary>
        public void LoadAll()
        {
            var saveMgr = SaveManager.Instance;
            if (saveMgr == null)
            {
                Debug.LogError("[GameSaveController] SaveManager 가 없습니다.");
                return;
            }

            // 1) 그리드 인벤
            var gridCtrl = GridInventoryUIController.Instance;
            var gridData = saveMgr.LoadGridInventory();
            if (gridData != null && gridCtrl != null && gridCtrl.grid != null)
            {
                gridCtrl.grid.ApplySaveData(gridData);
            }

            // 2) 사이드 인벤
            var sideMgr = SideInventoryManager.Instance;
            var sideData = saveMgr.LoadSideInventory();
            if (sideData != null && sideMgr != null)
            {
                sideMgr.ApplySaveData(sideData);
            }

            Debug.Log("[GameSaveController] LoadAll(인벤토리) 완료");
        }
    }
}
