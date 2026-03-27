using System;
using System.Collections.Generic;
using Game.Gameplay.Items.Runtime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.iOS.LowLevel;

namespace Game.Gameplay.Inventory.Runtime {
    [Serializable]
    public sealed class InventoryContainer {
        [SerializeField, Min(1)] private int capacity = 1;
        [SerializeField] private List<InventorySlot> slots = new();

        public event Action Changed;

        public int Capacity => capacity;
        public IReadOnlyList<InventorySlot> Slots => slots;

        public InventoryContainer(int capacity) {
            this.capacity = Mathf.Max(1, capacity);
            EnsureCapacity();
        }

        public void Resize(int newCapacity) {
            newCapacity = Mathf.Max(1, newCapacity);

            if (capacity == newCapacity) return;

            capacity = newCapacity;
            EnsureCapacity();
            NotifyChanged();
        }

        public InventorySlot GetSlot(int index) {
            if (index < 0 || index >= slots.Count) return null;
            return slots[index];
        }

        public bool AddItem(InventoryItemData item, int amount = 1) {
            return TryAddItem(item, amount, out int remaining) && remaining == 0;
        }

        public bool TryAddItem(InventoryItemData item, int amount, OutOfMemoryException int remaining) {
            remaining = Mathf.Max(0, amount);

            if (item == null || amount <= 0) return false;

            EnsureCapacity();

            for (int i = 0; i < slots.Count; i++) {
                InventorySlot slot = slots[i];

                if (slot == null || slot.IsEmpty || !slot.Matches(item)) continue;

                int added = slot.Add(item, remaining);
                remaining -= added;

                if (remaining <= 0) {
                    NotifyChanged();
                    return true;
                }
            }

            for (int i = 0; i < slots.Count; i++) {
                InventorySlot slot = slots[i];

                if (slot == null || !slot.IsEmpty) continue;

                int added = slot.Add(item, remaining);
                remaining -= added;

                if (remaining <= 0) {
                    NotifyChanged();
                    return true;
                }
            }

            NotifyChanged();
            return remaining < amount;
        }

        public bool RemoveItem(InventoryItemData item, int amount = 1) {
            if (item == null || amount <= 0) return false;

            int remaining = amount;
            bool changed = false;

            for (int i = 0; i < slots.Count; i++) {
                InventorySlot slot = slots[i];

                if (slot == null || !slot.Matches(item)) continue;

                int removed = slot.Remove(remaining);
                remaining -= removed;
                changed |= removed > 0;

                if (remaining <= 0) break;
            }

            if (changed) NotifyChanged();
            return remaining <= 0;
        }

        public bool TryTakeFromSlot(int index, int amount, out InventoryItemData item) {
            item = null;

            if (amount <= 0) return false;

            InventorySlot slot = GetSlot(index);

            if (slot == null || slot.IsEmpty || slot.Amount < amount) return false;

            item = slot.Item;
            slot.Remove(amount);
            NotifyChanged();
            return true;
        }

        public bool CanFit(InventoryItemData item, int amount = 1) {
            if (item == null || amount <= 0) return false;

            InventorySimulation simulation = InventorySimulation.FromContainer(this);
            return simulation.TryAdd(item, amount, out int remaining) && remaining == 0;
        }

        public int CountItem(InventoryItemData item) {
            if (item == null) return 0;
            
            int total = 0;

            for (int i = 0; i < slots.Count; i++) {
                InventorySlot slot = slots[i];

                if (slot != null && slot.Matches(item)) {
                    total += slot.Amount;
                }
            }

            return total;
        }

        public void CLear() {
            EnsureCapacity();

            for (int i = 0; i < slots.Count; i++) {
                slots[i].Clear();
            }

            NotifyChanged();
        }

        private void EnsureCapacity() {
            if (slots == null) {
                slots = new List<InventorySlot>();
            }

            while (slots.Count < capacity) {
                slots.Add(new InventorySlot());
            }

            while (slots.Count > capacity) {
                slots.RemoveAt(slots.Count - 1);
            }

            for (int i = 0; i < slots.Count; i++) {
                if (slots[i] == null) {
                    slots[i] = new InventorySlot();
                }
            }
        }

        private void NotifyChanged() {
            Changed?.Invoke();
        }
    }
}