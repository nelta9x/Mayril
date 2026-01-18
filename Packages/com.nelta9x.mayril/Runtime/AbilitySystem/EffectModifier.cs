using Mayril.TagSystem;
using Mayril.StatSystem;

namespace Mayril.AbilitySystem
{
    [System.Serializable]
    public struct EffectModifier
    {
        /// <summary>
        /// 변경할 스탯(Attribute) 태그.
        /// </summary>
        public GameTag StatTag;

        /// <summary>
        /// 변경 방식 (Add, Multiply, Override).
        /// </summary>
        public StatModifierType ModifierType;

        /// <summary>
        /// 변경 값.
        /// </summary>
        public float ModifierValue;

        /// <summary>
        /// StatModifier로 변환합니다.
        /// </summary>
        public StatModifier ToStatModifier()
        {
            return new StatModifier(ModifierType, ModifierValue);
        }
    }
}
