using System.Collections;
using _00.Work.Scripts.Managers;
using Unity.Cinemachine;
using UnityEngine;

namespace _00.Work.Resource.Scripts.Managers
{
    public enum CamAnchor
    {
        None,
        Default,
        Target,
        PlayerSide,
        EnemySide
    }
    
    [DefaultExecutionOrder(-100)]
    public class SkillCameraManager : MonoSingleton<SkillCameraManager>
    {
        [Header("References")]
        [SerializeField] private CinemachineCamera cam;
        
        [Header("AnchorPos")]
        [SerializeField] private Transform defaultAnchor;
        [SerializeField] private Transform playerSideAnchor;
        [SerializeField] private Transform enemySideAnchor;
        
        [Header("Follow Damping")]
        [SerializeField] private float followDamping = 0.15f;

        private Transform _initialFollow;
        private float _initialOrthoSize;
        private float _initialFov;
        
        private Coroutine _zoomRoutine;

        protected override void Awake()
        {
            Application.targetFrameRate = 60;
            base.Awake();
            CacheInitialState();
        }

        private void CacheInitialState()
        {
            if (cam == null) return;

            var lens = cam.Lens;
            _initialFollow = cam.Follow;
            _initialOrthoSize = lens.OrthographicSize;
            _initialFov = lens.FieldOfView;
        }

        public void SetAnchor(CamAnchor anchor, Transform targetTrm = null)
        {
            Debug.Assert(cam != null, "cam is null");
            
            Transform nextFollow = null;

            switch (anchor)
            {
                case CamAnchor.None:
                    return;
                case CamAnchor.Target:
                    if (targetTrm == null)
                    {
                        Debug.LogWarning("카메라가 타겟 엥커이지만 targetTrm이 null입니다.");
                        return;
                    }
                    nextFollow = targetTrm;
                    break;
                case CamAnchor.Default:
                    nextFollow = defaultAnchor;
                    break;
                case CamAnchor.PlayerSide:
                    nextFollow = playerSideAnchor;
                    break;
                case CamAnchor.EnemySide:
                    nextFollow = enemySideAnchor;
                    break;
            }

            if (nextFollow == null)
            {
                Debug.LogWarning("카메라 메니저 용 앵커 Trm이 세팅되지 않았습니다.");
                return;
            }
            
            Debug.Log(nextFollow.gameObject.name);
            
            cam.Follow = nextFollow;

            if (followDamping > 0f) StartCoroutine(OneFrameNudge());
        }

        private IEnumerator OneFrameNudge()
        {
            float t = 0f;
            while (t < followDamping)
            {
                t += Time.deltaTime;
                yield return null;
            }
        }

        public void Reset(float zoomDuration = 0.25f)
        {
            if (_initialFollow != null && cam != null)
            {
                cam.Follow = _initialFollow;
            }
            
            float back = IsOrthographic() ? _initialOrthoSize : _initialFov;
            ZoomTo(back, zoomDuration);
        }

        public void ZoomTo(float targetSizeOrFov, float duration = 0.25f)
        {
            if (cam == null) return;
            if (_zoomRoutine != null) StopCoroutine(_zoomRoutine);
            _zoomRoutine = StartCoroutine(ZoomRoutine(targetSizeOrFov, duration));
        }

        private IEnumerator ZoomRoutine(float targetSizeOrFov, float duration)
        {
            var lens = cam.Lens;
            
            bool ortho = IsOrthographic();
            float start = ortho ? lens.OrthographicSize : lens.FieldOfView;
            float t = 0f;

            while (t < 1f)
            {
                t += duration <= 0f ? 1f : Time.unscaledDeltaTime / duration;
                float eased = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
                float v = Mathf.Lerp(start, targetSizeOrFov, eased);

                if (ortho)
                {
                    lens.OrthographicSize = Mathf.Max(0.001f, v);
                }
                else
                {
                    lens.FieldOfView = Mathf.Clamp(v, 1f, 170f);
                }
                
                cam.Lens = lens;
                yield return null;
            }
            
            _zoomRoutine = null;
        }

        private bool IsOrthographic()
        {
            return cam != null && cam.Lens.OrthographicSize > 0f;
        }
    }
}