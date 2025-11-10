using System;
using System.Collections.Generic;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Background
{
    [ExecuteAlways]
    public class ParallaxGroup2D : MonoBehaviour
    {
        [Serializable]
        public class Layer
        {
            [Header("Reference")]
            public Transform layer; //이 레이어의 루트(자식에 타일 스프라이트들 배치)
            [Range(-1f, 2f)] public float xMultiplier = 0.5f; //카메라 X 이동에 대한 페럴랙스 비율
            [Range(-1f, 2f)] public float yMultiplier; //카메라 Y 이동에 대한 페럴랙스 비율

            [Header("infinite Option")]
            public bool infiniteX = true;
            public bool infiniteY;

            [Tooltip("TileSet")]
            public Vector2 tileSize = Vector2.zero;

            //내부 상태
            [HideInInspector] public Vector3 startPos;
            [HideInInspector] public Vector2 computedTile;
        }

        [Header("Camera")]
        public Transform cameraTransform; //비워두면 자동으로 MainCamera 사용

        [Header("Layers")]
        [Min(0)] public int maxLayers = 9;
        public List<Layer> layers = new List<Layer>();

        Vector3 _prevCamPos;
        bool _initialized;

        void OnEnable()
        {
            if (cameraTransform == null)
            {
                var cam = Camera.main;
                if (cam) cameraTransform = cam.transform;
            }
            Initialize();
        }

        void OnValidate()
        {
            if (layers.Count > maxLayers)
                layers.RemoveRange(maxLayers, layers.Count - maxLayers);

            if (!Application.isPlaying && cameraTransform == null)
            {
                var cam = Camera.main;
                if (cam) cameraTransform = cam.transform;
            }
            Initialize();
            EditorPreview();
        }

        void Initialize()
        {
            if (cameraTransform == null) return;

            _prevCamPos = cameraTransform.position;
            foreach (Layer currentLayer in layers)
            {
                Layer layer = currentLayer;
                if (layer == null || layer.layer == null) continue;

                layer.startPos = layer.layer.position;

                // 타일 크기 자동 계산
                layer.computedTile = layer.tileSize == Vector2.zero ? GetRendererBoundsSize(layer.layer) : layer.tileSize;
            }
            _initialized = true;
        }

        void LateUpdate()
        {
            if (!_initialized || cameraTransform == null) return;

            Vector3 camDelta = cameraTransform.position - _prevCamPos;

            foreach (Layer layer in layers)
            {
                if (layer == null || layer.layer == null) continue;

                //카메라 이동분 * 멀티플라이어만큼 레이어를 이동
                Vector3 move = new Vector3(camDelta.x * layer.xMultiplier, camDelta.y * layer.yMultiplier, 0f);
                layer.layer.position += move;

                //무한 스크롤(가로/세로) - 레이어 루트를 타일 크기 단위로 순간이동
                if (layer.infiniteX && layer.computedTile.x > 0.0001f)
                {
                    float distX = cameraTransform.position.x - layer.layer.position.x;
                    if (distX >= layer.computedTile.x)        layer.layer.position += new Vector3(layer.computedTile.x, 0f, 0f);
                    else if (distX <= -layer.computedTile.x)  layer.layer.position -= new Vector3(layer.computedTile.x, 0f, 0f);
                }

                if (layer.infiniteY && layer.computedTile.y > 0.0001f)
                {
                    float distY = cameraTransform.position.y - layer.layer.position.y;
                    if (distY >= layer.computedTile.y)        layer.layer.position += new Vector3(0f, layer.computedTile.y, 0f);
                    else if (distY <= -layer.computedTile.y)  layer.layer.position -= new Vector3(0f, layer.computedTile.y, 0f);
                }
            }

            _prevCamPos = cameraTransform.position;
        }

        //에디터에서 카메라를 움직이면 즉시 반응하도록 간단한 프리뷰
        void EditorPreview()
        {
            if (!Application.isPlaying || !_initialized)
            {
                _prevCamPos = cameraTransform ? cameraTransform.position : Vector3.zero;
            }
        }

        //레이어 루트/자식 렌더러들을 모두 합친 바운드 사이즈 계산
        static Vector2 GetRendererBoundsSize(Transform root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return Vector2.zero;

            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                b.Encapsulate(renderers[i].bounds);

            return new Vector2(b.size.x, b.size.y);
        }
    }
}
