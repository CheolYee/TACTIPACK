using System.Collections.Generic;
using System.IO;
using _00.Work.Scripts.Managers;
using DG.Tweening;
using UnityEngine;

namespace _00.Work.Resource.Scripts.Managers
{
    [System.Serializable]
    public class SoundSettings
    {
        public float bgmVolume = 0.5f;
        public float sfxVolume = 0.5f;
    }
    public enum BgmId
    {
        Title,
        Select,
        Shop,
        Rest,
        Random,
        Reward,
        Battle,
        Map
    }
    
    public enum SfxId
    {
        None,
        UiClick,
        UiConfirm,
        Heal,
        NormalHit,
        Barrier,
        Burn,
        Bleed,
        Stun,
        Critical,
        SmallSword,
        LargeSword,
        Healing,
        FireMagic,
        IceMagic,
        VoidMagic,
        Shild,
        Explosion,
        HolyMagic,
        ID37,
        ID42,
        ShortSword,
        ElectricMagic,
        ElectricBall,
        ID20,
        JangBeUp,
        MoneySpend,
        Dollimpan,
        Chest,
        Victory,
        ItemGet,
        CellClick,
        Phowhow,
        Windy,
        Dash,
        SpinSlash,
        Debuff,
        Buff,
        Glub,
        FireBottle,
        ManaCrystal,
        MangTo,
        RotateItem,
        Error
    }
    
    [System.Serializable]
    public class BgmEntry
    {
        public BgmId id;
        public List<AudioClip> clip;
    }

    [System.Serializable]
    public class SfxEntry
    {
        public SfxId id;
        public List<AudioClip> clip;
    }

    public class SoundManager : MonoSingleton<SoundManager>
    {

        [Header("Audio Sources")] public AudioSource bgmSource;
        public AudioSource sfxSource;

        [Header("BGM Clips")] public List<BgmEntry> bgmEntries = new();

        [Header("SFX Clips")] public List<SfxEntry> sfxEntries = new();

        private readonly Dictionary<BgmId, List<AudioClip>> _bgmTable = new();
        private readonly Dictionary<SfxId, List<AudioClip>> _sfxTable = new();

        private SoundSettings _settings = new SoundSettings();
        public float GetBGMVolume() => _settings.bgmVolume;
        public float GetSfxVolume() => _settings.sfxVolume;

        protected override void Awake()
        {
            base.Awake();
            if (Instance == this)
                DontDestroyOnLoad(this);

            BuildTables();
            LoadSettings();
            ApplyVolume();
        }

        private void BuildTables()
        {
            _bgmTable.Clear();
            foreach (var entry in bgmEntries)
            {
                if (entry == null || entry.clip == null) continue;
                _bgmTable[entry.id] = entry.clip;
            }

            _sfxTable.Clear();
            foreach (var entry in sfxEntries)
            {
                if (entry == null || entry.clip == null) continue;
                _sfxTable[entry.id] = entry.clip;
            }
        }

        #region BGM

        public void PlayBgm(BgmId id, float fadeOutDuration = 0.5f, float fadeInDuration = 1f)
        {
            if (!_bgmTable.TryGetValue(id, out var clips)) return;

            var index = Random.Range(0, clips.Count);
            var clip = clips[index];
            if (clip == null) return;
            
            // 이미 같은 BGM이면 다시 안틀어도 됨
            if (bgmSource.clip == clip && bgmSource.isPlaying)
                return;

            if (bgmSource.isPlaying)
            {
                bgmSource.DOFade(0f, fadeOutDuration).OnComplete(() =>
                {
                    bgmSource.Stop();
                    bgmSource.clip = clip;
                    bgmSource.loop = true;
                    bgmSource.volume = 0f;
                    bgmSource.Play();
                    bgmSource.DOFade(_settings.bgmVolume, fadeInDuration);
                });
            }
            else
            {
                bgmSource.clip = clip;
                bgmSource.loop = true;
                bgmSource.volume = 0f;
                bgmSource.Play();
                bgmSource.DOFade(_settings.bgmVolume, fadeInDuration);
            }
        }

        public void StopBgm(float fadeOutDuration = 0.5f)
        {
            if (!bgmSource.isPlaying) return;

            bgmSource.DOFade(0f, fadeOutDuration).OnComplete(() =>
            {
                bgmSource.Stop();
                bgmSource.clip = null;
            });
        }

        #endregion

        #region SFX

        /// <summary>
        /// SFX 재생. volumeScale은 효과음 개별 배율(0~1), pitch는 개별 피치.
        /// </summary>
        public void PlaySfx(SfxId id, float volumeScale = 1f, float pitch = 1f)
        {
            if (id == SfxId.None) return;
            if (!_sfxTable.TryGetValue(id, out var clips)) return;
            if (clips == null || clips.Count == 0) return;
            
            var index = Random.Range(0, clips.Count);
            var clip = clips[index];
            if (clip == null) return;

            var originalPitch = sfxSource.pitch;
            sfxSource.pitch = pitch;
            sfxSource.PlayOneShot(clip, _settings.sfxVolume * volumeScale);
            sfxSource.pitch = originalPitch;
        }

        #endregion

        #region Volume & Save

        public void SetSfxVolume(float volume)
        {
            _settings.sfxVolume = Mathf.Clamp01(volume);
            ApplyVolume();
            SaveSettings();
        }

        public void SetBgmVolume(float volume)
        {
            _settings.bgmVolume = Mathf.Clamp01(volume);
            ApplyVolume();
            SaveSettings();
        }

        private static string SoundSavePath =>
            Application.persistentDataPath + "/soundData.json";

        private void SaveSettings()
        {
            var json = JsonUtility.ToJson(_settings);
            File.WriteAllText(SoundSavePath, json);
        }

        private void LoadSettings()
        {
            if (File.Exists(SoundSavePath))
            {
                var json = File.ReadAllText(SoundSavePath);
                var loaded = JsonUtility.FromJson<SoundSettings>(json);

                if (loaded != null)
                    _settings = loaded;
            }
            else
            {
                _settings = new SoundSettings
                {
                    bgmVolume = 0.5f,
                    sfxVolume = 0.5f
                };
                SaveSettings();
            }
        }

        private void ApplyVolume()
        {
            if (bgmSource != null)
                bgmSource.volume = _settings.bgmVolume;

            if (sfxSource != null)
                sfxSource.volume = _settings.sfxVolume;
        }

        #endregion
    }
}
