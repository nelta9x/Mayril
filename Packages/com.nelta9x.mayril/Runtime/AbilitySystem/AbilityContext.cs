using System.Collections.Generic;
using UnityEngine;

namespace Mayril.AbilitySystem
{
    /// <summary>
    /// Ability 실행 시 전달되는 컨텍스트 정보.
    /// </summary>
    public class AbilityContext
    {
        /// <summary>
        /// Ability를 시전한 Entity (CASTER).
        /// </summary>
        public Entity Caster { get; set; }

        /// <summary>
        /// 단일 타겟 지정 시 목표 Entity (TARGET).
        /// </summary>
        public Entity Target { get; set; }

        /// <summary>
        /// 위치 타겟 지정 시 목표 좌표 (POINT).
        /// </summary>
        public Vector3 TargetPoint { get; set; }

        /// <summary>
        /// Ability 자체에 대한 참조.
        /// </summary>
        public IAbility Ability { get; set; }

        /// <summary>
        /// 발사체 관련 이벤트에서의 발사체 인스턴스 (PROJECTILE).
        /// </summary>
        public Component Projectile { get; set; }

        /// <summary>
        /// 공격/피해 이벤트에서의 공격자 (ATTACKER).
        /// </summary>
        public Entity Attacker { get; set; }

        /// <summary>
        /// 채널링 종료 시 중단 여부.
        /// </summary>
        public bool IsInterrupted { get; set; }

        /// <summary>
        /// AOE 능력 등에서 사용되는 다중 타겟 목록.
        /// </summary>
        public List<Entity> MultipleTargets { get; set; }

        /// <summary>
        /// 커스텀 데이터 저장용 딕셔너리.
        /// </summary>
        public Dictionary<string, object> AdditionalData { get; private set; }

        public AbilityContext()
        {
            MultipleTargets = new List<Entity>();
            AdditionalData = new Dictionary<string, object>();
        }

        /// <summary>
        /// AdditionalData에 값을 설정합니다.
        /// </summary>
        public void SetData(string key, object value)
        {
            AdditionalData[key] = value;
        }

        /// <summary>
        /// AdditionalData에서 값을 가져옵니다.
        /// </summary>
        public T GetData<T>(string key, T defaultValue = default)
        {
            if (AdditionalData.TryGetValue(key, out object value) && value is T typedValue)
            {
                return typedValue;
            }
            return defaultValue;
        }
    }
}