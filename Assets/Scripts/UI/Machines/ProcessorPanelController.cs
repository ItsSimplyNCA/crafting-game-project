using System.Collections.Generic;
using Game.Gameplay.Machines.Common.Runtime;
using Game.Gameplay.Machines.Processing.Presentation;
using Game.Gameplay.Machines.Processing.Runtime;
using Game.Gameplay.Recipes.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Machines {
    [DisallowMultipleComponent]
    public sealed class ProcessorPanelController : MonoBehaviour {
        [Header("Root")]
        [SerializeField] private GameObject root;
        [SerializeField] private bool manageCursor = true;

        [Header("Header")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private TMP_Text recipeText;
        [SerializeField] private Button closeButton;

        [Header("Progress")]
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private Image progressFill;

        [Header("Buffers")]
        [SerializeField] private TMP_Text inputSummaryText;
        [SerializeField] private TMP_Text outputSummaryText;

        [Header("Recipes")]
        [SerializeField] private Transform recipeButtonParent;
        [SerializeField] private ProcessorRecipeButtonView recipeButtonPrefab;

        private readonly List<ProcessorRecipeButtonView> spawnedButtons = new();

        private ProcessorView currentProcessorView;
        private ProcessorRuntime currentRuntime;

        public bool IsOpen => root != null && root.activeSelf;
        public ProcessorView CurrentProcessorView => currentProcessorView;

        private void Awake() {
            if (closeButton != null) {
                closeButton.onClick.AddListener(Hide);
            }

            if (root != null) {
                root.SetActive(false);
            }
        }

        private void Update() {
            if (IsOpen && Input.GetKeyDown(KeyCode.Escape)) {
                Hide();
            }
        }

        private void OnDestroy() {
            if (closeButton != null) {
                closeButton.onClick.RemoveListener(Hide);
            }

            UnbindRuntime();
            ClearRecipeButtons();
        }

        public void Show(ProcessorView processorView) {
            if (processorView == null) return;

            if (currentProcessorView != processorView) {
                BindToProcessor(processorView);
            }

            if (root != null) {
                root.SetActive(true);
            }

            if (manageCursor) {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }

            Refresh();
        }

        public void Hide() {
            if (root != null) {
                root.SetActive(false);
            }

            if (manageCursor) {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        private void BindToProcessor(ProcessorView processorView) {
            UnbindRuntime();

            currentProcessorView = processorView;
            currentRuntime = processorView != null ? processorView.Runtime : null;

            if (currentRuntime != null) {
                currentRuntime.Changed += HandleRuntimeChanged;
            }

            RebuildRecipeButtons();
            Refresh();
        }

        private void UnbindRuntime() {
            if (currentRuntime != null) {
                currentRuntime.Changed -= HandleRuntimeChanged;
            }

            currentRuntime = null;
            currentProcessorView = null;
        }

        private void RebuildRecipeButtons() {
            ClearRecipeButtons();

            if (currentProcessorView == null || recipeButtonParent == null || recipeButtonPrefab == null) {
                return;
            }

            RecipeDefinition[] recipes = currentProcessorView.AvailableRecipes;

            for (int i = 0; i < recipes.Length; i++) {
                RecipeDefinition recipe = recipes[i];

                if (recipe == null) continue;

                ProcessorRecipeButtonView button = Instantiate(recipeButtonPrefab, recipeButtonParent);
                button.Setup(recipe, i, HandleRecipeSelected, currentRuntime != null && i == currentRuntime.SelectedRecipeIndex);
                spawnedButtons.Add(button);
            }
        }

        private void ClearRecipeButtons() {
            for (int i = 0; i < spawnedButtons.Count; i++) {
                if (spawnedButtons[i] != null) {
                    Destroy(spawnedButtons[i].gameObject);
                }
            }

            spawnedButtons.Clear();
        }

        private void HandleRecipeSelected(int index) {
            if (currentProcessorView == null | currentRuntime == null) return;
            if (currentProcessorView.SelectRecipe(index)) Refresh();
        }

        private void HandleRuntimeChanged(ProcessorRuntime _) {
            Refresh();
        }

        private void Refresh() {
            if (
                currentProcessorView == null ||
                currentRuntime == null ||
                currentProcessorView.MachineView == null ||
                currentProcessorView.MachineView.Runtime == null
            ) {
                if (titleText != null) titleText.text = "Processor";
                if (stateText != null) stateText.text = "State: -";
                if (recipeText != null) recipeText.text = "Recipe: -";
                if (progressText != null) progressText.text = "Progess: 0%";
                if (progressFill != null) progressFill.fillAmount = 0f;
                if (inputSummaryText != null) inputSummaryText.text = "Input: -";
                if (outputSummaryText != null) outputSummaryText.text = "Output: -";
                return;
            }

            MachineRuntime machineRuntime = currentProcessorView.MachineView.Runtime;

            if (titleText != null) {
                titleText.text = machineRuntime.Definition != null ? machineRuntime.Definition.DisplayName : "Processor";
            }

            if (stateText != null) {
                stateText.text = $"State: {machineRuntime.State}";
            }

            if (recipeText != null) {
                recipeText.text = currentRuntime.SelectedRecipe != null
                    ? $"Recipe: {currentRuntime.SelectedRecipe.DisplayName}"
                    : "Recipe: -";
            }

            if (progressText != null) {
                progressText.text = $"Progress: {Mathf.RoundToInt(currentRuntime.ProgressNormalized * 100f)}";
            }

            if (progressFill != null) {
                progressFill.fillAmount = currentRuntime.ProgressNormalized;
            }

            if (inputSummaryText != null) {
                inputSummaryText.text = BuildContainerSummary("Input", machineRuntime.Buffers != null ? machineRuntime.Buffers.Input : null);
            }

            if (outputSummaryText != null) {
                outputSummaryText.text = BuildContainerSummary("Output", machineRuntime.Buffers != null ? machineRuntime.Buffers.Output : null);
            }

            for (int i = 0; i < spawnedButtons.Count; i++) {
                if (spawnedButtons[i] != null) {
                    spawnedButtons[i].SetSelected(i == currentRuntime.SelectedRecipeIndex);
                }
            }
        }

        private static string BuildContainerSummary(string label, Game.Gameplay.Inventory.Runtime.InventoryContainer container) {
            if (container == null || container.Slots == null) {
                return $"{label}: -";
            }

            int occupied = 0;

            for (int i = 0; i < container.Slots.Count; i++) {
                Game.Gameplay.Inventory.Runtime.InventorySlot slot = container.GetSlot(i);

                if (slot != null && !slot.IsEmpty) {
                    occupied++;
                }
            }

            return $"{label}: {occupied}/{container.Capacity} slots";
        }
    }
}