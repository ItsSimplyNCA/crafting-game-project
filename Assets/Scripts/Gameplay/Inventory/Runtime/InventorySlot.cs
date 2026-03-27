using System;
using Game.Gameplay.Items.Runtime;
using UnityEngine;

namespace Game.Gameplay.Inventory.Runtime {
    [Serializable]
    public sealed class InventorySlot {
        [SerializeField] private ItemStack stack;

        public ItemStack Stack => stack;
        public InventoryItemData Item => stack.Item;
        public int Amount => stack.Amount;
        public bool IsEmpty => stack.IsEmpty;
        public int MaxStack => stack.MaxStack;

        public InventorySlot() {
            stack = ItemStack.Empty;
        }

        public InventorySlot(InventoryItemData item, int amount) {
            stack = new ItemStack(item, amount);
        }

        public void Clear() {
            stack = ItemStack.Empty;
        }

        public void Set(InventoryItemData item, int amount) {
            stack = new ItemStack(item, amount);
        }

        public bool CanAccept(InventoryItemData item, int amount = 1) {
            if (item == null || amount <= 0) return false;
            if (IsEmpty) return amount <= Mathf.Max(1, item.maxStack);
            if (Item != item) return false;
            return Amount + amount <= MaxStack;
        }

        public int GetAddableAmount(InventoryItemData item, int requestedAmount) {
            if (item == null || requestedAmount <= 0) return 0;
            if (IsEmpty) return Mathf.Min(requestedAmount, Mathf.Max(1, item.maxStack));
            if (Item != item) return 0;
            return Mathf.Min(requestedAmount, Mathf.Max(0, MaxStack - Amount));
        }

        public int Add(InventoryItemData item, int amount) {
            int accepted = GetAddableAmount(item, amount);

            if (accepted <= 0) return 0;

            if (IsEmpty) {
                stack = new ItemStack(item, accepted);
            } else {
                stack = stack.Add(accepted);
            }

            return accepted;
        }

        public int Remove(int amount) {
            if (IsEmpty || amount <= 0) return 0;

            int removed = Mathf.Min(amount, Amount);
            stack = stack.Remove(removed);
            return removed;
        }

        public bool Matches(InventoryItemData item) {
            return !IsEmpty && Item == item;
        }

        public InventorySlot Clone() {
            return IsEmpty
                ? new InventorySlot()
                : new InventorySlot(Item, Amount);
        }

        public override string ToString() {
            return stack.ToString();
        }
    }
}