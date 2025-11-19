using System.Collections.Generic;

namespace Mayril.AbilitySystem
{
    /// <summary>
    /// 어빌리티 모디파이어들을 관리하는 클래스.
    /// </summary>
    public class AbilityModifierManager
    {
        private readonly AbilityModifierContainer _modifiers = new();
        private readonly List<AbilityModifier> _tempModifierBuffer = new(8);

        /// <summary>
        /// 어빌리티 모디파이어들.
        /// </summary>
        public IEnumerable<AbilityModifier> Modifiers => _modifiers.Modifiers;
        
        /// <summary>
        /// 틱을 받는 어빌리티 모디파이어들.
        /// </summary>
        public IEnumerable<AbilityModifier> TickableModifiers => _modifiers.TickableModifiers;
        
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
        }

        /// <summary>
        /// 모디파이어를 제거합니다.
        /// </summary>
        public void RemoveModifier(AbilityModifier modifier)
        {
            if (modifier.IsEnabled)
            {
                modifier.OnDisabled();
            }

            _modifiers.RemoveModifier(modifier);
            modifier.OnDetached();

            // 기존의 어빌리티들의 활성화 여부를 수정 (제거 시 반대로 적용).
            UpdateExistingModifiersEnablerStack(modifier, isAdding: false);
        }

        /// <summary>
        /// 특정 어빌리티의 모디파이어들을 모두 제거합니다.
        /// </summary>
        public void RemoveModifiers(IAbility ability)
        {
            _tempModifierBuffer.AddRange(_modifiers.GetModifiersByAbility(ability));
            foreach (var modifier in _tempModifierBuffer)
            {
                RemoveModifier(modifier);
            }
            
            _tempModifierBuffer.Clear();
        }

        /// <summary>
        /// 어빌리티 모디파이어들을 업데이트합니다.
        /// </summary>
        public void Update(float deltaTime)
        {
            foreach (var tickableModifier in _modifiers.TickableModifiers)
            {
                tickableModifier.TickRemaining -= deltaTime;
                while (tickableModifier.TickRemaining <= 0)
                {// deltaTime이 IntervalTick을 여러번 호출 가능한 시간 이후에 호출되었을 경우를 대비해,
                 // 호출되었어야 하는 만큼 OnIntervalTick을 호출합니다.
                    tickableModifier.OnIntervalTick();
                    tickableModifier.TickRemaining += tickableModifier.TickInterval;
                }
            }
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
    }
}