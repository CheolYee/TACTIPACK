using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Items
{
    public static class GridShapeUtil
    {
        public static Vector2Int[] GetRotatedOffsets(Vector2Int[] offsets, int rotation, Vector2Int pivot = default)
        {
            Vector2Int[] result = new Vector2Int[offsets.Length];
            for (int i = 0; i < offsets.Length; i++)
            {
                //pivot 기준 회전 적용
                Vector2Int relative = offsets[i] - pivot;
                Vector2Int rotated = rotation switch
                {
                    90 => new Vector2Int(-relative.y, relative.x),
                    180 => new Vector2Int(-relative.x, -relative.y),
                    270 => new Vector2Int(relative.y, -relative.x),
                    _ => relative
                };
                result[i] = rotated + pivot;
            }
            return result;
        }
    }
}