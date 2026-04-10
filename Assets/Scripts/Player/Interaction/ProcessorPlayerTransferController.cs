using Game.Gameplay.Machines.Processing.Presentation;
using Game.Gameplay.Machines.Processing.Runtime;
using Game.Player.InventoryAccess;
using Game.UI.Machines;
using UnityEngine;

namespace Game.Player.Interaction {
    [DisallowMultipleComponent]
    public sealed class ProcessorPlayerTransferController : MonoBehaviour {
        [Header("References")]
        [SerializeField] private PlayerRaycaster playerRaycaster;
        [SerializeField] private PlayerInventoryController playerInventoryController;
        [SerializeField] private ProcessorPanelController processorPanelController;

        [Header("Input")]
        [SerializeField] private KeyCode fillInputsKey = KeyCode.F;
        [SerializeField] private KeyCode collectOutputsKey = KeyCode.G;
        [SerializeField] private KeyCode returnInputsKey = KeyCode.H;

        [Header("Options")]
        [SerializeField] private bool preferOpenPanelTarget = true;
        [SerializeField] private bool verboseLogging = false;

        private void Awake() {
            if (playerRaycaster == null) {
                playerRaycaster = FindObjectOfType<PlayerRaycaster>();
            }

            if (playerInventoryController == null) {
                playerInventoryController = FindObjectOfType<PlayerInventoryController>();
            }

            if (processorPanelController == null) {
                processorPanelController = FindObjectOfType<ProcessorPanelController>();
            }
        }

        private void Update() {
            if (fillInputsKey != KeyCode.None && Input.GetKeyDown(fillInputsKey)) {
                TryFillInputs();
            }

            if (collectOutputsKey != KeyCode.None && Input.GetKeyDown(collectOutputsKey)) {
                TryCollectOutputs();
            }

            if (returnInputsKey != KeyCode.None && Input.GetKeyDown(returnInputsKey)) {
                TryReturnInputs();
            }
        }

        [ContextMenu("Fill Inputs")]
        public void TryFillInputs() {
            ProcessorRuntime runtime = ResolveProcessorRuntime();

            if (runtime == null || playerInventoryController == null || playerInventoryController.Container == null) {
                return;
            }

            int moved = ProcessorInventoryTransferService.FillRequiredInputsFromInventory(
                runtime,
                playerInventoryController.Container
            );

            if (verboseLogging) {
                Debug.Log($"ProcessorPlayerTransferController: input moved = {moved}", this);
            }
        }

        [ContextMenu("Collect Outputs")]
        public void TryCollectOutputs() {
            ProcessorRuntime runtime = ResolveProcessorRuntime();

            if (runtime == null || playerInventoryController == null || playerInventoryController.Container == null) {
                return;
            }

            int moved = ProcessorInventoryTransferService.CollectAllOutputsToInventory(
                runtime,
                playerInventoryController.Container
            );

            if (verboseLogging) {
                Debug.Log($"ProcessorPlayerTransferController: output moved = {moved}", this);
            }
        }

        [ContextMenu("Return Inputs")]
        public void TryReturnInputs() {
            ProcessorRuntime runtime = ResolveProcessorRuntime();

            if (runtime == null || playerInventoryController == null || playerInventoryController.Container == null) {
                return;
            }

            int moved = ProcessorInventoryTransferService.ReturnAllInputsToInventory(
                runtime,
                playerInventoryController.Container
            );

            if (verboseLogging) {
                Debug.Log($"ProcessorPlayerTransferController: returned input = {moved}", this);
            }
        }

        private ProcessorRuntime ResolveProcessorRuntime() {
            if (
                preferOpenPanelTarget &&
                processorPanelController != null &&
                processorPanelController.IsOpen &&
                processorPanelController.CurrentProcessorView != null
            ) {
                return processorPanelController.CurrentProcessorView.Runtime;
            }

            if (playerRaycaster != null && playerRaycaster.TryRaycast(out RaycastHit hit) && hit.collider != null) {
                ProcessorView processorView = hit.collider.GetComponentInParent<ProcessorView>();

                if (processorView != null) return processorView.Runtime;
            }

            return null;
        }
    }
}