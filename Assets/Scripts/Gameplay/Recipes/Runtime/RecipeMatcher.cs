using Game.Gameplay.Inventory.Runtime;
using Game.Gameplay.Machines.Common.Runtime;
using Game.Gameplay.Recipes.Data;

namespace Game.Gameplay.Recipes.Runtime {
    public static class RecipeMatcher {
        public static bool IsRecipeValid(RecipeDefinition recipe) {
            if (recipe == null) return false;
            if (recipe.Inputs.Length == 0 || recipe.Outputs.Length == 0) return false;

            for (int i = 0; i < recipe.Inputs.Length; i++) {
                if (recipe.Inputs[i] == null || !recipe.Inputs[i].IsValid) return false;
            }

            for (int i = 0; i < recipe.Outputs.Length; i++) {
                if (recipe.Outputs[i] == null || !recipe.Outputs[i].IsValid) return false;
            }

            return true;
        }

        public static bool HasRequiredInputs(MachineBufferSet buffers, RecipeDefinition recipe) {
            if (buffers == null || recipe == null || buffers.Input == null) return false;

            for (int i = 0; i < recipe.Inputs.Length; i++) {
                RecipeIngredient ingredient = recipe.Inputs[i];

                if (ingredient == null || !ingredient.IsValid) return false;
                if (buffers.Input.CountItem(ingredient.Item) < ingredient.Amount) return false;
            }

            return true;
        }

        public static bool CanAcceptOutputs(MachineBufferSet buffers, RecipeDefinition recipe) {
            if (buffers == null || recipe == null || buffers.Output == null) return false;

            InventorySimulation simulation = InventorySimulation.FromContainer(buffers.Output);

            for (int i = 0; i < recipe.Outputs.Length; i++) {
                RecipeOutput output = recipe.Outputs[i];

                if (output == null || !output.IsValid) return false;

                if (!simulation.TryAdd(output.Item, output.Amount, out int remaining) || remaining > 0) {
                    return false;
                }
            }

            return true;
        }

        public static bool CanCraft(MachineBufferSet buffers, RecipeDefinition recipe) {
            return IsRecipeValid(recipe) &&
                HasRequiredInputs(buffers, recipe) &&
                CanAcceptOutputs(buffers, recipe);
        }

        public static bool TryConsumeInputs(MachineBufferSet buffers, RecipeDefinition recipe) {
            if (buffers == null || recipe == null || buffers.Input == null) return false;
            if (!HasRequiredInputs(buffers, recipe)) return false;

            for (int i = 0; i < recipe.Inputs.Length; i++) {
                RecipeIngredient ingredient = recipe.Inputs[i];

                if (!buffers.Input.RemoveItem(ingredient.Item, ingredient.Amount)) {
                    return false;
                }
            }

            return true;
        }

        public static bool TryProduceOutputs(MachineBufferSet buffers, RecipeDefinition recipe) {
            if (buffers == null || recipe == null || buffers.Output == null) return false;
            if (!CanAcceptOutputs(buffers, recipe)) return false;

            for (int i = 0; i < recipe.Outputs.Length; i++) {
                RecipeOutput output = recipe.Outputs[i];

                if (!buffers.Output.AddItem(output.Item, output.Amount)) return false;
            }

            return true;
        }
    }
}