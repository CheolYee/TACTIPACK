using System;
using System.Collections.Generic;
using System.Reflection;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Agents;
using _00.Work.WorkSpace.CheolYee._04.Scripts.Core.Attacks;
using DG.Tweening;

namespace _00.Work.Resource.Scripts.Utils
{
    //트윈 리플렉션
    //런타임에서 정보를 찾아 실행하는 방법
    //스위치로 상태를 하드코딩하지 않고 표시하여 자동으로 메서드를 찾는 형태
    
    //리플렉션 어트리부트
    //이 속성은 메서드에만 붙일 수 있다는 선언을 한다.
    [AttributeUsage((AttributeTargets.Method))] 
    //고정된 의미로 sealed를 붙여 상속 불가능하도록 한다.
    public sealed class StanceHandlerAttribute : Attribute
    {
        //생성자에서 받은 Stance를 보관한다.
        public AttackStance Stance { get; private set; }

        //생성자에서 Stance를 받는다. (매개변수 생성자)
        public StanceHandlerAttribute(AttackStance stance)
        {
            Stance = stance;
        }
    }
    
    //리플렉션 실행기
    public static class StanceInvoker
    {
        //인스턴스 생성 없이 전역 유틸로 쓰기 위해 정적 딕셔너리 선언
        //스탠스를 키로, 메서드 정보를 값으로 캐싱한다.
        //MethodInfo : 이 메서드를 어떻게 호출하는가? 를 담은 리플렉션 타입
        //리플렉션은 느리기 때문에 한번만 찾고 Map에서 꺼내쓰는게 유리하다.
        private static readonly Dictionary<AttackStance, MethodInfo> Map = new();

        //맵을 생성한다 (찾는다)
        public static void BuildMap(object instance) //객체 인스턴스를 넘김
        {
            //인스턴스 객체의 타입을 가져온다.
            Type type = instance.GetType(); //실제 넘겨준 객체의 타입을 가져온다.
            //구한 타입 안에 있는 모든 메서드들을 다 가져온다.
            //BindingFlags : public, private, protected 메서드를 가져올 수 있도록 허용하겠다는의미
            MethodInfo[] methodInfoList = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            
            foreach (MethodInfo method in methodInfoList)
            {
                //메서드 중 StanceHandler가 달린 메서드만 선별하겠다
                StanceHandlerAttribute attribute = method.GetCustomAttribute<StanceHandlerAttribute>();
                //만약 속성이 달린게 없다면 continue하고 다음 값 확인하기
                if (attribute == null) continue;

                //반환형 강제
                //메서드의 반환형이 반드시 Tween이어야 한다.
                //호출한 쪽에서 await / Oncomplete 등 트윈을 일관적으로 다루기 위해.
                if (method.ReturnType != typeof(Tween))
                {
                    //Tween이 아니었다면 익셉션 로그 발생
                    throw new InvalidOperationException($"{method.Name} must return a Tween");
                }
                
                //파라미터 강제 제한
                //파라미터의 개수가 1개여야 하며 매개변수의 타입이 SkillContent여야 한다.
                //원하는 메서드를 정확히 선별하기 위해서 사용한다.
                ParameterInfo[] parameters = method.GetParameters();
                //만약 조건이 충족되지 않았다면 익셉션 로그를 뱉는다.
                if (parameters.Length != 1 || parameters[0].ParameterType != typeof(SkillContent))
                    throw new InvalidOperationException($"{method.Name} must have a single SkillContent");
                
                //완벽히 필터링 된 메서드가 있다면 딕셔너리에 Stance를 키로, 메서드를 값으로 저장한다.
                Map[attribute.Stance] = method;
            }
        }

        //Stance로 메서드를 찾는다.
        public static Tween Invoke(object instance, AttackStance stance, SkillContent skillContent)
        {
            //찾았다면 메서드를 실제 호출한다.
            if (Map.TryGetValue(stance, out MethodInfo methodInfo))
            {
                //메서드를 실행한다.
                //1. 인스턴스 : 그 객체의 메서드를 호출해야함
                //2. 파라미터 배열 : 짜피 위에서 선별했으므로 하나만 들어갈 것이기 때문에 skillContent를 설정해준다.
                return (Tween)methodInfo.Invoke(instance, new object[] { skillContent });
            }
            
            //안됐으면 널 반환
            return null;
        }
    }
}