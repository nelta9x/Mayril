using System;
using Mayril.InventorySystem;
using Mayril.Tests;
using NUnit.Framework;

namespace Mayril.Tests.InventorySystem
{
    [TestFixture]
    public class ItemCollectionTests
    {
        private ItemCollection collection;
        private MockItem mockItem1;
        private MockItem mockItem2;

        [SetUp]
        public void SetUp()
        {
            collection = new ItemCollection();
            mockItem1 = new MockItem(1) { ItemCode = 1 };
            mockItem2 = new MockItem(1) { ItemCode = 2 };
        }

        #region Constructor Tests

        [TestCase(0)]
        [TestCase(3)]
        [TestCase(10)]
        public void Constructor_WithInitialSlots_CreatesCorrectNumberOfSlots(int initialSlots)
        {
            var collection = new ItemCollection(initialSlots);

            Assert.AreEqual(initialSlots, collection.TotalSlotCount);
            Assert.AreEqual(initialSlots, collection.Slots.Count);
            Assert.AreEqual(initialSlots, collection.AvailableSlotCount);
            Assert.AreEqual(0, collection.OccupiedSlotCount);
        }

        [Test]
        public void DefaultConstructor_CreatesEmptyCollection()
        {
            var collection = new ItemCollection();

            Assert.AreEqual(0, collection.TotalSlotCount);
            Assert.AreEqual(0, collection.AvailableSlotCount);
            Assert.AreEqual(0, collection.OccupiedSlotCount);
            Assert.AreEqual(-1, collection.FirstAvailableSlotIndex);
        }

        #endregion

        #region Slot Count Tests

        [TestCase(1, 1)]
        [TestCase(5, 3)]
        public void OccupiedSlots_AfterAddingItems_ReturnsCorrectCount(int totalSlots, int itemCount)
        {
            var collection = new ItemCollection(totalSlots);

            for (int i = 0; i < itemCount; i++)
            {
                var newItem = new MockItem(1) { ItemCode = i + 1 };
                collection.AddItemAt(newItem, i);
            }

            Assert.AreEqual(itemCount, collection.OccupiedSlotCount);
            Assert.AreEqual(itemCount, collection.OccupiedIndices.Count);

            for (int i = 0; i < itemCount; i++)
            {
                Assert.IsFalse(collection.Slots[i].IsEmpty);
                Assert.IsNotNull(collection.GetItemAt(i));
            }
        }

        [TestCase(1, 1, 0)]
        [TestCase(5, 3, 2)]
        [TestCase(10, 0, 10)]
        public void AvailableSlotCount_AfterAddingItems_ReturnsCorrectCount(int totalSlots, int itemCount, int expectedAvailable)
        {
            var collection = new ItemCollection(totalSlots);

            for (int i = 0; i < itemCount; i++)
            {
                var newItem = new MockItem(1) { ItemCode = i + 1 };
                collection.AddItemAt(newItem, i);
            }

            Assert.AreEqual(expectedAvailable, collection.AvailableSlotCount);
            Assert.AreEqual(totalSlots, collection.TotalSlotCount);
            Assert.AreEqual(itemCount, collection.OccupiedSlotCount);
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(10)]
        public void TotalSlotCount_RemainsConstantAfterAddingItems(int totalSlots)
        {
            var collection = new ItemCollection(totalSlots);

            Assert.AreEqual(totalSlots, collection.TotalSlotCount);

            if (totalSlots > 0)
            {
                collection.AddItemAt(mockItem1, 0);
                Assert.AreEqual(totalSlots, collection.TotalSlotCount);
            }
        }

        #endregion

        #region GetItemAt Tests

        [TestCase(3, 0)]
        [TestCase(3, 1)]
        [TestCase(3, 2)]
        public void GetItemAt_AfterAddingItem_ReturnsCorrectItem(int totalSlots, int slotIndex)
        {
            var collection = new ItemCollection(totalSlots);
            var item = new MockItem(1) { ItemCode = 1 };

            collection.AddItemAt(item, slotIndex);
            var retrieved = collection.GetItemAt(slotIndex);

            Assert.IsNotNull(retrieved);
            Assert.AreEqual(item, retrieved);
        }

        [TestCase(2, -1)]
        [TestCase(2, 2)]
        [TestCase(0, 0)]
        public void GetItemAt_WithInvalidIndex_ReturnsNull(int totalSlots, int invalidIndex)
        {
            var collection = new ItemCollection(totalSlots);

            var result = collection.GetItemAt(invalidIndex);

            Assert.IsNull(result);
        }

        #endregion

        #region IsAvailable Tests

        [TestCase(3, 0)]
        [TestCase(3, 1)]
        [TestCase(3, 2)]
        public void IsAvailable_AfterAddingItem_ReturnsFalse(int totalSlots, int slotIndex)
        {
            var collection = new ItemCollection(totalSlots);

            Assert.IsTrue(collection.IsAvailableSlot(slotIndex));

            collection.AddItemAt(mockItem1, slotIndex);

            Assert.IsFalse(collection.IsAvailableSlot(slotIndex));
        }

        #endregion

        #region FirstAvailableSlotIndex Tests

        [Test]
        public void FirstAvailableSlotIndex_WhenFirstSlotOccupied_ReturnsOne()
        {
            var collection = new ItemCollection(3);
            Assert.AreEqual(0, collection.FirstAvailableSlotIndex);

            collection.AddItemAt(mockItem1, 0);

            Assert.AreEqual(1, collection.FirstAvailableSlotIndex);
        }

        [Test]
        public void FirstAvailableSlotIndex_WhenAllSlotsOccupied_ReturnsMinusOne()
        {
            var collection = new ItemCollection(2);

            collection.AddItemAt(mockItem1, 0);
            collection.AddItemAt(mockItem2, 1);

            Assert.AreEqual(-1, collection.FirstAvailableSlotIndex);
        }

        [Test]
        public void FirstAvailableSlotIndex_WhenAllSlotsEmpty_ReturnsZero()
        {
            var collection = new ItemCollection(3);

            Assert.AreEqual(0, collection.FirstAvailableSlotIndex);
        }

        [Test]
        public void FirstAvailableSlotIndex_AfterRemovingItem_UpdatesCorrectly()
        {
            var collection = new ItemCollection(3);
            collection.AddItemAt(mockItem1, 0);
            collection.AddItemAt(mockItem2, 1);

            Assert.AreEqual(2, collection.FirstAvailableSlotIndex);

            collection.RemoveItemAt(0);

            Assert.AreEqual(0, collection.FirstAvailableSlotIndex);
        }

        #endregion

        #region AddItemAt Tests

        [Test]
        public void AddItemAt_ToEmptySlot_ReturnsTrue()
        {
            var collection = new ItemCollection(3);

            var result = collection.AddItemAt(mockItem1, 1);

            Assert.IsTrue(result);
            Assert.AreEqual(mockItem1, collection.GetItemAt(1));
            Assert.AreEqual(1, collection.OccupiedSlotCount);
        }

        [Test]
        public void AddItemAt_ToOccupiedSlot_ReturnsFalse()
        {
            var collection = new ItemCollection(3);
            collection.AddItemAt(mockItem1, 1);

            var result = collection.AddItemAt(mockItem2, 1);

            Assert.IsFalse(result);
            Assert.AreEqual(mockItem1, collection.GetItemAt(1));
            Assert.AreEqual(1, collection.OccupiedSlotCount);
        }

        [Test]
        public void AddItemAt_WithNullItem_ReturnsFalse()
        {
            var collection = new ItemCollection(3);

            var result = collection.AddItemAt(null, 0);

            Assert.IsFalse(result);
            Assert.IsNull(collection.GetItemAt(0));
            Assert.AreEqual(0, collection.OccupiedSlotCount);
        }

        [TestCase(-1)]
        [TestCase(3)]
        public void AddItemAt_ToInvalidIndex_ReturnsFalse(int invalidIndex)
        {
            var collection = new ItemCollection(3);

            var result = collection.AddItemAt(mockItem1, invalidIndex);

            Assert.IsFalse(result);
            Assert.AreEqual(0, collection.OccupiedSlotCount);
        }

        #endregion

        #region RemoveItemAt Tests

        [Test]
        public void RemoveItemAt_FromOccupiedSlot_ReturnsItem()
        {
            var collection = new ItemCollection(3);
            collection.AddItemAt(mockItem1, 1);

            var removed = collection.RemoveItemAt(1);

            Assert.AreEqual(mockItem1, removed);
            Assert.IsNull(collection.GetItemAt(1));
            Assert.AreEqual(0, collection.OccupiedSlotCount);
            Assert.AreEqual(0, collection.FirstAvailableSlotIndex); // Slot 0 is the first available slot
        }

        [Test]
        public void RemoveItemAt_FromEmptySlot_ReturnsNull()
        {
            var collection = new ItemCollection(3);

            var removed = collection.RemoveItemAt(1);

            Assert.IsNull(removed);
            Assert.AreEqual(0, collection.OccupiedSlotCount);
        }

        [TestCase(-1)]
        [TestCase(3)]
        public void RemoveItemAt_WithInvalidIndex_ReturnsNull(int invalidIndex)
        {
            var collection = new ItemCollection(3);

            var removed = collection.RemoveItemAt(invalidIndex);

            Assert.IsNull(removed);
        }

        #endregion

        #region IsValidSlotIndex Tests

        [TestCase(3, -1, false)]
        [TestCase(3, 0, true)]
        [TestCase(3, 1, true)]
        [TestCase(3, 2, true)]
        [TestCase(3, 3, false)]
        public void IsValidSlotIndex_ReturnsCorrectResult(int totalSlots, int slotIndex, bool expectedResult)
        {
            var collection = new ItemCollection(totalSlots);

            Assert.AreEqual(expectedResult, collection.IsValidSlotIndex(slotIndex));
        }

        #endregion

        #region Extend Tests

        [TestCase(0)]
        [TestCase(3)]
        [TestCase(10)]
        public void Extend_AddsCorrectNumberOfSlots(int additionalSlots)
        {
            var collection = new ItemCollection(2);
            var initialCount = collection.TotalSlotCount;

            collection.Extend(additionalSlots);

            Assert.AreEqual(initialCount + additionalSlots, collection.TotalSlotCount);
            Assert.AreEqual(initialCount + additionalSlots, collection.AvailableSlotCount);
        }

        [Test]
        public void Extend_WithNegativeValue_ThrowsException()
        {
            var collection = new ItemCollection();

            Assert.Throws<ArgumentOutOfRangeException>(() => collection.Extend(-1));
        }

        [Test]
        public void Extend_UpdatesFirstAvailableSlotIndex()
        {
            var collection = new ItemCollection(2);
            collection.AddItemAt(mockItem1, 0);
            collection.AddItemAt(mockItem2, 1);

            Assert.AreEqual(-1, collection.FirstAvailableSlotIndex);

            collection.Extend(2);

            Assert.AreEqual(2, collection.FirstAvailableSlotIndex);
        }

        #endregion

        #region Shrink Tests

        [TestCase(5, 0, 5)]
        [TestCase(5, 2, 3)]
        [TestCase(5, 5, 0)]
        public void Shrink_RemovesCorrectNumberOfSlots(int initialSlots, int slotsToRemove, int expectedSlots)
        {
            var collection = new ItemCollection(initialSlots);

            collection.Shrink(slotsToRemove, null);

            Assert.AreEqual(expectedSlots, collection.TotalSlotCount);
        }

        [Test]
        public void Shrink_WithNegativeValue_ThrowsException()
        {
            var collection = new ItemCollection(3);

            Assert.Throws<ArgumentOutOfRangeException>(() => collection.Shrink(-1));
        }

        [Test]
        public void Shrink_WithValueGreaterThanTotalSlots_ThrowsException()
        {
            var collection = new ItemCollection(3);

            Assert.Throws<ArgumentOutOfRangeException>(() => collection.Shrink(4));
        }

        [Test]
        public void Shrink_UpdatesOccupiedSlotCount()
        {
            var collection = new ItemCollection(5);
            collection.AddItemAt(mockItem1, 3);
            collection.AddItemAt(mockItem2, 4);

            Assert.AreEqual(2, collection.OccupiedSlotCount);

            collection.Shrink(2, null);

            Assert.AreEqual(3, collection.TotalSlotCount);
            Assert.AreEqual(0, collection.OccupiedSlotCount);
        }

        [Test]
        public void Shrink_UpdatesFirstAvailableSlotIndex()
        {
            var collection = new ItemCollection(5);
            collection.AddItemAt(mockItem1, 0);
            collection.AddItemAt(mockItem2, 1);
            var mockItem3 = new MockItem(1) { ItemCode = 3 };
            collection.AddItemAt(mockItem3, 2);

            Assert.AreEqual(3, collection.FirstAvailableSlotIndex);

            collection.Shrink(2, null); // Remove slots 3 and 4

            // After shrinking, all remaining slots (0, 1, 2) are occupied
            Assert.AreEqual(-1, collection.FirstAvailableSlotIndex);
        }

        #endregion
    }
}