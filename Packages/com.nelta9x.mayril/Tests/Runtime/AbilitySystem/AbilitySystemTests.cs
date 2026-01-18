using NUnit.Framework;
using UnityEngine;
using Mayril.AbilitySystem;
using Mayril.StatSystem;
using Mayril.TagSystem;
using Unity.Netcode;
using System.Collections.Generic;

namespace Mayril.Tests.AbilitySystem
{
    public class AbilitySystemTests
    {
        private GameObject _actorObject;
        private AbilitySystemComponent _asc;
        private TestAbilityStatSet _stats;

        // Test StatSet
        public class TestAbilityStatSet : NetworkBehaviour, IStatSet
        {
            public StatValue Health { get; } = new StatValue(100f);

            public StatValue GetStatValue(GameTag tag)
            {
                if (tag.Id == new GameTag("Test.Health").Id) return Health;
                return null;
            }

            public void OnStatValueChanging(StatValue stat, ref float newCurrentValue) { }
            public void OnStatValueChanged(StatValue stat) { }
        }

        [SetUp]
        public void Setup()
        {
            _actorObject = new GameObject("TestActor");
            _stats = _actorObject.AddComponent<TestAbilityStatSet>();
            _asc = _actorObject.AddComponent<AbilitySystemComponent>();

            // Explicit Registration
            _asc.RegisterStats(_stats);
        }

        [TearDown]
        public void Teardown()
        {
            if (_actorObject != null) Object.DestroyImmediate(_actorObject);
        }

        [Test]
        public void TestInstantEffect_Damage()
        {
            // Arrange
            var damageEffect = new Effect
            {
                Name = "Damage",
                DurationPolicy = EffectDurationType.Instant,
                Modifiers = new List<EffectModifier>
                {
                    new EffectModifier
                    {
                        StatTag = new GameTag("Test.Health"),
                        ModifierType = StatModifierType.Additive,
                        ModifierValue = -10f
                    }
                }
            };

            // Act
            _asc.ApplyEffectToSelf(damageEffect);

            // Assert
            Assert.AreEqual(90f, _stats.Health.CurrentValue);
        }

        [Test]
        public void TestDurationEffect_Buff()
        {
            // Arrange
            var buffEffect = new Effect
            {
                Name = "Buff",
                DurationPolicy = EffectDurationType.HasDuration,
                Duration = 5f,
                Modifiers = new List<EffectModifier>
                {
                    new EffectModifier
                    {
                        StatTag = new GameTag("Test.Health"),
                        ModifierType = StatModifierType.Additive,
                        ModifierValue = 50f
                    }
                }
            };

            // Act
            var activeEffect = _asc.ApplyEffectToSelf(buffEffect);

            // Assert
            Assert.AreEqual(150f, _stats.Health.CurrentValue);
            Assert.IsTrue(activeEffect.IsActive);

            // Cleanup
            _asc.RemoveActiveEffect(activeEffect);
            Assert.AreEqual(100f, _stats.Health.CurrentValue);
        }

        [Test]
        public void TestTagBlocking()
        {
            // Arrange
            GameTag stunTag = new GameTag("State.Stun");

            var ability = new TestAbility();
            ability.ActivationBlockedTags.AddTag(stunTag);

            _asc.AddAbility(ability);

            // Act 1: Normal activation
            bool activated = _asc.TryActivateAbility(ability);
            Assert.IsTrue(activated);
            ability.EndAbility();

            // Act 2: Apply Stun Tag
            _asc.AddLooseTags(new[] { stunTag });

            bool activatedBlocked = _asc.TryActivateAbility(ability);
            Assert.IsFalse(activatedBlocked);
        }

        private class TestAbility : Ability
        {
            protected override void OnActivateAbility()
            {
            }
        }
    }
}
