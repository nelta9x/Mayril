using System;

namespace Mayril.StatSystem
{
    /// <summary>
    /// 스탯 모디파이어.
    /// 이 클래스는 어떤 엔티티 스탯에 변경을 줄 때 사용됩니다.
    /// </summary>
    public readonly struct StatModifier : IEquatable<StatModifier>
    {
        /// <summary>
        /// 스탯 모디파이어의 타입.
        /// Type에 따라 어떤식으로 영향을 주는지가 다르며,
        /// Additive, Multiplicative, Flat 순서로 연산이 진행됩니다.
        /// </summary>
        public readonly StatModifierType Type;
        
        /// <summary>
        /// 얼마나 값을 변경할지.
        /// </summary>
        public readonly float Value;
  
        public StatModifier(StatModifierType type, float value)
        {
            Type = type;
            Value = value;
        }
        
        /// <summary>
        /// 문자열로 반환합니다.
        /// </summary>
        public override string ToString()
        {
            return $"Type: {Type}, Value: {Value})";
        }

        public bool Equals(StatModifier other)
        {
            return Type == other.Type && Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is StatModifier other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)Type, Value);
        }
    }
}