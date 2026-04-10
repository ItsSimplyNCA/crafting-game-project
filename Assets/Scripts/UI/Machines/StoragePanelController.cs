using System.Collections.Generic;
using Game.Gameplay.Inventory.Runtime;
using Game.Gameplay.Machines.Storage.Presentation;
using Game.Gameplay.Machines.Storage.Runtime;
using Game.UI.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Machines {
    [DisallowMultipleComponent]
    public sealed class StoragePanelController : MonoBehaviour {
        [Header("Root")]
        [SerializeField] private GameObject root;
        [SerializeField] private bool manageCursor = true;

        [Header("Header")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text summaryText;
        [SerializeField] private Button closeButton;

        [Header("Slots")]
        [SerializeField] private Transform slotParent;
        [SerializeField] private InventorySlotView slotViewPrefab;

        private readonly List<InventorySlotView> slotViews = new List<InventorySlotView>();

        private StorageView currentStorageView;
        private StorageRuntime currentRuntime;

        public bool IsOpen => root != null && root.activeSelf;
        public StorageView CurrentStorageView => currentStorageView;

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
            ClearSlotViews();
        }

        public void Show(StorageView storageView) {
            if (storageView == null) return;

            if (currentStorageView != storageView) {
                BindToStorage(storageView);
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

        public void Refresh() {
            if (currentRuntime == null || currentRuntime.Storage == null) {
                if (titleText != null) {
                    titleText.text = "Storage";
                }

                if (summaryText != null) {
                    summaryText.text = "Empty";
                }

                return;
            }

            InventoryContainer storage = currentRuntime.Storage;

            if (slotViews.Count != storage.Capacity) {
                RebuildSlots(storage.Capacity);
            }

            if (titleText != null) {
                titleText.text = currentStorageView != null ? currentStorageView.name : "Storage";
            }

            if (summaryText != null) {
                summaryText.text = BuildSummary(storage);
            }

            for (int i = 0; i < slotViews.Count; i++) {
                InventorySlotView slotView = slotViews[i];

                if (slotView == null) continue;

                slotView.Bind(storage.GetSlot(i));
            }
        }

        private void BindToStorage(StorageView storageView) {
            UnbindRuntime();

            currentStorageView = storageView;
            currentRuntime = storageView != null ? storageView.Runtime : null;

            if (currentRuntime != null) {
                currentRuntime.Changed += HandleRuntimeChanged;
            }

            RebuildSlots(currentRuntime != null && currentRuntime.Storage != null ? currentRuntime.Storage.Capacity : 0);
            Refresh();
        }

        private void UnbindRuntime() {
            if (currentRuntime != null) {
                currentRuntime.Changed -= HandleRuntimeChanged;
            }

            currentRuntime = null;
            currentStorageView = null;
        }

        private void HandleRuntimeChanged(StorageRuntime _) {
            Refresh();
        }

        private void RebuildSlots(int slotCount) {
            ClearSlotViews();

            if (slotParent == null || slotViewPrefab == null || slotCount <= 0) return;

            for (int i = 0; i < slotCount; i++) {
                InventorySlotView slotView = Instantiate(slotViewPrefab, slotParent);
                slotView.Setup(i);
                slotViews.Add(slotView);
            }
        }

        private void ClearSlotViews() {
            for (int i = 0; i < slotViews.Count; i++) {
                InventorySlotView slotView = slotViews[i];

                if (slotView == null) continue;

                Destroy(slotView.gameObject);
            }

            slotViews.Clear();
        }

        private static string BuildSummary(InventoryContainer storage) {
            if (storage == null || storage.Slots == null) {
                return "0 / 0";
            }

            int occupied = 0;

            for (int i = 0; i < storage.Slots.Count; i++) {
                InventorySlot slot = storage.GetSlot(i);

                if (slot != null && !slot.IsEmpty) {
                    occupied++;
                }
            }

            return $"{occupied} / {storage.Capacity} slots";
        }
    }
}