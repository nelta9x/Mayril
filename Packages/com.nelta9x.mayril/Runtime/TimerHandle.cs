using System;

namespace Mayril
{
    /// <summary>
    /// 타이머 이벤트를 식별하는 핸들입니다.
    /// </summary>
    public readonly struct TimerHandle : IEquatable<TimerHandle>
    {
        private readonly ulong _id;

        internal TimerHandle(ulong id)
        {
            _id = id;
        }

        internal ulong Id => _id;

        /// <summary>
        /// 핸들이 유효한지 확인합니다. ID가 0이면 무효한 핸들입니다.
        /// </summary>
        public bool IsValid => _id != 0;

        /// <summary>
        /// 무효한 핸들을 나타냅니다.
        /// </summary>
        public static TimerHandle Invalid => default;

        public bool Equals(TimerHandle other) => _id == other._id;

        public override bool Equals(object obj) => obj is TimerHandle other && Equals(other);

        public override int GetHashCode() => (int)_id;

        public static bool operator ==(TimerHandle left, TimerHandle right) => left.Equals(right);

        public static bool operator !=(TimerHandle left, TimerHandle right) => !left.Equals(right);

        public override string ToString() => $"TimerHandle({_id})";
    }
}