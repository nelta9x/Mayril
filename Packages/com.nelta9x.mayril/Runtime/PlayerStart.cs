using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mayril
{
    /// <summary>
    /// 플레이어 시작 위치들을 관리하는 클래스.
    /// </summary>
    public class PlayerStart : MonoBehaviour
    {
        public const string PlayerStartTag = "PlayerStart";
        private GameObject[] _starts;
        private static PlayerStart _instance;
        
        /// <summary>
        /// 플레이어 스타트 인스턴스를 반환합니다.
        /// </summary>
        public static PlayerStart Instance
        {
            get
            {
                if (_instance == null)
                {
                    var gameObject = new GameObject(nameof(PlayerStart));
                    _instance = gameObject.AddComponent<PlayerStart>();
                    DontDestroyOnLoad(gameObject);
                }

                return _instance;
            }
        }
        
        /// <summary>
        /// 플레이어 시작 위치들.
        /// </summary>
        public GameObject[] Starts => _starts;

        /// <summary>
        /// 컴포넌트가 초기화 될 때 호출됩니다.
        /// </summary>
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _starts = GameObject.FindGameObjectsWithTag(PlayerStartTag);
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        /// <summary>
        /// 파괴될 때 호출됩니다.
        /// </summary>
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        /// <summary>
        /// 씬이 로드될 때 호출됩니다.
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _starts = GameObject.FindGameObjectsWithTag(PlayerStartTag);
        }
        
        /// <summary>
        /// 씬이 언로드 될 때 호출됩니다.
        /// </summary>
        private void OnSceneUnloaded(Scene scene)
        {
            _starts = Array.Empty<GameObject>();
        }
    }
}