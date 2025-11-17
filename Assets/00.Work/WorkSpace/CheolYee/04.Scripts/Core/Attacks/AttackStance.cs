using System.Collections.Generic;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks
{
    /// <summary>
    /// Stationary, 가만히 있기
    /// StepForward, 앞으로 가기 (특정 지점 위치 한개)
    /// DashToTarget, 타겟 앞으로 가기
    /// CustomPoint 특정 지정 위치 설정해주기
    /// </summary>
    public enum AttackStance
    {
        Stationary, //가만히 있기
        StepForward, //앞으로 가기 (특정 지점 위치 한개)
        DashToTarget, //타겟 앞으로 가기
        CustomPoint //특정 지정 위치 설정해주기
    }

    public enum TargetingMode
    {
        Single, //단일
        Area, //범위
        Random, //랜덤
        None
    }

    //스킬 실행에 필요한 것들
    public sealed class SkillContent
    {
        public Agent User; //시전자
        public List<AgentHealth> Targets = new List<AgentHealth>(); //적이나 플레이어 리스트
        public Vector3 CastPoint; //캐스트 위치
        public TargetingMode TargetingMode; //타겟팅 이넘
        public AttackStance Stance; //이동 이넘
        public float ApproachOffset; //대쉬 거리 오프셋 등
    }
}