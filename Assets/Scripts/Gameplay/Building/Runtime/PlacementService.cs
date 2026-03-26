using System;
using System.Collections.Generic;
using Game.Gameplay.Grid.Runtime;
using Game.Gameplay.WorldEntities.Data;
using Game.Gameplay.WorldEntities.Presentation;
using Game.Gameplay.WorldEntities.Runtime;
using UnityEngine;

namespace Game.Gameplay.Building.Runtime {
    [DisallowMultipleComponent]
    public sealed class PlacementService : MonoBehaviour {
        [Header("References")]
        [SerializeField] private GridService gridService;
        [SerializeField] private Transform placedObjectRoot;

        [Header("Options")]
        [SerializeField] private bool autoResolveGridService = true;
        [SerializeField] private bool addPlaceableViewIfMissing = true;

        private readonly Dictionary<string, PlaceableView> placedViewsByRuntimeId = new();

        private PlacementValidator validator;

        public GridService GridService => gridService;
        public IReadOnlyDictionary<string, PlaceableView> PlacedViewsByRuntimeId => placedViewsByRuntimeId;

        public Action<PlaceableRuntime, PlaceableView> PlaceablePlaced;
        public Action<PlaceableRuntime, PlaceableView> PlaceableRemoved;

        private void Awake() {
            if (autoResolveGridService && gridService == null) {
                gridService = FindObjectOfType<GridService>();
            }

            validator = new PlacementValidator(gridService);
        }

        private void OnValidate() {
            if (!Application.isPlaying && autoResolveGridService && gridService == null) {
                gridService = FindObjectOfType<GridService>();
            }
        }

        public bool CanPlace(PlaceableDefinition definition, Vector3Int originCell, int rotationQuarterTurns) {
            EnsureValidator();
            return validator.CanPlace(definition, originCell, rotationQuarterTurns);
        }

        public PlacementValidationResult ValidatePlacement(
            PlaceableDefinition definition,
            Vector3Int originCell,
            int rotationQuarterTurns
        ) {
            EnsureValidator();
            return validator.Validate(definition, originCell, rotationQuarterTurns);
        }

        public bool TryPlace(
            PlaceableDefinition definition,
            Vector3Int originCell,
            int rotationQuarterTurns,
            out PlaceableView placedView
        ) {
            placedView = null;

            EnsureValidator();

            PlacementValidationResult validation = validator.Validate(
                definition,
                originCell,
                rotationQuarterTurns
            );

            if (!validation.IsValid) {
                Debug.LogWarning($"PlacementService: placement denied -> {validation}");
                return false;
            }

            PlaceableRuntime runtime = new PlaceableRuntime(
                definition,
                originCell,
                rotationQuarterTurns
            );

            if (!gridService.TryRegister(runtime)) {
                Debug.LogWarning("PlacementService: a runtime nem regisztrálható a gridben.");
                return false;
            }

            placedView = CreateView(runtime);

            if (placedView == null) {
                gridService.Remove(runtime);
                Debug.LogError("PlacementService: nem sikerült létrehozni a PlaceableView-t.");
                return false;
            }

            placedViewsByRuntimeId[runtime.RuntimeId] = placedView;
            PlaceablePlaced?.Invoke(runtime, placedView);
            return true;
        }

        public bool Remove(PlaceableView view) {
            if (view == null || view.Runtime == null) return false;
            return Remove(view.Runtime);
        }

        public bool Remove(PlaceableRuntime runtime) {
            if (runtime == null) return false;

            placedViewsByRuntimeId.TryGetValue(runtime.RuntimeId, out PlaceableView view);

            bool removedFromGrid = gridService != null && gridService.Remove(runtime);
            bool removedFromDictionary = placedViewsByRuntimeId.Remove(runtime.RuntimeId);

            if (view != null) Destroy(view.gameObject);

            if (removedFromGrid || removedFromDictionary) {
                PlaceableRemoved?.Invoke(runtime, view);
                return true;
            }

            return false;
        }

        public bool TryGetView(string runtimeId, out PlaceableView view) {
            if (string.IsNullOrWhiteSpace(runtimeId)) {
                view = null;
                return false;
            }

            return placedViewsByRuntimeId.TryGetValue(runtimeId, out view);
        }

        public void CLearPlacedViewsOnly() {
            List<PlaceableView> views = new List<PlaceableView>(placedViewsByRuntimeId.Values);

            placedViewsByRuntimeId.Clear();

            for (int i = 0; i < views.Count; i++) {
                if (views[i] != null) {
                    Destroy(views[i].gameObject);
                }
            }
        }

        private PlaceableView CreateView(PlaceableRuntime runtime) {
            if (runtime == null || runtime.Definition == null) return null;

            GameObject prefab = runtime.Definition.Prefab;

            if (prefab == null) {
                Debug.LogError($"PlacementService: a(z) '{runtime.Definition.DisplayName}' placeable definitionhöz nincs prefab rendelve.");
                return null;
            }

            Vector3 worldPosition = gridService.GetPlacementWorldPosition(
                runtime.Definition,
                runtime.OriginCell,
                runtime.RotationQuarterTurns
            );

            Quaternion worldRotation = runtime.WorldRotation;

            Transform parent = placedObjectRoot != null ? placedObjectRoot : null;
            GameObject instance = Instantiate(prefab, worldPosition, worldRotation, parent);

            PlaceableView view = instance.GetComponent<PlaceableView>();

            if (view == null && addPlaceableViewIfMissing) {
                view = instance.AddComponent<PlaceableView>();
            }

            if (view == null) {
                Debug.LogError($"PlacementService: a(z) '{prefab.name}' prefabon nincs PlaceableView, és az auto-add ki van kapcsolva.");
                Destroy(instance);
                return null;
            }

            view.Initialize(runtime, gridService);
            return view;
        }

        private void EnsureValidator() {
            if (validator == null || gridService != validatorGridServiceCache) {
                validator = new PlacementValidator(gridService);
                validatorGridServiceCache = gridService;
            }
        }

        private GridService validatorGridServiceCache;
    }
}