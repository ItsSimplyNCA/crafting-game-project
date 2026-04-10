using Game.Gameplay.Inventory.Runtime;
using Game.Gameplay.Machines.Storage.Runtime;
using TMPro;
using UnityEngine;

namespace Game.Gameplay.Machines.Storage.Presentation {
    [DisallowMultipleComponent]
    public sealed class StorageView : MonoBehaviour {
        [Header("Optional UI")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text summaryText;
        [SerializeField] private TMP_Text occupiedSlotsText;

        [Header("Options")]
        [SerializeField] private bool refreshEveryFrame = false;

        private StorageRuntime runtime;
        private string displayNameOverride;

        public StorageRuntime Runtime => runtime;
        public bool IsInitialized => runtime != null;

        public void Initialize(StorageRuntime storageRuntime, string displayName = null) {
            if (runtime != null) {
                runtime.Changed -= HandleRuntimeChanged;
            }

            runtime = storageRuntime;
            displayNameOverride = displayName;

            if (runtime != null) {
                runtime.Changed += HandleRuntimeChanged;
            }

            RefreshVisuals();
        }

        public void RefreshVisuals() {
            if (runtime == null || runtime.Storage == null) {
                if (titleText != null) {
                    titleText.text = "Storage";
                }

                if (summaryText != null) {
                    summaryText.text = "Empty";
                }

                if (occupiedSlotsText != null) {
                    occupiedSlotsText.text = "0 / 0";
                }

                return;
            }

            InventoryContainer storage = runtime.Storage;
            int occupied = CountOccupiedSlots(storage);

            if (titleText != null) {
                titleText.text = string.IsNullOrWhiteSpace(displayNameOverride) ? "Storage" : displayNameOverride;
            }

            if (summaryText != null) {
                summaryText.text = $"Slots: {occupied}/{storage.Capacity}";
            }

            if (occupiedSlotsText.text != null) {
                occupiedSlotsText.text = $"{occupied} / {storage.Capacity}";
            }
        }

        private void Update() {
            if (refreshEveryFrame) {
                RefreshVisuals();
            }
        }

        private void OnDestroy() {
            if (runtime != null) {
                runtime.Changed -= HandleRuntimeChanged;
            }
        }

        private void HandleRuntimeChanged(StorageRuntime _) {
            RefreshVisuals();
        }

        private static int CountOccupiedSlots(InventoryContainer storage) {
            int occupied = 0;

            if (storage == null || storage.Slots == null) return 0;

            for (int i = 0; i < storage.Slots.Count; i++) {
                InventorySlot slot = storage.GetSlot(i);

                if (slot != null && !slot.IsEmpty) {
                    occupied++;
                }
            }

            return occupied;
        }
    }
}