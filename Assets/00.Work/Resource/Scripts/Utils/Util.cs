using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;

namespace _00.Work.Resource.Scripts.Utils
{
    public class Util
    {
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