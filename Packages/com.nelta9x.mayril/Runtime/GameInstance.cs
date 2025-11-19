using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mayril
{
    /// <summary>
    /// 게임 인스턴스를 표현하는 클래스입니다.
    /// 단일 게임 인스턴스 당 단 하나만 존재합니다.
    /// </summary>
    public class GameInstance : MonoBehaviour
    {
        private static GameInstance _instance;
        private NetworkManager _networkManager;
        private UnityTransport _transport;

        /// <summary>
        /// 인스턴스 싱글톤.
        /// </summary>
        public static GameInstance Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = CreateGameInstance();
                    DontDestroyOnLoad(_instance.gameObject);
                }

                return _instance;
            }
        }
        
        /// <summary>
        /// 네트워크 매니저 싱글톤.
        /// </summary>
        public NetworkManager NetworkManager => _networkManager;

        /// <summary>
        /// 세션을 생성합니다.
        /// </summary>
        public void CreateSession(SessionSettings settings)
        {
            if (_networkManager.ShutdownInProgress)
            {
                Debug.Log("[GameInstance] Shutdown in progress.");
                return;
            }

            StartCoroutine(CreateSessionCoroutine(settings));
        }

        /// <summary>
        /// 세션에 참여합니다.
        /// </summary>
        public void JoinSession(string address, ushort port)
        {
            if (_networkManager.ShutdownInProgress)
            {
                Debug.Log("[GameInstance] Shutdown in progress.");
                return;
            }
            
            StartCoroutine(JoinSessionCoroutine(address, port));
        }

        /// <summary>
        /// 게임 인스턴스 생성 직후 호출됩니다.
        /// </summary>
        public virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }
        
        /// <summary>
        /// 게임 인스턴스가 시작될 때 호출됩니다.
        /// </summary>
        public virtual void Start()
        {
            _networkManager = NetworkManager.Singleton;
            if (_networkManager == null)
            {
                // NetworkManager가 없다면 추가.
                Debug.Log("[GameInstance] NetworkManager is not found. Failed to create GameInstance.");
                Destroy(gameObject);
                return;
            }
            
            _transport = _networkManager.GetComponent<UnityTransport>();
            if (!_transport)
            {
                Debug.LogError("[GameInstance] UnityTransport is not initialized.");
            }
            
            bool hasNetworkMode = _networkManager.IsHost || _networkManager.IsClient;
            if (!hasNetworkMode)
            {
                var world = GetOrCreateWorld();
                bool isSuccess = _networkManager.StartHost();
                if (!isSuccess)
                {
                    Debug.LogError("[GameInstance] Failed to start host.");
                }

                world.NetworkMode = WorldNetworkMode.Standalone;
            }
        }
        
        /// <summary>
        /// 게임 인스턴스가 파괴될 때 호출됩니다.
        /// </summary>
        public virtual void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        /// <summary>
        /// 세션을 생성하는 코루틴.
        /// </summary>
        private IEnumerator CreateSessionCoroutine(SessionSettings settings)
        {
            if (_networkManager.IsListening)
            {
                _networkManager.Shutdown();
                yield return new WaitUntil(() => !_networkManager.ShutdownInProgress);
            }
            
            // UnityTransport의 포트 설정
            _transport.ConnectionData.Port = settings.Port;

            // 서버 콜백 등록
            if (!_networkManager.StartHost())
            {
                Debug.LogError("[GameInstance] Failed to start host.");
                yield break;
            }
            
            Debug.Log($"[GameInstance] Host started. (Endpoint: {_transport.ConnectionData.Address}:{_transport.ConnectionData.Port})");
            if (settings.ShouldChangeScene)
            {
                if (_networkManager.NetworkConfig.EnableSceneManagement)
                {
                    if (_networkManager.IsServer)
                    {
                        _networkManager.SceneManager.LoadScene(settings.SceneName, LoadSceneMode.Single);
                    }
                }
                else
                {
                    // 수동 씬 로드 (NetworkManager의 씬 관리 비활성화된 경우)
                    SceneManager.LoadScene(settings.SceneName);
                }
            }
        }
        
        /// <summary>
        /// 세션을 생성하는 코루틴.
        /// </summary>
        private IEnumerator JoinSessionCoroutine(string address, ushort port)
        {
            if (_networkManager.IsListening)
            {
                _networkManager.Shutdown();
                yield return new WaitUntil(() => !_networkManager.ShutdownInProgress);
            }
            
            // 접속할 IP 주소와 포트 설정
            _transport.ConnectionData.Address = address;
            _transport.ConnectionData.Port = port;
            if (!_networkManager.StartClient())
            {
                Debug.LogError($"[GameInstance] Failed to start client. (Endpoint: {address}:{port})");
                yield break;
            }

            Debug.Log($"[GameInstance] Client started. (Endpoint: {address}:{port})");
        }
        
        /// <summary>
        /// 씬이 로드되었을 때 호출됩니다.
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"[GameInstance] Scene loaded. (Scene: {scene.name})");
            
            var world = GetOrCreateWorld();
            if (_networkManager == null)
            {
                world.NetworkMode = WorldNetworkMode.Standalone;
            }
            else
            {
                if (_networkManager.IsServer)
                {
                    world.NetworkMode = WorldNetworkMode.Host;
                }
                else if (_networkManager.IsClient)
                {
                    world.NetworkMode = WorldNetworkMode.Client;
                }
            }
        }

        /// <summary>
        /// 씬이 언로드되었을 때 호출됩니다.
        /// </summary>
        private void OnSceneUnloaded(Scene scene)
        {
            Debug.Log($"[GameInstance] Scene unloaded. (Scene: {scene.name})");
        }

        /// <summary>
        /// 월드를 씬에서 가져오거나, 필요 시 생성합니다.
        /// </summary>
        private World GetOrCreateWorld()
        {
            var world = FindFirstObjectByType<World>();
            if (!world)
            {
                var worldObject = new GameObject("World_AutoCreated");
                world = worldObject.AddComponent<World>();
            }

            return world;
        }

        /// <summary>
        /// 게임 인스턴스를 생성합니다.
        /// </summary>
        private static GameInstance CreateGameInstance()
        {
            var newGameObject = new GameObject("GameInstance_AutoCreated");
            return newGameObject.AddComponent<GameInstance>();
        }
    }
}
