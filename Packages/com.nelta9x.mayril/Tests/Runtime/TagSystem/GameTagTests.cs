using NUnit.Framework;
using Mayril.TagSystem;

namespace Mayril.Tests.TagSystem
{
    public class GameTagTests
    {
        [Test]
        public void TestTagHierarchy()
        {
            // Arrange
            // "State.Debuff.Stun"
            GameTag stunTag = new GameTag("State.Debuff.Stun");
            GameTag debuffTag = new GameTag("State.Debuff");
            GameTag stateTag = new GameTag("State");
            GameTag otherTag = new GameTag("Other.Tag");

            // Act & Assert

            // 1. 자기 자신 매칭
            Assert.IsTrue(stunTag.MatchesTag(stunTag));

            // 2. 부모 매칭 (Stun is a Debuff)
            Assert.IsTrue(stunTag.MatchesTag(debuffTag));

            // 3. 조상 매칭 (Stun is a State)
            Assert.IsTrue(stunTag.MatchesTag(stateTag));

            // 4. 역방향은 매칭 안됨 (Debuff is NOT a Stun)
            Assert.IsFalse(debuffTag.MatchesTag(stunTag));

            // 5. 전혀 다른 태그
            Assert.IsFalse(stunTag.MatchesTag(otherTag));
        }

        [Test]
        public void TestTagManagerNameLookup()
        {
            // Arrange
            string tagName = "Test.Tag.Name";
            GameTag tag = new GameTag(tagName);

            // Act
            string retrievedName = GameTagManager.GetTagName(tag.Id);

            // Assert
            Assert.AreEqual(tagName, retrievedName);
        }
    }
}
