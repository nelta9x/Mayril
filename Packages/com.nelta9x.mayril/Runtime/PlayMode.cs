using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        /// 이 메소드를 오버라이드 시, 반드시 base.OnPlayerEnterRequested() 를 호출해야 합니다.
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
        }

        /// <summary>
        /// 플레이어가 나갔을 때 호출됩니다.
        /// </summary>
        public virtual void OnPlayerLeft(ulong clientId)
        {
        }

        /// <summary>
        /// 모든 플레이어가 씬 로드를 완료했을 때 호출됩니다.
        /// </summary>
        public virtual void OnAllPlayersLoaded()
        {
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
        /// 모드가 초기화 될 때 호출됩니다.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            _networkManager = NetworkManager.Singleton;
            OwningWorld.Mode = this;
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
            _networkManager.SceneManager.OnLoadEventCompleted += OnSceneLoadEventCompleted;
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
            _networkManager.SceneManager.OnLoadEventCompleted -= OnSceneLoadEventCompleted;
        }
        
        /// <summary>
        /// GameState를 스폰합니다.
        /// </summary>
        private void SpawnGameState()
        {
            if (gameStatePrefab == null)
            {
                return;
            }

            var newGameState = Instantiate(gameStatePrefab);
            OwningWorld.GameState = newGameState;
            newGameState.NetworkObject.Spawn(true);
        }
        
        /// <summary>
        /// PlayerState 스폰 가능 여부를 확인하고, 가능하다면 스폰합니다.
        /// </summary>
        private void TrySpawnPlayerStates()
        {
            if (playerStatePrefab == null)
            {
                return;
            }
            
            var world = OwningWorld;
            if (world == null)
            {
                return;
            }

            foreach (var client in _networkManager.ConnectedClientsList)
            {
                SpawnPlayerState(client);
            }
        }

        /// <summary>
        /// 플레이어 스테이트를 스폰합니다.
        /// </summary>
        private void SpawnPlayerState(NetworkClient client)
        {
            var newPlayerState = Instantiate(playerStatePrefab);
            newPlayerState.PlayerClientId = client.ClientId;
            newPlayerState.NetworkObject.Spawn(true);
        }

        // <summary>
        // 모두가 씬 로드를 완료했을 때 호출됩니다.
        // </summary>
        private void OnSceneLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            // 주의:
            // 씬 로드가 완료되었을 때 서버 외에 공유되는 런타임 오브젝트들을 스폰해야 합니다.
            // 이는 아래에 사항에 근거합니다.
            // - 클라이언트들이 씬 이동이 완료되기 전에 스폰할 경우, 이동되기 전 씬에서 Spawn이 발생합니다.
            // - 생성 후 스폰하지 않은 한 상태로 씬 이동이 완료될 경우, SceneObject로 취급됩니다. 이 경우 그런 In Scene 오브젝트에 NetworkObjectHash를 가진 게 없다는 오류가 발생합니다.
            SpawnGameState();
            TrySpawnPlayerStates();
            OnAllPlayersLoaded();
        }
        
        /// <summary>
        /// 서버가 시작되었을 때 호출됩니다.
        /// </summary>
        private void OnServerStarted()
        {
            Debug.Log("[PlayMode] Server started.");
        }

        /// <summary>
        /// 서버가 중단되었을 때 호출됩니다.
        /// </summary>
        private void OnServerStopped(bool isGraceful)
        {
            Debug.Log($"[PlayMode] Server stopped. (IsGraceful: {isGraceful})");
        }

        /// <summary>
        /// 클라이언트가 서버에 연결되었을 때 호출됩니다.
        /// </summary>
        private void OnClientConnected(ulong clientId)
        {
            if (!_networkManager.ConnectedClients.TryGetValue(clientId, out var client))
            {// 클라이언트가 연결되었는데, 클라이언트 인스턴스가 없음. 내부적인 오류.
                Debug.LogError("[GameInstance] Internal error. client not found.");
                return;
            }
            
            Debug.Log($"[PlayMode] Client connected. (ClientId: {clientId})");
            if (playerStatePrefab != null)
            {
                SpawnPlayerState(client);
            }

            OnPlayerEntered(clientId, client);
        }

        /// <summary>
        /// 클라이언트 연결이 끊겼을 때 호출됩니다.
        /// </summary>
        private void OnClientDisconnected(ulong clientId)
        {
            Debug.Log($"[PlayMode] Client disconnected. (ClientId: {clientId})");
            OnPlayerLeft(clientId);
        }
    }
}