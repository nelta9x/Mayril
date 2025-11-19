using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mayril
{
    /// <summary>
    /// 게임 내 시간 관련 이벤트를 관리하는 클래스입니다.
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        private class TimeEvent
        {
            public GameObject Executor;
            public float ExecutionTime;
            public Action Callback;
        }
        
        private readonly List<TimeEvent> _postedEvents = new(); // TODO PriorityQueue로 수정

        /// <summary>
        /// 현재 게임 플레이 시간을 반환합니다.
        /// </summary>
        public float PlayTime => Time.time;

        /// <summary>
        /// 이벤트를 지연 시간 후에 실행합니다.
        /// </summary>
        /// <param name="delay">지연 시간.</param>
        /// <param name="obj">이벤트를 실행할 객체.</param>
        /// <param name="callback">이벤트가 실행될 때 호출될 콜백 함수.</param>
        public void PostEvent(float delay, GameObject obj, Action callback)
        {
            float executionTime = Time.time + delay;
            InsertSorted(new TimeEvent()
            {
                Executor = obj,
                ExecutionTime = executionTime,
                Callback = callback
            });
        }

        /// <summary>
        /// 이벤트를 지연 시간 후에 실행합니다.
        /// </summary>
        /// <param name="delay">지연 시간.</param>
        /// <param name="callback">이벤트가 실행될 때 호출될 콜백 함수.</param>
        public void PostEvent(float delay, Action callback)
        {
            PostEvent(delay, gameObject, callback);
        }

        /// <summary>
        /// 실행 시간에 도달한 이벤트들을 처리합니다.
        /// </summary>
        public void PollEvents()
        {
            float currentTime = Time.time;
            
            // 리스트가 ExecutionTime으로 정렬되어 있으므로 앞에서부터 확인합니다.
            while (_postedEvents.Count > 0 && _postedEvents[0].ExecutionTime <= currentTime)
            {
                var timeEvent = _postedEvents[0];
                _postedEvents.RemoveAt(0);

                // Executor가 파괴되었을 수 있으므로 null 체크를 합니다.
                if (timeEvent.Executor)
                {
                    timeEvent.Callback();
                }
            }
        }
        
        /// <summary>
        /// 이진 탐색으로 TimeEvent를 정렬된 순서로 삽입합니다.
        /// </summary>
        /// <param name="newEvent">새로운 TimeEvent.</param>
        private void InsertSorted(TimeEvent newEvent)
        {
            for (int i = 0; i < _postedEvents.Count; i++)
            {
                if (_postedEvents[i].ExecutionTime > newEvent.ExecutionTime)
                {
                    _postedEvents.Insert(i, newEvent);
                    return;
                }
            }
            
            _postedEvents.Add(newEvent);
        }

        /// <summary>
        /// 매 틱 호출됩니다.
        /// </summary>
        private void Update()
        {
            PollEvents();
        }
    }
}