using UnityEngine;

namespace Game.Gameplay.Inventory.Runtime {
    public static class InventoryTransferService {
        public static bool TransferAllOfItem (
            InventoryContainer source,
            InventoryContainer target,
            InventoryItemData item
        ) {
            if (source == null || target == null || item == null) return false;

            int amount = source.CountItem(item);

            if (amount <= 0) return false;

            return TransferItem(source, target, item, amount) > 0;
        }

        public static int TransferItem(
            InventoryContainer source,
            InventoryContainer target,
            InventoryItemData item,
            int amount
        ) {
            if (source == null || target == null || item == null || amount <= 0) return 0;

            int available = source.CountItem(item);

            int requested = Mathf.Min(available, amount);
            if (requested <= 0) return 0;

            InventorySimulation simulation = InventorySimulation.FromContainer(target);
            if (!simulation.TryAdd(item, requested, out int remaining)) return 0;

            int transferable = requested - remaining;
            if (transferable <= 0) return 0;

            bool removed = source.RemoveItem(item, transferable);
            if (!removed) return 0;

            bool added = target.AddItem(item, transferable);
            if (!added) {
                source.AddItem(item, transferable);
                return 0;
            }

            return transferable;
        }

        public static bool TryMoveSingleSlotStack(
            InventoryContainer source,
            int sourceSlotIndex,
            InventoryContainer target
        ) {
            if (source == null || target == null) return false;

            InventorySlot sourceSlot = source.GetSlot(sourceSlotIndex);

            if (sourceSlot == null || sourceSlot.IsEmpty) return false;

            InventoryItemData item = sourceSlot.Item;
            int amount = sourceSlot.Amount;

            int moved = TransferItem(source, target, item, amount);
            return moved > 0;
        }
    }
}