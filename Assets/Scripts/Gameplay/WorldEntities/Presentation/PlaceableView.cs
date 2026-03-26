using Game.Gameplay.Grid.Runtime;
using Game.Gameplay.WorldEntities.Data;
using Game.Gameplay.WorldEntities.Runtime;
using UnityEngine;

namespace Game.Gameplay.WorldEntities.Presentation {
    [DisallowMultipleComponent]
    public sealed class PlaceableView : MonoBehaviour {
        [Header("Definition")]
        [SerializeField] private PlaceableDefinition definition;

        [Header("Behavior")]
        [SerializeField] private bool syncTransformFromRuntime = true;

        public PlaceableDefinition Definition => runtime != null ? runtime.Definition : definition;
        public PlaceableRuntime Runtime => runtime;
        public GridService GridService => gridService;
        public bool IsInitialized => runtime != null && gridService != null;


        private PlaceableRuntime runtime;
        private GridService gridService;

        public void Initialize(PlaceableRuntime runtime, GridService gridService) {
            if (runtime == null) {
                Debug.LogError("PlaceableView.Initialize: runtime is null.", this);
                return;
            }

            if (gridService == null) {
                Debug.LogError("PlaceableView.Initialize: gridService is null.", this);
                return;
            }

            this.runtime = runtime;
            this.gridService = gridService;
            definition = runtime.Definition;

            RefreshFromRuntime();
        }

        public void RefreshFromRuntime() {
            if (!IsInitialized || !syncTransformFromRuntime) return;

            transform.position = gridService.GetPlacementWorldPosition(
                runtime.Definition,
                runtime.OriginCell,
                runtime.RotationQuarterTurns
            );

            transform.rotation = runtime.WorldRotation;
        }

        public void SetVisible(bool visible) {
            gameObject.SetActive(visible);
        }

        private void LateUpdate() {
            if (syncTransformFromRuntime && IsInitialized) {
                RefreshFromRuntime();
            }
        }
    }
}