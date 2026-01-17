using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mayril
{
    /// <summary>
    /// 게임 내 시간 관련 이벤트를 관리하는 클래스입니다.
    /// </summary>
    public class TimerManager
    {
        private readonly TimerEventMinHeap _postedEvents = new();
        private readonly HashSet<ulong> _cancelledHandles = new();
        private float _elapsedTime;
        private ulong _nextHandleId = 1;

        /// <summary>
        /// 흐른 시간.
        /// </summary>
        public float ElapsedTime => _elapsedTime;

        /// <summary>
        /// 현재 대기 중인 이벤트 개수.
        /// </summary>
        public int PendingEventCount => _postedEvents.Count;

        /// <summary>
        /// 이벤트를 지연 시간 후에 실행합니다.
        /// </summary>
        /// <param name="delay">지연 시간 (초).</param>
        /// <param name="callback">이벤트가 실행될 때 호출될 콜백 함수.</param>
        /// <returns>등록된 이벤트의 핸들. 취소에 사용할 수 있습니다.</returns>
        public TimerHandle PostEvent(float delay, Action callback)
        {
            if (callback == null)
            {
                Debug.LogWarning("[TimerManager] Callback is null. Event not posted.");
                return TimerHandle.Invalid;
            }

            if (delay < 0)
            {
                Debug.LogWarning("[TimerManager] Delay time is negative.");
                return TimerHandle.Invalid;
            }

            ulong handleId = _nextHandleId++;
            double executionTime = _elapsedTime + delay;
            _postedEvents.Enqueue(handleId, executionTime, callback);

            return new TimerHandle(handleId, executionTime);
        }

        /// <summary>
        /// 등록된 이벤트를 취소합니다.
        /// </summary>
        /// <param name="handle">취소할 이벤트의 핸들.</param>
        /// <returns>취소에 성공하면 true, 이미 취소되었거나 무효한 핸들이면 false.</returns>
        public bool CancelEvent(TimerHandle handle)
        {
            if (!handle.IsValid)
            {
                return false;
            }

            // [Data-Oriented Optimization]
            // 이미 실행 시간이 지난 핸들은 취소할 필요가 없습니다. (Zombie Handle)
            // 이를 통해 _cancelledHandles에 불필요한 데이터가 쌓이는 것을 방지합니다.
            // 부동소수점 오차를 고려해 약간의 여유값(Epsilon)을 둘 수도 있지만, 
            // 여기서는 단순성을 위해 엄격하게 비교합니다.
            if (handle.ExecutionTime < _elapsedTime)
            {
                return false;
            }

            return _cancelledHandles.Add(handle.Id);
        }

        /// <summary>
        /// 실행 시간에 도달한 이벤트들을 처리합니다.
        /// </summary>
        public void PollEvents()
        {
            double currentTime = _elapsedTime;

            // 폴링 시작 시점에 등록된 이벤트까지만 처리 (콜백 내 무한 루프 방지)
            ulong maxHandleId = _nextHandleId - 1;

            // Min Heap에서 가장 빠른 시간의 이벤트들을 처리
            while (_postedEvents.TryPeek(out var timeEvent) &&
                   timeEvent.ExecutionTime <= currentTime &&
                   timeEvent.HandleId <= maxHandleId)
            {
                _postedEvents.Dequeue();

                // 취소된 이벤트는 건너뜁니다.
                if (_cancelledHandles.Remove(timeEvent.HandleId))
                {
                    continue;
                }

                try
                {
                    timeEvent.Callback();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        /// <summary>
        /// 모든 대기 중인 이벤트를 취소합니다.
        /// </summary>
        public void ClearAllEvents()
        {
            _postedEvents.Clear();
            _cancelledHandles.Clear();
        }

        /// <summary>
        /// 매 프레임 호출됩니다.
        /// </summary>
        public void Update(float deltaTime)
        {
            _elapsedTime += deltaTime;
            PollEvents();
        }
    }
}