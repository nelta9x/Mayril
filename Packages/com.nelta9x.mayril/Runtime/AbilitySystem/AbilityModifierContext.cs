using System.Collections.Generic;

namespace Mayril.AbilitySystem
{
    public class AbilityModifierContext
    {
        /// <summary>
        /// 모디파이어 키.
        /// </summary>
        public int ModifierKey;
        
        /// <summary>
        /// 틱 수.
        /// </summary>
        public int TickCount;

        /// <summary>
        /// 다음 Tick 호출까지 남은 시간.
        /// </summary>
        public float TickRemaining;
        
        /// <summary>
        /// 값들.
        /// </summary>
        public readonly Dictionary<string, dynamic> SpecialValues = new();
        
        /// <summary>
        /// 연결된 모디파이어.
        /// </summary>
        public AbilityModifier Modifier;

        /// <summary>
        /// 값을 반환합니다.
        /// </summary>
        public dynamic GetValueFor(string name)
        {
            return SpecialValues.GetValueOrDefault(name);
        }

        /// <summary>
        /// 값을 설정합니다.
        /// </summary>
        public void SetValueFor<T>(string name, T value)
        {
            SpecialValues.Add(name, value);
        }
    }
}