using System.Collections.Generic;

namespace Mayril.AbilitySystem
{
    /// <summary>
    /// EffectInstance 객체 풀.
    /// 런타임 가비지 생성을 줄이기 위해 EffectInstance를 재사용합니다.
    /// </summary>
    public static class EffectInstancePool
    {
        private static readonly Stack<EffectInstance> _pool = new Stack<EffectInstance>();

        /// <summary>
        /// 풀에서 EffectInstance를 가져오거나 새로 생성합니다.
        /// </summary>
        public static EffectInstance Get(AbilitySystemComponent owner, Effect spec)
        {
            if (_pool.Count > 0)
            {
                var instance = _pool.Pop();
                instance.Initialize(owner, spec);
                return instance;
            }

            return new EffectInstance(owner, spec);
        }

        /// <summary>
        /// EffectInstance를 풀에 반납합니다.
        /// </summary>
        public static void Release(EffectInstance instance)
        {
            if (instance == null) return;
            
            // 안전을 위해 이미 풀에 있는지 확인하는 로직은 생략 (성능 최적화)
            // 호출자가 중복 반납하지 않도록 주의해야 함.
            
            // 참조 해제 (메모리 누수 방지)
            instance.Initialize(null, null); 
            
            _pool.Push(instance);
        }
        
        /// <summary>
        /// 풀을 비웁니다.
        /// </summary>
        public static void Clear()
        {
            _pool.Clear();
        }
    }
}
