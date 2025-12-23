using System.Collections.Generic;

namespace Mayril
{
    /// <summary>
    /// 모든 클라이언트가 알아야 하는 게임 상태를 공유하는 역할을 담당합니다.
    /// 예) PvP 게임에서 각 팀의 스코어, 협동 게임에서의 현재 라운드나 점수.
    /// </summary>
    public class GameState : Entity
    {
        private readonly List<PlayerState> _playerStates = new();
        
        /// <summary>
        /// 플레이어 스테이트들을 반환합니다.
        /// </summary>
        public IReadOnlyList<PlayerState> PlayerStates => _playerStates;

        /// <summary>
        /// 플레이어 스테이트를 반환합니다.
        /// </summary>
        public PlayerState GetPlayerState(ulong clientId)
        {
            return _playerStates.Find(x => x.PlayerClientId == clientId);
        }
        
        /// <summary>
        /// 플레이어 스테이트를 추가합니다.
        /// 이 메소드는 PlayerState 스폰 시, PlayerState가 호출합니다.
        /// </summary>
        public void AddPlayerState(PlayerState newPlayerState)
        {
            _playerStates.Add(newPlayerState);
        }
        
        /// <summary>
        /// 플레이어 스테이트를 제거합니다.
        /// 이 메소드는 PlayerState 디스폰 시, PlayerState가 호출합니다.
        /// </summary>
        public void RemovePlayerState(PlayerState playerState)
        {
            _playerStates.Remove(playerState);
        }
        
        /// <summary>
        /// 네트워크 스폰 시 호출됩니다.
        /// 이 메소드를 재정의 시, 반드시 base.OnNetworkSpawn()를 호출해야 합니다.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            OwningWorld.GameState = this;
        }

        /// <summary>
        /// 네트워크 디스폰 시 호출됩니다.
        /// 이 메소드를 재정의 시, 반드시 base.OnNetworkDespawn()를 호출해야 합니다.
        /// </summary>
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            OwningWorld.GameState = null;
        }

        /// <summary>
        /// 플레이 가능해졌을 때 호출됩니다.
        /// </summary>
        protected override void BeginPlay()
        {
        }

        /// <summary>
        /// 플레이가 종료되었을 때 호출됩니다.
        /// </summary>
        protected override void EndPlay()
        {
        }
        
        /// <summary>
        /// (클라이언트 전용) 클라이언트에서 네트워크 세션 동기화가 완료되었을 때 호출됩니다.
        /// 이 메소드를 재정의 시, 반드시 base.OnNetworkSessionSynchronized()를 호출해야 합니다.
        /// </summary>
        protected override void OnNetworkSessionSynchronized()
        {
            OwningWorld.IsNetworkSessionSynchronized = true;
            base.OnNetworkSessionSynchronized();
        }
    }
}