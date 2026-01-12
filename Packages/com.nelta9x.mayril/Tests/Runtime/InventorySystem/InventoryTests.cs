using System;
using System.Collections;
using System.Collections.Generic;
using Mayril.InventorySystem;
using Mayril.Tests;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Mayril.Tests.InventorySystem
{
    [TestFixture]
    public class InventoryTests
    {
        private MockItem mockItem1;
        private MockItem mockItem2;
        private MockInventory inventory;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            mockItem1 = new MockItem(1) { ItemCode = 1, MaxStack = 10 };
            mockItem2 = new MockItem(1) { ItemCode = 2, MaxStack = 10 };
            inventory = new GameObject("TestInventory").AddComponent<MockInventory>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (inventory != null)
            {
                Object.Destroy(inventory.gameObject);
            }
            yield return null;
        }

        [Test]
        public void AddItem_ToEmptyInventory_PlacesItemInFirstSlot()
        {
            inventory.Extend(2);

            var result = inventory.AddItem(mockItem1.Clone());

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.SlotIndex);
            Assert.IsFalse(result.WasStacked);
            Assert.AreEqual(1, inventory.OccupiedSlotCount);
            Assert.IsNotNull(inventory.GetItemAt(0));
            Assert.IsNull(inventory.GetItemAt(1));
        }

        [Test]
        public void AddItem_WhenAllSlotsOccupied_ReturnsFailed()
        {
            inventory.Extend(2);

            inventory.AddItem(mockItem1.Clone());
            inventory.AddItem(mockItem2.Clone());

            var item3 = new MockItem(1) { ItemCode = 3 };
            var result = inventory.AddItem(item3);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(-1, result.SlotIndex);
        }

        [Test]
        public void AddItem_StackableItem_StacksInExistingSlot()
        {
            inventory.Extend(2);

            var initialItem = mockItem1.Clone();
            initialItem.ModifyQuantity(2); // 3 total
            inventory.AddItem(initialItem);

            var newItem = mockItem1.Clone();
            newItem.ModifyQuantity(1); // 2 total
            
            var result = inventory.AddItem(newItem);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.SlotIndex);
            Assert.IsTrue(result.WasStacked);
            Assert.AreEqual(5, inventory.GetItemAt(0).Quantity);
            Assert.AreEqual(0, newItem.Quantity);
        }

        [Test]
        public void SwapSlots_MovesItemToEmptySlot()
        {
            inventory.Extend(3);
            var item = mockItem1.Clone();
            inventory.AddItemAt(item, 0);

            bool success = inventory.SwapSlots(0, 2);

            Assert.IsTrue(success);
            Assert.IsNull(inventory.GetItemAt(0));
            Assert.AreEqual(item, inventory.GetItemAt(2));
        }

        [Test]
        public void SwapSlots_SwapsTwoItems()
        {
            inventory.Extend(2);
            var item1 = mockItem1.Clone();
            var item2 = mockItem2.Clone();
            inventory.AddItemAt(item1, 0);
            inventory.AddItemAt(item2, 1);

            bool success = inventory.SwapSlots(0, 1);

            Assert.IsTrue(success);
            Assert.AreEqual(item2, inventory.GetItemAt(0));
            Assert.AreEqual(item1, inventory.GetItemAt(1));
        }

        [Test]
        public void SwapSlots_SameItemType_StacksThem()
        {
            inventory.Extend(2);
            var item1 = mockItem1.Clone();
            item1.ModifyQuantity(2); // 3 total
            var item2 = mockItem1.Clone();
            item2.ModifyQuantity(1); // 2 total
            
            inventory.AddItemAt(item1, 0);
            inventory.AddItemAt(item2, 1);

            bool success = inventory.SwapSlots(1, 0);

            Assert.IsTrue(success);
            Assert.IsNull(inventory.GetItemAt(1));
            Assert.AreEqual(5, inventory.GetItemAt(0).Quantity);
        }

        [Test]
        public void SplitStack_SplitsToEmptySlot()
        {
            inventory.Extend(2);
            var item = mockItem1.Clone();
            item.ModifyQuantity(4); // 5 total
            inventory.AddItemAt(item, 0);

            bool success = inventory.SplitStack(0, 1, 2);

            Assert.IsTrue(success);
            Assert.AreEqual(3, inventory.GetItemAt(0).Quantity);
            Assert.AreEqual(2, inventory.GetItemAt(1).Quantity);
            Assert.AreEqual(mockItem1.ItemCode, inventory.GetItemAt(1).ItemCode);
        }

        [Test]
        public void ModifyItemQuantityAt_ZeroQuantity_RemovesItem()
        {
            inventory.Extend(1);
            inventory.AddItemAt(mockItem1.Clone(), 0);

            bool success = inventory.ModifyItemQuantityAt(0, -1);

            Assert.IsTrue(success);
            Assert.IsNull(inventory.GetItemAt(0));
            Assert.AreEqual(0, inventory.OccupiedSlotCount);
        }

        [Test]
        public void OnItemMoved_FiresWhenSwapping()
        {
            inventory.Extend(2);
            var item = mockItem1.Clone();
            inventory.AddItemAt(item, 0);

            int movedFrom = -1;
            int movedTo = -1;
            inventory.OnItemMoved += (movedItem, from, to) =>
            {
                movedFrom = from;
                movedTo = to;
            };

            inventory.SwapSlots(0, 1);

            Assert.AreEqual(0, movedFrom);
            Assert.AreEqual(1, movedTo);
        }

        [TestCase(5, 0, true)]
        [TestCase(5, 4, true)]
        [TestCase(5, 5, false)]
        [TestCase(5, -1, false)]
        public void IsValidSlotIndex_ReturnsExpected(int total, int index, bool expected)
        {
            inventory.Extend(total);
            Assert.AreEqual(expected, inventory.IsValidSlotIndex(index));
        }

        [Test]
        public void Shrink_CollectsLostItems()
        {
            inventory.Extend(3);
            var item = mockItem1.Clone();
            inventory.AddItemAt(item, 2);

            var lostItems = new List<IItem>();
            inventory.Shrink(1, lostItems);

            Assert.AreEqual(2, inventory.TotalSlotCount);
            Assert.AreEqual(1, lostItems.Count);
            Assert.AreEqual(item, lostItems[0]);
        }

        [Test]
        public void ClientSideSync_UpdatesLocalCollection()
        {
            inventory.Extend(1);
            
            // 서버 강제 해제 (클라이언트 모드 시뮬레이션)
            inventory.ForceServer = false;
            
            // 리플렉션으로 private NetworkList에 접근하여 이벤트 발생 시뮬레이션
            var networkSlotsField = typeof(Inventory).GetField("_networkSlots", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var networkSlots = (NetworkList<InventorySlot>)networkSlotsField.GetValue(inventory);
            
            // OnNetworkSpawn을 수동으로 호출하여 이벤트 구독 활성화
            inventory.OnNetworkSpawn();

            // 네트워크 리스트에 값 추가 (서버 권한 없이 직접 추가하여 이벤트 유도)
            networkSlots[0] = new InventorySlot(mockItem1.ItemCode, 5);

            // 로컬 컬렉션이 업데이트되었는지 확인
            Assert.IsNotNull(inventory.GetItemAt(0));
            Assert.AreEqual(mockItem1.ItemCode, inventory.GetItemAt(0).ItemCode);
            Assert.AreEqual(5, inventory.GetItemAt(0).Quantity);
        }
    }
}