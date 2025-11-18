using System.Collections.Generic;
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

        //인덱스 같이 반환
        public static void GetRotatedOffsetsWithIndex(
            IReadOnlyList<Vector2Int> baseOffsets, //베이스 오프셋
            int rotation, //회전각
            Vector2Int pivot, //중심점
            List<Vector2Int> outRotated, //회전 후 로컬 좌표
            List<int> outIndices) //각 좌표의 원래 인덱스
        {
            outRotated.Clear();
            outIndices.Clear();

            // 각 오프셋을 pivot 기준으로 회전
            for (int i = 0; i < baseOffsets.Count; i++)
            {
                var o = baseOffsets[i];
                //pivot 기준 원점 이동
                var p = o - pivot;

                //회전
                Vector2Int r = p;
                int rot = ((rotation % 360) + 360) % 360; // 0,90,180,270
                switch (rot)
                {
                    case 0: r = new Vector2Int(p.x, p.y); break;
                    case 90: r = new Vector2Int(-p.y, p.x); break;
                    case 180: r = new Vector2Int(-p.x, -p.y); break;
                    case 270: r = new Vector2Int(p.y, -p.x); break;
                }

                //pivot 복원
                var rotated = r + pivot;

                outRotated.Add(rotated);
                outIndices.Add(i); //이 좌표는 baseOffsets의 몇 번째 칸이었는가?
            }
        }
    }
}