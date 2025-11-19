namespace Mayril.StatSystem
{
    /// <summary>
    /// 스탯을 제공하는 인터페이스.
    /// </summary>
    public interface IStatProvider
    {
        /// <summary>
        /// 스탯 값을 반환합니다.
        /// </summary>
        /// <param name="statName">스탯 이름.</param>
        public StatValue GetStatValue(string statName);
    }
}