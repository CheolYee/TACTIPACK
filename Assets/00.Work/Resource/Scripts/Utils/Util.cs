using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;

namespace _00.Work.Resource.Scripts.Utils
{
    public class Util
    {
        public static Vector2 RotateOffset(Vector2 v, int rotation)
        {
            rotation = ((rotation % 360) + 360) % 360;

            switch (rotation)
            {
                case 0:
                    return v;
                case 90:
                    return new Vector2(-v.y, v.x);
                case 180:
                    return new Vector2(-v.x, -v.y);
                case 270:
                    return new Vector2(v.y, -v.x);
                default:
                    float rad = rotation * Mathf.Deg2Rad;
                    float cos = Mathf.Cos(rad);
                    float sin = Mathf.Sin(rad);
                    return new Vector2(
                        v.x * cos - v.y * sin,
                        v.x * sin + v.y * cos
                    );
            }
        }
        
        public static void CallFuncAfterTime(float time, System.Action onEnd, MonoBehaviour monoBehaviour = null)
        {
            if (monoBehaviour == null)
            {
                Timing.RunCoroutine(StartCallFuncAfterTiming(time, onEnd));
            }
            else
            {
                monoBehaviour.StartCoroutine(StartCallFuncAfterTime(time, onEnd));
            }
        }
        static IEnumerator<float> StartCallFuncAfterTiming(float time, System.Action onEnd) // Timing 플러그인
        {
            yield return Timing.WaitForSeconds(time);

            if (onEnd != null)
            {
                onEnd();
            }
        }
        static IEnumerator StartCallFuncAfterTime(float time, System.Action onEnd) // 기본 코루틴
        {
            yield return new WaitForSeconds(time);

            if (onEnd != null)
            {
                onEnd();
            }
        }
    }
}