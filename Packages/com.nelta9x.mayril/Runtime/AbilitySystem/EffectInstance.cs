using System.Collections.Generic;
using Mayril.TagSystem;

namespace Mayril.AbilitySystem
{
    /// <summary>
    /// 활성화된 이펙트 (Runtime Instance).
    /// Effect(Data)의 인스턴스로, 적용된 시간, 남은 지속시간 등을 관리합니다.
    /// </summary>
    public class EffectInstance
    {
        /// <summary>
        /// 이펙트 데이터 원본 (Spec).
        /// </summary>
        public Effect Spec { get; private set; }

        /// <summary>
        /// 소유자 ASC.
        /// </summary>
        public AbilitySystemComponent Owner { get; private set; }

        /// <summary>
        /// 시작 시간 (월드 시간).
        /// </summary>
        public float StartWorldTime { get; private set; }

        /// <summary>
        /// 지속 시간.
        /// </summary>
        public float Duration { get; private set; }

        /// <summary>
        /// 주기적 실행을 위한 마지막 틱 시간.
        /// </summary>
        public float LastPeriodicTickTime { get; set; }

        /// <summary>
        /// 활성화 여부.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 생성자.
        /// </summary>
        public EffectInstance(AbilitySystemComponent owner, Effect effectSpec)
        {
            Initialize(owner, effectSpec);
        }

        /// <summary>
        /// 이펙트를 초기화합니다.
        /// </summary>
        /// <param name="owner">이펙트 소유자 ASC</param>
        /// <param name="effectSpec">이펙트 데이터</param>
        public void Initialize(AbilitySystemComponent owner, Effect effectSpec)
        {
            Owner = owner;
            Spec = effectSpec;
            IsActive = true;
            LastPeriodicTickTime = 0f;
            StartWorldTime = 0f;

            if (effectSpec == null)
            {
                Duration = 0f;
                return;
            }

            // Duration 결정 (나중에 레벨 스케일링 등 고려 가능)
            if (effectSpec.DurationPolicy == EffectDurationType.HasDuration)
            {
                Duration = effectSpec.Duration;
            }
            else
            {
                Duration = 0f; // Instant or Infinite
            }
        }

        /// <summary>
        /// 시작 시간을 설정합니다.
        /// </summary>
        public void SetStartTime(float worldTime)
        {
            StartWorldTime = worldTime;
            LastPeriodicTickTime = worldTime;
        }

        /// <summary>
        /// 남은 시간을 반환합니다.
        /// </summary>
        public float GetTimeRemaining(float worldTime)
        {
            if (Spec.DurationPolicy == EffectDurationType.Infinite)
            {
                return -1f;
            }
            if (Spec.DurationPolicy == EffectDurationType.Instant)
            {
                return 0f;
            }

            return (StartWorldTime + Duration) - worldTime;
        }

        /// <summary>
        /// 이 이펙트가 부여하는 태그들.
        /// </summary>
        public IEnumerable<GameTag> GrantedTags => Spec.GrantedTags;
    }
}