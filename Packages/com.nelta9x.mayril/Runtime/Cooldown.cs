using UnityEngine;

namespace Mayril
{
    /// <summary>
    /// 스킬이나 능력의 쿨다운을 관리하는 유틸리티 클래스입니다.
    /// </summary>
    public class Cooldown
    {
        private float _duration;
        private float _lastUseTime = -Mathf.Infinity;

        /// <summary>
        /// 쿨다운이 완료되어 사용 가능한지 여부
        /// </summary>
        public bool IsReady => Time.time >= _lastUseTime + _duration;

        /// <summary>
        /// 남은 쿨다운 시간 (초)
        /// </summary>
        public float TimeRemaining => Mathf.Max(0, (_lastUseTime + _duration) - Time.time);
        
        public Cooldown(float duration)
        {
            _duration = duration;
        }
        
        /// <summary>
        /// 쿨다운 진행도 (0.0 ~ 1.0)
        /// </summary>
        public float Progress
        {
            get
            {
                if (_duration <= 0)
                {
                    return 1f;
                }

                return Mathf.Clamp01((Time.time - _lastUseTime) / _duration);
            }
        }

        /// <summary>
        /// 쿨다운 지속 시간
        /// </summary>
        public float Duration
        {
            get => _duration;
            set => _duration = value;
        }

        /// <summary>
        /// 쿨다운을 시작합니다.
        /// </summary>
        public void Use()
        {
            _lastUseTime = Time.time;
        }

        /// <summary>
        /// 쿨다운을 즉시 리셋하여 사용 가능 상태로 만듭니다.
        /// </summary>
        public void Reset()
        {
            _lastUseTime = -Mathf.Infinity;
        }

        /// <summary>
        /// 쿨다운이 준비되었는지 확인하고, 준비되었다면 즉시 사용합니다.
        /// </summary>
        /// <returns>사용 가능하여 쿨다운을 시작했는지 여부</returns>
        public bool TryUse()
        {
            if (IsReady)
            {
                Use();
                return true;
            }

            return false;
        }
    }
}
