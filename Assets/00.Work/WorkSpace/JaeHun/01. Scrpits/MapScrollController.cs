using UnityEngine;

public class MapScrollController : MonoBehaviour
{
    [Header("스크롤할 대상")]
    [SerializeField] private RectTransform mapContent; // 움직이는 지도 패널
    [SerializeField] private GameObject mapUI;         // 지도 루트 (활성화 중일 때만 작동)

    [Header("스크롤 설정")]
    [SerializeField] private float scrollSpeed = 800f;     // 마우스 휠 속도
    [SerializeField] private float keyScrollSpeed = 400f;  // ↑↓ 키 속도
    [SerializeField] private float minY = 0f;              // 최소 위치 (직접 설정)
    [SerializeField] private float maxY = 3000f;           // 최대 위치 (직접 설정)

    void Update()
    {
        if (!mapUI || !mapUI.activeSelf) return;

        float move = 0f;
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scrollInput) > 0.001f)
            move += scrollInput * scrollSpeed;

        if (Input.GetKey(KeyCode.UpArrow))
            move -= keyScrollSpeed * Time.deltaTime;   // ↑ → 위쪽 보기
        if (Input.GetKey(KeyCode.DownArrow))
            move += keyScrollSpeed * Time.deltaTime;   // ↓ → 아래쪽 보기

        if (Mathf.Abs(move) > 0.001f)
        {
            Vector2 pos = mapContent.anchoredPosition;
            pos.y += move;
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            mapContent.anchoredPosition = pos;

            Debug.Log($"[SCROLL] move={move:F1}, posY={pos.y:F1} (min:{minY}, max:{maxY})");
        }
    }
}
