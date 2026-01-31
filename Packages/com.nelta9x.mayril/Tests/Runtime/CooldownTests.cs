using NUnit.Framework;

namespace Mayril.Tests
{
    public class CooldownTests
    {
        [Test]
        public void NewCooldown_IsReady()
        {
            var cooldown = new Cooldown(5f);
            Assert.IsTrue(cooldown.IsReady);
        }

        [Test]
        public void Use_StartsCooldown()
        {
            var cooldown = new Cooldown(5f);
            cooldown.Use();

            Assert.IsFalse(cooldown.IsReady);
            Assert.AreEqual(5f, cooldown.TimeRemaining);
            Assert.AreEqual(0f, cooldown.Progress);
        }

        [Test]
        public void Update_ReducesTimeRemaining()
        {
            var cooldown = new Cooldown(10f);
            cooldown.Use();

            cooldown.Update(3f);

            Assert.AreEqual(7f, cooldown.TimeRemaining, 0.001f);
            Assert.AreEqual(0.3f, cooldown.Progress, 0.001f);
            Assert.IsFalse(cooldown.IsReady);
        }

        [Test]
        public void Update_FullDuration_BecomesReady()
        {
            var cooldown = new Cooldown(5f);
            cooldown.Use();

            cooldown.Update(5f);

            Assert.IsTrue(cooldown.IsReady);
            Assert.AreEqual(0f, cooldown.TimeRemaining);
            Assert.AreEqual(1f, cooldown.Progress);
        }

        [Test]
        public void Update_BeyondDuration_ClampsValues()
        {
            var cooldown = new Cooldown(5f);
            cooldown.Use();

            cooldown.Update(10f);

            Assert.IsTrue(cooldown.IsReady);
            Assert.AreEqual(0f, cooldown.TimeRemaining);
            Assert.AreEqual(1f, cooldown.Progress);
        }

        [Test]
        public void Update_WhenReady_DoesNothing()
        {
            var cooldown = new Cooldown(5f);
            // 생성 직후 IsReady == true, _elapsed == duration
            float remaining = cooldown.TimeRemaining;

            cooldown.Update(10f);

            Assert.AreEqual(remaining, cooldown.TimeRemaining);
        }

        [Test]
        public void Reset_MakesCooldownReady()
        {
            var cooldown = new Cooldown(5f);
            cooldown.Use();
            cooldown.Update(2f);

            cooldown.Reset();

            Assert.IsTrue(cooldown.IsReady);
            Assert.AreEqual(0f, cooldown.TimeRemaining);
        }

        [Test]
        public void TryUse_WhenReady_ReturnsTrue()
        {
            var cooldown = new Cooldown(5f);

            bool result = cooldown.TryUse();

            Assert.IsTrue(result);
            Assert.IsFalse(cooldown.IsReady);
        }

        [Test]
        public void TryUse_WhenNotReady_ReturnsFalse()
        {
            var cooldown = new Cooldown(5f);
            cooldown.Use();

            bool result = cooldown.TryUse();

            Assert.IsFalse(result);
        }

        [Test]
        public void Duration_CanBeChanged()
        {
            var cooldown = new Cooldown(5f);
            cooldown.Use();

            cooldown.Duration = 3f;
            cooldown.Update(3f);

            Assert.IsTrue(cooldown.IsReady);
        }

        [Test]
        public void ZeroDuration_AlwaysReady()
        {
            var cooldown = new Cooldown(0f);
            cooldown.Use();

            Assert.IsTrue(cooldown.IsReady);
            Assert.AreEqual(1f, cooldown.Progress);
        }

        [Test]
        public void MultipleUseCycles_WorkCorrectly()
        {
            var cooldown = new Cooldown(2f);

            cooldown.Use();
            Assert.IsFalse(cooldown.IsReady);

            cooldown.Update(2f);
            Assert.IsTrue(cooldown.IsReady);

            cooldown.Use();
            Assert.IsFalse(cooldown.IsReady);

            cooldown.Update(1f);
            Assert.IsFalse(cooldown.IsReady);

            cooldown.Update(1f);
            Assert.IsTrue(cooldown.IsReady);
        }
    }
}
