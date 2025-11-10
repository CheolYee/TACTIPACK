using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.AnimatorSystem;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Agents
{
    public interface IAgentRenderer
    {
        float FacingDirection { get; } //보는 방향
        Sprite CurrentSprite { get; } //현재 스프라이트
        public void SetParam(AnimParamSo param, bool value); //파라미터 세팅 오버로딩 bool
        public void SetParam(AnimParamSo param, int value); //int
        public void SetParam(AnimParamSo param, float value); //float
        public void SetParam(AnimParamSo param); //트리거용
    }
}