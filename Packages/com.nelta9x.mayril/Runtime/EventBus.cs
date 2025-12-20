using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mayril
{
    /// <summary>
    /// 이벤트 버스는 게임 내부에서 발생하는 이벤트를 구독을 관리하고, 전파하는 역할을 합니다.
    /// </summary>
    public static class EventBus<T> where T : class
    {
        private const int InitialCapacity = 8;
        private static readonly HashSet<Action<T>> _frontSubscribers = new(InitialCapacity);
        private static readonly List<Action<T>> _backSubscribers = new(InitialCapacity);
        private static bool _isFrontSubscribersDirty = true; // 초기 동기화 필요 여부.
        
        /// <summary>
        /// 이벤트를 전파합니다.
        /// </summary>
        public static void Trigger(T message)
        {
            SyncBackSubscribersIfNeeded();

            // 이벤트 순회 중에 이벤트 바인딩이 추가, 제거될 수 있으므로,
            // FrontBindings의 복제본을 기준으로 이벤트 발생을 전파합니다.
            foreach (var backSubscriber in _backSubscribers)
            {
                try
                {
                    backSubscriber.Invoke(message);   
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        /// <summary>
        /// 이벤트를 구독합니다.
        /// </summary>
        public static void Register(Action<T> callback)
        {
            if (_frontSubscribers.Add(callback))
            {
                _isFrontSubscribersDirty = true;   
            }
        }

        /// <summary>
        /// 이벤트 구독을 해제합니다.
        /// </summary>
        public static void Unregister(Action<T> callback)
        {
            if (_frontSubscribers.Remove(callback))
            {
                _isFrontSubscribersDirty = true;   
            }
        }

        /// <summary>
        /// 모든 정적 구독자 컬렉션과 상태를 초기 상태로 재설정합니다.
        /// 이벤트 구독자 리스트를 초기화하고, 동기화 상태 플래그를 리셋합니다.
        /// </summary>
        public static void Reset()
        {
            _frontSubscribers.Clear();
            _backSubscribers.Clear();
            _isFrontSubscribersDirty = true;
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubsystemRegistrationReset()
        {
            Reset();
        }

        /// <summary>
        /// <see cref="_frontSubscribers"/> 가 변경되었는지 확인하고,
        /// 변경되었을 시 <see cref="_backSubscribers"/> 를 그에 맞춰 동기화합니다.
        /// </summary>
        private static void SyncBackSubscribersIfNeeded()
        {
            if (!_isFrontSubscribersDirty)
            {
                return;
            }

            // 이벤트 순회 중에 바인딩이 변경될 수 있으므로.
            _isFrontSubscribersDirty = false;
            _backSubscribers.Clear();
            foreach (var subscriber in _frontSubscribers)
            {
                _backSubscribers.Add(subscriber);
            }
        }
    }
}