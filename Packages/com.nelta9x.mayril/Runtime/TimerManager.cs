using System;
using Mayril.Internal;
using UnityEngine;

namespace Mayril
{
    /// <summary>
    /// 게임 내 시간 관련 이벤트를 관리하는 클래스입니다.
    /// </summary>
    public class TimerManager
    {
        private readonly TimerEventMinHeap _postedEvents = new();
        private float _elapsedTime;

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
        public void PostEvent(float delay, Action callback)
        {
            if (callback == null)
            {
                Debug.LogWarning("[TimerManager] Callback is null. Event not posted.");
                return;
            }

            if (delay < 0)
            {
                Debug.LogWarning("[TimerManager] Delay time is negative.");
                return;
            }

            float executionTime = _elapsedTime + delay;
            _postedEvents.Enqueue(executionTime, callback);
        }

        /// <summary>
        /// 실행 시간에 도달한 이벤트들을 처리합니다.
        /// </summary>
        public void PollEvents()
        {
            float currentTime = _elapsedTime;

            // Min Heap에서 가장 빠른 시간의 이벤트들을 처리
            while (_postedEvents.TryPeek(out var timeEvent) &&
                   timeEvent.ExecutionTime <= currentTime)
            {
                _postedEvents.Dequeue();
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