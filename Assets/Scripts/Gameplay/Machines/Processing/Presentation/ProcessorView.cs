using Game.Gameplay.Machines.Common.Presentation;
using Game.Gameplay.Machines.Common.Runtime;
using Game.Gameplay.Machines.Processing.Runtime;
using Game.Gameplay.Recipes.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay.Machines.Processing.Presentation {
    [DisallowMultipleComponent]
    public sealed class ProcessorView : MonoBehaviour {
        [Header("References")]
        [SerializeField] private MachineView machineView;
        
        [Header("Recipes")]
        [SerializeField] private RecipeDefinition[] availableRecipes;
        [SerializeField] private bool autoCreateProcessorRuntimeOnAwake = true;
        [SerializeField] private bool autoTickRuntime = true;

        [Header("Optional UI")]
        [SerializeField] private TMP_Text machineStateText;
        [SerializeField] private TMP_Text selectedRecipeText;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private Image progressFill;

        [Header("Debug")]
        [SerializeField] private bool verboseLogging = false;

        private ProcessorRuntime runtime;

        public MachineView MachineView => machineView;
        public ProcessorRuntime Runtime => runtime;
        public RecipeDefinition[] AvailableRecipes => availableRecipes ?? System.Array.Empty<RecipeDefinition>();
        public RecipeDefinition SelectedRecipe => runtime != null ? runtime.SelectedRecipe : null;
        public bool IsInitialized => runtime != null;

        private void Awake() {
            if (machineView == null) {
                machineView = GetComponent<MachineView>();
            }

            if (autoCreateProcessorRuntimeOnAwake) {
                TryCreateRuntime();
            }
        }

        private void Update() {
            if (autoTickRuntime && runtime != null) {
                runtime.Tick(Time.deltaTime);
            }
        }

        private void OnDestroy() {
            if (runtime != null) {
                runtime.Changed -= HandleRuntimeChanged;
            }
        }

        [ContextMenu("Create Runtime")]
        public bool TryCreateRuntime() {
            if (machineView == null || machineView.Runtime == null) {
                if (verboseLogging) {
                    Debug.LogWarning("ProcessorView: a MachineView vagy annak runtime-ja hiányzik.", this);
                }

                return false;
            }

            if (runtime != null) {
                runtime.Changed -= HandleRuntimeChanged;
            }

            runtime = new ProcessorRuntime(machineView.Runtime, availableRecipes);
            runtime.Changed += HandleRuntimeChanged;

            RefreshVisuals();

            if (verboseLogging) {
                Debug.Log("ProcessorView: ProcessorRuntime létrehozva.", this);
            }

            return true;
        }

        public void Initialize(MachineView boundMachineView, ProcessorRuntime processorRuntime) {
            if (runtime != null) {
                runtime.Changed -= HandleRuntimeChanged;
            }

            machineView = boundMachineView;
            runtime = processorRuntime;

            if (runtime != null) {
                runtime.Changed += HandleRuntimeChanged;
            }

            RefreshVisuals();
        }

        public bool SelectRecipe(int index) {
            if (runtime == null) return false;

            bool result = runtime.SelectRecipe(index);

            if (result) {
                RefreshVisuals();
            }

            return result;
        }

        public void RefreshVisuals() {
            if (runtime == null || machineView == null || machineView.Runtime == null) {
                if (machineStateText != null) machineStateText.text = "State: -";
                if (selectedRecipeText != null) selectedRecipeText.text = "Recipe: -";
                if (progressText != null) progressText.text = "Progress: 0%";
                if (progressFill != null) progressFill.fillAmount = 0f;
                return;
            }

            MachineRuntime machineRuntime = machineView.Runtime;

            if (machineStateText != null) {
                machineStateText.text = $"State: {machineRuntime.State}";
            }

            if (selectedRecipeText != null) {
                selectedRecipeText.text = runtime.SelectedRecipe != null
                    ? $"Recipe: {runtime.SelectedRecipe.DisplayName}"
                    : "Recipe: -";
            }

            if (progressText != null) {
                progressText.text = $"Progress: {Mathf.RoundToInt(runtime.ProgressNormalized * 100f)}%";
            }

            if (progressFill != null) {
                progressFill.fillAmount = runtime.ProgressNormalized;
            }
        }

        private void HandleRuntimeChanged(ProcessorRuntime _) {
            RefreshVisuals();
        }
    }
}