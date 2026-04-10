using Game.Gameplay.Machines.Storage.Presentation;
using Game.Gameplay.Machines.Storage.Runtime;
using Game.Player.InventoryAccess;
using Game.UI.Machines;
using UnityEngine;

namespace Game.Player.Interaction {
    [DisallowMultipleComponent]
    public sealed class PlayerStorageInteractionController : MonoBehaviour {
        [Header("References")]
        [SerializeField] private PlayerRaycaster playerRaycaster;
        [SerializeField] private PlayerInventoryController playerInventoryController;
        [SerializeField] private StoragePanelController storagePanelController;

        [Header("Interaction")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private bool blockInteractWhileOpen = false;

        [Header("Transfer")]
        [SerializeField] private KeyCode depositAllKey = KeyCode.F;
        [SerializeField] private KeyCode withdrawAllKey = KeyCode.G;
        [SerializeField] private bool preferOpenPanelTarget = true;
        [SerializeField] private bool verboseLogging = false;

        private void Awake() {
            if (playerRaycaster == null) {
                playerRaycaster = FindObjectOfType<PlayerRaycaster>();
            }

            if (playerInventoryController == null) {
                playerInventoryController = FindObjectOfType<PlayerInventoryController>();
            }

            if (storagePanelController == null) {
                storagePanelController = FindObjectOfType<StoragePanelController>();
            }
        }

        private void Update() {
            if (interactKey != KeyCode.None && Input.GetKeyDown(interactKey)) {
                TryInteract();
            }

            if (depositAllKey != KeyCode.None && Input.GetKeyDown(depositAllKey)) {
                DepositAll();
            }

            if (withdrawAllKey != KeyCode.None && Input.GetKeyDown(withdrawAllKey)) {
                WithdrawAll();
            }
        }

        [ContextMenu("Try Interact")]
        public void TryInteract() {
            if (blockInteractWhileOpen && storagePanelController != null & storagePanelController.IsOpen) {
                return;
            }

            if (playerRaycaster == null || storagePanelController == null) {
                return;
            }

            if (!playerRaycaster.TryRaycast(out RaycastHit hit) || hit.collider == null) {
                return;
            }

            StorageView storageView = hit.collider.GetComponentInParent<StorageView>();

            if (storageView == null) return;

            storagePanelController.Show(storageView);
        }

        [ContextMenu("Deposit All")]
        public void DepositAll() {
            StorageRuntime runtime = ResolveStorageRuntime();

            if (runtime == null || playerInventoryController == null || playerInventoryController.Container == null) {
                return;
            }

            int moved = StorageInventoryTransferService.MoveAllFromInventoryToStorage(
                playerInventoryController.Container,
                runtime
            );

            if (verboseLogging) {
                Debug.Log($"PlayerStorageInteractionController: deposited = {moved}", this);
            }
        }

        [ContextMenu("Withdraw All")]
        public void WithdrawAll() {
            StorageRuntime runtime = ResolveStorageRuntime();

            if (runtime == null || playerInventoryController == null || playerInventoryController.Container == null) {
                return;
            }

            int moved = StorageInventoryTransferService.MoveAllFromStorageToInventory(
                runtime,
                playerInventoryController.Container
            );

            if (verboseLogging) {
                Debug.Log($"PlayerStorageInteractionController: withdrawn = {moved}", this);
            }
        }

        private StorageRuntime ResolveStorageRuntime() {
            if (
                preferOpenPanelTarget &&
                storagePanelController != null &&
                storagePanelController.IsOpen &&
                storagePanelController.CurrentStorageView != null
            ) {
                return storagePanelController.CurrentStorageView.Runtime;
            }

            if (playerRaycaster != null && playerRaycaster.TryRaycast(out RaycastHit hit) && hit.collider != null) {
                StorageView storageView = hit.collider.GetComponentInParent<StorageView>();

                if (storageView != null) {
                    return storageView.Runtime;
                }
            }

            return null;
        }
    }
}