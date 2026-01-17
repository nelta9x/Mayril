using System;
using System.Collections.Generic;
using Mayril.EventSystem;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mayril.Tests.EventSystem
{
    public class EventBusTests
    {
        // Test Event Data
        public class TestEvent { public int Value; }
        public class AnotherEvent { }

        [SetUp]
        public void SetUp()
        {
            // Ensure clean state before each test
            EventBus<TestEvent>.Reset();
            EventBus<AnotherEvent>.Reset();
        }

        [Test]
        public void Register_And_Trigger_BasicFlow()
        {
            int callCount = 0;
            int lastValue = 0;

            EventBus<TestEvent>.Register(e => 
            {
                callCount++;
                lastValue = e.Value;
            });

            EventBus<TestEvent>.Trigger(new TestEvent { Value = 10 });
            EventBus<TestEvent>.Trigger(new TestEvent { Value = 20 });

            Assert.AreEqual(2, callCount);
            Assert.AreEqual(20, lastValue);
        }

        [Test]
        public void Unregister_PreventsEvaluation()
        {
            int callCount = 0;
            Action<TestEvent> callback = e => callCount++;

            EventBus<TestEvent>.Register(callback);
            EventBus<TestEvent>.Trigger(new TestEvent());
            Assert.AreEqual(1, callCount);

            EventBus<TestEvent>.Unregister(callback);
            EventBus<TestEvent>.Trigger(new TestEvent());
            Assert.AreEqual(1, callCount, "Callback should not be called after unregistering.");
        }

        [Test]
        public void MultipleSubscribers_ReceiveEvent()
        {
            int[] received = new int[3];
            
            EventBus<TestEvent>.Register(e => received[0] = e.Value);
            EventBus<TestEvent>.Register(e => received[1] = e.Value);
            EventBus<TestEvent>.Register(e => received[2] = e.Value);

            EventBus<TestEvent>.Trigger(new TestEvent { Value = 99 });

            Assert.AreEqual(99, received[0]);
            Assert.AreEqual(99, received[1]);
            Assert.AreEqual(99, received[2]);
        }

        [Test]
        public void ExceptionInSubscriber_DoesNotStopPropagation()
        {
            bool secondCallbackExecuted = false;

            EventBus<TestEvent>.Register(e => throw new Exception("Intentionally throwing fail"));
            EventBus<TestEvent>.Register(e => secondCallbackExecuted = true);

            // LogAssert enables us to expect an exception log without failing the test
            LogAssert.Expect(LogType.Exception, "Exception: Intentionally throwing fail");

            // Should not crash
            Assert.DoesNotThrow(() => EventBus<TestEvent>.Trigger(new TestEvent()));
            
            Assert.IsTrue(secondCallbackExecuted, "Subsequent subscribers should still be executed.");
        }

        [Test]
        public void Modification_DuringCallback_IsSafe()
        {
            // Scenario 1: Unregister self during callback
            bool called = false;
            Action<TestEvent> callback = null;
            callback = e => 
            {
                called = true;
                EventBus<TestEvent>.Unregister(callback);
            };

            EventBus<TestEvent>.Register(callback);
            
            // First trigger: executes and unregisters
            Assert.DoesNotThrow(() => EventBus<TestEvent>.Trigger(new TestEvent()));
            Assert.IsTrue(called);

            // Second trigger: should not execute
            called = false;
            EventBus<TestEvent>.Trigger(new TestEvent());
            Assert.IsFalse(called, "Self-unregistered callback should not be called again.");
        }

        [Test]
        public void Modification_RegisterNew_DuringCallback_IsSafe()
        {
            // Scenario 2: Register new callback during callback
            bool secondCallbackCalled = false;
            
            EventBus<TestEvent>.Register(e => 
            {
                // This new registration should NOT be executed in the CURRENT trigger loop
                // (Depends on implementation, but typically snapshot-based execution avoids infinite loops)
                EventBus<TestEvent>.Register(inner => secondCallbackCalled = true);
            });

            EventBus<TestEvent>.Trigger(new TestEvent());
            
            Assert.IsFalse(secondCallbackCalled, "Newly added callback should not be executed in the same frame it was added.");

            // But it should be executed next time
            EventBus<TestEvent>.Trigger(new TestEvent());
            Assert.IsTrue(secondCallbackCalled);
        }

        [Test]
        public void Reset_ClearsAllSubscribers()
        {
            int callCount = 0;
            EventBus<TestEvent>.Register(e => callCount++);

            EventBus<TestEvent>.Trigger(new TestEvent());
            Assert.AreEqual(1, callCount);

            EventBus<TestEvent>.Reset();

            EventBus<TestEvent>.Trigger(new TestEvent());
            Assert.AreEqual(1, callCount, "Should not increment after Reset()");
        }
    }
}
