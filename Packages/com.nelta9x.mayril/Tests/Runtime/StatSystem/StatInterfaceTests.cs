using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Mayril.StatSystem;
using Mayril.TagSystem;
using Unity.Netcode;

namespace Mayril.Tests.StatSystem
{
    // Concrete implementation for testing (Implementing IStatSet directly)
    public class TestStatSet : NetworkBehaviour, IStatSet
    {
        public StatValue Health { get; } = new StatValue(100f);
        public StatValue Mana { get; } = new StatValue(50f);
        public StatValue UnregisteredStat { get; } = new StatValue(10f);

        // Manual implementation of GetStatValue (Switch-based, fastest)
        public StatValue GetStatValue(GameTag tag)
        {
            if (tag.Id == new GameTag("Test.Health").Id) return Health;
            if (tag.Id == new GameTag("Test.Mana").Id) return Mana;
            return null;
        }

        public void OnStatValueChanging(StatValue stat, ref float newCurrentValue)
        {
            // Test logic: clamp Health between 0 and Max (simplified)
            if (stat == Health)
            {
                if (newCurrentValue < 0) newCurrentValue = 0;
            }
        }

        public void OnStatValueChanged(StatValue stat)
        {
        }
    }

    public class StatInterfaceTests
    {
        private GameObject _gameObject;
        private TestStatSet _statSet;

        [SetUp]
        public void Setup()
        {
            _gameObject = new GameObject("StatSetTest");
            _statSet = _gameObject.AddComponent<TestStatSet>();
        }

        [TearDown]
        public void Teardown()
        {
            if (_gameObject != null)
            {
                Object.DestroyImmediate(_gameObject);
            }
        }

        [Test]
        public void GetStatValue_ReturnsCorrectStat()
        {
            // Act
            var healthTag = new GameTag("Test.Health");
            var manaTag = new GameTag("Test.Mana");

            // Assert
            Assert.IsNotNull(_statSet.GetStatValue(healthTag), "Health should be registered");
            Assert.IsNotNull(_statSet.GetStatValue(manaTag), "Mana should be registered");

            Assert.AreEqual(_statSet.Health, _statSet.GetStatValue(healthTag));
            Assert.AreEqual(_statSet.Mana, _statSet.GetStatValue(manaTag));
        }

        [Test]
        public void GetStatValue_ReturnsNull_ForUnmappedStat()
        {
            // Arrange
            var unregisteredTag = new GameTag("Test.Unregistered");

            // Act
            var stat = _statSet.GetStatValue(unregisteredTag);

            // Assert
            Assert.IsNull(stat);
        }

        [Test]
        public void GetStatValue_ReturnsNull_ForInvalidTag()
        {
            // Arrange
            var invalidTag = new GameTag("Invalid.Tag");

            // Act
            var stat = _statSet.GetStatValue(invalidTag);

            // Assert
            Assert.IsNull(stat);
        }
    }
}
