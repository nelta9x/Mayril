using Mayril.Events;
using Unity.Netcode;
using UnityEngine;

namespace Mayril
{
    /// <summary>
    /// 월드 상 단위 오브젝트를 표현하는 클래스.
    /// 모든 레벨에 배치되는 월드 오브젝트들은 이 클래스를 상속받아야 합니다.
    /// </summary>
    public abstract class Entity : NetworkBehaviour
    {
        private World _owningWorld;
        private bool _hasBegunPlay;
        
        /// <summary>
        /// 엔티티가 소속된 월드.
        /// </summary>
        public World OwningWorld => _owningWorld;

        /// <summary>
        /// 엔티티가 플레이 가능해졌는지 여부.
        /// </summary>
        public bool HasBegunPlay => _hasBegunPlay;

        /// <summary>
        /// 컨트롤러에 의해 조종될 때 호출됩니다.
        /// </summary>
        public virtual void OnPossessedBy(Controller controller)
        {
        }
        
        /// <summary>
        /// 컨트롤러로부터 조종이 해제될 때 호출됩니다.
        /// </summary>
        public virtual void Unpossessed()
        {
        }

        /// <summary>
        /// 엔티티가 플레이 가능해졌을 때 호출됩니다.
        /// 이 메소드가 호출된 이후 Update, FixedUpdate 들이 호출됩니다.
        /// </summary>
        protected abstract void BeginPlay();

        /// <summary>
        /// 엔티티의 플레이가 종료되었을 때 호출됩니다.
        /// </summary>
        protected abstract void EndPlay();

        /// <summary>
        /// 네트워크 스폰 시 호출됩니다.
        /// 이 메소드를 재정의 시, 반드시 base.OnNetworkSpawn()를 호출해야 합니다.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            TryBeginPlay();
        }

        /// <summary>
        /// 네트워크 디스폰 시 호출됩니다.
        /// 이 메소드를 재정의 시, 반드시 base.OnNetworkDespawn()를 호출해야 합니다.
        /// </summary>
        public override void OnNetworkDespawn()
        {
            EventBus<WorldStarted>.Unregister(OnWorldStarted);
            if (_hasBegunPlay)
            {
                InternalEndPlay();
            }

            base.OnNetworkDespawn();
            if (NetworkObject.IsSceneObject is true)
            {// 씬 오브젝트는 Destroy 하면 안 되기 때문에, Active 상태만 false로 변경.
                gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 엔티티가 깨어났을 때 호출됩니다.
        /// 이 메소드를 재정의 시, 반드시 base.Awake()를 호출해야 합니다.
        /// </summary>
        protected virtual void Awake()
        {
            _owningWorld = World.Instance;
        }

        /// <summary>
        /// 엔티티 파괴 시 호출됩니다.
        /// </summary>
        public override void OnDestroy()
        {
            // OnNetworkDespawn을 거치지 않고 파괴되는 예외 상황 방어
            if (_hasBegunPlay)
            {
                InternalEndPlay();
            }
            
            EventBus<WorldStarted>.Unregister(OnWorldStarted);
            base.OnDestroy();
        }

        /// <summary>
        /// (클라이언트 전용) 클라이언트에서 네트워크 세션 동기화가 완료되었을 때 호출됩니다.
        /// 이 메소드를 재정의 시, 반드시 base.OnNetworkSessionSynchronized()를 호출해야 합니다.
        /// </summary>
        protected override void OnNetworkSessionSynchronized()
        {
            base.OnNetworkSessionSynchronized();
            TryBeginPlay();
        }

        /// <summary>
        /// (내부 전용) 엔티티의 플레이가 종료되었을 때 호출됩니다.
        /// </summary>
        private void InternalEndPlay()
        {
            EndPlay();
            _hasBegunPlay = false;
            enabled = false;
            _owningWorld.RemoveEntity(this);
            EventBus<EntityPlayEnded>.Trigger(new EntityPlayEnded()
            {
                EndedEntity = this
            });
        }

        /// <summary>
        /// 월드가 시작될 때 호출됩니다.
        /// </summary>
        private void OnWorldStarted(WorldStarted message)
        {
            TryBeginPlay();
        }
        
        /// <summary>
        /// BeginPlay가 호출 가능한지 확인하고, 호출 가능한 조건이라면 BeginPlay를 실행합니다.
        /// </summary>
        private void TryBeginPlay()
        {
            if (_hasBegunPlay)
            {// 이미 BeginPlay 호출 됨.
                return;
            }
            
            if (!didStart)
            {// Start가 실행되지 않음. Start 시점으로 BeginPlay를 미룹니다.
                return;
            }

            if (_owningWorld == null)
            {// 씬 오브젝트의 경우, Awake보다 OnNetworkSpawn이 먼저 호출 되므로 World 인스턴스를 여기서도 설정.
                _owningWorld = World.Instance;
                if (_owningWorld == null)
                {
                    Debug.LogError($"[Entity] OwningWorld is not found. (name: {name})");
                    return;
                }
            }
            
            if (!_owningWorld.didStart)
            {// 월드가 시작되지 않음. 월드 시작 시점으로 BeginPlay를 미룹니다.
                enabled = false;
                EventBus<WorldStarted>.Unregister(OnWorldStarted);
                EventBus<WorldStarted>.Register(OnWorldStarted);
                return;
            }

            if (!IsSpawned)
            {// 스폰되지 않음. 스폰 시점으로 BeginPlay를 미룹니다.
                enabled = false;
                return;
            }

            if (IsServer)
            {
                InternalBeginPlay();
            }
            else if (IsClient)
            {
                if (!_owningWorld.IsNetworkSessionSynchronized)
                {// 아직 세션 동기화가 되지 않음. 세션 동기화 완료 시점으로 BeginPlay를 미룹니다.
                    enabled = false;
                    return;
                }

                InternalBeginPlay();
            }
        }
        
        /// <summary>
        /// 엔티티가 플레이 가능해졌을 때 호출됩니다.
        /// </summary>
        private void InternalBeginPlay()
        {
            EventBus<WorldStarted>.Unregister(OnWorldStarted);
            _hasBegunPlay = true;
            _owningWorld.AddEntity(this);
            BeginPlay();
            enabled = true;
            EventBus<EntityPlayStarted>.Trigger(new EntityPlayStarted()
            {
                StartedEntity = this
            });
        }

        /// <summary>
        /// 엔티티의 첫 Update 직전에 호출됩니다.
        /// NetworkObject의 라이프 사이클과는 맞지 않으므로, 잘못된 사용을 방지하기 위해 비활성화 합니다.
        /// 대신 <see cref="BeginPlay"/> 사용하세요.
        /// </summary>
        private void Start()
        {
            TryBeginPlay();
        }
    }
}