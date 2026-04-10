using System.Collections.Generic;
using Game.Gameplay.Inventory.Runtime;

namespace Game.Gameplay.Machines.Storage.Runtime {
    public static class StorageInventoryTransferService {
        public static int MoveAllFromInventoryToStorage(
            InventoryContainer sourceInventory,
            StorageRuntime storageRuntime
        ) {
            if (sourceInventory == null || storageRuntime == null || storageRuntime.Storage == null) {
                return 0;
            }

            List<(InventoryItemData item, int amount)> snapshot = BuildSnapshot(sourceInventory);
            int movedTotal = 0;

            for (int i = 0; i < snapshot.Count; i++) {
                (InventoryItemData item, int amount) entry = snapshot[i];

                if (entry.item == null || entry.amount <= 0) continue;

                movedTotal += InventoryTransferService.TransferItem(
                    sourceInventory,
                    storageRuntime.Storage,
                    entry.item,
                    entry.amount
                );
            }

            return movedTotal;
        }

        public static int MoveAllFromStorageToInventory(
            StorageRuntime storageRuntime,
            InventoryContainer targetInventory
        ) {
            if (storageRuntime == null || storageRuntime.Storage == null || targetInventory == null) {
                return 0;
            }

            List<(InventoryItemData item, int amount)> snapshot = BuildSnapshot(storageRuntime.Storage);
            int movedTotal = 0;

            for (int i = 0; i < snapshot.Count; i++) {
                (InventoryItemData item, int amount) entry = snapshot[i];

                if (entry.item == null || entry.amount <= 0) continue;

                movedTotal += InventoryTransferService.TransferItem(
                    storageRuntime.Storage,
                    targetInventory,
                    entry.item,
                    entry.amount
                );
            }

            return movedTotal;
        }

        public static int MoveItemFromInventoryToStorage(
            InventoryContainer sourceInventory,
            StorageRuntime storageRuntime,
            InventoryItemData item,
            int amount
        ) {
            if (sourceInventory == null || storageRuntime == null || storageRuntime.Storage == null || item == null || amount <= 0) {
                return 0;
            }

            return InventoryTransferService.TransferItem(
                sourceInventory,
                storageRuntime.Storage,
                item,
                amount
            );
        }

        public static int MoveItemFromStorageToInventory(
            StorageRuntime storageRuntime,
            InventoryContainer targetInventory,
            InventoryItemData item,
            int amount
        ) {
            if (storageRuntime == null || storageRuntime.Storage == null || targetInventory == null || item == null || amount <= 0) {
                return 0;
            }

            return InventoryTransferService.TransferItem(
                storageRuntime.Storage,
                targetInventory,
                item,
                amount
            );
        }

        private static List<(InventoryItemData item, int amount)> BuildSnapshot(InventoryContainer container) {
            List<(InventoryItemData item, int amount)> snapshot = new List<(InventoryItemData item, int amount)>();

            if (container == null || container.Slots == null) return snapshot;

            for (int i = 0; i < container.Slots.Count; i++) {
                InventorySlot slot = container.GetSlot(i);

                if (slot == null || slot.IsEmpty || slot.Item == null || slot.Amount <= 0) {
                    continue;
                }

                snapshot.Add((slot.Item, slot.Amount));
            }

            return snapshot;
        }
    }
}