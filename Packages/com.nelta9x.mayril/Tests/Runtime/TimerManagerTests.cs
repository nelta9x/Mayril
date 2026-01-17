using System;
using System.Collections;
using Mayril.Tests;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mayril.Tests
{
    [TestFixture]
    public class TimerManagerTests
    {
        private TimerManager _timerManager;

        [SetUp]
        public void SetUp()
        {
            _timerManager = new TimerManager();
        }

        [Test]
        public void PostEvent_ExecutesCallback_AfterDelay()
        {
            bool executed = false;
            _timerManager.PostEvent(1.0f, () => executed = true);

            // 0.5초 경과
            _timerManager.Update(0.5f);
            Assert.IsFalse(executed, "Event should not execute before delay.");

            // 0.6초 추가 경과 (총 1.1초)
            _timerManager.Update(0.6f);
            Assert.IsTrue(executed, "Event should execute after delay.");
        }

        [Test]
        public void CancelEvent_PreventsCallbackExecution()
        {
            bool executed = false;
            var handle = _timerManager.PostEvent(1.0f, () => executed = true);

            bool cancelResult = _timerManager.CancelEvent(handle);
            Assert.IsTrue(cancelResult, "CancelEvent should return true for valid handle.");

            _timerManager.Update(1.1f);
            Assert.IsFalse(executed, "Cancelled event should not execute.");
        }

        [Test]
        public void CancelEvent_ReturnsFalse_ForInvalidHandle()
        {
            Assert.IsFalse(_timerManager.CancelEvent(TimerHandle.Invalid));
        }

        [Test]
        public void CancelEvent_ReturnsFalse_ForAlreadyCancelledHandle()
        {
            var handle = _timerManager.PostEvent(1.0f, () => { });
            _timerManager.CancelEvent(handle);
            
            Assert.IsFalse(_timerManager.CancelEvent(handle), "Should return false for already cancelled handle.");
        }
        
        [Test]
        public void Events_ExecuteInCorrectOrder()
        {
            var executionOrder = new System.Collections.Generic.List<int>();

            _timerManager.PostEvent(2.0f, () => executionOrder.Add(2));
            _timerManager.PostEvent(1.0f, () => executionOrder.Add(1));
            _timerManager.PostEvent(3.0f, () => executionOrder.Add(3));

            _timerManager.Update(3.1f);

            Assert.AreEqual(3, executionOrder.Count);
            Assert.AreEqual(1, executionOrder[0]);
            Assert.AreEqual(2, executionOrder[1]);
            Assert.AreEqual(3, executionOrder[2]);
        }
        
        [Test]
        public void PostEvent_WithNegativeDelay_ReturnsInvalidHandle()
        {
            var handle = _timerManager.PostEvent(-1.0f, () => { });
            Assert.IsFalse(handle.IsValid);
        }
        
        [Test]
        public void ExceptionInCallback_DoesNotCrashTimerManager()
        {
            bool secondEventExecuted = false;
            
            _timerManager.PostEvent(1.0f, () => throw new Exception("Test Exception"));
            _timerManager.PostEvent(1.0f, () => secondEventExecuted = true); // Same time, should execute

            // Should treat the exception internally and continue
            LogAssert.Expect(LogType.Exception, "Exception: Test Exception");
            _timerManager.Update(1.1f);
            
            Assert.IsTrue(secondEventExecuted, "Second event should execute even if first one throws exception.");
        }
        [Test]
        public void CancelEvent_ForExpiredHandle_ReturnsFalse_AndDoesNotLeakMemory()
        {
            // 1. 이벤트 등록
            var handle = _timerManager.PostEvent(1.0f, () => { });
            
            // 2. 시간 경과 -> 이벤트 실행 완료
            _timerManager.Update(1.1f);
            
            // 3. 이미 끝난 이벤트 취소 시도
            bool result = _timerManager.CancelEvent(handle);
            
            // 4. [Optimization Verification]
            // 최적화 전: true를 반환하고 내부 Set에 ID를 남김 (Memory Leak)
            // 최적화 후: 시간 체크로 인해 false를 반환하고 내부 Set에 추가하지 않음
            Assert.IsFalse(result, "Expired handle should not be cancellable (optimization check).");
        }
    }
}
