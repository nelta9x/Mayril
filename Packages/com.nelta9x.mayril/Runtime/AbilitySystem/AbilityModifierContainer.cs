using System.Collections.Generic;

namespace Mayril.AbilitySystem
{
    /// <summary>
    /// 모디파이어들을 담는 컨테이너.
    /// </summary>
    public class AbilityModifierContainer
    {
        private readonly HashSet<AbilityModifier> _modifiers = new();
        private readonly Dictionary<IAbility, List<AbilityModifier>> _modifiersByAbility = new();

        /// <summary>
        /// 모디파이어 수.
        /// </summary>
        public int ModifierCount => _modifiers.Count;
        
        /// <summary>
        /// 모디파이어들.
        /// </summary>
        public IEnumerable<AbilityModifier> Modifiers => _modifiers;

        /// <summary>
        /// 모디파이어를 추가합니다.
        /// </summary>
        public void AddModifier(AbilityModifier abilityModifier)
        {
            _modifiers.Add(abilityModifier);
   
            // 어빌리티별로 모디파이어 추가
            if (abilityModifier.Ability != null)
            {
                if (!_modifiersByAbility.ContainsKey(abilityModifier.Ability))
                {
                    _modifiersByAbility[abilityModifier.Ability] = new List<AbilityModifier>();
                }

                _modifiersByAbility[abilityModifier.Ability].Add(abilityModifier);
            }
        }

        /// <summary>
        /// 모디파이어를 제거합니다.
        /// </summary>
        public void RemoveModifier(AbilityModifier abilityModifier)
        {
            _modifiers.Remove(abilityModifier);

            // 어빌리티별 목록에서 제거
            if (abilityModifier.Ability != null && _modifiersByAbility.ContainsKey(abilityModifier.Ability))
            {
                _modifiersByAbility[abilityModifier.Ability].Remove(abilityModifier);

                // 리스트가 비었으면 키 제거
                if (_modifiersByAbility[abilityModifier.Ability].Count == 0)
                {
                    _modifiersByAbility.Remove(abilityModifier.Ability);
                }
            }
        }

        /// <summary>
        /// 특정 어빌리티의 모든 모디파이어를 가져옵니다.
        /// </summary>
        public IEnumerable<AbilityModifier> GetModifiersByAbility(IAbility ability)
        {
            if (!_modifiersByAbility.TryGetValue(ability, out var abilityModifiers))
            {
                return new List<AbilityModifier>();
            }

            return abilityModifiers;
        }

        /// <summary>
        /// 특정 어빌리티의 모든 모디파이어를 제거합니다.
        /// </summary>
        public void RemoveModifiersByAbility(IAbility ability)
        {
            if (!_modifiersByAbility.TryGetValue(ability, out var abilityModifiers))
            {
                return;
            }

            var modifiersToRemove = new List<AbilityModifier>(abilityModifiers);
            foreach (var modifier in modifiersToRemove)
            {
                RemoveModifier(modifier);
            }
        }

        /// <summary>
        /// 특정 어빌리티의 모디파이어가 존재하는지 확인합니다.
        /// </summary>
        public bool HasModifiersFromAbility(IAbility ability)
        {
            return ability != null && _modifiersByAbility.ContainsKey(ability) && _modifiersByAbility[ability].Count > 0;
        }
    }
}