using System;
using System.Collections.Generic;

namespace Mayril.AbilitySystem
{
    /// <summary>
    /// 어빌리티 모디파이어들을 관리하는 클래스.
    /// </summary>
    public class AbilityModifierManager
    {
        private readonly TimerManager _timerManager;
        private readonly AbilityModifierContainer _modifiers = new();
        private readonly List<AbilityModifier> _tempModifierBuffer = new(8);
        private readonly Dictionary<AbilityModifier, TimerHandle> _durationHandles = new();
        private readonly Dictionary<AbilityModifier, TimerHandle> _intervalTickHandles = new();

        public AbilityModifierManager(TimerManager timerManager)
        {
            _timerManager = timerManager ?? throw new ArgumentNullException(nameof(timerManager));
        }

        /// <summary>
        /// 어빌리티 모디파이어들.
        /// </summary>
        public IEnumerable<AbilityModifier> Modifiers => _modifiers.Modifiers;

        /// <summary>
        /// 모디파이어를 추가합니다.
        /// </summary>
        public void AddModifier(AbilityModifier modifier)
        {
            _modifiers.AddModifier(modifier);
            modifier.OnAttached();
            if (modifier.EnablerStack > 0)
            {
                modifier.IsEnabled = true;
                modifier.OnEnabled();
            }

            // 기존의 어빌리티들의 활성화 여부를 수정.
            UpdateExistingModifiersEnablerStack(modifier, isAdding: true);

            // Duration 타이머 등록
            if (modifier.Duration > 0)
            {
                var handle = _timerManager.PostEvent(modifier.Duration, () => RemoveModifier(modifier));
                _durationHandles[modifier] = handle;
            }

            // IntervalTick 타이머 등록
            if (modifier.UseIntervalTick && modifier.TickInterval > 0)
            {
                ScheduleNextIntervalTick(modifier);
            }
        }

        /// <summary>
        /// 모디파이어를 제거합니다.
        /// </summary>
        public void RemoveModifier(AbilityModifier modifier)
        {
            // 타이머 취소
            CancelModifierTimers(modifier);
            if (modifier.IsEnabled)
            {
                modifier.OnDisabled();
            }

            _modifiers.RemoveModifier(modifier);
            modifier.OnDetached();
            UpdateExistingModifiersEnablerStack(modifier, isAdding: false);
        }

        /// <summary>
        /// 특정 어빌리티의 모디파이어들을 모두 제거합니다.
        /// </summary>
        public void RemoveModifiers(IAbility ability)
        {
            _tempModifierBuffer.AddRange(_modifiers.GetModifiersByAbility(ability));
            _modifiers.RemoveModifiersByAbility(ability);
            foreach (var modifier in _tempModifierBuffer)
            {
                // 타이머 취소
                CancelModifierTimers(modifier);
                if (modifier.IsEnabled)
                {
                    modifier.OnDisabled();
                }

                modifier.OnDetached();
                UpdateExistingModifiersEnablerStack(modifier, isAdding: false);
            }

            _tempModifierBuffer.Clear();
        }

        /// <summary>
        /// 두 태그 리스트 간에 매칭되는 태그가 있는지 확인합니다.
        /// </summary>
        private bool HasMatchingTag(List<string> searchTags, List<string> targetTags)
        {
            foreach (var tag in searchTags)
            {
                if (targetTags.Contains(tag))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 모디파이어의 EnablerStack 변경 시 활성화 상태를 업데이트합니다.
        /// </summary>
        private void TryChangeModifierEnabledState(AbilityModifier modifier, int oldEnablerStack)
        {
            if (modifier.EnablerStack == oldEnablerStack)
            {
                return;
            }

            if (modifier.EnablerStack > 0 && !modifier.IsEnabled)
            {
                modifier.IsEnabled = true;
                modifier.OnEnabled();
            }
            else if (modifier.EnablerStack <= 0 && modifier.IsEnabled)
            {
                modifier.IsEnabled = false;
                modifier.OnDisabled();
            }
        }

        /// <summary>
        /// 기존 모디파이어들의 EnablerStack을 업데이트합니다.
        /// </summary>
        /// <param name="triggerModifier">추가되거나 제거되는 모디파이어</param>
        /// <param name="isAdding">true면 추가, false면 제거</param>
        private void UpdateExistingModifiersEnablerStack(AbilityModifier triggerModifier, bool isAdding)
        {
            foreach (var existingModifier in _modifiers.Modifiers)
            {
                bool hasDecreaseTag = HasMatchingTag(triggerModifier.DecreaseEnablerTags, existingModifier.ModifierTags);
                bool hasIncreaseTag = HasMatchingTag(triggerModifier.IncreaseEnablerTags, existingModifier.ModifierTags);
                int oldEnablerStack = existingModifier.EnablerStack;
                if (isAdding)
                {
                    // 모디파이어 추가 시
                    if (hasDecreaseTag)
                    {
                        --existingModifier.EnablerStack;
                    }

                    if (hasIncreaseTag)
                    {
                        ++existingModifier.EnablerStack;
                    }
                }
                else
                {
                    // 모디파이어 제거 시 (반대로 적용)
                    if (hasDecreaseTag)
                    {
                        ++existingModifier.EnablerStack;
                    }

                    if (hasIncreaseTag)
                    {
                        --existingModifier.EnablerStack;
                    }
                }

                TryChangeModifierEnabledState(existingModifier, oldEnablerStack);
            }
        }

        /// <summary>
        /// 모디파이어의 모든 타이머를 취소합니다.
        /// </summary>
        private void CancelModifierTimers(AbilityModifier modifier)
        {
            if (_durationHandles.TryGetValue(modifier, out var durationHandle))
            {
                _timerManager.CancelEvent(durationHandle);
                _durationHandles.Remove(modifier);
            }

            if (_intervalTickHandles.TryGetValue(modifier, out var tickHandle))
            {
                _timerManager.CancelEvent(tickHandle);
                _intervalTickHandles.Remove(modifier);
            }
        }

        /// <summary>
        /// 다음 IntervalTick을 예약합니다.
        /// </summary>
        private void ScheduleNextIntervalTick(AbilityModifier modifier)
        {
            var handle = _timerManager.PostEvent(modifier.TickInterval, () =>
            {
                // 모디파이어가 이미 제거된 경우 (타이머 취소와 콜백 실행 사이의 경합 상태 대비)
                if (!_intervalTickHandles.Remove(modifier))
                {
                    return;
                }

                if (modifier.IsEnabled)
                {
                    modifier.OnIntervalTick();

                    // 다음 틱 예약 (모디파이어가 아직 유효한 경우)
                    if (modifier.UseIntervalTick && modifier.TickInterval > 0)
                    {
                        ScheduleNextIntervalTick(modifier);
                    }
                }
            });

            _intervalTickHandles[modifier] = handle;
        }
    }
}