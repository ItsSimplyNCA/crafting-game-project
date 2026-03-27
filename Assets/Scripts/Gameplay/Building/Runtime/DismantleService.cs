using System;
using Game.Gameplay.WorldEntities.Presentation;
using UnityEngine;

namespace Game.Gameplay.Building.Runtime {
    [DisallowMultipleComponent]
    public sealed class DismantleService : MonoBehaviour {
        [Header("References")]
        [SerializeField] private PlacementService placementService;

        [Header("Options")]
        [SerializeField] private bool autoResolvePlacementService = true;
        [SerializeField] private bool verboseLogging = false;

        public PlacementService PlacementService => placementService;

        public event Action<PlaceableView> PlaceableDismantled;

        private void Awake() {
            if (autoResolvePlacementService && placementService == null) {
                placementService = FindObjectOfType<PlacementService>();
            }
        }

        private void OnValidate() {
            if (!Application.isPlaying && autoResolvePlacementService && placementService == null) {
                placementService = FindObjectOfType<PlacementService>();
            }
        }

        public bool TryDismantleFromHit(RaycastHit hit) {
            if (hit.collider == null) return false;

            PlaceableView view = hit.collider.GetComponentInParent<PlaceableView>();
            return TryDismantle(view);
        }

        public bool TryDismantle(PlaceableView view) {
            if (placementService == null || view == null || view.Runtime == null) {
                return false;
            }

            if (view.Runtime.Definition == null) {
                return false;
            }

            if (!view.Runtime.Definition.CanBeRemoved) {
                if (verboseLogging) {
                    Debug.Log($"DismantleService: '{view.name}' nem bontható.");
                }

                return false;
            }

            bool removed = placementService.Remove(view);

            if (removed) {
                PlaceableDismantled?.Invoke(view);
            }

            return removed;
        }
    }
}