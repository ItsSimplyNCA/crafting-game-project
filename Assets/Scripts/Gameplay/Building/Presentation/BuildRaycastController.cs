using System;
using Game.Gameplay.Grid.Runtime;
using UnityEngine;

namespace Game.Gameplay.Building.Presentation {
    [DisallowMultipleComponent]
    public sealed class BuildRaycastController : MonoBehaviour {
        [Header("References")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private GridService gridService;

        [Header("Raycast")]
        [SerializeField] private LayerMask placementMask = ~0;
        [SerializeField, Min(0.1f)] private float maxBuildDistance = 8f;
        [SerializeField] private Vector2 viewportPoint = new Vector2(0.5f, 0.5f);
        [SerializeField, Min(0f)] private float surfaceOffset = 0.02f;

        [Header("Options")]
        [SerializeField] private bool autoResolveReferences = true;
        [SerializeField] private bool updateEveryFrame = true;

        public event Action<bool> TargetStateChanged;
        public event Action<Vector3Int> TargetCellChanged;

        public Camera PlayerCamera => playerCamera;
        public GridService GridService => gridService;
        public bool HasTarget { get; private set; }
        public RaycastHit CurrentHit { get; private set; }
        public Vector3Int CurrentCell { get; private set; }

        private void Awake() {
            if (autoResolveReferences) {
                ResolveMissingReferences();
            }
        }

        private void OnValidate() {
            maxBuildDistance = Mathf.Max(0.1f, maxBuildDistance);
            surfaceOffset = Mathf.Max(0f, surfaceOffset);

            if (!Application.isPlaying && autoResolveReferences) {
                ResolveMissingReferences();
            }
        }

        private void Update() {
            if (updateEveryFrame) {
                RefreshTarget();
            }
        }

        [ContextMenu("Resolve Missing References")]
        public void ResolveMissingReferences() {
            if (playerCamera == null) {
                playerCamera = Camera.main;

                if (playerCamera == null) {
                    playerCamera = FindObjectOfType<Camera>();
                }
            }

            if (gridService == null) {
                gridService = FindObjectOfType<GridService>();
            }
        }

        public bool RefreshTarget() {
            bool previousHasTarget = HasTarget;
            Vector3Int previousCell = CurrentCell;

            if (!TryComputeTarget(out RaycastHit hit, out Vector3Int targetCell)) {
                HasTarget = false;
                CurrentHit = default;
                CurrentCell = default;

                if (previousHasTarget) {
                    TargetStateChanged?.Invoke(false);
                }

                return false;
            }

            HasTarget = true;
            CurrentHit = hit;
            CurrentCell = targetCell;

            if (!previousHasTarget) {
                TargetStateChanged?.Invoke(true);
            }

            if (!previousHasTarget || previousCell != CurrentCell) {
                TargetCellChanged?.Invoke(CurrentCell);
            }

            return true;
        }

        public bool TryGetTargetCell(out Vector3Int targetCell) {
            if (!HasTarget) {
                return RefreshTargetInternal(out _, out targetCell);
            }

            targetCell = CurrentCell;
            return true;
        }

        public bool TryGetTarget(out RaycastHit hit, out Vector3Int targetCell) {
            if (!HasTarget) {
                return RefreshTargetInternal(out hit, out targetCell);
            }

            hit = CurrentHit;
            targetCell = CurrentCell;
            return true;
        }

        private bool RefreshTargetInternal(out RaycastHit hit, out Vector3Int targetCell) {
            bool success = TryComputeTarget(out hit, out targetCell);

            bool previousHasTarget = HasTarget;
            Vector3Int previousCell = CurrentCell;

            if (success) {
                HasTarget = true;
                CurrentHit = hit;
                CurrentCell = targetCell;

                if (!previousHasTarget) {
                    TargetStateChanged?.Invoke(true);
                }

                if (!previousHasTarget || previousCell != CurrentCell) {
                    TargetCellChanged?.Invoke(CurrentCell);
                }

                return true;
            }

            HasTarget = false;
            CurrentHit = default;
            CurrentCell = default;

            if (previousHasTarget) {
                TargetStateChanged?.Invoke(false);
            }

            return false;
        }

        private bool TryComputeTarget(out RaycastHit hit, out Vector3Int targetCell) {
            if (playerCamera == null || gridService == null) {
                hit = default;
                targetCell = default;
                return false;
            }

            Ray ray = playerCamera.ViewportPointToRay(new Vector3(viewportPoint.x, viewportPoint.y, 0f));

            if (!Physics.Raycast(ray, out hit, maxBuildDistance, placementMask, QueryTriggerInteraction.Ignore)) {
                targetCell = default;
                return false;
            }

            Vector3 point = hit.point + hit.normal * surfaceOffset;
            targetCell = gridService.WorldToCell(point);
            return true;
        }
    }
}