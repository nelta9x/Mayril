using Mayril.InventorySystem;
using UnityEngine;

namespace Mayril.Tests
{
    public class MockInventory : Inventory
    {
        public bool ForceServer { get; set; } = true;
        protected override bool IsServerInstance => ForceServer;

        protected override IItem CreateItem(int itemCode, int quantity)
        {
            return new MockItem(quantity) { ItemCode = itemCode };
        }
    }
}