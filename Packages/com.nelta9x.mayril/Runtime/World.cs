using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mayril.Events;
using Unity.Netcode;
using UnityEngine;

namespace Mayril
{
    /// <summary>
    /// 게임의 월드를 표현하는 클래스입니다.
    /// 씬 단위로 고유하며, 모든 엔티티들을 관리하는 컨테이너입니다.
    /// </summary>
    public class World : MonoBehaviour
    {
        [SerializeField] private PlayMode modePrefab;
        [SerializeField] private WorldNetworkMode networkMode = WorldNetworkMode.Standalone;
        
        private GameInstance _owningGameInstance;
        private NetworkManager _networkManager;
        private PlayMode _mode;
        private GameState _gameState;
        private readonly HashSet<Entity> _entities = new(256);
        private readonly Dictionary<int, List<Entity>> _entitiesByLayer = new(8);
        private readonly Dictionary<string, List<Entity>> _entitiesByTag = new(8);
        private readonly List<WorldSystem> _worldSystems = new();
        private readonly Dictionary<Type, WorldSystem> _worldSystemByType = new();
        private readonly TimerManager _timerManager = new();
        
        /// <summary>
        /// 게임 인스턴스.
        /// </summary>
        public GameInstance OwningGameInstance => _owningGameInstance;

        /// <summary>
        /// 월드의 네트워크 모드.
        /// </summary>
        public WorldNetworkMode NetworkMode
        {
            get => networkMode;
            set => networkMode = value;
        }

        /// <summary>
        /// 월드 플레이 모드.
        /// 플레이 모드는 서버에서만 스폰됩니다.
        /// </summary>
        public PlayMode Mode => _mode;

        /// <summary>
        /// 게임 스테이트.
        /// </summary>
        public GameState GameState
        {
            get => _gameState;
            set => _gameState = value;
        }

        /// <summary>
        /// 모든 엔티티들.
        /// </summary>
        public IEnumerable<Entity> Entities => _entities;
        
        /// <summary>
        /// 월드의 타이머 관리자.
        /// </summary>
        public TimerManager WorldTimerManager => _timerManager;
        
        /// <summary>
        /// 월드 시스템들.
        /// </summary>
        public IReadOnlyList<WorldSystem> WorldSystems => _worldSystems;

        /// <summary>
        /// 월드 시스템을 반환합니다.
        /// </summary>
        public WorldSystem GetWorldSystem(Type worldSystemType)
        {
            return _worldSystemByType.GetValueOrDefault(worldSystemType);
        }
        
        /// <summary>
        /// 월드 시스템을 반환합니다.
        /// </summary>
        public WorldSystem GetWorldSystem<T>() where T : WorldSystem
        {
            return GetWorldSystem(typeof(T));
        }

        /// <summary>
        /// 특정 태그의 엔티티들을 반환합니다.
        /// </summary>
        /// <param name="tagToSearch">태그.</param>
        /// <returns>특정 태그의 엔티티들.</returns>
        public IReadOnlyList<Entity> GetEntitiesByTag(string tagToSearch)
        {
            if (!_entitiesByTag.TryGetValue(tagToSearch, out var entities))
            {
                return Array.Empty<Entity>();
            }
            
            return entities;
        }
        
        /// <summary>
        /// 특정 레이어의 엔티티들을 반환합니다.
        /// </summary>
        /// <param name="layerToSearch">레이어.</param>
        /// <returns>특정 레이어의 엔티티들.</returns>
        public IReadOnlyList<Entity> GetEntitiesByLayer(int layerToSearch)
        {
            if (!_entitiesByLayer.TryGetValue(layerToSearch, out var entities))
            {
                return Array.Empty<Entity>();
            }
            
            return entities;
        }
        
        /// <summary>
        /// 엔티티가 깨어났을 때 호출됩니다.
        /// </summary>
        private void OnEntityAwakened(EntityAwakened message)
        {
            message.AwakenedEntity.OwningWorld = this;
        }

        /// <summary>
        /// 엔티티가 시작될 때 호출됩니다.
        /// </summary>
        private void OnEntitySpawned(EntitySpawned message)
        {
            if (_entities.Contains(message.SpawnedEntity))
            {
                return;
            }

            AddEntity(message.SpawnedEntity);
        }
        
        /// <summary>
        /// 엔티티가 파괴될 때 호출됩니다.
        /// </summary>
        private void OnEntityDespawned(EntityDespawned message)
        {
            RemoveEntity(message.DespawnedEntity);
        }
        
        /// <summary>
        /// 엔티티를 추가합니다.
        /// </summary>
        private void AddEntity(Entity entity)
        {
            if (!_entities.Add(entity))
            {
                Debug.LogWarning($"[World] Entity was already added to world. (Entity: {entity.name})");
                return;
            }
            
            entity.OwningWorld = this;
            
            if (!_entitiesByTag.TryGetValue(entity.tag, out var taggedEntities))
            {
                taggedEntities = new List<Entity>();
                _entitiesByTag.Add(entity.tag, taggedEntities);
            }
            
            if (!_entitiesByLayer.TryGetValue(entity.gameObject.layer, out var layeredEntities))
            {
                layeredEntities = new List<Entity>();
                _entitiesByLayer.Add(entity.gameObject.layer, layeredEntities);
            }
            
            taggedEntities.Add(entity);
            layeredEntities.Add(entity);
        }
        
        /// <summary>
        /// 엔티티를 제거합니다.
        /// </summary>
        private void RemoveEntity(Entity entity)
        {
            if (entity == null)
            {
                return;
            }

            if (!_entities.Remove(entity))
            {
                Debug.LogWarning($"[World] Entity was destroyed but not found in world. (Entity: {entity.name})");
                return;
            }
            
            _entitiesByTag[entity.tag].Remove(entity);
            _entitiesByLayer[entity.gameObject.layer].Remove(entity);
        }

        /// <summary>
        /// 이벤트들을 등록합니다.
        /// </summary>
        private void RegisterEvents()
        {
            EventBus<EntityAwakened>.Register(OnEntityAwakened);
            EventBus<EntitySpawned>.Register(OnEntitySpawned);
            EventBus<EntityDespawned>.Register(OnEntityDespawned);
        }
        
        /// <summary>
        /// 이벤트들의 등록을 해제합니다.
        /// </summary>
        private void UnregisterEvents()
        {
            EventBus<EntityAwakened>.Unregister(OnEntityAwakened);
            EventBus<EntitySpawned>.Unregister(OnEntitySpawned);
            EventBus<EntityDespawned>.Unregister(OnEntityDespawned);
        }

        /// <summary>
        /// 초기화 시 호출됩니다.
        /// </summary>
        private void Awake()
        {
            _networkManager = NetworkManager.Singleton;
            if (_networkManager == null)
            {
                Debug.LogError("[World] NetworkManager is not found.");
                return;
            }

            if (_networkManager.IsServer)
            {
                NetworkMode = WorldNetworkMode.Host;
            }
            else if (_networkManager.IsClient)
            {
                NetworkMode = WorldNetworkMode.Client;
            }
            else
            {
                NetworkMode = WorldNetworkMode.Standalone;
            }
            
            // 월드 시스템 생성.
            var worldSystemTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(WorldSystem)));
            foreach (var worldSystemType in worldSystemTypes)
            {
                var worldSystem = (WorldSystem)Activator.CreateInstance(worldSystemType);
                if (!worldSystem.ShouldCreate(this))
                {
                    continue;
                }

                _worldSystems.Add(worldSystem);
                _worldSystemByType.Add(worldSystemType, worldSystem);
            }

            _owningGameInstance = GameInstance.Instance;
            
            RegisterEvents();
            foreach (var entity in FindObjectsByType<Entity>(FindObjectsSortMode.None))
            {
                if (!entity.IsSpawned)
                { // 스폰 시 EntitySpawned 이벤트를 통해 World에 추가될 것이기에 지금 추가하지 않음.
                    continue;
                }

                if (entity.OwningWorld == this)
                { // 이미 월드에 추가되어 있음.
                    continue;
                }

                AddEntity(entity);
            }

            bool shouldSpawnMode = NetworkMode != WorldNetworkMode.Client;
            if (shouldSpawnMode)
            {
                if (modePrefab == null)
                {
                    var newGameObject = new GameObject("PlayMode_AutoCreated");
                    newGameObject.SetActive(false);
                    newGameObject.AddComponent<NetworkObject>();
                    _mode = newGameObject.AddComponent<PlayMode>();
                    _mode.NetworkObject.SpawnWithObservers = false;
                    newGameObject.SetActive(true);
                }
                else
                {
                    _mode = Instantiate(modePrefab);
                }
                
                _mode.OwningWorld = this;
            }
        }
        
        /// <summary>
        /// 시작될 때 호출됩니다.
        /// </summary>
        private void Start()
        {
            Debug.Log($"[World] World started. (World: {name}, NetworkMode: {networkMode})");
            EventBus<WorldStarted>.Trigger(new WorldStarted
            {
                StartedWorld = this
            });
        }

        /// <summary>
        /// 매 프레임 호출됩니다.
        /// </summary>
        public void Update()
        {
            float deltaTime = Time.deltaTime;
            _timerManager.Update(deltaTime);
            foreach (var worldSystem in _worldSystems)
            {
                worldSystem.Update(deltaTime);
            }
        }

        /// <summary>
        /// 파괴될 때 호출됩니다.
        /// </summary>
        public void OnDestroy()
        {
            EventBus<WorldDestroyed>.Trigger(new WorldDestroyed
            {
                DestroyedWorld = this
            });
            UnregisterEvents();
            Debug.Log($"[World] World destroyed. (World: {name}, NetworkMode: {networkMode})");
        }
    }
}