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

    public void MoveToPoint(Vector2 targetPos)
    {
        Vector3 newPos = new Vector3(targetPos.x, targetPos.y, mainCamera.transform.position.z);
        mainCamera.transform.position = newPos;
    }

    public void MoveToPoint(Transform target)
    {
        if (target == null) return;
        Vector3 newPos = new Vector3(target.position.x, target.position.y, mainCamera.transform.position.z);
        mainCamera.transform.position = newPos;
    }
}
