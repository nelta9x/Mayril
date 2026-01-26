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

            // 콜백 시점에 태그 체크용
            public AbilitySystemComponent Asc { get; set; }
            public GameTag TagToCheck { get; set; }
            public bool TagWasPresentDuringChange { get; private set; }

            public StatValue GetStatValue(GameTag tag)
            {
                if (tag.Id == new GameTag("Test.Health").Id) return Health;
                return null;
            }

            public void OnStatValueChanging(StatValue stat, ref float newCurrentValue) { }

            public void OnStatValueChanged(StatValue stat)
            {
                // ExecuteEffect 중에 태그가 있는지 확인
                if (Asc != null && TagToCheck.Id != 0)
                {
                    TagWasPresentDuringChange = Asc.HasTag(TagToCheck);
                }
            }
        }

        [SetUp]
        public void Setup()
        {
            _actorObject = new GameObject("TestActor");
            _stats = _actorObject.AddComponent<TestAbilityStatSet>();
            _asc = _actorObject.AddComponent<AbilitySystemComponent>();

            // StatValue의 Stats 프로퍼티 설정 (콜백 활성화)
            _stats.Health.Stats = _stats;

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

        [Test]
        public void TestInstantEffect_GrantedTags_ShouldBeAppliedDuringExecution()
        {
            // Arrange
            GameTag instantTag = new GameTag("Effect.InstantTag");
            _stats.Asc = _asc;
            _stats.TagToCheck = instantTag;

            var instantEffect = new Effect
            {
                Name = "InstantWithTag",
                DurationPolicy = EffectDurationType.Instant,
                GrantedTags = new List<GameTag> { instantTag },
                Modifiers = new List<EffectModifier>
                {
                    new EffectModifier
                    {
                        StatTag = new GameTag("Test.Health"),
                        ModifierType = StatModifierType.Additive,
                        ModifierValue = -1f
                    }
                }
            };

            // Act
            _asc.ApplyEffectToSelf(instantEffect);

            // Assert: ExecuteEffect 실행 시점에 태그가 존재했어야 함
            Assert.IsTrue(_stats.TagWasPresentDuringChange,
                "GrantedTags should be present during ExecuteEffect");

            // Assert: Instant 이펙트 종료 후 태그는 제거되어야 함
            Assert.IsFalse(_asc.HasTag(instantTag),
                "GrantedTags should be removed after Instant effect");
        }

        [Test]
        public void TestRemoveAbility_ActiveAbility_ShouldCallEndAbility()
        {
            // Arrange
            var ability = new TestAbilityWithEndCallback();
            _asc.AddAbility(ability);
            _asc.TryActivateAbility(ability);
            Assert.IsTrue(ability.IsActive);

            // Act
            _asc.RemoveAbility(ability);

            // Assert
            Assert.IsTrue(ability.EndAbilityCalled, "EndAbility should be called when removing an active ability");
            Assert.IsFalse(ability.IsActive);
        }

        [Test]
        public void TestRemoveAbility_InactiveAbility_ShouldNotThrow()
        {
            // Arrange
            var ability = new TestAbilityWithEndCallback();
            _asc.AddAbility(ability);
            Assert.IsFalse(ability.IsActive);

            // Act & Assert (should not throw)
            _asc.RemoveAbility(ability);
            Assert.IsFalse(ability.EndAbilityCalled, "EndAbility should not be called for inactive ability");
        }

        private class TestAbility : Ability
        {
            protected override void OnActivateAbility()
            {
            }
        }

        private class TestAbilityWithEndCallback : Ability
        {
            public bool EndAbilityCalled { get; private set; }

            protected override void OnActivateAbility()
            {
            }

            protected override void OnEndAbility()
            {
                EndAbilityCalled = true;
            }
        }
    }
}
