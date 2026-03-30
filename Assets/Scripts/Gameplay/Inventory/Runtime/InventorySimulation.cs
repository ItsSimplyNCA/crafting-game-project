using System.Collections.Generic;

namespace Game.Gameplay.Inventory.Runtime {
    public sealed class InventorySimulation {
        private readonly List<InventorySlot> slots;

        public IReadOnlyList<InventorySlot> Slots => slots;

        private InventorySimulation(List<InventorySlot> slots) {
            this.slots = slots ?? new List<InventorySlot>();
        }

        public static InventorySimulation FromContainer(InventoryContainer container) {
            List<InventorySlot> clonedSlots = new List<InventorySlot>();

            if (container != null && container.Slots != null) {
                for (int i = 0; i < container.Slots.Count; i++) {
                    InventorySlot slot = container.Slots[i];
                    clonedSlots.Add(slot != null ? slot.Clone() : new InventorySlot());
                }
            }

            return new InventorySimulation(clonedSlots);
        }

        public bool TryAdd(InventoryItemData item, int amount, out int remaining) {
            remaining = amount;

            if (item == null || amount <= 0) return false;

            for (int i = 0; i < slots.Count; i++) {
                InventorySlot slot = slots[i];

                if (slot == null || slot.IsEmpty || !slot.Matches(item)) continue;

                int added = slot.Add(item, remaining);
                remaining -= added;

                if (remaining <= 0) return true;
            }

            for (int i = 0; i < slots.Count; i++) {
                InventorySlot slot = slots[i];

                if (slot == null || !slot.IsEmpty) continue;

                int added = slot.Add(item, remaining);
                remaining -= added;

                if (remaining <= 0) return true;
            }

            return remaining < amount;
        }
    }
}