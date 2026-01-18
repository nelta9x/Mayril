using Mayril.TagSystem;

namespace Mayril.AbilitySystem
{
    /// <summary>
    /// 어빌리티.
    /// 액터가 수행할 수 있는 행동(스킬, 공격 등)의 논리를 정의합니다.
    /// </summary>
    public abstract class Ability
    {
        /// <summary>
        /// 어빌리티 이름.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 이 어빌리티를 소유한 AbilitySystemComponent.
        /// </summary>
        public AbilitySystemComponent Owner { get; private set; }

        // Tags
        /// <summary>
        /// 이 어빌리티가 가지는 태그들.
        /// </summary>
        public GameTagContainer AbilityTags { get; } = new();

        /// <summary>
        /// 이 어빌리티 발동 시 취소시킬, 해당 태그를 가진 다른 어빌리티들.
        /// </summary>
        public GameTagContainer CancelAbilitiesWithTags { get; } = new();

        /// <summary>
        /// 이 어빌리티가 활성화된 동안, 발동을 막을 다른 어빌리티들의 태그.
        /// </summary>
        public GameTagContainer BlockAbilitiesWithTags { get; } = new();

        /// <summary>
        /// 이 어빌리티가 활성화된 동안, 소유자에게 부여될 태그들.
        /// </summary>
        public GameTagContainer ActivationOwnedTags { get; } = new();

        /// <summary>
        /// 이 어빌리티를 발동하기 위해 소유자가 반드시 가지고 있어야 하는 태그들.
        /// </summary>
        public GameTagContainer ActivationRequiredTags { get; } = new();

        /// <summary>
        /// 이 어빌리티를 발동하기 위해 소유자가 가지고 있으면 안 되는 태그들.
        /// </summary>
        public GameTagContainer ActivationBlockedTags { get; } = new();

        /// <summary>
        /// 비용 이펙트.
        /// </summary>
        public Effect CostEffect;

        /// <summary>
        /// 쿨타임 이펙트.
        /// </summary>
        public Effect CooldownEffect;

        /// <summary>
        /// 어빌리티가 현재 활성화 상태인지 여부.
        /// </summary>
        public bool IsActive { get; protected set; }

        /// <summary>
        /// 어빌리티가 ASC에 추가될 때 호출됩니다.
        /// </summary>
        public virtual void OnGiveAbility(AbilitySystemComponent owner)
        {
            Owner = owner;
        }

        /// <summary>
        /// 어빌리티가 ASC에서 제거될 때 호출됩니다.
        /// </summary>
        public virtual void OnRemoveAbility()
        {
            Owner = null;
        }

        /// <summary>
        /// 어빌리티를 현재 발동할 수 있는지 확인합니다.
        /// <br/>
        /// 기본 구현에서는 다음 항목들을 순서대로 확인합니다:
        /// 1. 어빌리티가 이미 활성화 상태(`IsActive`)가 아님
        /// 2. 소유자(`Owner`)가 존재함
        /// 3. 소유자가 `ActivationRequiredTags`를 모두 가지고 있음
        /// 4. 소유자가 `ActivationBlockedTags`를 하나라도 가지고 있지 않음
        /// 5. 쿨타임(`CooldownEffect` 태그)이 적용 중이 아님
        /// </summary>
        public virtual bool CanActivateAbility()
        {
            if (IsActive)
            {
                return false;
            }

            if (Owner == null)
            {
                return false;
            }

            if (!CheckTagRequirements())
            {
                return false;
            }

            if (!CheckCooldown())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 어빌리티 발동을 위한 태그 조건을 확인합니다.
        /// (RequiredTags, BlockedTags)
        /// </summary>
        protected virtual bool CheckTagRequirements()
        {
            if (Owner == null) return false;

            if (!Owner.HasAllTags(ActivationRequiredTags.Tags))
            {
                return false;
            }

            if (Owner.HasAnyTag(ActivationBlockedTags.Tags))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 어빌리티를 발동합니다.
        /// </summary>
        public virtual void ActivateAbility()
        {
            if (!CanActivateAbility())
            {
                return;
            }

            // Cancel other abilities
            Owner.CancelAbilities(CancelAbilitiesWithTags);

            IsActive = true;

            // Add Activation Tags
            if (ActivationOwnedTags.Count > 0)
            {
                Owner.AddLooseTags(ActivationOwnedTags.Tags);
            }

            CommitAbility();
            OnActivateAbility();
        }

        /// <summary>
        /// 어빌리티 발동 시 실행될 로직을 정의합니다. (오버라이드용)
        /// </summary>
        protected virtual void OnActivateAbility() { }

        /// <summary>
        /// 어빌리티를 종료합니다.
        /// </summary>
        public virtual void EndAbility()
        {
            if (!IsActive)
            {
                return;
            }

            // Remove Activation Tags
            if (ActivationOwnedTags.Count > 0)
            {
                Owner.RemoveLooseTags(ActivationOwnedTags.Tags);
            }

            IsActive = false;
            OnEndAbility();
        }

        /// <summary>
        /// 어빌리티 종료 시 실행될 로직을 정의합니다. (오버라이드용)
        /// </summary>
        protected virtual void OnEndAbility() { }

        /// <summary>
        /// 어빌리티의 비용과 쿨타임을 적용합니다.
        /// 비용 지불(ApplyCost)이 필요한 경우 이 메소드를 오버라이드하여 구현해야 합니다.
        /// </summary>
        public virtual void CommitAbility()
        {
            ApplyCooldown();
        }

        /// <summary>
        /// 쿨타임 상태인지 확인합니다.
        /// </summary>
        protected virtual bool CheckCooldown()
        {
            if (CooldownEffect == null)
            {
                return true;
            }

            // CooldownEffect가 부여하는 태그가 현재 Owner에게 있다면 쿨타임 중인 것으로 간주.
            foreach (var tag in CooldownEffect.GrantedTags)
            {
                if (Owner.HasTag(tag))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 쿨타임을 적용합니다.
        /// </summary>
        protected virtual void ApplyCooldown()
        {
            if (CooldownEffect == null)
            {
                return;
            }

            Owner.ApplyEffectToSelf(CooldownEffect);
        }
    }
}