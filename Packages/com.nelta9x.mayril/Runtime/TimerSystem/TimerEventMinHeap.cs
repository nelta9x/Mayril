using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Mayril
{
    /// <summary>
    /// TimeManager 전용 Min Heap 구현.
    /// ExecutionTime 기준으로 정렬되며, 가장 빠른 시간의 이벤트가 루트에 위치합니다.
    /// </summary>
    internal sealed class TimerEventMinHeap
    {
        /// <summary>
        /// 시간 기반 이벤트를 표현하는 구조체.
        /// struct로 정의하여 힙 할당을 피하고 캐시 지역성을 향상시킵니다.
        /// </summary>
        internal struct TimeEvent
        {
            public ulong HandleId;
            public double ExecutionTime;
            public Action Callback;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TimeEvent(ulong handleId, double executionTime, Action callback)
            {
                HandleId = handleId;
                ExecutionTime = executionTime;
                Callback = callback;
            }
        }

        private TimeEvent[] _heap;
        private int _count;

        private const int DefaultCapacity = 16;
        private const int MaxCapacity = 1024 * 1024; // 1M events

        /// <summary>
        /// 현재 힙에 저장된 이벤트 개수.
        /// </summary>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count;
        }

        /// <summary>
        /// 힙이 비어있는지 여부.
        /// </summary>
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count == 0;
        }

        public TimerEventMinHeap() : this(DefaultCapacity)
        {
        }

        public TimerEventMinHeap(int capacity)
        {
            if (capacity <= 0)
            {
                capacity = DefaultCapacity;   
            }

            _heap = new TimeEvent[capacity];
            _count = 0;
        }

        /// <summary>
        /// 새 이벤트를 힙에 추가합니다. O(log n)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(ulong handleId, double executionTime, Action callback)
        {
            if (_count == _heap.Length)
            {
                Grow();
            }

            // 배열 끝에 추가하고 위로 올림
            _heap[_count] = new TimeEvent(handleId, executionTime, callback);
            HeapifyUp(_count);
            _count++;
        }

        /// <summary>
        /// 가장 빠른 시간의 이벤트를 제거하고 반환합니다. O(log n)
        /// </summary>
        public TimeEvent Dequeue()
        {
            if (_count == 0)
            {
                throw new InvalidOperationException("Heap is empty");
            }

            TimeEvent result = _heap[0];
            _count--;

            if (_count > 0)
            {
                // 마지막 요소를 루트로 이동하고 아래로 내림
                _heap[0] = _heap[_count];
                HeapifyDown(0);
            }

            // 참조 제거 (GC 지원)
            _heap[_count] = default;

            return result;
        }

        /// <summary>
        /// 가장 빠른 시간의 이벤트를 제거하지 않고 확인합니다. O(1)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TimeEvent Peek()
        {
            if (_count == 0)
            {
                throw new InvalidOperationException("Heap is empty");
            }

            return _heap[0];
        }

        /// <summary>
        /// 가장 빠른 시간의 이벤트를 안전하게 확인합니다. O(1)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeek(out TimeEvent timeEvent)
        {
            if (_count == 0)
            {
                timeEvent = default;
                return false;
            }

            timeEvent = _heap[0];
            return true;
        }

        /// <summary>
        /// 힙을 비웁니다.
        /// </summary>
        public void Clear()
        {
            // 참조 제거 (GC 지원)
            Array.Clear(_heap, 0, _count);
            _count = 0;
        }

        /// <summary>
        /// a가 b보다 우선순위가 높은지 (먼저 실행되어야 하는지) 확인합니다.
        /// ExecutionTime이 같으면 HandleId가 작은 것(먼저 등록된 것)이 우선입니다.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool HasHigherPriority(in TimeEvent a, in TimeEvent b)
        {
            return a.ExecutionTime < b.ExecutionTime ||
                   (a.ExecutionTime == b.ExecutionTime && a.HandleId < b.HandleId);
        }

        /// <summary>
        /// 지정된 인덱스의 요소를 위로 올립니다. (Bubble Up)
        /// 부모보다 작은 경우 부모와 교환하며 올라갑니다.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HeapifyUp(int index)
        {
            TimeEvent item = _heap[index];

            // 부모 인덱스: (index - 1) / 2 = (index - 1) >> 1
            while (index > 0)
            {
                int parentIndex = (index - 1) >> 1;
                TimeEvent parent = _heap[parentIndex];

                // Min Heap: 자식이 부모보다 우선순위가 높지 않으면 중단
                if (!HasHigherPriority(item, parent))
                {
                    break;
                }

                // 부모를 아래로 내림
                _heap[index] = parent;
                index = parentIndex;
            }

            _heap[index] = item;
        }

        /// <summary>
        /// 지정된 인덱스의 요소를 아래로 내립니다. (Bubble Down)
        /// 자식보다 큰 경우 가장 작은 자식과 교환하며 내려갑니다.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HeapifyDown(int index)
        {
            TimeEvent item = _heap[index];
            int halfCount = _count >> 1; // _count / 2

            // 자식이 있는 동안 반복
            while (index < halfCount)
            {
                // 왼쪽 자식: 2 * index + 1 = (index << 1) + 1
                int leftChildIndex = (index << 1) + 1;
                int rightChildIndex = leftChildIndex + 1;

                // 더 우선순위가 높은 자식 찾기
                TimeEvent higherPriorityChild = _heap[leftChildIndex];
                int higherPriorityChildIndex = leftChildIndex;

                if (rightChildIndex < _count)
                {
                    TimeEvent rightChild = _heap[rightChildIndex];
                    if (HasHigherPriority(rightChild, higherPriorityChild))
                    {
                        higherPriorityChild = rightChild;
                        higherPriorityChildIndex = rightChildIndex;
                    }
                }

                // Min Heap: 부모가 자식보다 우선순위가 높거나 같으면 중단
                if (!HasHigherPriority(higherPriorityChild, item))
                {
                    break;
                }

                // 우선순위 높은 자식을 위로 올림
                _heap[index] = higherPriorityChild;
                index = higherPriorityChildIndex;
            }

            _heap[index] = item;
        }

        /// <summary>
        /// 배열 크기를 증가시킵니다.
        /// 현재 용량의 2배로 증가하되, 최대 용량을 초과하지 않습니다.
        /// </summary>
        private void Grow()
        {
            int newCapacity = _heap.Length * 2;
            if (newCapacity > MaxCapacity)
            {
                newCapacity = MaxCapacity;
            }

            if (newCapacity <= _heap.Length)
            {
                throw new InvalidOperationException($"[TimerEventMinHeap] Heap capacity exceeded maximum limit ({MaxCapacity})");
            }

            TimeEvent[] newHeap = new TimeEvent[newCapacity];
            Array.Copy(_heap, newHeap, _count);
            _heap = newHeap;
        }

        /// <summary>
        /// 디버깅용: 힙 속성이 유지되는지 검증합니다.
        /// </summary>
        internal bool ValidateHeapProperty()
        {
            for (int i = 0; i < _count; i++)
            {
                int leftChild = (i << 1) + 1;
                int rightChild = leftChild + 1;
                if (leftChild < _count && HasHigherPriority(_heap[leftChild], _heap[i]))
                {
                    Debug.LogError($"[TimerEventMinHeap] Heap property violated at index {i}: parent ({_heap[i].ExecutionTime}, {_heap[i].HandleId}) < left child ({_heap[leftChild].ExecutionTime}, {_heap[leftChild].HandleId})");
                    return false;
                }

                if (rightChild < _count && HasHigherPriority(_heap[rightChild], _heap[i]))
                {
                    Debug.LogError($"[TimerEventMinHeap] Heap property violated at index {i}: parent ({_heap[i].ExecutionTime}, {_heap[i].HandleId}) < right child ({_heap[rightChild].ExecutionTime}, {_heap[rightChild].HandleId})");
                    return false;
                }
            }

            return true;
        }
    }
}