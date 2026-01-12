using System;
using Mayril.InventorySystem;

namespace Mayril.Tests
{
    public class MockItem : IItem
    {
        public int ItemCode { get; set; }
        public int Quantity { get; private set; }
        public int MaxStack { get; set; } = 999;

        public MockItem(int quantity)
        {
            Quantity = quantity;
        }
        
        public bool CanStack(IItem sourceItem, int quantityToStack)
        {
            if (quantityToStack < 0 || sourceItem == null || sourceItem == this)
            {
                return false;
            }

            if (sourceItem.ItemCode != ItemCode)
            {
                return false;
            }

            return Quantity < MaxStack;
        }

        public void Stack(IItem item, int quantity)
        {
            if (item is not MockItem otherMockItem)
            {
                return;
            }

            if (quantity <= 0 || quantity > otherMockItem.Quantity)
            {
                return;
            }
            
            Quantity += quantity;
            otherMockItem.ModifyQuantity(-quantity);
        }

        public bool CanModifyQuantity(int delta)
        {
            if (delta < 0)
            {
                return Quantity >= -delta;
            }
            return Quantity + delta <= MaxStack;
        }

        public void ModifyQuantity(int delta)
        {
            if (!CanModifyQuantity(delta))
            {
                throw new InvalidOperationException("Cannot modify quantity beyond limits.");
            }
            
            Quantity += delta;
        }

        public IItem Split(int quantity)
        {
            if(quantity <= 0 || quantity > Quantity)
            {
                return null;
            }

            var splitItem = new MockItem(quantity) { ItemCode = this.ItemCode, MaxStack = this.MaxStack };
            ModifyQuantity(-quantity);
            return splitItem;
        }

        public MockItem Clone()
        {
            return new MockItem(Quantity) { ItemCode = this.ItemCode, MaxStack = this.MaxStack };
        }
    }
}