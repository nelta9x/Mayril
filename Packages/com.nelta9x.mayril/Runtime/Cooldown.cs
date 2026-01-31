using UnityEngine;

namespace Mayril
{
    /// <summary>
    /// 스킬이나 능력의 쿨다운을 관리하는 유틸리티 클래스입니다.
    /// 외부에서 경과 시간을 전달받아 업데이트합니다.
    /// </summary>
    public class Cooldown
    {
        private float _duration;
        private float _elapsed;

        /// <summary>
        /// 쿨다운이 완료되어 사용 가능한지 여부
        /// </summary>
        public bool IsReady => _elapsed >= _duration;

        /// <summary>
        /// 남은 쿨다운 시간 (초)
        /// </summary>
        public float TimeRemaining => Mathf.Max(0, _duration - _elapsed);

        public Cooldown(float duration)
        {
            _duration = duration;
            _elapsed = duration;
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

                return Mathf.Clamp01(_elapsed / _duration);
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
        /// 경과 시간을 전달받아 쿨다운을 진행시킵니다.
        /// </summary>
        /// <param name="elapsedTime">경과 시간 (초)</param>
        public void Update(float elapsedTime)
        {
            if (IsReady)
            {
                return;
            }

            _elapsed += elapsedTime;
        }

        /// <summary>
        /// 쿨다운을 시작합니다.
        /// </summary>
        public void Use()
        {
            _elapsed = 0f;
        }

        /// <summary>
        /// 쿨다운을 즉시 리셋하여 사용 가능 상태로 만듭니다.
        /// </summary>
        public void Reset()
        {
            _elapsed = _duration;
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
