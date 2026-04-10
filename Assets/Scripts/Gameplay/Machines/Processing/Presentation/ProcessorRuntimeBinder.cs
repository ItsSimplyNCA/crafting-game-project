using Game.Gameplay.Machines.Common.Presentation;
using Game.Gameplay.Machines.Common.Runtime;
using Game.Gameplay.Machines.Processing.Data;
using Game.Gameplay.Machines.Processing.Runtime;
using Game.Gameplay.WorldEntities.Presentation;
using UnityEngine;

namespace Game.Gameplay.Machines.Processing.Presentation {
    [DisallowMultipleComponent]
    public sealed class ProcessorRuntimeBinder : MonoBehaviour {
        [Header("References")]
        [SerializeField] private ProcessorView processorView;
        [SerializeField] private MachineView machineView;
        [SerializeField] private PlaceableView placeableView;
        [SerializeField] private ProcessorDefinition processorDefinition;

        [Header("Options")]
        [SerializeField] private bool autoResolveReferences = true;
        [SerializeField] private bool autoCreateRuntimeOnAwake = true;
        [SerializeField] private bool selectDefaultRecipeAfterBind = true;
        [SerializeField] private bool verboseLogging = false;

        private MachineRuntime machineRuntime;
        private ProcessorRuntime processorRuntime;

        public ProcessorView ProcessorView => processorView;
        public MachineView MachineView => machineView;
        public PlaceableView PlaceableView => placeableView;
        public ProcessorDefinition ProcessorDefinition => processorDefinition;

        private void Awake() {
            if (autoResolveReferences) {
                ResolveMissingReferences();
            }

            if (autoCreateRuntimeOnAwake) {
                TryCreateAndBindRuntime();
            }
        }

        private void OnValidate() {
            if (!Application.isPlaying && autoResolveReferences) {
                ResolveMissingReferences();
            }
        }

        [ContextMenu("Resolve Missing References")]
        public void ResolveMissingReferences() {
            if (processorView == null) {
                processorView = GetComponent<ProcessorView>();
            }

            if (machineView == null) {
                machineView = GetComponent<MachineView>();
            }

            if (placeableView == null) {
                placeableView = GetComponent<PlaceableView>();
            }
        }

        [ContextMenu("Create And Bind Runtime")]
        public bool TryCreateAndBindRuntime() {
            if (processorView == null || machineView == null || placeableView == null) {
                if (verboseLogging) {
                    Debug.LogWarning("ProcessorRuntimeBinder: hiányzik a ProcessorView, MachineView vagy PlaceableView.", this);
                }

                return false;
            }

            if (!placeableView.IsInitialized || placeableView.Runtime == null) {
                if (verboseLogging) {
                    Debug.LogWarning("ProcessorRuntimeBinder: a PlaceableView még nincs runtime-hoz kötve.", this);
                }

                return false;
            }

            if (processorDefinition == null) {
                Debug.LogError("ProcessorRuntimeBinder: nincs ProcessorDefinition beállítva.", this);
                return false;
            }

            if (!processorDefinition.HasMachineDefinition) {
                Debug.LogError("ProcessorRuntimeBinder: a ProcessorDefinition nem tartalmaz MachineDefinition referenciát.", this);
                return false;
            }

            machineRuntime = new MachineRuntime(
                processorDefinition.MachineDefinition,
                placeableView.Runtime
            );

            machineView.SetMachineDefinition(processorDefinition.MachineDefinition);
            machineView.Initialize(machineRuntime);

            processorView.Initialize(machineView, processorRuntime);

            if (selectDefaultRecipeAfterBind && processorDefinition.HasRecipes) {
                int clampedIndex = Mathf.Clamp(
                    processorDefinition.DefaultRecipeIndex,
                    0,
                    processorDefinition.AvailableRecipes.Length - 1
                );

                processorRuntime.SelectRecipe(clampedIndex);
            }

            if (verboseLogging) {
                Debug.Log($"ProcessorRuntimeBinder: runtime létrehozva -> {processorDefinition.DisplayName}", this);
            }

            return true;
        }
    }
}