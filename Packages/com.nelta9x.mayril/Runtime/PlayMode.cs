using Unity.Netcode;
using UnityEngine;

namespace Mayril
{
    /// <summary>
    /// 게임 플레이 모드.
    /// 호스트에서 월드가 시작되면 스폰되며, 호스트에게만 노출됩니다.
    /// 플레이어가 접속 시 어떤 오브젝트를 가질지, 게임의 컨텐츠는 어떻게 스폰될지 등 플레이 환경 구성을 담당합니다.
    /// </summary>
    public class PlayMode : Entity
    {
        [SerializeField] private GameState gameStatePrefab;
        [SerializeField] private PlayerState playerStatePrefab;

        private NetworkManager _networkManager;
  
        /// <summary>
        /// 모드가 초기화 될 때 호출됩니다.
        /// </summary>
        public override void Awake()
        {
            base.Awake();
            _networkManager = NetworkManager.Singleton;
        }

        /// <summary>
        /// 모드가 스폰될 때 호출됩니다.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            BindNetworkManagerCallback();
        }

        /// <summary>
        /// 모드가 디스폰될 때 호출됩니다.
        /// </summary>
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            UnbindNetworkManagerCallback();
        }

        /// <summary>
        /// 플레이어가 접속을 요청할 때 호출됩니다.
        /// </summary>
        public virtual void OnPlayerEnterRequested(NetworkClient client)
        {
        }

        /// <summary>
        /// 플레이어가 접속했을 때 호출됩니다.
        /// 이 메소드를 오버라이드 시, 반드시 base.OnPlayerEntered() 를 호출해야 합니다.
        /// </summary>
        public virtual void OnPlayerEntered(ulong clientId, NetworkClient client)
        {
            PlayerState newPlayerState;
            if (playerStatePrefab == null)
            {
                var newGameObject = new GameObject("PlayerState_AutoCreated");
                newGameObject.SetActive(false);
                newGameObject.AddComponent<NetworkObject>();
                newPlayerState = newGameObject.AddComponent<PlayerState>();
                newGameObject.SetActive(true);
            }
            else
            {
                newPlayerState = Instantiate(playerStatePrefab);
            }

            newPlayerState.PlayerClientId = clientId;
            newPlayerState.NetworkObject.Spawn(true);
        }

        /// <summary>
        /// 플레이어가 나갔을 때 호출됩니다.
        /// </summary>
        public virtual void OnPlayerLeft(ulong clientId)
        {
        }

        /// <summary>
        /// 네트워크 매니저 콜백을 설정합니다.
        /// </summary>
        private void BindNetworkManagerCallback()
        {
            if (!_networkManager)
            {
                return;
            }

            _networkManager.OnServerStarted += OnServerStarted;
            _networkManager.OnServerStopped += OnServerStopped;
            _networkManager.OnClientConnectedCallback += OnClientConnected;
            _networkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }

        /// <summary>
        /// 네트워크 매니저 콜백을 해제합니다.
        /// </summary>
        private void UnbindNetworkManagerCallback()
        {
            if (!_networkManager)
            {
                return;
            }

            _networkManager.OnServerStarted -= OnServerStarted;
            _networkManager.OnServerStopped -= OnServerStopped;
            _networkManager.OnClientConnectedCallback -= OnClientConnected;
            _networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }
        
        /// <summary>
        /// 게임 스테이트를 스폰합니다.
        /// </summary>
        private void SpawnGameState()
        {
            GameState newGameState;
            if (gameStatePrefab == null)
            {
                var newGameObject = new GameObject("GameState_AutoCreated");
                newGameObject.SetActive(false);
                newGameObject.AddComponent<NetworkObject>();
                newGameState = newGameObject.AddComponent<GameState>();
                newGameObject.SetActive(true);
            }
            else
            {
                newGameState = Instantiate(gameStatePrefab);
            }
            
            OwningWorld.GameState = newGameState;
            newGameState.NetworkObject.Spawn(true);
        }
        
        /// <summary>
        /// 서버가 시작되었을 때 호출됩니다.
        /// </summary>
        private void OnServerStarted()
        {
            Debug.Log("[PlayMode] Server started.");
            SpawnGameState();
        }

        /// <summary>
        /// 서버가 중단되었을 때 호출됩니다.
        /// </summary>
        private void OnServerStopped(bool isGraceful)
        {
            Debug.Log($"[PlayMode] Server stopped. (IsGraceful: {isGraceful})");
        }

        /// <summary>
        /// 클라이언트가 서버에 연결되었을 때 호출
        /// </summary>
        private void OnClientConnected(ulong clientId)
        {
            if (!_networkManager.ConnectedClients.TryGetValue(clientId, out var player))
            {// 클라이언트가 연결되었는데, 클라이언트 인스턴스가 없음. 내부적인 오류.
                Debug.LogError("[GameInstance] Internal error. client not found.");
                return;
            }
            
            Debug.Log($"[PlayMode] Client connected. (ClientId: {clientId})");
            OnPlayerEntered(clientId, player);
        }

        /// <summary>
        /// 클라이언트 연결이 끊겼을 때 호출
        /// </summary>
        private void OnClientDisconnected(ulong clientId)
        {
            // 클라이언트 연결이 끊긴 시점엔 _networkManager.ConnectedClients에 NetworkClient 인스턴스가 없음.
            Debug.Log($"[PlayMode] Client disconnected. (ClientId: {clientId})");
            OnPlayerLeft(clientId);
        }
    }
}