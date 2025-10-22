using UnityEngine;

[CreateAssetMenu(fileName = "MapSO", menuName = "Scriptable Objects/MapSO")]
public class MapSO : ScriptableObject
{
    public string mapName;
    public MapSO[] nextMap;
    public bool isLook;  // 처음에는 모두 잠겨있게. 
    public Vector2 cameraPosition;
}
