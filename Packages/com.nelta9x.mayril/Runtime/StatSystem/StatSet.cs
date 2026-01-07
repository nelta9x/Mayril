using Unity.Netcode;

namespace Mayril.StatSystem
{
    /// <summary>
    /// 스탯들을 표현하는 클래스입니다.
    /// </summary>
    /// <code>
    /// public class CharacterStats : StatSet
    /// {
    ///     public readonly StatValue Strength = new();
    ///     // ...
    /// }
    /// </code>
    public abstract class StatSet : NetworkBehaviour
    {
        /// <summary>
        /// 값이 변경되기 전에 호출됩니다.
        /// 이 시점에서 변경될 값을 변경할 수 있습니다. (예: 최종 값이 0 미만이면 0으로 보정)
        /// </summary>
        public abstract void OnStatValueChanging(StatValue stat, ref float newCurrentValue);

        /// <summary>
        /// 값이 변경된 후에 호출됩니다.
        /// </summary>
        /// <code>
        /// public override OnStatValueChanged(StatValue stat)
        /// {
        ///     if (stat == Strength)
        ///     {// 힘이 변경 됨.
        ///         // ...
        ///     }
        /// }
        /// </code>
        public abstract void OnStatValueChanged(StatValue stat);
    }
}