using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.AnimatorSystem
{
    [CreateAssetMenu(fileName = "Animator Param", menuName = "SO/Animator/Param", order = 0)]
    public class AnimParamSo : ScriptableObject
    {
        //애니메이션 파라미터 이름을 So로 관리한다.
        [field: SerializeField] public string ParamName { get; private set; }
        [field: SerializeField] public int HashValue { get; private set; } //헤쉬값

        private void OnValidate()
        {
            //글자를 쓰면 SO 헤쉬변환
            HashValue = Animator.StringToHash(ParamName);
        }
    }
}