using System.Collections.Generic;
using UnityEngine;
using Mayril.StatSystem;
using Mayril.TagSystem;
using Unity.Netcode;

namespace Mayril.AbilitySystem
{
    /// <summary>
    /// 어빌리티 시스템 컴포넌트.
    /// 어빌리티, 이펙트, 태그, 스탯을 중앙 관리하는 컴포넌트입니다.
    /// </summary>
    public class AbilitySystemComponent : NetworkBehaviour
    {
        private readonly List<IStatSet> _statSets = new();
        private readonly List<EffectInstance> _activeEffects = new();
        private readonly GameTagContainer _ownedTags = new();
        private readonly List<Ability> _grantedAbilities = new();

        public IReadOnlyList<EffectInstance> ActiveEffects => _activeEffects;
        public IReadOnlyList<Ability> GrantedAbilities => _grantedAbilities;
        public GameTagContainer OwnedTags => _ownedTags;

        /// <summary>
        /// 스탯 셋을 등록합니다.
        /// </summary>
        public void RegisterStats(IStatSet stats)
        {
            if (stats != null && !_statSets.Contains(stats))
            {
                _statSets.Add(stats);
            }
        }

        /// <summary>
        /// 스탯 셋 등록을 해제합니다.
        /// </summary>
        public void UnregisterStats(IStatSet stats)
        {
            if (stats != null)
            {
                _statSets.Remove(stats);
            }
        }

        /// <summary>
        /// 스탯을 가져옵니다.
        /// </summary>
        public StatValue GetStat(GameTag statTag)
        {
            if (_statSets == null)
            {
                return null;
            }

            foreach (var stats in _statSets)
            {
                var stat = stats.GetStatValue(statTag);
                if (stat != null)
                {
                    return stat;
                }
            }

            return null;
        }

        /// <summary>
        /// 태그를 직접(Loose) 추가합니다.
        /// 이펙트에 의해 관리되지 않는 수동 태그입니다.
        /// </summary>
        public void AddLooseTags(IEnumerable<GameTag> tags)
        {
            _ownedTags.AddTags(tags);
        }

        /// <summary>
        /// 직접 추가된 태그를 제거합니다.
        /// </summary>
        public void RemoveLooseTags(IEnumerable<GameTag> tags)
        {
            _ownedTags.RemoveTags(tags);
        }

        /// <summary>
        /// 특정 태그를 보유하고 있는지 확인합니다.
        /// </summary>
        public bool HasTag(GameTag tag) => _ownedTags.HasTag(tag);

        /// <summary>
        /// 모든 태그를 보유하고 있는지 확인합니다.
        /// </summary>
        public bool HasAllTags(IEnumerable<GameTag> tags) => _ownedTags.HasAll(tags);

        /// <summary>
        /// 하나라도 태그를 보유하고 있는지 확인합니다.
        /// </summary>
        public bool HasAnyTag(IEnumerable<GameTag> tags) => _ownedTags.HasAny(tags);

        /// <summary>
        /// 어빌리티를 추가합니다.
        /// </summary>
        public void AddAbility(Ability ability)
        {
            if (ability == null)
            {
                return;
            }
            if (_grantedAbilities.Contains(ability))
            {
                return;
            }

            ability.OnGiveAbility(this);
            _grantedAbilities.Add(ability);
        }

        /// <summary>
        /// 어빌리티를 제거합니다.
        /// 활성 상태인 어빌리티는 자동으로 EndAbility()가 호출됩니다.
        /// </summary>
        public void RemoveAbility(Ability ability)
        {
            if (_grantedAbilities.Remove(ability))
            {
                ability.EndAbility();
                ability.OnRemoveAbility();
            }
        }

        /// <summary>
        /// 어빌리티 활성화를 시도합니다.
        /// </summary>
        public bool TryActivateAbility(Ability ability)
        {
            if (!_grantedAbilities.Contains(ability))
            {
                return false;
            }

            if (ability.CanActivateAbility())
            {
                ability.ActivateAbility();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 태그와 일치하는 활성화된 어빌리티들을 취소합니다.
        /// </summary>
        public void CancelAbilities(GameTagContainer tags)
        {
            if (tags == null || tags.Count == 0)
            {
                return;
            }

            // 순회 중 리스트 변경 가능성 대비 (EndAbility -> RemoveAbility?)
            // 보통 EndAbility는 IsActive만 끄고 리스트에서 제거하진 않음.
            foreach (var ability in _grantedAbilities)
            {
                if (ability.IsActive && ability.AbilityTags.HasAny(tags.Tags))
                {
                    ability.EndAbility();
                }
            }
        }

        /// <summary>
        /// 이펙트를 자신에게 적용합니다.
        /// </summary>
        /// <param name="effectSpec">적용할 이펙트 데이터</param>
        /// <returns>생성된 활성 이펙트 (Instant인 경우 null)</returns>
        public EffectInstance ApplyEffectToSelf(Effect effectSpec)
        {
            if (effectSpec == null)
            {
                return null;
            }

            // TODO: ApplicationTagRequirements (면역 등) 확인 로직

            var activeEffect = EffectInstancePool.Get(this, effectSpec);
            activeEffect.SetStartTime(Time.time);

            bool isInstant = effectSpec.DurationPolicy == EffectDurationType.Instant;

            if (isInstant)
            {
                _ownedTags.AddTags(effectSpec.GrantedTags);
                ExecuteEffect(activeEffect);
                _ownedTags.RemoveTags(effectSpec.GrantedTags);
                EffectInstancePool.Release(activeEffect);
                return null; // Instant 이펙트는 활성 리스트에 남지 않음
            }
            else
            {
                // Duration or Infinite
                _activeEffects.Add(activeEffect);

                // Grant Tags
                _ownedTags.AddTags(effectSpec.GrantedTags);

                // Apply Passive Modifiers (Duration-based Buff/Debuff)
                ApplyEffectModifiers(activeEffect);

                // Execute Periodic logic on application if requested
                if (effectSpec.Period > 0 && effectSpec.ExecutePeriodicEffectOnApplication)
                {
                    ExecuteEffect(activeEffect);
                }

                return activeEffect;
            }
        }

        /// <summary>
        /// 이펙트 로직(스탯 변경 등)을 즉시 실행합니다. (Instant, Periodic)
        /// </summary>
        private void ExecuteEffect(EffectInstance activeEffect)
        {
            foreach (var modSpec in activeEffect.Spec.Modifiers)
            {
                var stat = GetStat(modSpec.StatTag);
                if (stat != null)
                {
                    float rawValue = modSpec.ModifierValue;

                    switch (modSpec.ModifierType)
                    {
                        case StatModifierType.Additive:
                            stat.BaseValue += rawValue;
                            break;
                        case StatModifierType.Multiplicative:
                            stat.BaseValue *= rawValue;
                            break;
                        case StatModifierType.Fixed:
                            stat.BaseValue = rawValue;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 이펙트의 모디파이어를 스탯에 등록합니다. (Duration)
        /// </summary>
        private void ApplyEffectModifiers(EffectInstance activeEffect)
        {
            foreach (var modSpec in activeEffect.Spec.Modifiers)
            {
                var stat = GetStat(modSpec.StatTag);
                if (stat != null)
                {
                    stat.AddModifier(modSpec.ToStatModifier());
                }
            }
        }

        /// <summary>
        /// 이펙트의 모디파이어를 스탯에서 제거합니다.
        /// </summary>
        private void RemoveEffectModifiers(EffectInstance activeEffect)
        {
            foreach (var modSpec in activeEffect.Spec.Modifiers)
            {
                var stat = GetStat(modSpec.StatTag);
                if (stat != null)
                {
                    stat.RemoveModifier(modSpec.ToStatModifier());
                }
            }
        }

        /// <summary>
        /// 활성화된 이펙트를 제거합니다.
        /// </summary>
        public void RemoveActiveEffect(EffectInstance effect)
        {
            if (_activeEffects.Contains(effect))
            {
                // Remove Tags
                RemoveLooseTags(effect.GrantedTags);

                // Remove Modifiers
                RemoveEffectModifiers(effect);

                _activeEffects.Remove(effect);
                effect.IsActive = false;

                // 풀 반납
                EffectInstancePool.Release(effect);
            }
        }

        private void Update()
        {
            float time = Time.time;

            // 역순 순회 (삭제 안전)
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                var effect = _activeEffects[i];
                if (!effect.IsActive)
                {
                    RemoveActiveEffect(effect);
                    continue;
                }

                // Check Duration
                if (effect.Spec.DurationPolicy == EffectDurationType.HasDuration)
                {
                    if (effect.GetTimeRemaining(time) <= 0)
                    {
                        RemoveActiveEffect(effect);
                        continue;
                    }
                }

                // Check Periodic
                if (effect.Spec.Period > 0)
                {
                    if (time - effect.LastPeriodicTickTime >= effect.Spec.Period)
                    {
                        effect.LastPeriodicTickTime = time;
                        ExecuteEffect(effect);
                    }
                }
            }
        }
    }
}
