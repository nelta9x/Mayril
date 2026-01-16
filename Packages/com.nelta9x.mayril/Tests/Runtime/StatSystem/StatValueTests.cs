using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Mayril.StatSystem;

namespace Mayril.Tests.StatSystem
{
    public class StatValueTests
    {
        [Test]
        public void BaseValue_Set_UpdatesCurrentValue()
        {
            var stat = new StatValue(100f);
            Assert.AreEqual(100f, stat.CurrentValue);

            stat.BaseValue = 200f;
            Assert.AreEqual(200f, stat.CurrentValue);
        }

        [Test]
        public void AddModifier_Additive_UpdatesCorrectly()
        {
            var stat = new StatValue(100f);
            var modifier = new StatModifier(StatModifierType.Additive, 50f);
            
            stat.AddModifier(modifier);
            
            // 100 + 50 = 150
            Assert.AreEqual(150f, stat.CurrentValue);
        }

        [Test]
        public void AddModifier_Multiplicative_UpdatesCorrectly()
        {
            var stat = new StatValue(100f);
            // Multiplicative 0.5 means +50%
            var modifier = new StatModifier(StatModifierType.Multiplicative, 0.5f);
            
            stat.AddModifier(modifier);
            
            // 100 * (1 + 0.5) = 150
            Assert.AreEqual(150f, stat.CurrentValue);
        }

        [Test]
        public void AddModifier_Fixed_UpdatesCorrectly()
        {
            var stat = new StatValue(100f);
            var modifier = new StatModifier(StatModifierType.Fixed, 20f);
            
            stat.AddModifier(modifier);
            
            // 100 + 20 = 120 (Assuming no Multiplicative)
            Assert.AreEqual(120f, stat.CurrentValue);
        }

        [Test]
        public void CalculationOrder_IsCorrect()
        {
            // Formula: (Base + Additive) * (1 + Multiplicative) + Fixed
            var stat = new StatValue(100f);
            
            stat.AddModifier(new StatModifier(StatModifierType.Additive, 50f));        // +50
            stat.AddModifier(new StatModifier(StatModifierType.Multiplicative, 1.0f)); // +100% (x2)
            stat.AddModifier(new StatModifier(StatModifierType.Fixed, 100f));       // +100

            // Expected: ((100 + 50) * (1 + 1.0)) + 100
            //         = (150 * 2) + 100
            //         = 300 + 100
            //         = 400
            
            Assert.AreEqual(400f, stat.CurrentValue);
        }

        [Test]
        public void RemoveModifier_RecalculatesValue()
        {
            var stat = new StatValue(100f);
            var modifier = new StatModifier(StatModifierType.Additive, 50f);
            
            stat.AddModifier(modifier);
            Assert.AreEqual(150f, stat.CurrentValue);

            bool removed = stat.RemoveModifier(modifier);
            Assert.IsTrue(removed);
            
            // Should return to 100
            Assert.AreEqual(100f, stat.CurrentValue);
        }

        [Test]
        public void MultipleModifiers_SameType_AccumulateCorrectly()
        {
            var stat = new StatValue(100f);
            
            stat.AddModifier(new StatModifier(StatModifierType.Additive, 10f));
            stat.AddModifier(new StatModifier(StatModifierType.Additive, 20f));
            
            // 100 + 10 + 20 = 130
            Assert.AreEqual(130f, stat.CurrentValue);
        }
    }
}
