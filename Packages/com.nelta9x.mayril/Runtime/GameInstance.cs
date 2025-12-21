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
        }

        /// <summary>
        /// 게임 인스턴스가 활성화 될 때 호출됩니다.
        /// </summary>
        public virtual void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        /// <summary>
        /// 게임 인스턴스가 비활성화 될 때 호출됩니다.
        /// </summary>
        public virtual void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        /// <summary>
        /// 게임 인스턴스가 시작될 때 호출됩니다.
        /// </summary>
        public virtual void Start()
        {
        }
        
        /// <summary>
        /// 게임 인스턴스가 파괴될 때 호출됩니다.
        /// </summary>
        public virtual void OnDestroy()
        {
        }

        /// <summary>
        /// 씬이 로드되었을 때 호출됩니다.
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"[GameInstance] Scene loaded. (Scene: {scene.name})");
        }

        /// <summary>
        /// 씬이 언로드되었을 때 호출됩니다.
        /// </summary>
        private void OnSceneUnloaded(Scene scene)
        {
            Debug.Log($"[GameInstance] Scene unloaded. (Scene: {scene.name})");
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
