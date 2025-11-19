using System.Collections.Generic;
using System.Linq;

namespace Mayril.StatSystem
{
    /// <summary>
    /// 스탯 모디파이어 컨테이너.
    /// 모든 종류의 모디파이어들을 관리합니다.
    /// </summary>
    public class StatModifierContainer
    {
        public readonly List<StatModifier> Additive = new();
        public readonly List<StatModifier> Multiplicative = new();
        public readonly List<StatModifier> Fixed = new();

        /// <summary>
        /// 기반 값에 모디파이어를 적용했을 때의 값을 계산합니다.
        /// 계산 순서: (Base + Additive) * (1 + Multiplicative) + Fixed
        /// </summary>
        public float Apply(float baseValue)
        {
            float resultValue = baseValue;
            resultValue += Additive.Sum(x => x.Value);
            resultValue *= (1f + Multiplicative.Sum(x => x.Value));
            resultValue += Fixed.Sum(x => x.Value);
            return resultValue;
        }

        /// <summary>
        /// 모디파이어를 추가합니다.
        /// </summary>
        public void Add(StatModifier modifier)
        {
            switch (modifier.Type)
            {
                case StatModifierType.Additive:
                    Additive.Add(modifier);
                    break;
                case StatModifierType.Multiplicative:
                    Multiplicative.Add(modifier);
                    break;
                case StatModifierType.Fixed:
                    Fixed.Add(modifier);
                    break;
            }
        }

        /// <summary>
        /// 모디파이어를 제거합니다.
        /// </summary>
        public bool Remove(StatModifier modifier)
        {
            return modifier.Type switch
            {
                StatModifierType.Additive => Additive.Remove(modifier),
                StatModifierType.Multiplicative => Multiplicative.Remove(modifier),
                StatModifierType.Fixed => Fixed.Remove(modifier),
                _ => false
            };
        }

        /// <summary>
        /// 모디파이어들을 제거합니다.
        /// </summary>
        public void Clear()
        {
            Additive.Clear();
            Multiplicative.Clear();
            Fixed.Clear();
        }
    }
}