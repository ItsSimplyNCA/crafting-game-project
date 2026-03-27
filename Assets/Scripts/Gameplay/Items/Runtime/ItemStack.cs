using System;
using NUnit.Framework;
using UnityEngine;

namespace Game.Gameplay.Items.Runtime {
    [Serializable]
    public struct ItemStack : IEquatable<ItemStack> {
        [SerializeField] private InventoryItemData item;
        [SerializeField] private int amount;

        public InventoryItemData Item => item;
        public int Amount => amount;
        public bool IsEmpty => item == null || amount <= 0;
        public int MaxStack => item != null ? Mathf.Max(1, item.maxStack) : 0;

        public ItemStack(InventoryItemData item, int amount) {
            this.item = item;
            this.amount = item == null ? 0 : Mathf.Max(0, amount);

            if (this.amount <= 0) {
                this.item = null;
                this.amount = 0;
            }
        }

        public static ItemStack Empty => default;

        public bool CanStackWith(InventoryItemData otherItem) {
            return item != null && item == otherItem;
        }

        public bool CanStackWith(ItemStack other) {
            return !IsEmpty && !other.IsEmpty && item == other.item;
        }

        public int GetRemainingCapacity() {
            if (item == null) return 0;
            return Mathf.Max(0, MaxStack - amount);
        }

        public ItemStack WithAmount(int newAmount) {
            return new ItemStack(item, newAmount);
        }

        public ItemStack Add(int addAmount) {
            if (item == null || addAmount <= 0) return this;
            return new ItemStack(item, Mathf.Min(amount + addAmount, MaxStack));
        }

        public ItemStack Remove(int removeAmount) {
            if (item == null || removeAmount <= 0) return this;
            int newAmount = Mathf.Max(0, amount - removeAmount);
            return new ItemStack(item, newAmount);
        }

        public bool Equals(ItemStack other) {
            return item == other.item && amount == other.amount;
        }

        public override bool Equals(object obj) {
            return obj is ItemStack other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                int hash = 17;
                hash = hash * 31 + (item != null ? item.GetHashCode() : 0);
                hash = hash * 31 + amount;
                return hash;
            }
        }

        public static bool operator ==(ItemStack left, ItemStack right) {
            return left.Equals(right);
        }

        public static bool operator !=(ItemStack left, ItemStack right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            return IsEmpty ? "Empty" : $"{item.itemName} x{amount}";
        }
    }
}