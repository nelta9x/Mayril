using Mayril.Events;
using Unity.Netcode;
using UnityEngine;

namespace Mayril
{
    /// <summary>
    /// 월드 상 단위 오브젝트를 표현하는 클래스.
    /// 모든 레벨에 배치되는 월드 오브젝트들은 이 클래스를 상속받아야 합니다.
    /// </summary>
    [DefaultExecutionOrder(1)]
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
            if (_hasBegunPlay)
            {// 이미 플레이 됨.
                return;
            }

            if (!_owningWorld.didStart)
            {
                // 아직 월드가 시작되지 않음.
                // 이 경우 WorldStarted 메시지를 받았을 때, BeginPlay를 실행합니다.
                return;
            }
            
            // 클라이언트에서 엔티티가 스폰 되었더라도, 모든 동기화가 완료되기 전까지는 BeginPlay를 호출하지 않고 미룹니다.
            if (IsServer || _owningWorld.IsNetworkSessionSynchronized)
            {
                InternalBeginPlay();
            }
        }

        /// <summary>
        /// 네트워크 디스폰 시 호출됩니다.
        /// 이 메소드를 재정의 시, 반드시 base.OnNetworkDespawn()를 호출해야 합니다.
        /// </summary>
        public override void OnNetworkDespawn()
        {
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
            // 엔티티는 비활성화 되어있다가, 플레이 가능해 질 때 Active 여부를 활성화합니다.
            // 주의: 이 시점에 다른 컴포넌트의 OnDisable이 호출됨
            // 예)
            // - 서버: 네트워크 오브젝트 스폰 완료,
            // - 클라이언트: 세션 동기화 완료 시
            _owningWorld = World.Instance;
            enabled = false; // BeginPlay 호출 전까진 Tick이 돌지 못하도록 보장.
            if (!_owningWorld.didStart)
            {
                EventBus<WorldStarted>.Register(OnWorldStarted);   
            }
        }

        /// <summary>
        /// 엔티티 파괴 시 호출됩니다.
        /// </summary>
        public override void OnDestroy()
        {
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
            if (IsClient && !_hasBegunPlay && _owningWorld.didStart)
            {
                InternalBeginPlay();
            }
        }

        /// <summary>
        /// (내부 전용) 엔티티가 플레이 가능해졌을 때 호출됩니다.
        /// </summary>
        private void InternalBeginPlay()
        {
            _owningWorld.AddEntity(this);
            BeginPlay();
            _hasBegunPlay = true;
            enabled = true;
            EventBus<EntityPlayStarted>.Trigger(new EntityPlayStarted()
            {
                StartedEntity = this
            });
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
            // 월드 시작 이전에 스폰되고, 씬 동기화도 완료된 오브젝트의 경우, 월드 시작 시점에 BeginPlay를 실행합니다.
            if (IsSpawned && !_hasBegunPlay)
            {
                // 서버는 처음부터 동기화가 완료된 상태이므로 _owningWorld.IsNetworkSessionSynchronized 여부에 관계 없음.
                if (IsServer || _owningWorld.IsNetworkSessionSynchronized)
                {
                    InternalBeginPlay();
                }
            }
            
            EventBus<WorldStarted>.Unregister(OnWorldStarted);
        }

        /// <summary>
        /// 엔티티의 첫 Update 직전에 호출됩니다.
        /// NetworkObject의 라이프 사이클과는 맞지 않으므로, 잘못된 사용을 방지하기 위해 비활성화 합니다.
        /// 대신 <see cref="BeginPlay"/> 사용하세요.
        /// </summary>
        private void Start()
        {
        }
    }
}