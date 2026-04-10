using System;
using Game.Gameplay.Machines.Common.Runtime;
using Game.Gameplay.Recipes.Data;
using Game.Gameplay.Recipes.Runtime;
using UnityEngine;

namespace Game.Gameplay.Machines.Processing.Runtime {
    [Serializable]
    public sealed class ProcessorRuntime {
        private readonly MachineRuntime machine;
        private readonly RecipeDefinition[] availableRecipes;

        private int selectedRecipeIndex = -1;
        private float progressSeconds;

        public event Action<ProcessorRuntime> Changed;

        public MachineRuntime Machine => machine;
        public MachineBufferSet Buffers => machine.Buffers;
        public RecipeDefinition[] AvailableRecipes => availableRecipes ?? Array.Empty<RecipeDefinition>();
        public int SelectedRecipeIndex => selectedRecipeIndex;
        public RecipeDefinition SelectedRecipe => IsValidRecipeIndex(selectedRecipeIndex) ? availableRecipes[selectedRecipeIndex] : null;

        public float ProgressSeconds => progressSeconds;
        public float CurrentDuration => SelectedRecipe != null ? SelectedRecipe.CraftDuration : machine.Definition.WorkDuration;
        public float ProgressNormalized => CurrentDuration <= 0f ? 0f : Mathf.Clamp01(progressSeconds / CurrentDuration);

        public ProcessorRuntime(MachineRuntime machine, RecipeDefinition[] availableRecipes) {
            if (machine == null) {
                throw new ArgumentNullException(nameof(machine));
            }

            this.machine = machine;
            this.availableRecipes = availableRecipes ?? Array.Empty<RecipeDefinition>();

            if (this.availableRecipes.Length > 0) {
                selectedRecipeIndex = 0;
            }

            this.machine.Changed += HandleMachineChanged;
        }
        
        public bool SelectRecipe(int index) {
            if (!IsValidRecipeIndex(index)) return false;
            if (selectedRecipeIndex == index) return true;

            selectedRecipeIndex = index;
            progressSeconds = 0f;
            machine.MarkIdle(resetProgress: true);
            NotifyChanged();
            return true;
        }

        public void Tick(float deltaTime) {
            if (!machine.IsEnabled) {
                progressSeconds = 0f;
                NotifyChanged();
                return;
            }

            RecipeDefinition recipe = SelectedRecipe;

            if (recipe == null) {
                progressSeconds = 0f;
                machine.MarkIdle(resetProgress: true);
                NotifyChanged();
                return;
            }

            if (!RecipeMatcher.IsRecipeValid(recipe)) {
                progressSeconds = 0f;
                machine.MarkError();
                NotifyChanged();
                return;
            }

            if (!RecipeMatcher.HasRequiredInputs(Buffers, recipe)) {
                progressSeconds = 0f;
                machine.MarkWaitingForInput(resetProgress: true);
                NotifyChanged();
                return;
            }

            if (!RecipeMatcher.CanAcceptOutputs(Buffers, recipe)) {
                progressSeconds = 0f;
                machine.MarkBlockedOutput();
                NotifyChanged();
                return;
            }

            machine.SetState(MachineState.Processing);

            if (deltaTime > 0f) {
                progressSeconds += deltaTime;
            }

            if (progressSeconds < CurrentDuration) {
                NotifyChanged();
                return;
            }

            CompleteCraft(recipe);
        }

        private void CompleteCraft(RecipeDefinition recipe) {
            if (!RecipeMatcher.TryConsumeInputs(Buffers, recipe)) {
                progressSeconds = 0f;
                machine.MarkError();
                NotifyChanged();
                return;
            }

            if (!RecipeMatcher.TryProduceOutputs(Buffers, recipe)) {
                progressSeconds = 0f;
                machine.MarkError();
                NotifyChanged();
                return;
            }

            progressSeconds = 0f;
            machine.MarkIdle(resetProgress: true);
            NotifyChanged();
        }

        private bool IsValidRecipeIndex(int index) {
            return availableRecipes != null &&
                index >= 0 &&
                index < availableRecipes.Length &&
                availableRecipes[index] != null;
        }

        private void HandleMachineChanged(MachineRuntime _) {
            NotifyChanged();
        }

        private void NotifyChanged() {
            Changed?.Invoke(this);
        }
    }
}