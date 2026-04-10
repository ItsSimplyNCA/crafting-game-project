using Game.Gameplay.Machines.Common.Data;
using Game.Gameplay.Machines.Common.Runtime;
using Game.Gameplay.WorldEntities.Presentation;
using UnityEngine;

namespace Game.Gameplay.Machines.Common.Presentation {
    [DisallowMultipleComponent]
    public class MachineView : MonoBehaviour {
        [Header("Definition")]
        [SerializeField] private MachineDefinition machineDefinition;

        [Header("References")]
        [SerializeField] private PlaceableView placeableView;

        [Header("Behavior")]
        [SerializeField] private bool syncTransformFromPlaceableView = false;

        private MachineRuntime runtime;

        public MachineDefinition MachineDefinition => machineDefinition;
        public PlaceableView PlaceableView => placeableView;
        public MachineRuntime Runtime => runtime;
        public bool IsInitialized => runtime != null;

        protected virtual void Awake() {
            if (placeableView == null) {
                placeableView = GetComponent<PlaceableView>();
            }
        }

        protected virtual void LateUpdate() {
            if (!syncTransformFromPlaceableView) return;
            if (placeableView == null || !placeableView.IsInitialized) return;

            transform.position = placeableView.transform.position;
            transform.rotation = placeableView.transform.rotation;
        }

        public virtual void SetMachineDefinition(MachineDefinition definition) {
            machineDefinition = definition;
        }

        public virtual void Initialize(MachineRuntime machineRuntime) {
            if (runtime != null) {
                runtime.Changed -= HandleRuntimeChanged;
            }

            runtime = machineRuntime;

            if (runtime != null) {
                machineDefinition = runtime.Definition;
                runtime.Changed += HandleRuntimeChanged;
            }

            RefreshVisuals();
        }

        public virtual void RefreshVisuals() {

        }

        protected virtual void OnDestroy() {
            if (runtime != null) {
                runtime.Changed -= HandleRuntimeChanged;
            }
        }

        protected virtual void HandleRuntimeChanged(MachineRuntime _) {
            RefreshVisuals();
        }
    }
}