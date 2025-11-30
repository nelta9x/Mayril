using Unity.Netcode;
using UnityEngine;

namespace Mayril
{
    /// <summary>
    /// 유닛을 컨트롤하는 기능을 제공합니다.
    /// </summary>
    public abstract class Controller : Entity
    {
        private Entity _possessedTarget;
        
        /// <summary>
        /// 컨트롤 중인 엔티티.
        /// </summary>
        public Entity PossessedTarget => _possessedTarget;
        
        /// <summary>
        /// 컨트롤러에 의해 조종될 때 호출됩니다.
        /// </summary>
        public override void OnPossessedBy(Controller controller)
        {
        }
        
        /// <summary>
        /// 컨트롤러로부터 조종이 해제될 때 호출됩니다.
        /// </summary>
        public override void Unpossessed()
        {
        }
        
        /// <summary>
        /// 컨트롤 할 엔티티를 설정합니다.
        /// </summary>
        public void Possess(Entity targetRef)
        {
            if (!IsServer)
            {
                return;
            }

            PossessRpc(targetRef);
        }

        /// <summary>
        /// 컨트롤할 엔티티가 설정되었을 때 호출됩니다.
        /// 만약 파생 컨트롤러에서 컨트롤 할 엔티티를 거부할 경우, base 함수 호출을 하지 마세요.
        /// </summary>
        protected virtual void OnPossess(Entity target)
        {
            target.OnPossessedBy(this);
            _possessedTarget = target;
        }

        /// <summary>
        /// 컨트롤 중인 엔티티를 해제합니다.
        /// </summary>
        public void Unpossess()
        {
            if (!IsServer)
            {
                return;
            }
            
            if (_possessedTarget == null)
            {
                return;
            }

            UnpossessRpc();
        }

        /// <summary>
        /// 컨트롤 중인 엔티티가 성공적으로 해제되었을 때 호출됩니다.
        /// </summary>
        protected virtual void OnUnpossess(Entity unpossessedTarget)
        {
        }
        
        /// <summary>
        /// (Rpc) 컨트롤할 엔티티를 설정합니다.
        /// </summary>
        [Rpc(SendTo.Everyone)]
        private void PossessRpc(NetworkBehaviourReference targetRef)
        {
            if (!targetRef.TryGet(out Entity targetEntity))
            {
                Debug.LogWarning("[Controller] Target is not Entity.");
                return;
            }

            if (_possessedTarget != null)
            {
                Debug.LogWarning("[Controller] Controller already has an entity. Unpossess first.");
                return;
            }

            OnPossess(targetEntity);
        }
        
        /// <summary>
        /// 컨트롤 중인 엔티티를 해제합니다.
        /// </summary>
        [Rpc(SendTo.Everyone)]
        private void UnpossessRpc()
        {
            if (_possessedTarget == null)
            {
                return;
            }

            var oldTarget = _possessedTarget;
            _possessedTarget = null;
            oldTarget.Unpossessed();
            OnUnpossess(oldTarget);
        }
    }
}