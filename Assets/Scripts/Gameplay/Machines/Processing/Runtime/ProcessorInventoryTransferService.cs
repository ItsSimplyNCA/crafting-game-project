using Game.Gameplay.Inventory.Runtime;
using Game.Gameplay.Machines.Common.Runtime;
using Game.Gameplay.Recipes.Data;

namespace Game.Gameplay.Machines.Processing.Runtime {
    public static class ProcessorInventoryTransferService {
        public static bool HasProcessor(ProcessorRuntime processor) {
            return processor != null && processor.Machine != null && processor.Machine.Buffers != null;
        }

        public static int FillRequiredInputsFromInventory(
            ProcessorRuntime processor,
            InventoryContainer playerInventory
        ) {
            if (!HasProcessor(processor) || playerInventory == null) return 0;

            RecipeDefinition recipe = processor.SelectedRecipe;
            if (recipe == null || recipe.Inputs == null || recipe.Inputs.Length == 0) return 0;

            MachineBufferSet buffers = processor.Machine.Buffers;
            if (buffers.Input == null) return 0;

            int movedTotal = 0;

            for (int i = 0; i < recipe.Inputs.Length; i++) {
                RecipeIngredient ingredient = recipe.Inputs[i];

                if (ingredient == null || !ingredient.IsValid) continue;

                int existingInInput = buffers.Input.CountItem(ingredient.Item);
                int missing = ingredient.Amount - existingInInput;

                if (missing <= 0) continue;

                movedTotal += MachineBufferTransferService.MoveFromInventoryToInput(
                    playerInventory,
                    buffers,
                    ingredient.Item,
                    missing
                );
            }

            return movedTotal;
        }

        public static int CollectAllOutputsToInventory(
            ProcessorRuntime processor,
            InventoryContainer playerInventory
        ) {
            if (!HasProcessor(processor) || playerInventory == null) return 0;

            return MachineBufferTransferService.MoveAllOutputsToInventory(
                processor.Machine.Buffers,
                playerInventory
            );
        }

        public static int ReturnAllInputsToInventory(
            ProcessorRuntime processor,
            InventoryContainer playerInventory
        ) {
            if (!HasProcessor(processor) || playerInventory == null) return 0;

            MachineBufferSet buffers = processor.Machine.Buffers;

            if (buffers.Input == null) return 0;

            int movedTotal = 0;

            for (int i = 0; i < buffers.Input.Slots.Count; i++) {
                InventorySlot slot = buffers.Input.GetSlot(i);

                if (slot == null || slot.IsEmpty || slot.Item == null || slot.Amount <= 0) continue;

                movedTotal += MachineBufferTransferService.MoveFromInputToInventory(
                    buffers,
                    playerInventory,
                    slot.Item,
                    slot.Amount
                );
            }

            return movedTotal;
        }
    }
}