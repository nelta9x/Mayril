using Mayril.TagSystem;
using Unity.Netcode;

namespace Mayril.StatSystem
{
    /// <summary>
    /// 스탯들을 표현하는 인터페이스입니다.
    /// </summary>
    public interface IStatSet
    {
        /// <summary>
        /// 값이 변경되기 전에 호출됩니다.
        /// 이 시점에서 변경될 값을 변경할 수 있습니다. (예: 최종 값이 0 미만이면 0으로 보정)
        /// </summary>
        void OnStatValueChanging(StatValue stat, ref float newCurrentValue);

        /// <summary>
        /// 값이 변경된 후에 호출됩니다.
        /// </summary>
        void OnStatValueChanged(StatValue stat);

        /// <summary>
        /// 태그로 스탯 값을 가져옵니다.
        /// </summary>
        StatValue GetStatValue(GameTag tag);
    }
}