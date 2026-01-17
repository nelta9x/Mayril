using NUnit.Framework;
using Mayril.TagSystem;

namespace Mayril.Tests.TagSystem
{
    public class GameTagContainerTests
    {
        [Test]
        public void TestContainerHierarchySupport()
        {
            // Arrange
            var container = new GameTagContainer();
            var childTag = new GameTag("Status.Debuff.Stun");
            var parentTag = new GameTag("Status.Debuff");
            var grandParentTag = new GameTag("Status");

            // Act: Add Child
            container.AddTag(childTag);

            // Assert: Should have all levels
            Assert.IsTrue(container.HasTag(childTag), "Container should have the explicitly added tag.");
            Assert.IsTrue(container.HasTag(parentTag), "Container should implicitly have the parent tag.");
            Assert.IsTrue(container.HasTag(grandParentTag), "Container should implicitly have the grandparent tag.");
        }

        [Test]
        public void TestContainerReferenceCounting()
        {
            // Arrange
            var container = new GameTagContainer();
            var stun = new GameTag("Status.Debuff.Stun");
            var silence = new GameTag("Status.Debuff.Silence");
            var parent = new GameTag("Status.Debuff");

            // Act 1: Add Stun
            container.AddTag(stun);
            Assert.IsTrue(container.HasTag(parent));

            // Act 2: Add Silence
            container.AddTag(silence);
            Assert.IsTrue(container.HasTag(parent));

            // Act 3: Remove Stun
            container.RemoveTag(stun);
            Assert.IsFalse(container.HasTag(stun));
            Assert.IsTrue(container.HasTag(silence)); 
            Assert.IsTrue(container.HasTag(parent), "Parent should still exist because Silence is present.");

            // Act 4: Remove Silence
            container.RemoveTag(silence);
            Assert.IsFalse(container.HasTag(silence));
            Assert.IsFalse(container.HasTag(parent), "Parent should be removed when all children are removed.");
        }
    }
}
