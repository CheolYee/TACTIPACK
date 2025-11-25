using System.IO;
using _00.Work.Scripts.Managers;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Save
{
    public class SaveManager : MonoSingleton<SaveManager>
    {
        private const string PartyFileName   = "party.json";
        private const string MapFileName     = "map.json";
        private const string GridFileName    = "grid_inventory.json";
        private const string SideFileName    = "side_inventory.json";
        private const string ShopFileName    = "shop.json";

        protected override void Awake()
        {
            base.Awake();
            if (Instance == this)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        //파일 이름 넣어주면 저장 주소랑 합쳐서 리턴함
        private string GetPath(string fileName)
        {
            return Path.Combine(Application.persistentDataPath, fileName);
        }

        //로드할 떄 이어하기 가능한가 여부 판단
        public bool HasAnySave()
        {
            //파티랑 맵 2개의 세이브 데이터가 있어야 이어하기 최소조건 만족
            return File.Exists(GetPath(PartyFileName)) &&
                   File.Exists(GetPath(MapFileName));
        }

        //모든 데이브 데이터 정리
        public void DeleteAllSaves()
        {
            TryDelete(PartyFileName);
            TryDelete(MapFileName);
            TryDelete(GridFileName);
            TryDelete(SideFileName);
            TryDelete(ShopFileName);
        }

        //파일 제거 시도
        private void TryDelete(string file)
        {
            string path = GetPath(file);
            if (File.Exists(path))
                File.Delete(path);
        }


        //파티 세이브 / 로드
        #region Party

        public void SaveParty(PartySaveData party)
        {
            WriteJson(PartyFileName, party);
        }

        public PartySaveData LoadParty()
        {
            return ReadJson<PartySaveData>(PartyFileName);
        }

        #endregion
        
        
        //맵 세이브 / 로드
        #region Map
        
        //맵 세이브 데이터가 있는가
        public bool HasMapSave()
        {
            return File.Exists(GetPath(MapFileName));
        }

        //맵 세이브데이터 지우기
        public void DeleteMapSave()
        {
            string path = GetPath(MapFileName);
            if (File.Exists(path))
                File.Delete(path);
        }
        
        public void SaveMapProgress(MapProgressSaveData map)
        {
            WriteJson(MapFileName, map);
        }

        public MapProgressSaveData LoadMapProgress()
        {
            return ReadJson<MapProgressSaveData>(MapFileName);
        }
        #endregion

        //그리드 인벤토리 세이브 /로드
        #region Grid
        public bool HasGridInventorySave()
        {
            return File.Exists(GetPath(GridFileName));
        }
        
        public void DeleteGridInventorySave()
        {
            string path = GetPath(GridFileName);
            if (File.Exists(path))
                File.Delete(path);
        }

        public void SaveGridInventory(GridInventorySaveData data)
        {
            WriteJson(GridFileName, data);
        }
        
        public GridInventorySaveData LoadGridInventory()
        {
            return ReadJson<GridInventorySaveData>(GridFileName);
        }

        #endregion

        //사이드 인벤토리 세이브 /로드
        #region Side

        public bool HasSideInventorySave()
        {
            return File.Exists(GetPath(SideFileName));
        }
        
        public void DeleteSideInventorySave()
        {
            string path = GetPath(SideFileName);
            if (File.Exists(path))
                File.Delete(path);
        }
        
        public void SaveSideInventory(SideInventorySaveData data)
        {
            WriteJson(SideFileName, data);
        }

        public SideInventorySaveData LoadSideInventory()
        {
            return ReadJson<SideInventorySaveData>(SideFileName);
        }
        
        #endregion
        
        //상점 코인
        #region Shop
        
        public bool HasShopSave()
        {
            return File.Exists(GetPath(ShopFileName));
        }

        public void DeleteShopSave()
        {
            TryDelete(ShopFileName);
        }

        public void SaveShop(ShopSaveData data)
        {
            WriteJson(ShopFileName, data);
        }

        public ShopSaveData LoadShop()
        {
            return ReadJson<ShopSaveData>(ShopFileName);
        }

        #endregion

        private void WriteJson<T>(string fileName, T data)
        {
            if (data == null)
            {
                Debug.LogWarning($"[SaveManager] {fileName} : 저장할 데이터가 없습니다.");
                return;
            }

            string path = GetPath(fileName);
            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(path, json);
                Debug.Log($"[SaveManager] Saved {fileName} to {path}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveManager] {fileName} 저장 실패: {e}");
            }
        }

        private T ReadJson<T>(string fileName) where T : class
        {
            string path = GetPath(fileName);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[SaveManager] {fileName} 파일이 없습니다.");
                return null;
            }

            try
            {
                string json = File.ReadAllText(path);
                var data = JsonUtility.FromJson<T>(json);
                return data;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveManager] {fileName} 로드 실패: {e}");
                return null;
            }
        }
    }
}