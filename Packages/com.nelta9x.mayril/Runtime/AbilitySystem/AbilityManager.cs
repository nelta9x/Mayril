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
        
        /// <summary>
        /// 어빌리티들.
        /// </summary>
        public IReadOnlyList<IAbility> Abilities => _abilities;

        /// <summary>
        /// 어빌리티를 반환합니다.
        /// </summary>
        public IAbility GetAbility(int abilityIndex)
        {
            return _abilities.ElementAtOrDefault(abilityIndex);
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

            int abilityIndex = -1;
            for (int i = 0; i < _abilities.Count; i++)
            {// 빈 어빌리티 공간에 어빌리티 배치.
                if (_abilities[i] == null)
                {
                    abilityIndex = i;
                    _abilities[i] = ability;
                    ability.OnAdded();
                    break;
                }
            }

            if (abilityIndex == -1)
            {// 빈 어빌리티 공간이 없으므로 새로 할당.
                abilityIndex = _abilities.Count;
                _abilities.Add(ability);
                ability.OnAdded();
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
                    _abilities[i] = null;
                    ability.OnRemoved();
                    break;
                }
            }
        }
    }
}