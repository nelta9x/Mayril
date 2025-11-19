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
        /// 네트워크 매니저 콜백을 설정합니다.
        /// </summary>
        private void BindNetworkManagerCallback()
        {
            if (!_networkManager)
            {
                return;
            }
            
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

            _networkManager.OnClientConnectedCallback -= OnClientConnected;
            _networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
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