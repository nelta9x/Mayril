namespace Mayril
{
    /// <summary>
    /// 서버 설정을 지정하는 구조체.
    /// </summary>
    public struct SessionSettings
    {
        /// <summary>
        /// 포트.
        /// </summary>
        public ushort Port;

        /// <summary>
        /// 최대 플레이어 수. (본인 포함)
        /// </summary>
        public int MaxPlayerCount;
        
        /// <summary>
        /// 씬 변경 여부.
        /// </summary>
        public bool ShouldChangeScene;
        
        /// <summary>
        /// 씬 이름.
        /// 세션 생성 시, 이 씬으로 전환됩니다.
        /// </summary>
        public string SceneName;
    }
}