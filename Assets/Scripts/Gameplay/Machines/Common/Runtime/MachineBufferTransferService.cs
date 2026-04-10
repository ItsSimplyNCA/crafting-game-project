using Game.Gameplay.Inventory.Runtime;

namespace Game.Gameplay.Machines.Common.Runtime {
    public static class MachineBufferTransferService {
        public static int MoveFromInventoryToInput(
            InventoryContainer sourceInventory,
            MachineBufferSet buffers,
            InventoryItemData item,
            int amount
        ) {
            if (sourceInventory == null || buffers == null || buffers.Input == null || item == null || amount <= 0) {
                return 0;
            }

            return InventoryTransferService.TransferItem(
                sourceInventory,
                buffers.Input,
                item,
                amount
            );
        }

        public static int MoveFromInventoryToOutput(
            InventoryContainer sourceInventory,
            MachineBufferSet buffers,
            InventoryItemData item,
            int amount
        ) {
            if (sourceInventory == null || buffers == null || buffers.Input == null || item == null || amount <= 0) {
                return 0;
            }

            return InventoryTransferService.TransferItem(
                sourceInventory,
                buffers.Output,
                item,
                amount
            );
        }

        public static int MoveFromInputToInventory(
            MachineBufferSet buffers,
            InventoryContainer targetInventory,
            InventoryItemData item,
            int amount
        ) {
            if (targetInventory == null || buffers == null || buffers.Input == null || item == null || amount <= 0) {
                return 0;
            }

            return InventoryTransferService.TransferItem(
                buffers.Input,
                targetInventory,
                item,
                amount
            );
        }

        public static int MoveFromOutputToInventory(
            MachineBufferSet buffers,
            InventoryContainer targetInventory,
            InventoryItemData item,
            int amount
        ) {
            if (targetInventory == null || buffers == null || buffers.Output == null || item == null || amount <= 0) {
                return 0;
            }

            return InventoryTransferService.TransferItem(
                buffers.Output,
                targetInventory,
                item,
                amount
            );
        }

        public static int MoveAllOutputsToInventory(
            MachineBufferSet buffers,
            InventoryContainer targetInventory
        ) {
            if (buffers == null || buffers.Output == null || targetInventory == null) {
                return 0;
            }

            int movedTotal = 0;

            for (int i = 0; i < buffers.Output.Slots.Count; i++) {
                InventorySlot slot = buffers.Output.GetSlot(i);

                if (slot == null || slot.IsEmpty || slot.Item == null || slot.Amount <= 0) continue;

                movedTotal += InventoryTransferService.TransferItem(
                    buffers.Output,
                    targetInventory,
                    slot.Item,
                    slot.Amount
                );
            }

            return movedTotal;
        }
    }
}
