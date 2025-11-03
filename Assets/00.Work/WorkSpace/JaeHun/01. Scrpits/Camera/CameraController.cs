using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    private Camera mainCamera;

    private void Awake()
    {
        Instance = this;
        mainCamera = Camera.main;
    }

    // Vector2로 넘어오면 자동으로 Vector3로 변환
    public void MoveToPoint(Vector2 targetPos)
    {
        Vector3 newPos = new Vector3(targetPos.x, targetPos.y, mainCamera.transform.position.z);
        mainCamera.transform.position = newPos;
    }

    // 혹시 Vector3로 직접 호출하고 싶을 때도 호환 가능
    public void MoveToPoint(Vector3 targetPos)
    {
        Vector3 newPos = new Vector3(targetPos.x, targetPos.y, mainCamera.transform.position.z);
        mainCamera.transform.position = newPos;
    }
}
