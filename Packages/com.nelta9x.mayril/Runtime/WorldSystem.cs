namespace Mayril
{
    public abstract class WorldSystem
    {
        /// <summary>
        /// 생성 여부.
        /// 조건에 따라 생성되는 시스템의 경우, 이 메소드를 재정의합니다.
        /// </summary>
        public virtual bool ShouldCreate(World world)
        {
            return true;
        }

        /// <summary>
        /// 매 프레임 호출됩니다.
        /// </summary>
        public virtual void Update(float deltaTime)
        {
        }
    }
}