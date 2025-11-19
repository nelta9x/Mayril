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
        public override void PossessedBy(Controller controller)
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
        public virtual void Possess(Entity target)
        {
            if (target == null)
            {
                Debug.LogWarning("[Controller] Target is null.");
                return;
            }

            if (_possessedTarget != null)
            {
                Debug.LogWarning("[Controller] Controller already has an entity. Unpossess first.");
                return;
            }
            
            _possessedTarget = target;
            OnPossess(target);
            if (target)
            {
                // OnPossess 중에 Destroy 될 수 있으므로, null 체크 필수.
                target.PossessedBy(this);   
            }
        }

        /// <summary>
        /// 컨트롤할 엔티티가 설정되었을 때 호출됩니다.
        /// </summary>
        protected abstract void OnPossess(Entity target);

        /// <summary>
        /// 컨트롤 중인 엔티티를 해제합니다.
        /// </summary>
        public virtual void Unpossess()
        {
            if (_possessedTarget == null)
            {
                return;
            }
            
            var oldTarget = _possessedTarget;
            _possessedTarget = null;
            OnUnpossess();
            if (oldTarget)
            {
                // OnUnpossess 중에 Destroy 될 수 있으므로, null 체크 필수.
                oldTarget.Unpossessed();   
            }
        }

        /// <summary>
        /// 컨트롤 중인 엔티티가 성공적으로 해제되었을 때 호출됩니다.
        /// </summary>
        protected abstract void OnUnpossess();
    }
}