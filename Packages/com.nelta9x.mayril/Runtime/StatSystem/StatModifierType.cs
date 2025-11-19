namespace Mayril.StatSystem
{
    /// <summary>
    /// 스탯 모디파이어의 타입을 정의합니다.
    /// </summary>
    public enum StatModifierType
    {
        /// <summary>
        /// 스탯 기반 값에 값에 더해지는 값입니다.
        /// Base + Additive
        /// </summary>
        Additive = 0,
        
        /// <summary>
        /// 값에 곱해지는 값입니다.
        /// <see cref="Additive"/> 이후 계산됩니다.
        /// (Base + Additive) *  Multiplicative
        /// </summary>
        Multiplicative = 1,
        
        /// <summary>
        /// 값에 더해지는 값입니다.
        /// <see cref="Multiplicative"/> 이후 계산됩니다.
        /// ((Base + Additive) *  Multiplicative) + Fixed
        /// </summary>
        Fixed = 2,
    }
}