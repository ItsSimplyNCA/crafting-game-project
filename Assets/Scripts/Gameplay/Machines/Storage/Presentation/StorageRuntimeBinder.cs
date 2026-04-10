using Game.Gameplay.Machines.Storage.Data;
using Game.Gameplay.Machines.Storage.Runtime;
using Game.Gameplay.WorldEntities.Presentation;
using UnityEngine;

namespace Game.Gameplay.Machines.Storage.Presentation {
    [DisallowMultipleComponent]
    public sealed class StorageRuntimeBinder : MonoBehaviour {
        [Header("References")]
        [SerializeField] private StorageView storageView;
        [SerializeField] private PlaceableView placeableView;
        [SerializeField] private StorageDefinition storageDefinition;

        [Header("Options")]
        [SerializeField] private bool autoResolveReferences = true;
        [SerializeField] private bool autoCreateRuntimeOnAwake = true;
        [SerializeField] private bool verboseLogging = false;

        private StorageRuntime runtime;

        public StorageView StorageView => storageView;
        public PlaceableView PlaceableView => placeableView;
        public StorageDefinition StorageDefinition => storageDefinition;
        public StorageRuntime Runtime => runtime;

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
            if (storageView == null) {
                storageView = GetComponent<StorageView>();
            }

            if (placeableView == null) {
                placeableView = GetComponent<PlaceableView>();
            }
        }

        [ContextMenu("Create and Bind Runtime")]
        public bool TryCreateAndBindRuntime() {
            if (storageView == null || placeableView == null) {
                if (verboseLogging) {
                    Debug.LogWarning("StorageRuntimeBinder: hiányzik a StorageView vagy a PlaceableView.", this);
                }

                return false;
            }

            if (!placeableView.IsInitialized || placeableView.Runtime == null) {
                if (verboseLogging) {
                    Debug.LogWarning("StorageRuntimeBinder: a PlaceableView még nincs runtime-hoz kötve.", this);
                }

                return false;
            }

            if (storageDefinition == null) {
                Debug.LogError("StorageRuntimeBinder: nincs StorageDefinition beállítva.", this);
                return false;
            }

            runtime = new StorageRuntime(placeableView.Runtime, storageDefinition.SlotCount);
            storageView.Initialize(runtime, storageDefinition.DisplayName);

            if (verboseLogging) {
                Debug.Log($"StorageRuntimeBinder: runtime létrehozva -> {storageDefinition.DisplayName}", this);
            }

            return true;
        }
    }
}