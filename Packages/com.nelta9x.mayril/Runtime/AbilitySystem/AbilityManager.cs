using System;
using System.Collections.Generic;
using System.Linq;

namespace Mayril.AbilitySystem
{
    /// <summary>
    /// 어빌리티 관리자.
    /// </summary>
    public class AbilityManager
    {
        private readonly List<IAbility> _abilities = new();
        private readonly List<AbilityContext> _abilityContexts = new();
        
        /// <summary>
        /// 어빌리티들.
        /// </summary>
        public IReadOnlyList<IAbility> Abilities => _abilities;
        
        /// <summary>
        /// 어빌리티 컨텍스트들.
        /// </summary>
        public IReadOnlyList<AbilityContext> AbilityContexts => _abilityContexts;

        /// <summary>
        /// 어빌리티를 반환합니다.
        /// </summary>
        public IAbility GetAbility(int abilityIndex)
        {
            return _abilities.ElementAtOrDefault(abilityIndex);
        }

        /// <summary>
        /// 어빌리티 컨텍스트를 반환합니다.
        /// 어빌리티가 있을 경우, 항상 어빌리티 컨텍스트도 같은 인덱스에 있음이 보장됩니다.
        /// </summary>
        public AbilityContext GetAbilityContext(int abilityIndex)
        {
            return _abilityContexts.ElementAtOrDefault(abilityIndex);
        }

        /// <summary>
        /// 어빌리티를 추가합니다.
        /// </summary>
        public int AddAbility(IAbility ability)
        {
            if (ability == null)
            {
                throw new ArgumentNullException(nameof(ability));
            }
            
            var newContext = new AbilityContext()
            {
                Ability = ability
            };

            int abilityIndex = -1;
            for (int i = 0; i < _abilities.Count; i++)
            {// 빈 어빌리티 공간에 어빌리티 배치.
                if (_abilities[i] == null)
                {
                    abilityIndex = i;
                    _abilities[i] = ability;
                    _abilityContexts[i] = newContext;
                    ability.OnAdded(newContext);
                    break;
                }
            }

            if (abilityIndex == -1)
            {// 빈 어빌리티 공간이 없으므로 새로 할당.
                abilityIndex = _abilities.Count;
                _abilities.Add(ability);
                _abilityContexts.Add(newContext);
                ability.OnAdded(newContext);
            }
            
            return abilityIndex;
        }

        /// <summary>
        /// 어빌리티를 제거합니다.
        /// </summary>
        public void RemoveAbility(IAbility ability)
        {
            if (ability == null)
            {
                throw new ArgumentNullException(nameof(ability));
            }
            
            for (int i = 0; i < _abilities.Count; i++)
            {
                if (_abilities[i] == ability)
                {
                    var context = _abilityContexts[i];
                    _abilities[i] = null;
                    _abilityContexts[i] = null;
                    ability.OnRemoved(context);
                    break;
                }
            }
        }
    }
}