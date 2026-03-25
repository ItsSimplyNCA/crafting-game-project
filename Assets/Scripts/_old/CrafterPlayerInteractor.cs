using UnityEngine;

public class CrafterPlayerInteractor : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private CrafterUI crafterUI;

    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField, Min(0.1f)] private float interactDistance = 4f;
    [SerializeField] private LayerMask interactionMask = ~0;

    private void Awake() {
        if (playerCamera == null) {
            playerCamera = Camera.main;
        }
    }

    private void Update() {
        if (crafterUI != null && crafterUI.IsOpen) return;

        if (Input.GetKeyDown(interactKey)) {
            TryOpenCrafter();
        }
    }

    private void TryOpenCrafter() {
        if (playerCamera == null || crafterUI == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactionMask, QueryTriggerInteraction.Ignore)) {
            return;
        }

        CrafterMachine machine = hit.collider.GetComponentInParent<CrafterMachine>();
        if (machine == null) return;

        crafterUI.Show(machine);
    }
}
