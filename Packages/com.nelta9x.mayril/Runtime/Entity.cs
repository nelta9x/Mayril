using Mayril.Events;
using Unity.Netcode;

namespace Mayril
{
    /// <summary>
    /// 월드 상 단위 오브젝트를 표현하는 클래스.
    /// 모든 레벨에 배치되는 월드 오브젝트들은 이 클래스를 상속받아야 합니다.
    /// </summary>
    public abstract class Entity : NetworkBehaviour
    {
        private World _owningWorld;
        
        /// <summary>
        /// 엔티티가 소속된 월드.
        /// </summary>
        public World OwningWorld
        {
            get => _owningWorld;
            set => _owningWorld = value;
        }

        /// <summary>
        /// 컨트롤러에 의해 조종될 때 호출됩니다.
        /// </summary>
        public virtual void PossessedBy(Controller controller)
        {
        }
        
        /// <summary>
        /// 컨트롤러로부터 조종이 해제될 때 호출됩니다.
        /// </summary>
        public virtual void Unpossessed()
        {
        }

        /// <summary>
        /// 엔티티가 깨어났을 때 호출됩니다.
        /// </summary>
        public virtual void Awake()
        {
            EventBus<EntityAwakened>.Trigger(new EntityAwakened { AwakenedEntity = this });
        }
        
        /// <summary>
        /// 엔티티가 시작될 때 호출됩니다.
        /// </summary>
        public virtual void Start()
        {
        }
        
        /// <summary>
        /// 엔티티가 파괴될 때 호출됩니다.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();
        }
        
        /// <summary>
        /// 네트워크 스폰 시 호출됩니다.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            EventBus<EntitySpawned>.Trigger(new EntitySpawned { SpawnedEntity = this });
        }

        /// <summary>
        /// 네트워크 디스폰 시 호출됩니다.
        /// </summary>
        public override void OnNetworkDespawn()
        {
            EventBus<EntityDespawned>.Trigger(new EntityDespawned { DespawnedEntity = this });
            base.OnNetworkDespawn();
        }
    }
}