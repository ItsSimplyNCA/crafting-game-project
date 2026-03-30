using System.Collections.Generic;
using Game.Player.InventoryAccess;
using UnityEngine;

namespace Game.UI.Inventory {
    [DisallowMultipleComponent]
    public sealed class InventoryPanelController : MonoBehaviour {
        [Header("References")]
        [SerializeField] private PlayerInventoryController playerInventoryController;
        [SerializeField] private GameObject root;
        [SerializeField] private Transform slotParent;
        [SerializeField] private InventorySlotView slotViewPrefab;

        [Header("Options")]
        [SerializeField] private bool autoResolveReferences = true;
        [SerializeField] private bool syncVisibilityWithInventoryOpen = true;
        [SerializeField] private bool rebuildOnStart = true;

        private readonly List<InventorySlotView> slotViews = new List<InventorySlotView>();

        private void Awake() {
            if (autoResolveReferences) {
                ResolveMissingReferences();
            }

            BindController();
        }

        private void Start() {
            if (rebuildOnStart) {
                RebuildSlots();
            }

            RefreshAll();
            RefreshVisibility();
        }

        private void OnDestroy() {
            UnbindController();
        }

        private void OnValidate() {
            if (!Application.isPlaying && autoResolveReferences) {
                ResolveMissingReferences();
            }
        }

        [ContextMenu("Resolve Missing References")]
        public void ResolveMissingReferences() {
            if (playerInventoryController == null) {
                playerInventoryController = FindObjectOfType<PlayerInventoryController>();
            }
        }

        [ContextMenu("Rebuild Slots")]
        public void RebuildSlots() {
            ClearSlotViews();

            if (playerInventoryController == null || slotParent == null || slotViewPrefab == null) {
                return;
            }

            for (int i = 0; i < playerInventoryController.Capacity; i++) {
                InventorySlotView slotView = Instantiate(slotViewPrefab, slotParent);
                slotView.Setup(i);
                slotViews.Add(slotView);
            }
        }

        [ContextMenu("Refresh All")]
        public void RefreshAll() {
            if (playerInventoryController == null) return;

            if (slotViews.Count != playerInventoryController.Capacity) {
                RebuildSlots();
            }

            for (int i = 0; i < slotViews.Count; i++) {
                InventorySlotView slotView = slotViews[i];

                if (slotView == null) continue;

                slotView.Bind(playerInventoryController.GetSlot(i));
            }
        }

        public void Show() {
            if (root != null) {
                root.SetActive(true);
            }
        }

        public void Hide() {
            if (root != null) {
                root.SetActive(false);
            }
        }

        private void BindController() {
            if (playerInventoryController == null) return;

            playerInventoryController.Changed += HandleInventoryChanged;
            playerInventoryController.OpenStateChanged += HandleOpenStateChanged;
        }

        private void UnbindController() {
            if (playerInventoryController == null) return;

            playerInventoryController.Changed -= HandleInventoryChanged;
            playerInventoryController.OpenStateChanged -= HandleOpenStateChanged;
        }

        private void HandleInventoryChanged() {
            RefreshAll();
        }

        private void HandleOpenStateChanged(bool _) {
            RefreshVisibility();
        }

        private void RefreshVisibility() {
            if (!syncVisibilityWithInventoryOpen || root == null || playerInventoryController == null) {
                return;
            }

            root.SetActive(playerInventoryController.IsOpen);
        }

        private void ClearSlotViews() {
            for (int i = 0; i < slotViews.Count; i++) {
                InventorySlotView view = slotViews[i];

                if (view == null) continue;

                if (Application.isPlaying) {
                    Destroy(view.gameObject);
                } else {
                    DestroyImmediate(view.gameObject);
                }
            }

            slotViews.Clear();
        }
    }
}