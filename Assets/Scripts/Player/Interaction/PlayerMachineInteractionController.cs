using Game.Gameplay.Machines.Processing.Presentation;
using Game.UI.Machines;
using UnityEngine;

namespace Game.Player.Interaction {
    [DisallowMultipleComponent]
    public sealed class PlayerMachineInteractionController : MonoBehaviour {
        [Header("References")]
        [SerializeField] private PlayerRaycaster playerRaycaster;
        [SerializeField] private ProcessorPanelController processorPanelController;

        [Header("Input")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private bool blockInteractionWhilePanelOpen = false;

        private void Awake() {
            if (playerRaycaster == null) {
                playerRaycaster = FindObjectOfType<PlayerRaycaster>();
            }

            if (processorPanelController == null) {
                processorPanelController = FindObjectOfType<ProcessorPanelController>();
            }
        }

        private void Update() {
            if (interactKey == KeyCode.None) return;
            if (!Input.GetKeyDown(interactKey)) return;
            if (blockInteractionWhilePanelOpen && processorPanelController != null && processorPanelController.IsOpen) {
                return;
            }

            TryInteract();
        }

        private void TryInteract() {
            if (playerRaycaster == null || processorPanelController == null) return;
            if (!playerRaycaster.TryRaycast(out RaycastHit hit)) return;

            ProcessorView processorView = hit.collider != null
                ? hit.collider.GetComponentInParent<ProcessorView>()
                : null;

            if (processorView == null) return;

            processorPanelController.Show(processorView);
        }
    }
}