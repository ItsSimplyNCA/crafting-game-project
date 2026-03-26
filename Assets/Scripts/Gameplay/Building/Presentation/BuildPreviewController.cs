using System.Collections.Generic;
using Game.Gameplay.Building.Runtime;
using Game.Gameplay.Grid.Runtime;
using Game.Gameplay.WorldEntities.Data;
using UnityEngine;

namespace Game.Gameplay.Building.Presentation {
    [DisallowMultipleComponent]
    public sealed class BuildPreviewController : MonoBehaviour {
        [Header("References")]
        [SerializeField] private PlacementService placementService;
        [SerializeField] private BuildRaycastController raycastController;
        [SerializeField] private Transform previewRoot;

        [Header("Preview Materials")]
        [SerializeField] private Material validPreviewMaterial;
        [SerializeField] private Material invalidPreviewMaterial;

        [Header("Preview Behavior")]
        [SerializeField] private bool autoResolveReferences = true;
        [SerializeField] private bool hidePreviewWhenNoSelection = true;
        [SerializeField] private bool disableBehavioursOnPreview = true;
        [SerializeField] private bool disableCollidersOnPreview = true;
        [SerializeField] private bool makeRigidbodiesKinematic = true;
        [SerializeField] private bool disableShadows = true;
        [SerializeField] private bool updateEveryFrame = true;

        [Header("Runtime")]
        [SerializeField] private BuildSelectionState selectionState = new BuildSelectionState();

        public PlacementService PlacementService => placementService;
        public BuildRaycastController RaycastController => raycastController;
        public BuildSelectionState SelectionState => selectionState;

        public bool HasPreviewInstance => previewInstance != null;
        public bool IsCurrentPlacementValid { get; private set; }
        public PlaceableDefinition CurrentDefinition => selectionState.SelectedDefinition;

        private GameObject previewInstance;
        private PlaceableDefinition previewDefinition;
        private readonly List<Renderer> previewRenderers = new();
        private GridService cachedGridService;

        private void Awake() {
            if (autoResolveReferences) {
                ResolveMissingReferences();
            }

            cachedGridService = placementService != null ? placementService.GridService : null;

            if (selectionState != null) {
                selectionState.Changed += HandleSelectionChanged;
            }
        }

        private void OnDestroy() {
            if (selectionState != null) {
                selectionState.Changed -= HandleSelectionChanged;
            }

            DestroyPreviewInstance();
        }

        private void OnValidate() {
            if (!Application.isPlaying && autoResolveReferences) {
                ResolveMissingReferences();
            }
        }

        private void Update() {
            if (updateEveryFrame) {
                RefreshPreview();
            }
        }

        [ContextMenu("Resolve Missing References")]
        public void ResolveMissingReferences() {
            if (placementService == null) {
                placementService = FindObjectOfType<PlacementService>();
            }

            if (raycastController == null) {
                raycastController = FindObjectOfType<BuildRaycastController>();
            }
        }

        public void SetSelectedDefinition(PlaceableDefinition definition, bool resetRotation = true) {
            selectionState.Select(definition, resetRotation);
        }

        public void ClearSelection() {
            selectionState.Clear();
        }

        public void RotateClockwise() {
            selectionState.RotateClockwise();
        }

        public void RotateCounteClockwise() {
            selectionState.RotateCounterClockwise();
        }

        public void RefreshPreview() {
            if (placementService == null || raycastController == null) {
                HidePreview();
                return;
            }

            cachedGridService = placementService.GridService;
            
            if (selectionState == null || !selectionState.HasSelection) {
                if (hidePreviewWhenNoSelection) {
                    HidePreview();
                }
                
                return;
            }

            PlaceableDefinition definition = selectionState.SelectedDefinition;

            if (definition == null || definition.Prefab == null) {
                HidePreview();
                return;
            }

            if (!raycastController.TryGetTargetCell(out Vector3Int targetCell)) {
                HidePreview();
                return;
            }

            EnsurePreviewForDefinition(definition);

            if (previewInstance == null || cachedGridService == null) {
                HidePreview();
                return;
            }

            PlacementValidationResult validation = placementService.ValidatePlacement(
                definition,
                targetCell,
                selectionState.RotationQuarterTurns
            );

            IsCurrentPlacementValid = validation.IsValid;

            previewInstance.SetActive(true);
            previewInstance.transform.position = cachedGridService.GetPlacementWorldPosition(
                definition,
                targetCell,
                selectionState.RotationQuarterTurns
            );
            previewInstance.transform.rotation = definition.GetWorldRotation(selectionState.RotationQuarterTurns);

            ApplyPreviewMaterial(IsCurrentPlacementValid ? validPreviewMaterial : invalidPreviewMaterial);
        }

        public bool TryGetCurrentPlacement(
            out PlaceableDefinition definition,
            out Vector3Int originCell,
            out int rotationQuarterTurns,
            out PlacementValidationResult validation
        ) {
            definition = selectionState != null ? selectionState.SelectedDefinition : null;
            originCell = default;
            rotationQuarterTurns = selectionState != null ? selectionState.RotationQuarterTurns : 0;
            validation = default;

            if (placementService == null || raycastController == null || definition == null) {
                return false;
            }

            if (!raycastController.TryGetTargetCell(out originCell)) {
                return false;
            }

            validation = placementService.ValidatePlacement(definition, originCell, rotationQuarterTurns);
            return true;
        }

        private void HandleSelectionChanged(BuildSelectionState _) {
            if (selectionState == null || !selectionState.HasSelection) {
                HidePreview();
                return;
            }

            EnsurePreviewForDefinition(selectionState.SelectedDefinition);
            RefreshPreview();
        }

        private void EnsurePreviewForDefinition(PlaceableDefinition definition) {
            if (definition == null) {
                DestroyPreviewInstance();
                return;
            }

            if (previewInstance != null && previewDefinition == definition) {
                return;
            }

            DestroyPreviewInstance();

            if (definition.Prefab == null) return;

            previewDefinition = definition;
            previewInstance = Instantiate(
                definition.Prefab,
                Vector3.zero,
                definition.GetWorldRotation(selectionState.RotationQuarterTurns),
                previewRoot
            );

            previewInstance.name = $"{definition.DisplayName}_Preview";

            ConfigurePreviewInstance(previewInstance);

            previewRenderers.Clear();
            previewRenderers.AddRange(previewInstance.GetComponentsInChildren<Renderer>(true));
        }

        
    }
}