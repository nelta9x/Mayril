using System.Collections.Generic;

namespace Mayril.AbilitySystem
{
    /// <summary>
    /// 게임 내 어빌리티(스킬, 아이템으로 인한 효과 등)에 적용되는 모디파이어를 표현하는 클래스입니다.
    /// </summary>
    public class AbilityModifier
    {
        private double _creationTime;

        /// <summary>
        /// 모디파이어 이름.
        /// </summary>
        public string ModifierName { get; set; } = "";

        /// <summary>
        /// 모디파이어의 어빌리티.
        /// </summary>
        public IAbility Ability { get; set; }
        
        /// <summary>
        /// 모디파이어 어빌리티의 소유자. (타겟)
        /// </summary>
        public Entity Owner { get; set; }
        
        /// <summary>
        /// 증가시킬 어빌리티 모디파이어 태그들.
        /// </summary>
        public List<string> IncreaseEnablerTags { get; set; } = new();
        
        /// <summary>
        /// 감소시킬 어빌리티 모디파이어 태그들.
        /// </summary>
        public List<string> DecreaseEnablerTags { get; set; } = new();
        
        /// <summary>
        /// 모디파이어 태그들.
        /// </summary>
        public List<string> ModifierTags { get; set; } = new();

        /// <summary>
        /// 활성화 여부.
        /// </summary>
        public bool IsEnabled { get; set; } = true;
        
        /// <summary>
        /// 활성화 스택.
        /// 이 값이 1을 넘으면 활성화, 0이하가 되면 비활성화 됩니다.
        /// </summary>
        public int EnablerStack { get; set; } = 1;
        
        /// <summary>
        /// 틱 사용 여부.
        /// 이 값이 true인 경우에만 OnIntervalTick이 호출됩니다.
        /// </summary>
        public bool UseIntervalTick { get; set; }
        
        /// <summary>
        /// 틱 호출 간격.
        /// 이 값은 UseIntervalTick이 true일 때에만 유효합니다.
        /// </summary>
        public float TickInterval { get; set; }
        
        /// <summary>
        /// 지속시간.
        /// 지속시간 이후, 어빌리티는 제거됩니다.
        /// </summary>
        public float Duration { get; set; }
        
        /// <summary>
        /// 생성된 시간.
        /// </summary>
        public float CreationTime { get; set; } = 0;
        
        /// <summary>
        /// 남은 지속시간.
        /// </summary>
        public float RemainingDuration => Duration - ElapsedTime;

        /// <summary>
        /// 지난시간.
        /// </summary>
        public float ElapsedTime => Owner.OwningWorld.WorldTimerManager.ElapsedTime - CreationTime;
        
        /// <summary>
        /// 남은 지속시간.
        /// </summary>
        public float RemainingTime => Duration > 0 ? Duration - ElapsedTime : 0f;

        /// <summary>
        /// 모디파이어가 생성될 때 호출됩니다.
        /// </summary>
        public virtual void OnCreated()
        {
        }

        /// <summary>
        /// 모디파이어가 붙었을 때 호출됩니다.
        /// </summary>
        public virtual void OnAttached()
        {
        }

        /// <summary>
        /// 모디파이어가 활성화 될 때 호출됩니다.
        /// </summary>
        public virtual void OnEnabled()
        {
        }

        /// <summary>
        /// 모디파이어가 비활성화 될 때 호출됩니다.
        /// </summary>
        public virtual void OnDisabled()
        {
        }
        
        /// <summary>
        /// 틱 간격마다 호출됩니다.
        /// </summary>
        public virtual void OnIntervalTick()
        {
        }

        /// <summary>
        /// 모디파이어가 떨어질 때 호출됩니다.
        /// </summary>
        public virtual void OnDetached()
        {
        }
    }
}