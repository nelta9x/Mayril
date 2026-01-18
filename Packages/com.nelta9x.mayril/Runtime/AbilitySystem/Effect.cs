using System.Collections.Generic;
using Mayril.TagSystem;

namespace Mayril.AbilitySystem
{
    /// <summary>
    /// 이펙트.
    /// 어빌리티나 외부 요인에 의해 발생하는 상태 변화(스탯 변경, 태그 부여 등)를 정의합니다.
    /// 데이터 정의 클래스입니다. (Data Object)
    /// </summary>
    [System.Serializable]
    public class Effect
    {
        /// <summary>
        /// 이펙트 이름.
        /// </summary>
        public string Name;

        /// <summary>
        /// 지속 시간 타입.
        /// </summary>
        public EffectDurationType DurationPolicy;

        /// <summary>
        /// 지속 시간 (DurationPolicy가 HasDuration일 때 유효).
        /// </summary>
        public float Duration;

        /// <summary>
        /// 주기적 실행 간격 (0이면 사용 안함).
        /// </summary>
        public float Period;

        /// <summary>
        /// 주기적 실행을 적용 시점에도 즉시 실행할지 여부.
        /// </summary>
        public bool ExecutePeriodicEffectOnApplication = true;

        /// <summary>
        /// 적용할 모디파이어 목록.
        /// </summary>
        public List<EffectModifier> Modifiers = new();

        /// <summary>
        /// 이펙트가 활성화된 동안 소유자에게 부여되는 태그.
        /// </summary>
        public List<GameTag> GrantedTags = new();

        /// <summary>
        /// 이펙트 자체를 설명하는 태그.
        /// </summary>
        public List<GameTag> Tags = new();
    }
}
