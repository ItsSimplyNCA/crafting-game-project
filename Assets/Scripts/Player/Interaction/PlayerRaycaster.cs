using UnityEngine;

namespace Game.Player.Interaction {
    [DisallowMultipleComponent]
    public sealed class PlayerRaycaster : MonoBehaviour {
        [Header("References")]
        [SerializeField] private Camera playerCamera;

        [Header("Raycast")]
        [SerializeField, Min(0.1f)] private float distance = 4f;
        [SerializeField] private LayerMask hitMask = ~0;
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

        public Camera PlayerCamera => playerCamera;
        public float Distance => distance;
        public LayerMask HitMask => hitMask;

        private void Awake() {
            if (playerCamera == null) {
                playerCamera = Camera.main;
            }
        }

        public bool TryRaycast(out RaycastHit hit) {
            if (playerCamera == null) {
                hit = default;
                return false;
            }

            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            return Physics.Raycast(ray, out hit, distance, hitMask, triggerInteraction);
        }
    }
}