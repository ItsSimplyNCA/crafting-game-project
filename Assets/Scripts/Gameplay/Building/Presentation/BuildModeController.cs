using System;
using Game.Gameplay.Building.Data;
using Game.Gameplay.Building.Runtime;
using Game.Gameplay.WorldEntities.Data;
using Game.Gameplay.WorldEntities.Presentation;
using Game.Gameplay.WorldEntities.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Gameplay.Building.Presentation {
    [DisallowMultipleComponent]
    public sealed class BuildModeController : MonoBehaviour {
        [Header("References")]
        [SerializeField] private BuildPreviewController previewController;
        [SerializeField] private BuildRaycastController raycastController;
        [SerializeField] private PlacementService placementService;
        [SerializeField] private DismantleService dismantleService;

        [Header("Legacy Compatibility")]
        [SerializeField] private InventorySystem legacyInventorySystem;
        [SerializeField] private bool autoResolveLegacyInventorySystem = true;
        [SerializeField] private bool blockInputWhileInventoryIsOpen = true;

        [Header("Catalog")]
        [SerializeField] private BuildCatalogEntry[] buildCatalog;
        [SerializeField] private bool autoSelectFirstEntryOnStart = true;
        [SerializeField] private bool enableNumericFallbackHotkeys = true;
        [SerializeField] private KeyCode clearSelectionKey = KeyCode.Alpha0;

        [Header("Input")]
        [SerializeField] private KeyCode rotateClockwiseKey = KeyCode.R;
        [SerializeField] private KeyCode rotateCounterClockwiseKey = KeyCode.None;
        [SerializeField] private int placeMouseButton = 0;
        [SerializeField] private int removeMouseButton = 1;

        [Header("Debug")]
        [SerializeField] private bool verboseLogging = false;

        public BuildPreviewController PreviewController => previewController;
        public BuildRaycastController RaycastController => raycastController;
        public PlacementService PlacementService => placementService;
        public DismantleService DismantleService => dismantleService;

        public event Action<PlaceableDefinition> SelectionChanged;
        public event Action<PlaceableRuntime, PlaceableView> PlaceablePlaced;
        public event Action<PlaceableView> PlaceableRemoved;

        private void Awake() {
            ResolveMissingReferences();

            if (autoSelectFirstEntryOnStart) {
                TrySelectFirstValidEntry();
            }
        }

        private void OnValidate() {
            if (!Application.isPlaying) {
                ResolveMissingReferences();
            }
        }

        private void Update() {
            if (IsInputBlocked()) return;

            HandleSelectionInput();
            HandleRotationInput();
            HandlePlacementInput();
            HandleRemovalInput(); 
        }

        [ContextMenu("Resolve Missing References")]
        public void ResolveMissingReferences() {
            if (previewController == null) {
                previewController = FindObjectOfType<BuildPreviewController>();
            }

            if (raycastController == null) {
                raycastController = FindObjectOfType<BuildRaycastController>();
            }

            if (placementService == null) {
                placementService = FindObjectOfType<PlacementService>();
            }

            if (dismantleService == null) {
                dismantleService = FindObjectOfType<DismantleService>();
            }

            if (autoResolveLegacyInventorySystem && legacyInventorySystem == null) {
                legacyInventorySystem = FindObjectOfType<InventorySystem>();
            }
        }

        public bool TrySelectByIndex(int index) {
            if (buildCatalog == null || index < 0 || index >= buildCatalog.Length) {
                return false;
            }

            BuildCatalogEntry entry = buildCatalog[index];

            if (entry == null || !entry.IsSelectable) return false;
            if (previewController == null) return false;

            previewController.SetSelectedDefinition(entry.Definition);
            SelectionChanged?.Invoke(entry.Definition);

            if (verboseLogging) {
                Debug.Log($"BuildModeController: selected -> {entry.DisplayName}", this);
            }

            return true;
        }

        public void ClearSelection() {
            if (previewController == null) return;

            previewController.ClearSelection();
            SelectionChanged?.Invoke(null);

            if (verboseLogging) {
                Debug.Log("BuildModeController: selection cleared.", this);
            }
        }

        public bool TryPlaceCurrentSelection() {
            if (previewController == null || placementService == null) {
                return false;
            }

            if (!previewController.TryGetCurrentPlacement(
                out PlaceableDefinition definition,
                out Vector3Int originCell,
                out int rotationQuarterTurns,
                out PlacementValidationResult validation
            )) {
                return false;
            }

            if (!validation.IsValid) {
                if (verboseLogging) {
                    Debug.Log($"BuildModeController: invalid placement -> {validation}", this);
                }

                return false;
            }

            bool placed = placementService.TryPlace(
                definition,
                originCell,
                rotationQuarterTurns,
                out PlaceableView placedView
            );

            if (!placed || placedView == null || placedView.Runtime == null) {
                return false;
            }

            PlaceablePlaced?.Invoke(placedView.Runtime, placedView);

            if (verboseLogging) {
                Debug.Log($"BuildModeController: invalid placement -> {validation}", this);
            }

            return true;
        }

        public bool TryRemoveCurrentTarget() {
            if (raycastController == null || dismantleService == null) {
                return false;
            }

            if (!raycastController.TryGetTarget(out RaycastHit hit, out _)) {
                return false;
            }

            PlaceableView view = hit.collider != null
                ? hit.collider.GetComponentInParent<PlaceableView>()
                : null;
            
            bool removed = dismantleService.TryDismantleFromHit(hit);

            if (removed && view != null) {
                PlaceableRemoved?.Invoke(view);

                if (verboseLogging) {
                    Debug.Log($"BuildModeController: removed -> {view.name}", this);
                }
            }

            return removed;
        }

        private void HandleSelectionInput() {
            if (buildCatalog == null || buildCatalog.Length == 0) return;

            if (clearSelectionKey != KeyCode.None && Input.GetKeyDown(clearSelectionKey)) {
                ClearSelection();
                return;
            }

            for (int i = 0; i < buildCatalog.Length; i++) {
                BuildCatalogEntry entry = buildCatalog[i];

                if (entry == null || !entry.IsSelectable) continue;

                KeyCode resolvedKey = ResolveSelectionKey(entry, i);

                if (resolvedKey != KeyCode.None && Input.GetKeyDown(resolvedKey)) {
                    TrySelectByIndex(i);
                    return;
                }
            }
        }

        private void HandleRotationInput() {
            if (previewController == null) return;

            if (rotateClockwiseKey != KeyCode.None && Input.GetKeyDown(rotateClockwiseKey)) {
                previewController.RotateClockwise();
                return;
            }

            if (rotateCounterClockwiseKey != KeyCode.None && Input.GetKeyDown(rotateCounterClockwiseKey)) {
                previewController.RotateCounteClockwise();
            }
        }

        private void HandlePlacementInput() {
            if (placeMouseButton < 0) return;

            if (Input.GetMouseButtonDown(placeMouseButton)) {
                TryPlaceCurrentSelection();
            }
        }

        private void HandleRemovalInput() {
            if (removeMouseButton < 0) return;

            if (Input.GetMouseButtonDown(removeMouseButton)) {
                TryRemoveCurrentTarget();
            }
        }

        private bool IsInputBlocked() {
            return blockInputWhileInventoryIsOpen &&
                legacyInventorySystem != null &&
                legacyInventorySystem.IsOpen;
        }

        private void TrySelectFirstValidEntry() {
            if (buildCatalog == null || buildCatalog.Length == 0) return;

            for (int i = 0; i < buildCatalog.Length; i++) {
                if (TrySelectByIndex(i)) return;
            }
        }

        private KeyCode ResolveSelectionKey(BuildCatalogEntry entry, int index) {
            if (entry == null) {
                return KeyCode.None;
            }

            if (entry.HasExplicitHotkey) {
                return entry.SelectKey;
            }

            if (!enableNumericFallbackHotkeys) {
                return KeyCode.None;
            }

            if (index < 0 || index > 0) {
                return KeyCode.None;
            }

            return KeyCode.Alpha1 + index;
        }
    }
}