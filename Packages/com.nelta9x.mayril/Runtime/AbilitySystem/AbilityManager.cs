using System.Collections.Generic;

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
        /// 어빌리티를 추가합니다.
        /// </summary>
        public void AddAbility(IAbility ability)
        {
            _abilities.Add(ability);
            ability.OnAdded();
        }

        /// <summary>
        /// 어빌리티를 제거합니다.
        /// </summary>
        public void RemoveAbility(IAbility ability)
        {
            _abilities.Remove(ability);
            ability.OnRemoved();
        }

        /// <summary>
        /// 트리거에 들어왔을 때 호출됩니다.
        /// </summary>
        public void OnTriggerEnter(Entity entity)
        {
            foreach (var ability in _abilities)
            {
                ability.OnTriggerEnter(entity);
            }
        }
        
        /// <summary>
        /// 트리거에서 나갈 때 호출됩니다.
        /// </summary>
        public void OnTriggerExit(Entity entity)
        {
            foreach (var ability in _abilities)
            {
                ability.OnTriggerExit(entity);
            }
        }
    }
}