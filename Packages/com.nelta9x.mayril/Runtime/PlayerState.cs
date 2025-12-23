using Unity.Netcode;
using UnityEngine;

namespace Mayril
{
    /// <summary>
    /// 플레이어 상태 동기화를 담당합니다.
    /// 예) 플레이어의 킬 스코어, 보유 골드 등.
    /// </summary>
    public class PlayerState : Entity
    {
        private readonly NetworkVariable<ulong> _playerClientId = new();

        /// <summary>
        /// 플레이어의 클라이언트 Id.
        /// 0번은 또는 서버를 가리키며 그 외에는 서버에 입장한 플레이어 순서대로 인덱스를 부여받습니다.
        /// </summary>
        public ulong PlayerClientId
        {
            get => _playerClientId.Value;
            set
            {
                if (!IsServer)
                {
                    return;
                }
                
                if (IsSpawned)
                {
                    _playerClientId.Value = value;
                }
                else
                {
                    _playerClientId.Reset(value);
                }
            }
        }

        /// <summary>
        /// 플레이가 가능해졌을 때 호출됩니다.
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
        /// 네트워크 스폰 시 호출됩니다.
        /// 이 메소드를 오버라이드 시, 반드시 base.OnNetworkSpawn()를 호출해야 합니다.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            // 서버는 GameState를 우선 생성하는 걸 보장하므로 NetworkSpawn 시 PlayerState를 GameState에 추가합니다.
            // 그러나 클라이언트가 Late Join으로 접속 시, GameState와 PlayerState 는 스폰 순서가 바뀔 수 있습니다.
            // 이 경우를 대비하여 클라이언트의 OnNetworkSessionSynchronized() 에서 GameState에 PlayerState를 추가합니다.
            var gameState = OwningWorld.GameState;
            if (gameState == null)
            {
                return;
            }
            
            gameState.AddPlayerState(this);  
        }

        /// <summary>
        /// 네트워크 디스폰 시 호출됩니다.
        /// 이 메소드를 오버라이드 시, 반드시 base.OnNetworkDespawn()를 호출해야 합니다.
        /// </summary>
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            var gameState = OwningWorld.GameState;
            if (gameState == null)
            {
                return;
            }
            
            gameState.RemovePlayerState(this);
        }
        
        /// <summary>
        /// (클라이언트 전용) 클라이언트에서 서버에 접속한 후, 모든 네트워크 오브젝트들이 동기화 되었을 때 호출됩니다.
        /// 이 메소드를 오버라이드 시, 반드시 base.OnNetworkSessionSynchronized()를 호출해야 합니다.
        /// </summary>
        protected override void OnNetworkSessionSynchronized()
        {
            base.OnNetworkSessionSynchronized();
            var gameState = OwningWorld.GameState;
            if (gameState == null)
            {
                Debug.LogError("[PlayerState] GameState is null.");
                return;
            }
            
            // GameState가 PlayerState보다 늦게 스폰되었을 수도 있으므로 OnNetworkSessionSynchronized에서 설정.
            var existingPlayerState = gameState.GetPlayerState(_playerClientId.Value);
            if (existingPlayerState == null)
            {
                gameState.AddPlayerState(this);
            }
        }
    }
}