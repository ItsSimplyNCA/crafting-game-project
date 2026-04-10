using Game.Gameplay.Machines.Common.Data;
using Game.Gameplay.Machines.Common.Runtime;
using Game.Gameplay.WorldEntities.Presentation;
using UnityEngine;

namespace Game.Gameplay.Machines.Common.Presentation {
    [DisallowMultipleComponent]
    public sealed class MachineRuntimeBinder : MonoBehaviour {
        [Header("References")]
        [SerializeField] private MachineView machineView;
        [SerializeField] private PlaceableView placeableView;
        [SerializeField] private MachineDefinition machineDefinition;

        [Header("Options")]
        [SerializeField] private bool autoCreateRuntimeOnAwake = true;
        [SerializeField] private bool autoResolveReferences = true;
        [SerializeField] private bool verboseLogging = false;

        private MachineRuntime runtime;

        public MachineView MachineView => machineView;
        public PlaceableView PlaceableView => placeableView;
        public MachineDefinition MachineDefinition => machineDefinition;
        public MachineRuntime Runtime => runtime;

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
            if (machineView == null) {
                machineView = GetComponent<MachineView>();
            }

            if (placeableView == null) {
                placeableView = GetComponent<PlaceableView>();
            }
        }

        [ContextMenu("Create And Bind Runtime")]
        public bool TryCreateAndBindRuntime() {
            if (machineView == null || placeableView == null) {
                if (verboseLogging) {
                    Debug.LogWarning("MachineRuntimeBinder: hiányzik a MachineView vagy a PlaceableView.", this);
                }

                return false;
            }

            if (!placeableView.IsInitialized || placeableView.Runtime == null) {
                if (verboseLogging) {
                    Debug.LogWarning("MachineRuntimeBinder: a PlaceableView még nincs runtime-hoz kötve.", this);
                }

                return false;
            }

            MachineDefinition resolvedDefinition = machineDefinition != null
                ? machineDefinition
                : machineView.MachineDefinition;

            if (resolvedDefinition == null) {
                Debug.LogError("MachineRuntimeBinder: nincs MachineDefinition beállítva.", this);
                return false;
            }

            runtime = new MachineRuntime(resolvedDefinition, placeableView.Runtime);
            machineView.Initialize(runtime);

            if (verboseLogging) {
                Debug.Log($"MachineRuntimeBinder: runtime létrehozva -> {resolvedDefinition.DisplayName}", this);
            }

            return true;
        }
    }
}