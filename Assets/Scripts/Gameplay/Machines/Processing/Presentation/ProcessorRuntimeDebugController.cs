using Game.Gameplay.Machines.Processing.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Gameplay.Machines.Processing.Presentation {
    [DisallowMultipleComponent]
    public sealed class ProcessorRuntimeDebugController : MonoBehaviour {
        [Header("References")]
        [SerializeField] private ProcessorView processorView;
        [SerializeField] private ProcessorRuntimeBinder processorRuntimeBinder;

        [Header("Input")]
        [SerializeField] private KeyCode nextRecipeKey = KeyCode.Period;
        [SerializeField] private KeyCode previousRecipeKey = KeyCode.Comma;
        [SerializeField] private KeyCode recreateRuntimeKey = KeyCode.F6;
        [SerializeField] private KeyCode logStateKey = KeyCode.F7;

        [Header("Options")]
        [SerializeField] private bool autoResolveReferences = true;
        [SerializeField] private bool verboseLogging = true;

        private void Awake() {
            if (autoResolveReferences) {
                ResolveMissingReferences();
            }
        }

        private void OnValidate() {
            if (!Application.isPlaying && autoResolveReferences) {
                ResolveMissingReferences();
            }
        }

        private void Update() {
            if (nextRecipeKey != KeyCode.None && Input.GetKeyDown(nextRecipeKey)) {
                SelectRelativeRecipe(+1);
            }

            if (previousRecipeKey != KeyCode.None && Input.GetKeyDown(previousRecipeKey)) {
                SelectRelativeRecipe(-1);
            }

            if (recreateRuntimeKey != KeyCode.None && Input.GetKeyDown(recreateRuntimeKey)) {
                RecreateRuntime();
            }

            if (logStateKey != KeyCode.None && Input.GetKeyDown(logStateKey)) {
                LogState();
            }
        }

        [ContextMenu("Resolve Missing References")]
        public void ResolveMissingReferences() {
            if (processorView == null) {
                processorView = GetComponent<ProcessorView>();
            }

            if (processorRuntimeBinder == null) {
                processorRuntimeBinder = GetComponent<ProcessorRuntimeBinder>();
            }
        }

        [ContextMenu("Recreate Runtime")]
        public void RecreateRuntime() {
            ResolveMissingReferences();

            if (processorRuntimeBinder == null) {
                Debug.LogWarning("ProcessorRuntimeDebugController: nincs ProcessorRuntimeBinder.", this);
                return;
            }

            processorRuntimeBinder.TryCreateAndBindRuntime();

            if (verboseLogging) {
                Debug.Log("ProcessorRuntimeDebugController: runtime újralétrehozva.", this);
            }
        }

        [ContextMenu("Log State")]
        public void LogState() {
            ResolveMissingReferences();

            if (processorView == null || processorView.Runtime == null || processorView.MachineView == null || processorView.MachineView.Runtime == null) {
                Debug.Log("ProcessorRuntimeDebugController: nincs aktív runtime.", this);
                return;
            }

            ProcessorRuntime runtime = processorView.Runtime;

            Debug.Log(
                "=== PROCESSOR DEBUG ===\n" +
                $"SelectedRecipeIndex: {runtime.SelectedRecipeIndex}\n" +
                $"SelectedRecipe: {(runtime.SelectedRecipe != null ? runtime.SelectedRecipe.DisplayName : "-")}\n" +
                $"Progress: {runtime.ProgressNormalized:P0}\n" +
                $"MachineState: {processorView.MachineView.Runtime.State}",
                this
            );
        }

        private void SelectRelativeRecipe(int delta) {
            ResolveMissingReferences();

            if (processorView == null || processorView.Runtime == null) return;

            int recipeCount = processorView.AvailableRecipes != null ? processorView.AvailableRecipes.Length : 0;

            if (recipeCount <= 0) return;

            int currentIndex = processorView.Runtime.SelectedRecipeIndex;
            int nextIndex = (currentIndex +  delta) % recipeCount;

            if (nextIndex < 0) {
                nextIndex += recipeCount;
            }

            bool changed = processorView.SelectRecipe(nextIndex);

            if (changed && verboseLogging) {
                Debug.Log($"ProcessorRuntimeDebugController: recipe selected -> {nextIndex}", this);
            }
        }
    }
}